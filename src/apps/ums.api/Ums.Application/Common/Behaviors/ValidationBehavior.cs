namespace Ums.Application.Common.Behaviors;

using FluentValidation;
using MediatR;
using Ums.Domain.Kernel;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        // FIX-10: Use only ErrorMessage (not PropertyName) to prevent internal dotted-path
        // fragments like "Command.TenantId.Value" from matching DomainErrorStatusMapper
        // substring rules ("not found", "unauthorized", etc.) and returning the wrong HTTP
        // status code. The "Validation.Failed:" sentinel lets DomainErrorStatusMapper
        // short-circuit to 422 without inspecting individual messages.
        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .Select(error => error.ErrorMessage)
            .Distinct()
            .ToArray();

        if (errors.Length == 0)
        {
            return await next();
        }

        return CreateValidationResult<TResponse>($"Validation.Failed: {string.Join("; ", errors)}");
    }

    private static TResult CreateValidationResult<TResult>(string error)
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)(object)Result.Failure(error);
        }

        if (typeof(TResult).IsGenericType &&
            typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResult).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod(nameof(Result.Failure), new[] { typeof(string) });

            return (TResult)failureMethod!.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException($"ValidationBehavior cannot create a failure result for '{typeof(TResult).Name}'.");
    }
}
