import { z } from 'zod';
import { SchemaVersion } from '@domain/constants/schema-version';

// Effect and Source Enums
export enum AccessEffect {
  Allow = 'Allow',
  Deny = 'Deny',
  NotGranted = 'NotGranted',
}

export enum PermissionSource {
  Template = 'Template',
  ProfileOverride = 'ProfileOverride',
}

// ── Authentication ──

export const GraphAuthenticationSchema = z.object({
  sessionId: z.string().uuid(),
  method: z.string(),
  issuedAt: z.string().datetime(),
  expiresAt: z.string().datetime(),
});

export type GraphAuthentication = z.infer<typeof GraphAuthenticationSchema>;

// ── Context ──

export const GraphUserSchema = z.object({
  email: z.string().email(),
  username: z.string(),
  value: z.string(),
  status: z.string(),
});

export const GraphTenantSchema = z.object({
  code: z.string(),
  value: z.string(),
  status: z.string(),
  isManagementOwner: z.boolean(),
});

export const GraphSystemSuiteSchema = z.object({
  code: z.string(),
  value: z.string(),
  status: z.string(),
});

export const GraphRoleSchema = z.object({
  code: z.string(),
  value: z.string(),
  hierarchyLevel: z.number().int(),
});

export const GraphProfileSchema = z.object({
  scope: z.string(),
  isActive: z.boolean(),
});

export const GraphBranchSchema = z.object({
  code: z.string(),
  value: z.string(),
});

export const GraphContextSchema = z.object({
  user: GraphUserSchema,
  tenant: GraphTenantSchema,
  systemSuite: GraphSystemSuiteSchema,
  role: GraphRoleSchema,
  profile: GraphProfileSchema,
  branch: GraphBranchSchema.nullable(),
});

export type GraphContext = z.infer<typeof GraphContextSchema>;

// ── Actions ──

export const GraphActionSchema = z.object({
  code: z.string(),
  value: z.string(),
  description: z.string().nullable(),
});

export type GraphAction = z.infer<typeof GraphActionSchema>;

// ── Menu Access ──

export const GraphMenuOptionSchema = z.object({
  code: z.string(),
  value: z.string(),
  actionCode: z.string(),
  effect: z.nativeEnum(AccessEffect),
  source: z.nativeEnum(PermissionSource),
});

export const GraphSubMenuSchema = z.object({
  code: z.string(),
  value: z.string(),
  sortOrder: z.number().int(),
  options: z.array(GraphMenuOptionSchema),
});

export const GraphMenuSchema = z.object({
  code: z.string(),
  value: z.string(),
  sortOrder: z.number().int(),
  subMenus: z.array(GraphSubMenuSchema),
});

export const GraphMenuModuleSchema = z.object({
  code: z.string(),
  value: z.string(),
  sortOrder: z.number().int(),
  status: z.string(),
  menus: z.array(GraphMenuSchema),
});

export type GraphMenuModule = z.infer<typeof GraphMenuModuleSchema>;
export type GraphMenuOption = z.infer<typeof GraphMenuOptionSchema>;

// ── Domain Permissions ──

export const GraphDomainActionSchema = z.object({
  actionCode: z.string(),
  value: z.string(),
  effect: z.nativeEnum(AccessEffect),
  source: z.nativeEnum(PermissionSource),
});

export const GraphDomainPermissionSchema = z.object({
  resourceType: z.string(),
  resourceCode: z.string(),
  value: z.string(),
  actions: z.array(GraphDomainActionSchema),
});

export type GraphDomainPermission = z.infer<typeof GraphDomainPermissionSchema>;

// ── Feature Flags ──

export const GraphFeatureFlagSchema = z.object({
  flagCode: z.string(),
  isEnabled: z.boolean(),
  matchedCriteriaType: z.string().nullable(),
});

export type GraphFeatureFlag = z.infer<typeof GraphFeatureFlagSchema>;

// ── Effective Config ──

export const GraphEffectiveConfigSchema = z.object({
  sessionTimeoutMinutes: z.number().int(),
  maxLoginAttempts: z.number().int(),
  minPasswordLength: z.number().int(),
  mfaRequiredForAdmin: z.boolean(),
  mfaAllowedMethods: z.array(z.string()),
  accessTokenDurationMs: z.number().int(),
  authUseExternalIdp: z.boolean(),
});

export type GraphEffectiveConfig = z.infer<typeof GraphEffectiveConfigSchema>;

// ── Root Graph ──

export const AuthorizationGraphSchema = z.object({
  schemaVersion: z.string(),
  context: GraphContextSchema,
  authentication: GraphAuthenticationSchema,
  actions: z.array(GraphActionSchema),
  menuAccess: z.array(GraphMenuModuleSchema),
  domainPermissions: z.array(GraphDomainPermissionSchema),
  featureFlags: z.array(GraphFeatureFlagSchema),
  effectiveConfig: GraphEffectiveConfigSchema,
  scopes: z.array(z.string()),
  generatedAt: z.string().datetime(),
  validUntil: z.string().datetime(),
});

export type AuthorizationGraph = z.infer<typeof AuthorizationGraphSchema>;
