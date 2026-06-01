import type {
  AccessEffect,
  ActionRef,
  AuthorizationGraph,
  DomainResourcePermissions,
  IdpProviderRef,
  MenuModule,
  PermissionSource
} from '@ums/sdk-contracts';
import { SchemaVersion } from '@ums/sdk-contracts';

type FlagCriteria =
  | 'TenantId'
  | 'BranchId'
  | 'UserProfileId'
  | 'RoleCode'
  | 'Environment'
  | 'DateRange'
  | 'PercentageHash'
  | 'CustomRule';

interface MenuOptionSpec {
  optionCode: string;
  actionCode: string;
  effect: AccessEffect;
  source: PermissionSource;
}

interface DomainPermSpec {
  resource: string;
  action: string;
  effect: AccessEffect;
  source: PermissionSource;
}

interface FlagSpec {
  flagCode: string;
  enabled: boolean;
  matchedCriteriaType: FlagCriteria | null;
}

/**
 * Fluent builder for fabricating valid {@link AuthorizationGraph} instances in tests.
 * Same surface as the .NET `AuthGraphBuilder` — produces graphs without JSON parsing,
 * HTTP, or UMS connectivity.
 */
export class AuthGraphBuilder {
  private schemaVersion: string = SchemaVersion.Current;
  private tenantCode: string = 'TEST_TENANT';
  private tenantName: string = 'Test Tenant';
  private userEmail: string = 'test.user@example.com';
  private systemSuiteCode: string = 'TEST_SUITE';
  private roleCode: string = 'TEST_ROLE';
  private profileScope: 'OrgWide' | 'BranchScoped' = 'OrgWide';
  private branchCode: string | null = null;
  private authMethod: 'Local' | 'IDP' = 'Local';
  private provider: IdpProviderRef | null = null;
  private validityMs: number = 60 * 60 * 1000;
  private readonly scopes: string[] = [];
  private readonly menuOptions: MenuOptionSpec[] = [];
  private readonly domainPerms: DomainPermSpec[] = [];
  private readonly flags: FlagSpec[] = [];

  static forTenant(code: string): AuthGraphBuilder {
    const b = new AuthGraphBuilder();
    b.tenantCode = code;
    return b;
  }

  withTenantName(name: string): this {
    this.tenantName = name;
    return this;
  }

  withUser(email: string): this {
    this.userEmail = email;
    return this;
  }

  withSystemSuite(code: string): this {
    this.systemSuiteCode = code;
    return this;
  }

  withRole(code: string): this {
    this.roleCode = code;
    return this;
  }

  withBranchScopedProfile(branchCode: string): this {
    this.profileScope = 'BranchScoped';
    this.branchCode = branchCode;
    return this;
  }

  withSchemaVersion(version: string): this {
    this.schemaVersion = version;
    return this;
  }

  withValidity(ms: number): this {
    this.validityMs = ms;
    return this;
  }

  withIdpAuth(providerName: string, providerCode: string, strategy: string): this {
    this.authMethod = 'IDP';
    this.provider = { name: providerName, code: providerCode, strategy };
    return this;
  }

  withScope(scope: string): this {
    this.scopes.push(scope);
    return this;
  }

  withDeny(scope: string): this {
    const idx = scope.indexOf('.');
    if (idx <= 0 || idx === scope.length - 1) {
      throw new Error(`Invalid scope format '${scope}'. Expected 'RESOURCE.ACTION'.`);
    }
    this.domainPerms.push({
      resource: scope.substring(0, idx),
      action: scope.substring(idx + 1),
      effect: 'Deny',
      source: 'Override'
    });
    return this;
  }

  withDomainPermission(
    resourceCode: string,
    actionCode: string,
    effect: AccessEffect = 'Allow',
    source: PermissionSource = 'Template'
  ): this {
    this.domainPerms.push({ resource: resourceCode, action: actionCode, effect, source });
    if (effect === 'Allow') this.scopes.push(`${resourceCode}.${actionCode}`);
    return this;
  }

  withMenuOption(
    optionCode: string,
    actionCode: string,
    effect: AccessEffect = 'Allow',
    source: PermissionSource = 'Template'
  ): this {
    this.menuOptions.push({ optionCode, actionCode, effect, source });
    if (effect === 'Allow') this.scopes.push(`${optionCode}.${actionCode}`);
    return this;
  }

  withFeatureFlag(flagCode: string, enabled: boolean = true, matchedCriteriaType?: FlagCriteria): this {
    this.flags.push({
      flagCode,
      enabled,
      matchedCriteriaType: enabled ? matchedCriteriaType ?? 'TenantId' : null
    });
    return this;
  }

