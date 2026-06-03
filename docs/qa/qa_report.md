# Reporte de QA - Tenant & User CRUD (BMAD)

## 1. Resumen Ejecutivo
Se han ejecutado pruebas automatizadas (vía scripting local y consultas directas a los endpoints de la API) para certificar las operaciones**CRUD**correspondientes a la gestión de**Tenants**y**Usuarios**en la arquitectura de UMS.

**Resultado Global**: **APROBADO** (Los endpoints base funcionan y el estado es consistente. Se detectaron algunas validaciones estrictas del modelo de dominio que cumplen perfectamente con las reglas de negocio).

---

## 2. Escenarios Ejecutados y Evidencias Físicas

Todas las evidencias con los respectivos *Payloads* de entrada y salida, así como los *Status Codes*, se han generado físicamente dentro de la carpeta `docs/qa/evidences/` del repositorio, vinculados a sus Historias de Usuario correspondientes.

### Tenants (`docs/qa/evidences/tenants/`)
- [US-001: Create Tenant (Success)](file:///d:/Users/aarroyo/personal/sources/ums/docs/qa/evidences/tenants/US-001-Create-Tenant-Success.md) - **[PASÓ]** (Status 201)
- [US-002: Suspend Tenant](file:///d:/Users/aarroyo/personal/sources/ums/docs/qa/evidences/tenants/US-002-Suspend-Tenant.md) - **[PASÓ]** (Status 204)
- [US-003: Activate Tenant](file:///d:/Users/aarroyo/personal/sources/ums/docs/qa/evidences/tenants/US-003-Activate-Tenant.md) - **[PASÓ]** (Status 204)
- [US-004: Get Tenants via GraphQL](file:///d:/Users/aarroyo/personal/sources/ums/docs/qa/evidences/tenants/US-004-Get-Tenants.md) - **[PASÓ]** (Aislamiento y consistencia de lectura)

### User Accounts (`docs/qa/evidences/users/`)
- [US-005: Create User](file:///d:/Users/aarroyo/personal/sources/ums/docs/qa/evidences/users/US-005-Create-User.md) - **[PASÓ]** (Status 201)
- [US-006: Activate User](file:///d:/Users/aarroyo/personal/sources/ums/docs/qa/evidences/users/US-006-Activate-User.md) - **[PASÓ]** (Status 204)
- [US-007: Block User](file:///d:/Users/aarroyo/personal/sources/ums/docs/qa/evidences/users/US-007-Block-User.md) - **[PASÓ]** (Status 204)
- [US-008: Delete User (Soft Delete / Anonymize)](file:///d:/Users/aarroyo/personal/sources/ums/docs/qa/evidences/users/US-008-Delete-User.md) - **[PASÓ]** (Status 204)

---

## 3. Aislamiento y Seguridad (Tenant Context Isolation)
Se verificó que los endpoints transaccionales (Comandos) requieren estrictamente el header `X-User-Id` (para el `DevAuthMiddleware`), de lo contrario rechazan la solicitud. Adicionalmente, la base de datos restringe operaciones fuera de los límites de un Tenant, garantizando la arquitectura Multi-Tenant.

---

## 4. Bugs Encontrados / Hallazgos de Dominio
1. **Validación de Categoría de Usuario**: Al intentar inyectar usuarios con `category: "Employee"` o `identityReferenceType: "EmployeeId"`, la API rechaza la petición con HTTP `422 Unprocessable Entity` y el mensaje `"User category is not supported"`.
- *Conclusión*: Esto**NO**es un bug. Es la aplicación correcta de las reglas del dominio (`UserCategory.cs` y `IdentityReferenceType.cs`) que solo aceptan `Internal`, `External`, `B2B`, etc., y tipos `HrId`, `VendorCode`, etc.
2. **GraphQL Pagination**: La query de Tenants retorna el nodo `items` correctamente serializado de acuerdo a la interfaz de React (como lo consume `tenantService.getTenants`).

---

## 5. Regression Checklist
Para futuros despliegues o actualizaciones de dependencias, se debe validar:
- [ ] Creación de Tenant con RUC repetido falla con 409 Conflict.
- [ ] Listado de Branches solo devuelve los de su respectivo `tenantId`.
- [ ] Desactivar usuario (`BlockUserAccountCommand`) invalida tokens (a nivel Middleware).
- [ ] El Hard-Delete está prohibido; `DeleteUserAccount` anonimiza según GDPR.
