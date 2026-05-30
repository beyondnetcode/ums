using System.Reflection;
using Ums.Application.Common.Interfaces;
using BeyondNetCode.Shell.Aop;

namespace Ums.Application.Common.Aop;

public sealed class TransactionAspect : AbstractAspect<TransactionAspectAttribute>
{
    private static readonly MethodInfo WrapAsyncOfTMethod =
        typeof(TransactionAspect).GetMethod(nameof(WrapAsyncOfT), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private readonly IUnitOfWorkScope _unitOfWorkScope;

    public TransactionAspect(IUnitOfWorkScope unitOfWorkScope)
    {
        _unitOfWorkScope = unitOfWorkScope;
    }

    public override void Apply(IJoinPoint joinPoint)
    {
        var attribute = GetAttribute(joinPoint);
        if (attribute is null)
        {
            Proceed(joinPoint);
            return;
        }

        var tx = _unitOfWorkScope.BeginAsync().GetAwaiter().GetResult();

        try
        {
            Proceed(joinPoint);

            if (joinPoint.Return is Task returnedTask)
            {
                var returnType = joinPoint.MethodInfo.ReturnType;
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var resultType = returnType.GetGenericArguments()[0];
                    joinPoint.Return = WrapAsyncOfTMethod
                        .MakeGenericMethod(resultType)
                        .Invoke(this, [joinPoint, returnedTask, tx])!;
                }
                else
                {
                    joinPoint.Return = WrapAsync(joinPoint, returnedTask, tx);
                }

                return;
            }

            tx.CommitAsync().GetAwaiter().GetResult();
        }
        catch
        {
            tx.RollbackAsync().GetAwaiter().GetResult();
            throw;
        }
        finally
        {
            if (joinPoint.Return is not Task)
            {
                tx.DisposeAsync().GetAwaiter().GetResult();
            }
        }
    }

    private void Proceed(IJoinPoint joinPoint)
    {
        if (GetNext() is null)
            joinPoint.Proceed();
        else
            GetNext()!.Apply(joinPoint);
    }

    private async Task WrapAsync(IJoinPoint joinPoint, Task task, ITransactionScope tx)
    {
        try
        {
            await task.ConfigureAwait(false);
            await tx.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync().ConfigureAwait(false);
            throw;
        }
        finally
        {
            await tx.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task<TResult> WrapAsyncOfT<TResult>(IJoinPoint joinPoint, Task task, ITransactionScope tx)
    {
        try
        {
            var result = await ((Task<TResult>)task).ConfigureAwait(false);
            await tx.CommitAsync().ConfigureAwait(false);
            return result;
        }
        catch
        {
            await tx.RollbackAsync().ConfigureAwait(false);
            throw;
        }
        finally
        {
            await tx.DisposeAsync().ConfigureAwait(false);
        }
    }
}
