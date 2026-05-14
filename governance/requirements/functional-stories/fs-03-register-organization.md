# Functional Story 3: Register Organization and Configure IdP Strategy

## 1. Business Purpose

UMS must allow security administrators to onboard a new organization and define how its users will authenticate. This gives each tenant a clear identity strategy while keeping organization registration governed and auditable.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Global Security Administrator** | Registers organizations and chooses their identity strategy. |
| **Organization Administrator** | May later manage branches, users, and local settings. | ## 3. Business Preconditions

- The actor is authenticated as a global administrator.
- The organization is approved for onboarding.
- Required company reference data is available.

## 4. Main Functional Flow

1. The administrator opens the organization management area.
2. The administrator enters the organization's legal name, external reference code, and organization type.
3. The administrator selects the identity strategy that the organization will use.
4. If the selected strategy requires additional information, the administrator completes the required configuration.
5. The system validates that the organization can be registered.
6. The system activates the organization and records the onboarding decision.
7. The administrator can continue with branch and user setup.

## 5. Alternative Flows and Exceptions

### A. Identity Provider Configuration Cannot Be Verified

If the selected identity provider cannot be validated, the organization is not activated until the configuration is corrected.

### B. Duplicate External Reference

If the company reference already exists, the system prevents creating a duplicate organization.

## 6. Business Rules

1. Each organization must have a unique external reference when applicable.
2. Authentication strategy must be explicitly selected.
3. Organization creation must be auditable.
4. Branch registration depends on an active organization.

## 7. Acceptance Criteria

1. A global administrator can register a valid organization.
2. Duplicate company references are rejected.
3. Invalid identity provider configuration prevents activation.
4. A registered organization can be used for branch and user onboarding.

## 8. Technical Requirements

- Persist organization data in `TENANT` / `ORGANIZATION` model.
- Persist identity provider settings in `IDP_CONFIGURATION`.
- Enforce uniqueness for external company references.
- Emit `OrganizationCreatedEvent`.
- Validate IdP configuration according to the selected provider type.

## 9. Traceability

- Entities: `TENANT`, `BRANCH`, `IDP_CONFIGURATION`, `USER_ACCOUNT`
- ADRs: ADR-0031, ADR-0032, ADR-0034, ADR-0010
- Technical Enabler: TE-03
