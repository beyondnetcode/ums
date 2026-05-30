# Technical TODO

> Pending technical tasks and investigations that are not yet committed to a sprint but should be tracked.

---

## [TODO-001] Investigate and Fix GraphQL AppConfigurations Query

- **Priority**: Medium
- **Type**: Bug Investigation
- **Created**: 2026-05-30
- **Bounded Context**: Configuration
- **Status**: Open

### Problem
GraphQL endpoint `/graphql` for `appConfigurations` query returns empty results (`items: []`, `totalItems: 0`) while REST endpoint `/api/v1/app-configurations` returns correct data (7 items).

### Observations
- Handler executes successfully in ~25ms (no errors)
- REST API works correctly with identical query parameters
- Other GraphQL queries (e.g., `tenants`) work correctly
- AppConfigurationDto type exists in GraphQL schema
- Issue is specific to AppConfiguration bounded context

### Affected Files
- `src/apps/ums.api/Ums.Presentation/GraphQL/Configuration/AppConfigurationQueries.cs`
- `src/apps/ums.api/Ums.Application/Configuration/AppConfiguration/Queries/GetAllAppConfigurationsQueryHandler.cs`
- `src/apps/ums.api/Ums.Presentation/Middleware/TenantContextMiddleware.cs`
- `src/apps/ums.api/Ums.Presentation/Middleware/DevAuthMiddleware.cs`

### Workaround
Use `FRONTEND_CONFIG_TRANSPORT = "rest"` in AppConfigurations table (current default).

### Next Steps
1. Add debug logging to HotChocolate resolver execution
2. Compare GraphQL vs REST request pipeline differences
3. Investigate DataLoader caching issues
4. Check if there's a resolver scope mismatch

### Related Technical Debt
- [TD-002](./technical-debt.md#td-002-graphql-queries-return-empty-data-for-appconfigurations)

---

## [TODO-002] Remove GraphQL Code Path from AppConfiguration Service

- **Priority**: Low
- **Type**: Code Cleanup
- **Created**: 2026-05-30
- **Bounded Context**: Configuration (Frontend)
- **Status**: Open

### Problem
The frontend `app-configuration.service.ts` contains dual-transport logic (GraphQL + REST) but the system is configured to use REST 100% via `FRONTEND_CONFIG_TRANSPORT = "rest"`. The GraphQL code path is dead code.

### Affected Files
- `src/apps/ums.web-app/src/infrastructure/configuration/services/app-configuration.service.ts`
- `src/apps/ums.web-app/src/infrastructure/configuration/queries/app-configuration.graphql.ts`

### Next Steps
1. Simplify service to use REST only
2. Remove GraphQL query imports and functions
3. Remove transport mode detection and caching logic
4. Clean up related GraphQL query files if not used elsewhere

---

## [TODO-003] Implement Parameterization System with Loader and Provider

- **Priority**: High
- **Type**: Architecture / Feature Implementation
- **Created**: 2026-05-30
- **Bounded Context**: Configuration
- **Status**: In Progress

### Problem
Currently, configurable behaviors in UMS may be hardcoded in web, API, services, or components. A centralized parameterization system is required to store all configurations in the database, load them into memory at startup, and provide a single access point for all system logic.

### Specification
See: [Parameterization System Specification](../construction/ddd-design/parameterization-system-spec.md)

### Affected Areas
- Backend: New ConfigurationLoader and ConfigurationProvider services
- Frontend: Tenant configuration tab in Tenant dashboard
- Database: ScopeId support for Global (1) and Tenant (2) parameters
- API: New endpoints for global and tenant-specific configurations

### Implemented Components (Phase 1)

#### Backend ✅
1. **ConfigurationBootstrapper** (`Ums.Presentation/Bootstrapping/Bootstrappers/ConfigurationBootstrapper.cs`)
   - Bootstrapper pattern for synchronous initialization at startup
   - Loads parameters BEFORE other services resolve IConfigurationProvider
   - Uses CompositeBootstrapper to ensure proper ordering

2. **ConfigurationProvider** (`Ums.Infrastructure/Configuration/ConfigurationProvider.cs`)
   - Single access point for all configuration values
   - Implements precedence logic (Tenant > Global when allowed)
   - In-memory ConcurrentDictionary cache for global and tenant parameters
   - Provides ReloadAsync and ReloadTenantAsync methods
   - ConfigurationChanged event for audit integration

3. **IConfigurationProvider** (`Ums.Application/Configuration/Services/IConfigurationProvider.cs`)
   - Interface defining GetGlobal, GetForTenant, GetWithPrecedence
   - GetValueAs<T> for typed parameter retrieval
   - Event-based change notification

4. **ConfigurationAuditService** (`Ums.Application/Configuration/Services/ConfigurationAuditService.cs`)
   - Audit trail integration for parameter changes

5. **ConfigurationLoaderExtensions** (`Ums.Infrastructure/Configuration/ConfigurationLoader.cs`)
   - Extension method for DI registration of IConfigurationProvider

6. **ConfigurationValues** (`Ums.Application/Configuration/Services/ConfigurationValues.cs`)
   - Typed wrapper for common configuration values
   - Provides strongly-typed access to: SessionTimeout, MaxLoginAttempts, MinPasswordLength, etc.
   - Supports tenant-specific overrides with precedence logic

7. **Tenant-Specific Parameters Seeded**
   - RANSA_PERU: SESSION_TIMEOUT=45min, MFA_REQUIRED=true, CUSTOM_BRANDING=true
   - APM_CALLAO: SESSION_TIMEOUT=20min, MAX_LOGIN_ATTEMPTS=3, MFA_REQUIRED=true
   - NEPTUNIA: SESSION_TIMEOUT=60min, MIN_PASSWORD_LENGTH=14

#### Frontend ✅
1. **TenantConfigurationsPanel** (`presentation/identity/tenant/components/TenantConfigurationsPanel.tsx`)
   - Tab in TenantDetailPanel for tenant-specific parameters
   - View/edit tenant parameters
   - Add new tenant parameters
   - Visual indication of override vs global parameters

2. **Updated TenantDetailPanel**
   - Integrated TenantConfigurationsPanel in the 'configurations' tab

3. **New Translations Added**
   - Configuration parameter UI strings (EN/ES)

### Remaining Work
1. Create global configuration admin screen (Admin-only)
2. Add parameter-level authorization (Internal Admin vs Tenant Admin)
3. Implement cache invalidation when parameters are updated via API
4. **Integrate ConfigurationValues into actual system logic** (handlers, middleware, validators)
5. Add Redis migration preparation (TD-003)

### Related Technical Debt
- [TD-003](./technical-debt.md#td-003-prepare-configuration-system-for-redis)

---

## [TODO-004] Create Global Configuration Admin Screen

- **Priority**: High
- **Type**: Feature Implementation
- **Created**: 2026-05-30
- **Bounded Context**: Configuration
- **Status**: Open

### Problem
Internal Admin needs a dedicated screen to view and manage system-wide (Global) parameters separate from tenant-specific parameters.

### Requirements
- Admin-only access (Internal Admin role required)
- List all global parameters (ScopeId = 1)
- View parameter details (code, value, description, status, version)
- Edit global parameter values
- Create new global parameters
- View audit history for parameter changes

### Affected Areas
- Frontend: New `/app-configurations/global` route
- Backend: Global configuration API endpoints
- UI: Standalone configuration management screen (not in tenant context)

### Next Steps
1. Create global configurations service (reuse existing AppConfigurationService)
2. Create GlobalConfigurationsScreen component
3. Add route protection for Internal Admin only
4. Add audit trail view for parameters