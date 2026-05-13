# ADR 0020: Identity Provider Abstraction Strategy

## Status
Accepted

## Context
Traditional enterprise systems tightly couple their authentication flows to a specific proprietary Identity Provider (IdP) SDK (e.g., Zitadel, AWS Cognito, Okta), leading to high migration costs, severe vendor lock-in, and the inability to cater to enterprise B2B clients demanding SAML/OIDC SSO federation or air-gapped standalone environments requiring internal username/password authentication.

Under the **bMAD Method**, all critical platform capabilities must remain highly decoupled, extensible, and future-proof.

## Decision
We will decouple authentication from the core authorization domain using the **Strategy Pattern** wrapped behind a clean Hexagonal Port (`IAuthenticationPort`). While Zitadel remains the default external reference implementation, the platform core is entirely agnostic of the physical identity provider.

The architecture will dynamically select and execute pluggable adapters at runtime (based on the `Tenant-ID` context or configuration parameters) supporting:
*   **Internal Authentication**: Managed user stores utilizing Bcrypt hashing.
*   **External Enterprise IdPs**: Zitadel, AWS Cognito, Microsoft Entra ID (Azure AD), Okta, Auth0, Keycloak, or generic OIDC/OAuth2/SAML providers.
*   **Modern Security Standards**: Federated Single Sign-On (SSO), WebAuthn (Passkeys), Multi-Factor Authentication (MFA), and OpenID Connect (OIDC).

## Consequences

### Positive
*   **Zero Vendor Lock-In**: Switching identity providers is a zero-cost configuration change requiring no modifications to the core business logic.
*   **Enterprise Multi-Tenancy**: Individual tenants can self-manage and federate their own corporate identity stores (SAML/OIDC).
*   **Deployment Versatility**: Supports both standard cloud SaaS deployments (external federated IdP) and air-gapped standalone deployments (local credentials).

### Negative
*   **Factory Complexity**: Requires the maintenance of a dynamic authentication factory to resolve the appropriate adapter based on tenant context at runtime.
