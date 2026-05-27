using System.Text.Json;
using Ums.Domain.Configuration.FeatureFlag;

namespace Ums.Infrastructure.Configuration;

public sealed class FeatureFlagEvaluator : IFeatureFlagEvaluator
{
    public FlagEvaluationResult Evaluate(FeatureFlag flag, EvaluationContext context)
    {
        if (!flag.Criteria.Any())
            return new FlagEvaluationResult(true, null, "No restrictions — active for all");

        var groups = flag.Criteria.GroupBy(c => c.CriteriaType);

        foreach (var group in groups)
        {
            var contextValue = GetContextValue(context, group.Key);

            if (contextValue is null)
                return new FlagEvaluationResult(false, group.Key, $"Context missing value for {group.Key}");

            var groupPassed = false;
            foreach (var criterion in group)
            {
                if (ApplyOperator(criterion.Operator, contextValue, criterion.Value))
                {
                    groupPassed = true;
                    break;
                }
            }

            if (!groupPassed)
                return new FlagEvaluationResult(false, group.Key, $"No match for CriteriaType {group.Key}");
        }

        return new FlagEvaluationResult(true, null, "All criteria matched");
    }

    private static string? GetContextValue(EvaluationContext context, string criteriaType) =>
        criteriaType switch
        {
            "TenantId"       => context.TenantId?.ToString(),
            "BranchId"       => context.BranchId?.ToString(),
            "UserProfileId"  => context.ProfileId?.ToString(),
            "RoleCode"       => context.RoleCode,
            "Environment"    => context.Environment,
            _                => context.CustomAttributes?.GetValueOrDefault(criteriaType),
        };

    private static bool ApplyOperator(string @operator, string contextValue, string criteriaValue) =>
        @operator switch
        {
            "Equals"           => string.Equals(contextValue, criteriaValue, StringComparison.OrdinalIgnoreCase),
            "NotEquals"        => !string.Equals(contextValue, criteriaValue, StringComparison.OrdinalIgnoreCase),
            "In"               => TryDeserializeArray(criteriaValue)?.Any(v => string.Equals(v, contextValue, StringComparison.OrdinalIgnoreCase)) ?? false,
            "NotIn"            => !(TryDeserializeArray(criteriaValue)?.Any(v => string.Equals(v, contextValue, StringComparison.OrdinalIgnoreCase)) ?? false),
            "LessThanOrEqual"  => int.TryParse(criteriaValue, out var threshold) && int.TryParse(contextValue, out var val) && val <= threshold,
            "Between"          => EvaluateDateRange(contextValue, criteriaValue),
            _                  => false,
        };

    private static string[]? TryDeserializeArray(string value)
    {
        try { return JsonSerializer.Deserialize<string[]>(value); }
        catch { return null; }
    }

    private static bool EvaluateDateRange(string contextValue, string criteriaValue)
    {
        try
        {
            var range = JsonSerializer.Deserialize<DateRangeValue>(criteriaValue);
            if (range is null) return false;
            var current = DateTime.Parse(contextValue, null, System.Globalization.DateTimeStyles.RoundtripKind);
            return current >= range.From && current <= range.To;
        }
        catch { return false; }
    }

    private sealed record DateRangeValue(DateTime From, DateTime To);
}
