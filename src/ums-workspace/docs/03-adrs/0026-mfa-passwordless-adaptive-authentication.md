# 📜 ADR-0026 — Multi-Tenant Adaptive MFA and Passwordless (WebAuthn/Passkeys) Authentication Platform

**Status:** Accepted  
**Date:** 2026-05-09  
**Deciders:** Enterprise Architect, Product Owner, Lead Security Engineer  
**ADR Type:** Architectural Hardening — Core Identity Platform  
**Related Specs:** [`mfa-passwordless-security-spec.md`](../../04-artifacts/mfa-passwordless-security-spec.md), [`uc-11-mfa-passwordless-adaptive-auth.md`](../../01-requirements/usecases/uc-11-mfa-passwordless-adaptive-auth.md)

---

## 📋 Context

The UMS is the central identity and authorization core governing access across multiple enterprise SaaS platforms. To meet the stringent requirements of enterprise clients (SCM, ERP, WMS), comply with modern cybersecurity standards (OWASP ASVS Level 3, NIST SP 800-63B AAL3), and embrace a Zero Trust architectural posture, several security vulnerabilities must be addressed:

1. **Phishing Vulnerabilities**: Passwords and simple SMS/Email OTP secondary factors remain vulnerable to phishing and social engineering exploits.
2. **Static MFA Friction**: Standard "always-on" MFA causes significant user friction (MFA Fatigue), resulting in poor operational throughput.
3. **Coarse-Grained Policies**: MFA policies are currently managed monolithically, with no capability to customize enforcements based on User Roles, Applications (Systems), or transaction-specific Criticality Levels.
4. **Lack of Adaptive Risk Assessment**: Login requests are validated statically, with no real-time analysis of client risk contexts (IP geolocation velocity anomalies, device fingerprint mutations, suspicious behaviors).

To address these challenges, we require a decentralized, multi-tenant-aware security framework that decouples authentication policies from application code while introducing phishing-resistant WebAuthn/Passkeys, adaptive risk-scoring, and cryptographic device recognition.

---

## ⚖️ Decision

We will implement an **Adaptive MFA and Passwordless (WebAuthn/Passkeys) Security Subsystem** inside the UMS platform. This includes:

### 1. Phishing-Resistant Passwordless (WebAuthn/Passkey) Engine
Enables native WebAuthn support (FIDO2/U2F) allowing users to register and authenticate using hardware-backed cryptographic credentials (YubiKeys, native biometrics like Windows Hello or Apple TouchID/FaceID) directly through the customizable UMS Hosted Login page.

### 2. Context-Aware Adaptive Risk Evaluator
A low-latency, stateless risk evaluation engine integrated into the authentication pipeline. It scores the risk of each authentication attempt dynamically based on:
- **IP Allowlisting & Geo-fencing**: Validates requests against corporate IP blocks and geographic blacklists.
- **Geolocation Velocity Checks**: Detects "impossible travel" (e.g., login in Peru followed by a login in Germany 30 minutes later).
- **Device Fingerprint Mismatch**: Identifies changes in user browser profiles and hardware configurations.

### 3. Granular Multi-Tenant Enforcement Policies
A policy engine allowing Tenant Administrators to declare and publish flexible enforcement matrices via the `AUTH_POLICY_CONFIGURATION` store:
- **MANDATORY**: All tenant users must enroll and verify MFA.
- **ADAPTIVE**: MFA is only prompted if the risk score exceeds a defined threshold or if the device is unrecognized.
- **STEP-UP**: Prompts users for biometric re-validation when executing high-criticality actions or transactions.

### 4. Self-Service Secure Recovery
Provides an automated recovery strategy via Bcrypt-hashed secure one-time Recovery Codes generated during onboarding, allowing users to recover lost accounts securely without administrative intervention.

---

## 📐 Architecture Constraints

- **Hexagonal Decoupling**: All MFA factors are decoupled behind the `IMfaPort` and `IWebAuthnPort` interfaces, allowing plug-and-play extensions for external services (e.g., Auth0, Keycloak, Duo, or Zitadel) without modifying core business use cases.
- **Tenant RLS Enforced**: All newly introduced schemas (`AUTH_POLICY_CONFIGURATION`, `ENROLLED_MFA_FACTOR`, `TRUSTED_DEVICE`) incorporate a `tenant_id` column governed by PostgreSQL Row-Level Security policies.
- **Ultra-low Latency Budget**: The adaptive risk scoring and device verification must resolve in **under 10ms** to prevent degrading the standard UMS authentication performance.
- **Event-Driven Audit Trails**: Security anomalies and MFA mutations emit structured domain events (`MfaFactorRegisteredEvent`, `MfaChallengeFailedEvent`, `AccountPermanentlyLockedEvent`) logged immutably.
- **Encryption at Rest**: Enrolled TOTP shared secrets and backup recovery codes are stored using AES-256-GCM and Bcrypt (with a work factor of 10), respectively.

---

## ✅ Consequences

### Positive
- **Strong Anti-Phishing Posture**: WebAuthn/Passkeys bind credential signatures to specific domain origins, eliminating phishing threat vectors.
- **Zero Trust Alignment**: Verifies every request context explicitly based on risk-scoring, matching NIST AAL3 standards.
- **Reduced MFA Fatigue**: "Remember Device" utilizes cryptographic browser tokens to minimize MFA prompts for safe contexts while dynamically demanding re-authentication for high-risk operations.
- **Tenant Autonomy**: B2B clients manage their local security postures completely (e.g., enforcing WebAuthn for high-privilege business roles while allowing TOTP for standard operations).

### Negative
- **Onboarding Overhead**: Mandatory MFA increases the user onboarding curve, mitigated by a sleek, interactive onboarding UI.
- **Increased State Management**: Database schema expands with three new relational tables and two new Redis caching namespaces (`mfa:*`, `risk:*`).
- **Cryptographic Overhead**: Verifying WebAuthn signatures requires minor CPU utilization, which is optimized using compiled native libraries.

---

## 🔗 ADR Impact Cross-References

| ADR | Impact | Action |
| :--- | :--- | :--- |
| ADR-0010 (Multi-Tenancy) | RLS isolation rules apply automatically to new security tables | No change |
| ADR-0014 (Redis Caching) | Introduces two new cache namespaces: `mfa:*` (pending challenges) and `risk:*` (compiled fingerprints) | Extend Redis key policies |
| ADR-0016 (Immutable Audit)| Security event domains are streamed to the audit log outbox | Extend audit subscribers |
| ADR-0020 (IdP Abstraction) | Expands single-IdP OIDC federations by incorporating WebAuthn/Passkey native challenges at the gateway level | Integrated with multi-IdP fallback |
| ADR-0021 (High-Performance Auth) | Integrates adaptive step-up and MFA challenges into the stateless session token issuance pipeline | Modified sequence |
| ADR-0024 (Config Platform) | Security policy parameters are embedded within the versioned dynamic `SYSTEM_CONFIGURATION` schema | Integrated |
