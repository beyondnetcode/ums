namespace Ums.Sdk.Contracts;

/// <summary>
/// Compile-time constants describing the AuthorizationGraph schema this SDK was built against.
/// See ADR-0074 for the versioning policy and SCHEMA_VERSIONING.md for the operational summary.
/// </summary>
public static class SchemaVersion
{
    /// <summary>Current canonical schema version emitted and consumed by SDK 1.0.x.</summary>
    public const string Current = "1.0.0";

    /// <summary>Inclusive lower bound of the schema compatibility range.</summary>
    public const string CompatibilityMinInclusive = "1.0.0";

    /// <summary>Exclusive upper bound of the schema compatibility range.</summary>
    public const string CompatibilityMaxExclusive = "2.0.0";

    /// <summary>
    /// Returns true when <paramref name="schemaVersion"/> falls within the SDK's supported range.
    /// </summary>
    public static bool IsSupported(string? schemaVersion)
    {
        if (string.IsNullOrWhiteSpace(schemaVersion)) return false;
        if (!TryParse(schemaVersion, out var v)) return false;
        if (!TryParse(CompatibilityMinInclusive, out var min)) return false;
        if (!TryParse(CompatibilityMaxExclusive, out var max)) return false;
        return Compare(v, min) >= 0 && Compare(v, max) < 0;
    }

    /// <summary>True when the major component of <paramref name="schemaVersion"/> equals the current major.</summary>
    public static bool IsMajorMatch(string? schemaVersion)
    {
        if (!TryParse(schemaVersion, out var v)) return false;
        if (!TryParse(Current, out var c)) return false;
        return v.Major == c.Major;
    }

    /// <summary>True when <paramref name="schemaVersion"/> is a MINOR ahead of the current schema (server newer).</summary>
    public static bool IsMinorAhead(string? schemaVersion)
    {
        if (!TryParse(schemaVersion, out var v)) return false;
        if (!TryParse(Current, out var c)) return false;
        if (v.Major != c.Major) return false;
        return v.Minor > c.Minor;
    }

    /// <summary>True when <paramref name="schemaVersion"/> is a MINOR behind the current schema (SDK newer).</summary>
    public static bool IsMinorBehind(string? schemaVersion)
    {
        if (!TryParse(schemaVersion, out var v)) return false;
        if (!TryParse(Current, out var c)) return false;
        if (v.Major != c.Major) return false;
        return v.Minor < c.Minor;
    }

    internal static bool TryParse(string? input, out (int Major, int Minor, int Patch) parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var parts = input.Split('.');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor) ||
            !int.TryParse(parts[2], out var patch)) return false;
        parsed = (major, minor, patch);
        return true;
    }

    internal static int Compare((int Major, int Minor, int Patch) a, (int Major, int Minor, int Patch) b)
    {
        if (a.Major != b.Major) return a.Major.CompareTo(b.Major);
        if (a.Minor != b.Minor) return a.Minor.CompareTo(b.Minor);
        return a.Patch.CompareTo(b.Patch);
    }
}
