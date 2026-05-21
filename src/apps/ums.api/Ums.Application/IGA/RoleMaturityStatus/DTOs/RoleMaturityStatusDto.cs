namespace Ums.Application.IGA.RoleMaturityStatus.DTOs;

public sealed record RoleMaturityStatusDto(
    Guid RoleMaturityStatusId,
    Guid TenantId,
    Guid UserId,
    Guid RoleId,
    string CurrentMaturityLevel,
    string? NextEligibleMaturityLevel,
    DateTime AssignedAt,
    DateTime CurrentLevelSince,
    DateTime? EligibleForPromotionAt,
    int CompletedCertificationsCount,
    int CompletedTrainingsCount,
    decimal PerformanceScore,
    bool HasNoComplianceIssues,
    string? BlockingFactor,
    DateTime? LastReviewedAt);
