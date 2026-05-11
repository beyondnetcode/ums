# ADR 0017: Feature Flagging Strategy for Progressive Delivery

## Status
Approved

## Date
2026-05-09

## Context
Deploying new, complex, or risky features often requires halting the system or risking production stability. We need a mechanism to deploy code to production in a dormant state and enable it for specific users, tenants, or globally at runtime without recompiling or redeploying the application.

## Decision
We will treat Feature Flag management as a **first-class system feature**, handled strictly via **Infrastructure Layer logic** injected into the Core, completely decoupled from our own database infrastructure.

1. **Strict Infrastructure Adapters**: We will NEVER rely on custom database tables (like a Postgres module) to control feature flags. Instead, we will use dedicated, enterprise-grade Feature Management platforms (e.g., Unleash, LaunchDarkly, ConfigCat).
2. **Dependency Inversion**: The Domain and Application layers will solely depend on a logical interface (`IFeatureTogglePort`). The specific SDK (e.g., `unleash-client`) will be confined to an Infrastructure Adapter that evaluates the flags in memory based on the provider's API.
3. **Progressive Rollouts**: Code branches for new features within Use Cases will evaluate the injected port. This enables A/B testing, Canary releases, and instant "kill switches" without touching the main database or triggering database migrations.

## Consequences
* **Pros**: Zero database overhead. Full decoupling of deployment from release. Enables trunk-based development and delegates the complex flag evaluation rules (user segments, percentages) to specialized infrastructure providers.
* **Cons**: Introduces "Toggle Debt" (old feature flags left in the code base must be manually cleaned up after successful rollouts). Requires managing external provider connections.
