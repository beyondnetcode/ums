namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record AddImpactAnalysisCommand(
    Guid PromotionRequestId,
    decimal RiskScore,
    string RiskLevel,
    int NewPermissionsCount,
    int RemovedPermissionsCount,
    int AffectedSystemsCount,
    string? ConflictingPermissions,
    string? RiskFactors,
    string? SuggestedMitigations) : ICommand;
