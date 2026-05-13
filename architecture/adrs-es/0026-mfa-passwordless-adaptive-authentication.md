# ADR-0026: Plataforma de Autenticación MFA Adaptativa Multi-Inquilino y Passwordless (WebAuthn/Passkeys)

* **Status:** Accepted
* **Basado en:** [arc32-26](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0026-mfa-passwordless-adaptive-authentication.md)
* **Date:** 2026-05-08

## Resumen de Adaptación

The corporate standard is adopted with the following modificaciones específicas del proyecto:
1. Estrategia MFA independiente del runtime. Misma pila WebAuthn, TOTP, motor de riesgo adaptativo.
2. Implementación .NET con Fido2-net-lib en lugar de @simplewebauthn/server.

## Referencia Completa del Estándar

Ver la fuente corporativa para el contexto completo y la justificación de la decisión.
