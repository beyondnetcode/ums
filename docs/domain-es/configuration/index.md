# Contexto de Configuración (Configuration BC) — Arquitectura de Agregados

> **Idioma:** [English](../../domain/configuration/index.md) | [Español](./index.md)

**Contexto Delimitado:** Configuración (`Ums.Domain.Configuration`)  
**Raíces de Agregado (Aggregate Roots):** `AppConfiguration`, `FeatureFlag`, `IdpConfiguration`

---

### Configuraciones y Alternadores de Aplicaciones
- [AppConfiguration](./app-configuration.md) (Raíz de Agregado) — Controla los parámetros de configuración del inquilino, tiempos de expiración de sesión, políticas de MFA y variables de entorno.
- [FeatureFlag](./feature-flag.md) (Raíz de Agregado) — Define las banderas operativas y de lanzamiento de características a nivel de plataforma o inquilino.
- [FlagEvaluationLog](./flag-evaluation-log.md) (Entidad Propia) — Registra los contextos de evaluación y resultados en tiempo de ejecución para depuración y auditoría.
- [FeatureFlagCriteria](./feature-flag-criteria.md) (Entidad Propia) — Criterios de evaluación dinámicos que determinan cuándo una feature flag está activa para un contexto dado.

### Configuraciones de Integración
- [IdpConfiguration](./idp-configuration.md) (Raíz de Agregado) — Mapeos de secretos técnicos, claves privadas, IDs de cliente y endpoints para proveedores de identidad federados (OIDC, SAML, WS-Fed).

---

**[Volver al Índice de Dominio](../index.md)**
