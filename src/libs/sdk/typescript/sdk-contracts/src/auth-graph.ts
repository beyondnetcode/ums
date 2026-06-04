import type { AccessEffect, PermissionSource } from './access-effect.js';

/**
 * Self-contained, immutable snapshot of a user's authorization universe at authentication time.
 * Mirrors `src/libs/sdk/contracts/auth-graph.schema.json` (v1.0.0).
 * See ADR-0071 for the model and ADR-0074 for the versioning policy.
 */
export interface AuthorizationGraph {
  readonly schemaVersion: string;
  readonly context: PrincipalContext;
  readonly authentication: AuthenticationMetadata;
  readonly actions: ReadonlyArray<ActionRef>;
  readonly menuAccess: ReadonlyArray<MenuModule>;
  readonly domainPermissions: ReadonlyArray<DomainResourcePermissions>;
  readonly featureFlags: ReadonlyArray<FeatureFlagState>;
  readonly effectiveConfig: EffectiveConfig;
  readonly scopes: ReadonlyArray<string>;
  readonly generatedAt: string;
  readonly validUntil: string;
}

export interface PrincipalContext {
  readonly user: UserSummary;
  readonly tenant: TenantSummary;
  readonly systemSuite: SystemSuiteSummary;
  readonly role: RoleSummary;
  readonly profile: ProfileSummary;
  readonly branch: BranchSummary | null;
}

export interface UserSummary {
  readonly id: string;
  readonly email: string;
  readonly username: string;
  readonly displayName: string;
  readonly status: 'ACTIVE' | 'PENDING' | 'BLOCKED' | string;
}

export interface TenantSummary {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly status: 'ACTIVE' | 'SUSPENDED' | 'ARCHIVED' | string;
}

export interface SystemSuiteSummary {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly status: 'DRAFT' | 'PUBLISHED' | 'RETIRED' | string;
}

export interface RoleSummary {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly hierarchyLevel: number;
  readonly parentRoleId: string | null;
}

export interface ProfileSummary {
  readonly id: string;
  readonly scope: 'OrgWide' | 'BranchScoped';
  readonly isActive: boolean;
}

export interface BranchSummary {
  readonly id: string;
  readonly code: string;
  readonly name: string;
}

export interface AuthenticationMetadata {
  readonly method: 'Local' | 'IDP' | string;
  readonly provider: IdpProviderRef | null;
  readonly mfaRequired: boolean;
  readonly issuedAt: string;
  readonly sessionExpiresAt: string;
}

export interface IdpProviderRef {
  readonly name: string;
  readonly code: string;
  readonly strategy: string;
}

export interface ActionRef {
  readonly id: string;
  readonly code: string;
  readonly name: string;
}

export interface MenuModule {
  readonly module: ModuleSummary;
  readonly menus: ReadonlyArray<Menu>;
}

export interface ModuleSummary {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly sortOrder: number;
  readonly status: 'DRAFT' | 'PUBLISHED' | 'RETIRED' | string;
}

export interface Menu {
  readonly id: string;
  readonly code: string;
  readonly label: string;
  readonly sortOrder: number;
  readonly subMenus: ReadonlyArray<SubMenu>;
}

export interface SubMenu {
  readonly id: string;
  readonly code: string;
  readonly label: string;
  readonly sortOrder: number;
  readonly options: ReadonlyArray<MenuOption>;
}

export interface MenuOption {
  readonly id: string;
  readonly code: string;
  readonly label: string;
  readonly actionCode: string;
  readonly effect: AccessEffect;
  readonly source: PermissionSource;
}

export interface DomainResourcePermissions {
  readonly resource: DomainResource;
  readonly actions: ReadonlyArray<DomainActionResolution>;
}

export interface DomainResource {
  readonly id: string;
  readonly type: 'Aggregate' | 'Entity' | 'DomainMethod' | string;
  readonly code: string;
  readonly name: string;
  readonly moduleId: string | null;
  readonly parentResourceId: string | null;
}

export interface DomainActionResolution {
  readonly actionId: string;
  readonly actionCode: string;
  readonly actionName: string;
  readonly effect: AccessEffect;
  readonly source: PermissionSource;
}

export interface FeatureFlagState {
  readonly flagCode: string;
  readonly systemSuiteId: string;
  readonly isEnabled: boolean;
  readonly matchedCriteriaType:
    | 'TenantId'
    | 'BranchId'
    | 'UserProfileId'
    | 'RoleCode'
    | 'Environment'
    | 'DateRange'
    | 'PercentageHash'
    | 'CustomRule'
    | null;
}

export interface EffectiveConfig {
  readonly sessionTimeoutMinutes: number;
  readonly maxLoginAttempts: number;
  readonly minPasswordLength: number;
  readonly mfaRequiredForAdmin: boolean;
  readonly accessTokenDurationMs: number;
  readonly authUseExternalIdp: boolean;
}
