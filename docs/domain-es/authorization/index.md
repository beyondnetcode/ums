# Contexto de Autorización (Authorization BC) — Arquitectura de Agregados

> **Idioma:** [English](../../domain/authorization/index.md) | [Español](./index.md)

**Contexto Delimitado:** Autorización (`Ums.Domain.Authorization`)  
**Raíces de Agregado (Aggregate Roots):** `SystemSuite`, `PermissionTemplate`, `Profile`

---

### Modelo de Suite de Aplicaciones
La estructura de suites gobierna los menús de navegación y acciones en el sistema:
- [SystemSuite](./system-suite.md) (Raíz de Agregado) — Aplicaciones de nivel superior de la plataforma (ej. Portal de Administración, Portal de Sucursal).
- [Module](./module.md) (Entidad Propia) — Secciones funcionales modulares dentro de una suite.
- [Menu](./menu.md) (Entidad Propia) — Interfaces gráficas de menús.
- [SubMenu](./sub-menu.md) (Entidad Propia) — Bloques de submenús anidados.
- [Option](./option.md) (Entidad Propia) — Anclajes específicos de configuración de vistas/pantallas.
- [Action](./action.md) (Entidad Propia) — Tokens de acción granulares (ej. READ, WRITE, EXPORT) para asegurar comportamientos individuales.

### Permisos y Plantillas
- [PermissionTemplate](./permission-template.md) (Raíz de Agregado) — Paquetes de permisos reutilizables y estandarizados.
- [PermissionTemplateItem](./permission-template-item.md) (Entidad Propia) — Mapeos específicos de acciones definidos dentro de una plantilla.

### Perfiles de Seguridad
- [Profile](./profile.md) (Raíz de Agregado) — Roles asignados a los usuarios delimitados por su ámbito (GLOBAL, TENANT o BRANCH).
- [ProfilePermission](./profile-permission.md) (Entidad Propia) — Acciones permitidas específicas asignadas a un perfil.

---

**[Volver al Índice de Dominio](../index.md)**
