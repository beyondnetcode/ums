# Functional Story 3: Registrar Organización y Configurar Estrategia de IdP

## 1. Propósito de Negocio

UMS debe permitir que administradores de seguridad incorporen una nueva organización y definan cómo se autenticarán sus usuarios. Esto da a cada tenant una estrategia clara de identidad manteniendo el registro gobernado y auditable.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Seguridad Global** | Registra organizaciones y elige su estrategia de identidad. |
| **Administrador de Organización** | Podrá gestionar sedes, usuarios y configuraciones locales posteriormente. | ## 3. Precondiciones de Negocio

- El actor estáá autenticado como administrador global.
- La organización estáá aprobada para onboarding.
- La información empresarial requerida estáá disponible.

## 4. Flujo Funcional Principal

1. El administrador abre el área de gestión de organizaciones.
2. El administrador ingresa nombre legal, código de referencia externo y tipo de organización.
3. El administrador selecciona la estrategia de identidad que usará la organización.
4. Si la estrategia seleccionada requiere información adicional, el administrador completa la configuración requerida.
5. El sistema valida que la organización pueda registrarse.
6. El sistema activa la organización y registra la decisión de onboarding.
7. El administrador puede continuar con la configuración de sedes y usuarios.

## 5. Flujos Alternativos y Excepciones

### A. Configuración de Proveedor de Identidad No Verificable

Si el proveedor de identidad seleccionado no puede validarse, la organización no se activa hasta corregir la configuración.

### B. Referencia Externa Duplicada

Si la referencia empresarial ya existe, el sistema evita crear una organización duplicada.

## 6. Reglas de Negocio

1. Cada organización debe tener una referencia externa única cuando aplique.
2. La estrategia de autenticación debe seleccionarse explícitamente.
3. La creación de organización debe ser auditable.
4. El registro de sedes depende de una organización activa.

## 7. Criterios de Aceptación

1. Un administrador global puede registrar una organización válida.
2. Las referencias empresariales duplicadas son rechazadas.
3. Una configuración IdP inválida impide la activación.
4. Una organización registrada puede usarse para onboarding de sedes y usuarios.

## 8. Requisitos Técnicos

- Persistir datos de organización en el modelo `TENANT` / `ORGANIZATION`.
- Persistir configuración de proveedor de identidad en `IDP_CONFIGURATION`.
- Aplicar unicidad para referencias externas de compañía.
- Emitir `OrganizationCreatedEvent`.
- Validar configuración IdP según el tipo de proveedor seleccionado.

## 9. Trazabilidad

- Entidades: `TENANT`, `BRANCH`, `IDP_CONFIGURATION`, `USER_ACCOUNT`
- ADRs: ADR-0031, ADR-0032, ADR-0034, ADR-0010
- Technical Enabler: TE-03
