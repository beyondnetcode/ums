# Matriz Competitiva - UMS vs Lideres del Mercado

## Marco de Posicionamiento

UMS no busca reemplazar Keycloak ni competir de forma directa con Entra en todo. Su oportunidad es un control plane enfocado en autorizacion y gobierno para SaaS multi-tenant.

## Matriz Comparativa

| Capacidad | UMS | Microsoft Entra ID Governance | Okta Identity Governance | SailPoint | Keycloak | Auth0 |
|---|---|---|---|---|---|---|
| Control plane multi-tenant con marca blanca | Fuerte | Medio | Medio | Bajo | Medio | Medio |
| Revisiones de acceso / certificaciones | Emergente | Fuerte | Fuerte | Fuerte | Bajo | Bajo |
| Paquetes de acceso / bundles de entitlements | Emergente | Fuerte | Fuerte | Fuerte | Bajo | Bajo |
| Workflows de ciclo de vida / provisioning | Emergente | Fuerte | Fuerte | Fuerte | Medio | Medio |
| Elevacion privilegiada / JIT | Emergente | Fuerte | Fuerte | Fuerte | Bajo | Bajo |
| Delegacion fina de administracion | Fuerte | Fuerte | Fuerte | Fuerte | Fuerte | Medio |
| Grafo de autorizacion explicable | Fuerte | Medio | Medio | Medio | Bajo | Bajo |
| Paquetes con semantica de negocio | Oportunidad | Medio | Medio | Medio | Bajo | Bajo |
| Salud continua de acceso / postura | Oportunidad | Fuerte | Fuerte | Fuerte | Bajo | Bajo |
| Capa de politica productizada por cliente | Fuerte | Medio | Medio | Medio | Bajo | Bajo |

## Lecturas Clave

- Entra, Okta y SailPoint son mas fuertes en amplitud de gobierno.
- Keycloak es fuerte en identidad y admin scoping, pero la gobernanza profunda no es su wedge principal.
- Auth0 es fuerte para CIAM y experiencia de desarrollador, pero no para governance profundo.
- UMS puede diferenciarse con explicabilidad, semantica de negocio, delegacion por tenant y control graph nativo.

## Ventajas de Innovacion de UMS

1. Grafo de autorizacion explicable con simulacion what-if.
2. Paquetes de acceso con semantica de negocio alineados a tenant, sucursal y socios.
3. Salud continua de acceso que recomienda la siguiente accion de gobierno.
4. Guardrails operativos que protegen el propio plano de gobierno.
5. Administracion delegada por tenant como concepto de producto de primera clase.

## Fuentes

- [Microsoft Entra ID Governance](https://learn.microsoft.com/en-us/entra/id-governance/)
- [Okta Identity Governance](https://www.okta.com/products/identity-governance/)
- [SailPoint IdentityIQ Certifications and Access Reviews](https://documentation.sailpoint.com/identityiq/help/certifications_and_access_reviews/index.html)
- [Keycloak Server Administration Guide](https://www.keycloak.org/docs/latest/server_admin/)
- [Auth0 Multiple Organization Architecture](https://auth0.com/docs/media/articles/architecture-scenarios/planning/Multiple-Organization-Architecture-Multitenancy-Overview.pdf)