  build(): AuthorizationGraph {
    const now = new Date();
    const validUntil = new Date(now.getTime() + this.validityMs);

    const actions: ActionRef[] = [];
    const menuAccess = this.buildMenuAccess(actions);
    const domainPermissions = this.buildDomainPermissions(actions);

    return {
      schemaVersion: this.schemaVersion,
      context: {
        user: {
          id: randomUuid(),
          email: this.userEmail,
          username: this.userEmail.split('@')[0] ?? this.userEmail,
          displayName: this.userEmail,
          status: 'ACTIVE'
        },
        tenant: { id: randomUuid(), code: this.tenantCode, name: this.tenantName, status: 'ACTIVE' },
        systemSuite: { id: randomUuid(), code: this.systemSuiteCode, name: this.systemSuiteCode, status: 'PUBLISHED' },
        role: { id: randomUuid(), code: this.roleCode, name: this.roleCode, hierarchyLevel: 1, parentRoleId: null },
        profile: { id: randomUuid(), scope: this.profileScope, isActive: true },
        branch:
          this.profileScope === 'BranchScoped' && this.branchCode !== null
            ? { id: randomUuid(), code: this.branchCode, name: this.branchCode }
            : null
      },
      authentication: {
        method: this.authMethod,
        provider: this.provider,
        mfaRequired: false,
        issuedAt: now.toISOString(),
        sessionExpiresAt: validUntil.toISOString()
      },
      actions,
      menuAccess,
      domainPermissions,
      featureFlags: this.flags.map((f) => ({
        flagCode: f.flagCode,
        systemSuiteId: randomUuid(),
        isEnabled: f.enabled,
        matchedCriteriaType: f.matchedCriteriaType
      })),
      effectiveConfig: {
        sessionTimeoutMinutes: 60,
        maxLoginAttempts: 5,
        minPasswordLength: 12,
        mfaRequiredForAdmin: true,
        accessTokenDurationMs: 3600000,
        authUseExternalIdp: this.authMethod === 'IDP'
      },
      scopes: Array.from(new Set(this.scopes)),
      generatedAt: now.toISOString(),
      validUntil: validUntil.toISOString()
    };
  }

  buildExpired(): AuthorizationGraph {
    const graph = this.build();
    const past = new Date(Date.now() - 5 * 365 * 24 * 60 * 60 * 1000);
    return {
      ...graph,
      generatedAt: past.toISOString(),
      validUntil: new Date(past.getTime() + 60 * 60 * 1000).toISOString()
    };
  }

  private buildMenuAccess(actions: ActionRef[]): MenuModule[] {
    if (this.menuOptions.length === 0) return [];
    const options = this.menuOptions.map((m) => {
      const existing = actions.find((a) => a.code === m.actionCode);
      const actionRef = existing ?? { id: randomUuid(), code: m.actionCode, name: m.actionCode };
      if (!existing) actions.push(actionRef);
      return {
        id: randomUuid(),
        code: m.optionCode,
        label: m.optionCode,
        actionCode: m.actionCode,
        effect: m.effect,
        source: m.source
      };
    });
    return [
      {
        module: {
          id: randomUuid(),
          code: 'TEST_MODULE',
          name: 'Test Module',
          sortOrder: 1,
          status: 'PUBLISHED'
        },
        menus: [
          {
            id: randomUuid(),
            code: 'TEST_MENU',
            label: 'Test Menu',
            sortOrder: 1,
            subMenus: [
              {
                id: randomUuid(),
                code: 'TEST_SUB',
                label: 'Test SubMenu',
                sortOrder: 1,
                options
              }
            ]
          }
        ]
      }
    ];
  }

  private buildDomainPermissions(actions: ActionRef[]): DomainResourcePermissions[] {
    if (this.domainPerms.length === 0) return [];
    const byResource = new Map<string, DomainPermSpec[]>();
    for (const dp of this.domainPerms) {
      const list = byResource.get(dp.resource) ?? [];
      list.push(dp);
      byResource.set(dp.resource, list);
    }
    const result: DomainResourcePermissions[] = [];
    for (const [resourceCode, perms] of byResource) {
      const resolutions = perms.map((p) => {
        const existing = actions.find((a) => a.code === p.action);
        const actionRef = existing ?? { id: randomUuid(), code: p.action, name: p.action };
        if (!existing) actions.push(actionRef);
        return {
          actionId: actionRef.id,
          actionCode: p.action,
          actionName: p.action,
          effect: p.effect,
          source: p.source
        };
      });
      result.push({
        resource: {
          id: randomUuid(),
          type: 'Aggregate',
          code: resourceCode,
          name: resourceCode,
          moduleId: null
        },
        actions: resolutions
      });
    }
    return result;
  }
}

function randomUuid(): string {
  // RFC 4122 v4-ish; cryptographically random when crypto.randomUUID is available.
  const c = globalThis.crypto as { randomUUID?: () => string } | undefined;
  if (c?.randomUUID) return c.randomUUID();
  // Fallback for older environments.
  return 'xxxxxxxx-xxxx-4xxx-8xxx-xxxxxxxxxxxx'.replace(/[xy]/g, (ch) => {
    const r = Math.floor(Math.random() * 16);
    const v = ch === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}
