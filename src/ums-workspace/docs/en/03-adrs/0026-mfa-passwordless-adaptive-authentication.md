# ADR-0026: Multi-Tenant Adaptive MFA and Passwordless (WebAuthn/Passkeys) Authentication Platform

* **Status:** Accepted
* **Based on:** [arc32-26](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0026-mfa-passwordless-adaptive-authentication.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. MFA strategy is runtime-agnostic. Same WebAuthn, TOTP, adaptive risk engine adopted verbatim.
2. .NET implementation uses native ASP.NET Core Identity abstractions and Fido2.Net library (Fido2-net-lib) instead of NestJS @simplewebauthn/server.
3. Deferred: WebAuthn/Passkey enrollment UI until Phase 2 of UMS Console.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
