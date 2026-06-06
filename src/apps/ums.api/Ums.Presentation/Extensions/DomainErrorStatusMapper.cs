namespace Ums.Presentation.Extensions;

using Microsoft.AspNetCore.Http;
using Ums.Domain.Kernel;

internal static class DomainErrorStatusMapper
{
    public static (int Status, string Title) Map(string error)
    {
        if (string.IsNullOrEmpty(error))
        {
            return (StatusCodes.Status400BadRequest, "Bad Request");
        }

        // FIX-10: ValidationBehavior prefixes all FluentValidation failures with this
        // sentinel so they always resolve to 422 — before any substring fallback rules
        // that could misclassify them based on property-path content.
        if (error.StartsWith("Validation.Failed:", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity");
        }

        if (ContainsAny(error, DomainErrors.Common.NotFound, DomainErrors.Tenant.NotFound, DomainErrors.Tenant.BranchNotFound, DomainErrors.Tenant.IdpNotFound, DomainErrors.Tenant.BrandingNotFound, DomainErrors.SystemSuite.ConfigurationKeyNotFound, DomainErrors.Authorization.PermissionNotFound))
        {
            return (StatusCodes.Status404NotFound, "Not Found");
        }

        if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status404NotFound, "Not Found");
        }

        if (ContainsAny(error, DomainErrors.Common.Duplicate, DomainErrors.Tenant.SignupRequestAlreadyExists, DomainErrors.Tenant.BranchCodeNotUnique, DomainErrors.Tenant.IdpCodeNotUnique, DomainErrors.UserAccount.EmailNotUnique, DomainErrors.SystemSuite.OptionCodeNotUnique, DomainErrors.SystemSuite.SubMenuCodeNotUnique, DomainErrors.SystemSuite.MenuCodeNotUnique, DomainErrors.SystemSuite.ModuleCodeNotUnique, DomainErrors.SystemSuite.ConfigurationKeyAlreadyExists, DomainErrors.Authorization.TemplateItemTargetAlreadyExists, DomainErrors.Authorization.PermissionAlreadyExists, DomainErrors.Compliance.DocumentAlreadyExpired))
        {
            return (StatusCodes.Status409Conflict, "Conflict");
        }

        if (error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status409Conflict, "Conflict");
        }

        if (ContainsAny(error, DomainErrors.Common.Required, DomainErrors.Tenant.Required, DomainErrors.ValueObject.PropertyRequired, DomainErrors.ValueObject.EmailRequired, DomainErrors.UserAccount.PasswordHashRequired, DomainErrors.Audit.WhatChangedRequired, DomainErrors.Audit.AffectedEntityRequired))
        {
            return (StatusCodes.Status400BadRequest, "Validation Error");
        }

        if (ContainsAny(error, DomainErrors.Common.Invalid, DomainErrors.UserAccount.InvalidEmail, DomainErrors.Tenant.SignupRequestNotPending, DomainErrors.Tenant.SignupRequestAlreadyProcessed, DomainErrors.Branding.InvalidHexColor, DomainErrors.Branding.InvalidCustomDomain, DomainErrors.Branding.InvalidCnameTarget, DomainErrors.Branding.InvalidLogoFormat, DomainErrors.Configuration.IdpConfigPayloadInvalid, DomainErrors.Configuration.FlagPercentageOutOfRange, DomainErrors.Configuration.AppConfigNotDraft, DomainErrors.Configuration.AppConfigNotPublished, DomainErrors.Configuration.FlagArchivedCannotChange, DomainErrors.Configuration.AppConfigAlreadyArchived, DomainErrors.Compliance.ExpirationBeforeIssueDate, DomainErrors.Compliance.DocumentCannotTransition, DomainErrors.Compliance.DocumentNotPendingReview, DomainErrors.ValueObject.DateRangeInvalid))
        {
            return (StatusCodes.Status400BadRequest, "Validation Error");
        }

        if (error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) || error.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status403Forbidden, "Forbidden");
        }

        if (error.Contains("authenticated user is required", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status401Unauthorized, "Unauthorized");
        }

        return (StatusCodes.Status400BadRequest, "Bad Request");
    }

    private static bool ContainsAny(string error, params string[] values)
        => values.Any(value => error.Contains(value, StringComparison.OrdinalIgnoreCase));
}
