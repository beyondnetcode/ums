# 🎯 Visión del Producto - Sistema de Gestión de Usuarios (UMS)

## 1. Resumen Ejecutivo
El **Sistema de Gestión de Usuarios (UMS)** es un núcleo abstracto e independiente para la gobernanza de identidad y autorización. Su visión principal es centralizar y estandarizar el control de identidades, organizaciones y permisos granulares en una arquitectura **SaaS B2B Multi-tenant** federada y multisistema, a través de APIs altamente desacopladas y buses de mensajería.

En lugar de servir como un simple almacén de usuarios, UMS actúa como un **Motor Especializado de Autorización y Configuración Dinámica** que administra "lo que un usuario puede hacer", al mismo tiempo que proporciona una base de datos interna nativa y segura, y la flexibilidad plug-and-play para delegar "quién es el usuario" a Proveedores de Identidad (IdP) externos seguros y soberanos.

---

## 2. Pilares Estratégicos

### A. Identidad Soberana (Autenticación Nativa y Delegada)
- Las bases de datos de contraseñas tradicionales están obsoletas, pero el bloqueo de proveedores es peor. UMS exige un modelo de **identidad opcional y enchufable**.
- UMS proporciona su propio **Almacén de Identidad Nativo** completamente funcional (usando hashes seguros bcrypt) de forma predeterminada.
- Para los clientes corporativos (tenants), la autenticación se puede delegar sin problemas a Proveedores de Identidad externos (Zitadel, Azure AD, Okta, SAML/OIDC) a través de OIDC y OAuth 2.0 (Flujo de Código de Autorización con PKCE) bajo una estrategia plug-and-play.

### B. Multi-Tenancy B2B Dinámico
- El aislamiento de inquilinos de alto rendimiento se aplica directamente a nivel de la base de datos PostgreSQL mediante **Seguridad a Nivel de Fila (RLS)**.
- Las organizaciones (inquilinos) tienen total autonomía de autoservicio para administrar a sus empleados locales, roles y perfiles, liberando al operador de la plataforma principal de la sobrecarga administrativa diaria.

### C. Inyección Dinámica de UI y Autorización Granular
- Las interfaces de usuario de las aplicaciones se renderizan dinámicamente. El cliente frontend consulta a UMS para inyectar **Menús, Opciones y Acciones** personalizados en tiempo real, basados en el grafo de permisos compilado del usuario.

### D. Infraestructura Enchufable y Opcional (Cero Bloqueo de Proveedor)
- **Mandato de Cero Dependencia Externa**: Tanto los Proveedores de Identidad (IdP) externos como los Gestores de Feature Flags externos (ej. LaunchDarkly, Unleash, ConfigCat) son completamente **opcionales**.
- La plataforma proporciona una implementación nativa completamente operativa de todos los servicios.
- El intercambio por servicios externos opera como un cambio puro de configuración en tiempo de ejecución (plug-and-play), requiriendo cero despliegues o modificaciones de código.

---

## 3. Filosofía Central y Preparación para el Futuro
Al mantener el Núcleo del Dominio completamente puro y desacoplado de frameworks externos, UMS está diseñado para una evolución fluida y a prueba de futuro. El código de la aplicación adopta estrictamente la **Arquitectura Hexagonal** (Puertos y Adaptadores), garantizando que los SDKs de proveedores externos no se filtren en la lógica de negocio central. Esto prepara a UMS para su transición hacia microservicios independientes gobernados por sidecars de **Dapr** cuando se cumplan los disparadores de escalabilidad técnica.
