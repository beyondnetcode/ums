/**
 * Compile-time constants describing the AuthorizationGraph schema this SDK was built against.
 * See ADR-0074 for the versioning policy.
 */
export const SchemaVersion = {
  Current: '1.0.0',
  CompatibilityMinInclusive: '1.0.0',
  CompatibilityMaxExclusive: '2.0.0'
} as const;

interface Semver {
  readonly major: number;
  readonly minor: number;
  readonly patch: number;
}

function tryParse(input: string | null | undefined): Semver | null {
  if (input === null || input === undefined) return null;
  const trimmed = input.trim();
  if (trimmed.length === 0) return null;
  const parts = trimmed.split('.');
  if (parts.length !== 3) return null;
  const major = Number.parseInt(parts[0]!, 10);
  const minor = Number.parseInt(parts[1]!, 10);
  const patch = Number.parseInt(parts[2]!, 10);
  if (!Number.isFinite(major) || !Number.isFinite(minor) || !Number.isFinite(patch)) return null;
  if (String(major) !== parts[0] || String(minor) !== parts[1] || String(patch) !== parts[2]) return null;
  return { major, minor, patch };
}

function compare(a: Semver, b: Semver): number {
  if (a.major !== b.major) return a.major - b.major;
  if (a.minor !== b.minor) return a.minor - b.minor;
  return a.patch - b.patch;
}

/** Returns true when `version` falls within the SDK's supported range. */
export function isSchemaVersionSupported(version: string | null | undefined): boolean {
  const v = tryParse(version);
  if (v === null) return false;
  const min = tryParse(SchemaVersion.CompatibilityMinInclusive)!;
  const max = tryParse(SchemaVersion.CompatibilityMaxExclusive)!;
  return compare(v, min) >= 0 && compare(v, max) < 0;
}

/** True when the MAJOR of `version` matches the SDK's current MAJOR. */
export function isMajorMatch(version: string | null | undefined): boolean {
  const v = tryParse(version);
  if (v === null) return false;
  const current = tryParse(SchemaVersion.Current)!;
  return v.major === current.major;
}

/** True when `version` is a MINOR ahead of the current schema (server newer than SDK). */
export function isMinorAhead(version: string | null | undefined): boolean {
  const v = tryParse(version);
  if (v === null) return false;
  const current = tryParse(SchemaVersion.Current)!;
  if (v.major !== current.major) return false;
  return v.minor > current.minor;
}

/** True when `version` is a MINOR behind the current schema (SDK newer than server). */
export function isMinorBehind(version: string | null | undefined): boolean {
  const v = tryParse(version);
  if (v === null) return false;
  const current = tryParse(SchemaVersion.Current)!;
  if (v.major !== current.major) return false;
  return v.minor < current.minor;
}
