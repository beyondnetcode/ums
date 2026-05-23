namespace Ums.Application.Configuration.FeatureFlag.Commands;

using FluentValidation;

public sealed class EvaluateFeatureFlagCommandValidator : AbstractValidator<EvaluateFeatureFlagCommand>
{
    public EvaluateFeatureFlagCommandValidator()
    {
        RuleFor(command => command.FeatureFlagId).NotEmpty();
        RuleFor(command => command.Context).NotEmpty().MaximumLength(1000);
    }
}
