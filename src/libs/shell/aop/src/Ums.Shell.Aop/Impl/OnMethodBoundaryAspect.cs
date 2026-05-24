using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Ums.Shell.Aop
{
    /// <summary>
    /// Base class for aspects that need to intercept method entry, success, exception and exit.
    ///
    /// Async-aware: when the intercepted method returns a <see cref="Task"/> or
    /// <see cref="Task{TResult}"/>, the <see cref="OnSuccess"/>, <see cref="OnException"/> and
    /// <see cref="OnExit"/> hooks are deferred until the task actually completes rather than
    /// firing at the moment the Task object is returned.  The synchronous hooks still fire
    /// synchronously for methods that return value types or <c>void</c>.
    /// </summary>
    public abstract class OnMethodBoundaryAspect<T> : AbstractAspect<T> where T : AbstractAspectAttribute
    {
        protected bool HandleException;

        protected T CurrentAttribute;

        // Cached MethodInfo for the generic Task<TResult> wrapper; one entry per closed
        // generic instantiation of OnMethodBoundaryAspect<T>.
        private static readonly MethodInfo _wrapAsyncOfTMethod =
            typeof(OnMethodBoundaryAspect<T>)
                .GetMethod(nameof(WrapAsyncOfT), BindingFlags.NonPublic | BindingFlags.Instance)!;

        public override void Apply(IJoinPoint joinPoint)
        {
            CurrentAttribute = GetAttribute(joinPoint);

            Init(joinPoint);

            OnEntry(joinPoint);

            var asyncPath = false;

            try
            {
                if (Continue(joinPoint))
                {
                    if (GetNext() == null)
                        joinPoint.Proceed();
                    else
                        GetNext().Apply(joinPoint);

                    // ── Async path ────────────────────────────────────────────────────
                    // DispatchProxy.Invoke is synchronous; Task-returning methods hand
                    // back a hot Task immediately — the work has NOT finished yet.
                    // We must chain OnSuccess / OnException / OnExit onto that Task's
                    // continuation so the hooks fire after the awaitable completes.
                    if (joinPoint.Return is Task returnedTask)
                    {
                        asyncPath = true;

                        var returnType = joinPoint.MethodInfo.ReturnType;

                        if (returnType.IsGenericType &&
                            returnType.GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            // Task<TResult>: preserve the result value by using a typed wrapper.
                            var resultType = returnType.GetGenericArguments()[0];
                            joinPoint.Return = _wrapAsyncOfTMethod
                                .MakeGenericMethod(resultType)
                                .Invoke(this, new object[] { joinPoint, returnedTask });
                        }
                        else
                        {
                            // Plain Task (no result).
                            joinPoint.Return = WrapAsync(joinPoint, returnedTask);
                        }

                        return; // OnSuccess / OnExit will be called inside the wrapper tasks
                    }

                    // ── Synchronous path ──────────────────────────────────────────────
                    OnSuccess(joinPoint);
                }
            }
            catch (Exception ex)
            {
                if (HandleException)
                    OnException(joinPoint, ex);
                else
                    throw;
            }
            finally
            {
                // OnExit must NOT fire here for the async path; the wrapper tasks handle it.
                if (!asyncPath)
                    OnExit(joinPoint);
            }
        }

        // ── Async wrappers ────────────────────────────────────────────────────────────

        private async Task WrapAsync(IJoinPoint joinPoint, Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
                OnSuccess(joinPoint);
            }
            catch (Exception ex)
            {
                if (HandleException)
                    OnException(joinPoint, ex);
                else
                    throw;
            }
            finally
            {
                OnExit(joinPoint);
            }
        }

        // Invoked via reflection for Task<TResult> return types so the result is preserved.
        private async Task<TResult> WrapAsyncOfT<TResult>(IJoinPoint joinPoint, Task task)
        {
            try
            {
                var result = await ((Task<TResult>)task).ConfigureAwait(false);
                OnSuccess(joinPoint);
                return result;
            }
            catch (Exception ex)
            {
                if (HandleException)
                    OnException(joinPoint, ex);
                else
                    throw;
                return default!; // unreachable; satisfies the compiler
            }
            finally
            {
                OnExit(joinPoint);
            }
        }

        // ── Extension points ──────────────────────────────────────────────────────────

        protected virtual bool Continue(IJoinPoint joinPoint) => true;

        protected virtual void OnEntry(IJoinPoint joinPoint) { }

        protected virtual void OnSuccess(IJoinPoint joinPoint) { }

        protected virtual void OnExit(IJoinPoint joinPoint) { }

        protected virtual void OnException(IJoinPoint joinPoint, Exception ex) { }
    }
}
