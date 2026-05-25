# Functional Story 8: Authenticate via Customizable Hosted Login Page

## 1. Business Purpose

Client systems need a centralized login experience that can reflect each tenant or system brand while keeping authentication governed by UMS.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **End User** | Starts sign-in from a client system. |
| **Client System** | Redirects users to hosted login and receives the result. |
| **Tenant/System Administrator** | Configures branding and login behavior. | ## 3. Business Preconditions

- The client system is registered in UMS.
- Return locations are configured for the client system.
- Branding and login settings exist or fall back to defaults.

## 4. Main Functional Flow

1. The user starts login from a client system.
2. The user is sent to the hosted UMS login page.
3. UMS displays the appropriate branding and available sign-in options.
4. The user completes authentication through the configured method.
5. UMS returns the user to the client system.
6. The client system receives the authenticated user context and continues the user journey.

## 5. Alternative Flows and Exceptions

### A. Branding Not Configured

If no custom branding exists, UMS uses the approved default experience.

### B. Invalid Return Location

If the client system requests an unapproved return location, UMS blocks the redirect.

## 6. Business Rules

1. Hosted login must support tenant and system-level branding.
2. Only approved client return locations may be used.
3. The login page must not expose another tenant's branding or settings.
4. Authentication method selection follows configured tenant/system policy.

## 7. Acceptance Criteria

1. Users see the correct login experience for their tenant/system context.
2. Missing branding falls back to defaults.
3. Unauthorized return locations are rejected.
4. Successful authentication returns the user to the client system.

## 8. Technical Requirements

- Resolve branding and login behavior from system configuration.
- Support configured IdP and native fallback strategies, including the Magic Link fallback implementation.
- Validate redirect/callback locations.
- Issue approved session/token result after authentication.
- Audit successful and failed hosted-login attempts.

## 9. Traceability

- Entities: `SYSTEM_CONFIGURATION`, `IDP_CONFIGURATION`, `FEATURE_FLAG`
- ADRs: ADR-0020, ADR-0022
- Related Stories: FS-01, FS-09, FS-13
