namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed class AddImpactAnalysisCommandValidator : AbstractValidator<AddImpactAnalysisCommand>
{
    private static readonly string[] ValidRiskLevels = ["Low", "Medium", "High", "Critical"];

    public AddImpactAnalysisCommandValidator()
    {
        RuleFor(x => x.PromotionRequestId).NotEmpty();
        RuleFor(x => x.RiskScore).InclusiveBetween(0, 100);
        RuleFor(x => x.RiskLevel)
            .NotEmpty()
            .Must(v => ValidRiskLevels.Contains(v, StringComparer.OrdinalIgnoreCase))
            .WithMessage("RiskLevel must be one of: Low, Medium, High, Critical.");
        RuleFor(x => x.NewPermissionsCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RemovedPermissionsCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AffectedSystemsCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ConflictingPermissions).MaximumLength(2000).When(x => x.ConflictingPermissions is not null);
        RuleFor(x => x.RiskFactors).MaximumLength(2000).When(x => x.RiskFactors is not null);
        RuleFor(x => x.SuggestedMitigations).MaximumLength(2000).When(x => x.SuggestedMitigations is not null);
    }
}
