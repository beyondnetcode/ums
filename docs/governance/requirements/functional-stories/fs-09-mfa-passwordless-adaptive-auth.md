# Functional Story 9: Adaptive MFA and Passwordless Authentication

## 1. Business Purpose

UMS must strengthen authentication when risk or tenant policy requires it, while allowing modern passwordless access where permitted.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **End User** | Completes additional verification or passwordless sign-in. |
| **Tenant Administrator** | Configures authentication policy. |
| **UMS** | Evaluates policy and risk before granting access. | ## 3. Business Preconditions

- The user has an account in UMS.
- The tenant policy allows or requires MFA/passwordless authentication.
- The user has or can enroll an approved verification method.

## 4. Main Functional Flow

1. The user starts authentication.
2. UMS evaluates the tenant policy and current risk context.
3. If additional verification is required, UMS asks the user to complete the approved challenge.
4. If passwordless access is allowed, UMS lets the user authenticate with the registered method.
5. After successful verification, UMS establishes the user's session.
6. The user continues to the target system with the appropriate permissions.

## 5. Alternative Flows and Exceptions

### A. MFA Enrollment Required

If the user has no registered verification method and policy requires one, UMS guides the user through enrollment before granting access.

### B. Verification Failed

If the user fails the challenge, UMS blocks access and records the attempt.

### C. Elevated Risk

If the risk context is elevated, UMS requires stronger verification before allowing access.

## 6. Business Rules

1. Tenant policy determines whether MFA or passwordless is optional, required, or disabled.
2. Elevated risk can require additional verification.
3. Failed verification must not create a valid session.
4. Enrollment and authentication attempts must be auditable.

## 7. Acceptance Criteria

1. Users can complete MFA when required.
2. Users can use passwordless access when allowed and enrolled.
3. Failed verification blocks session creation.
4. Elevated risk triggers stronger verification.

## 8. Technical Requirements

- Support approved MFA and passwordless mechanisms, including WebAuthn/passkeys when configured.
- Evaluate tenant/system security policy before session issuance.
- Persist enrollment state and verification outcomes.
- Emit audit events for enrollment, challenge, success, failure, and risk escalation.
- Return an authorization failure when required verification is not completed.

## 9. Traceability

- Entities: `USER_ACCOUNT`, `IDP_CONFIGURATION`, `SYSTEM_CONFIGURATION`
- ADRs: ADR-0026
- Related Stories: FS-01, FS-08
