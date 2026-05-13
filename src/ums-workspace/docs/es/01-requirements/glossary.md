> ?? **Nota de Arquitectura:** Este documento se encuentra actualmente en su versi�n original (Ingl�s) y est� programado para traducci�n oficial en la hoja de ruta.

# 📖 Glossary of Terms

This document establishes the standardized, non-ambiguous glossary of terms for the **User Management System (UMS)** under the **spec-driven AI strategy BMAD-METHOD**.


---

## 🏛️ UMS Unified Glossary


| Term | Definition | SSoT Schema Owner |
| :--- | :--- | :--- |
| **User (Usuario)** | A unique human operator or service account registered in the system. Has credentials and assigned Profiles. | `Identity.Users` |
| **Organization (Organización)**| A corporate tenant or company operating within the multi-tenant workspace. Linked to an ERP code. | `Identity.Organizations` |
| **Branch (Sedes)** | A physical or logical sub-unit of an Organization (e.g., *Callao Port Terminal*, *Lurin Warehouse*). Acts as the branch context for hierarchical authorization routing. | `Identity.Branches` |
| **Network (Red)** | A logical network boundary (Private SCM, Public, Shared) governing access policies. | `Identity.Networks` |
| **System (Sistema)** | An independent application or sub-portal registered in the platform (e.g., SCM Route Planner, Billing). | `Auth.Systems` |
| **Menu (Menú)** | A structured navigation tree of sidebars and views belonging to a System. | `Auth.Menus` |
| **Submenu** | A secondary grouping within a Menu, containing one or more Options. | `Auth.Submenus` |
| **Option (Opción)** | A specific web page or UI view within a Submenu. | `Auth.Options` |
| **Action (Acción)** | A granular operation (e.g., `create`, `read`, `export`) mapped to an API endpoint. | `Auth.Actions` |
| **Profile (Perfil)** | A physical collection of authorizations assigned to Users, scoped to an Organization and optionally a Branch. | `Auth.Profiles` |
| **Authorization (Autorización)**| The mapping of an Allow/Deny policy to a specific Resource + Action. | `Auth.Authorizations` |
| **Auth Template (Plantilla)** | A reusable versioned blueprint of authorizations used to instantiate Profiles. | `Auth.Templates` |
| **Permission (Permiso)** | The runtime resolved ability of a User to execute an Action, following the precedence rules. | Resolved at Runtime |
| **Multi-Tenancy** | Architectural pattern enabling multiple secure tenants to share the same physical database. | `Core.Architecture` |
| **Authorization Graph** | The compiled hierarchical JSON structure mapping a principal's full permission set for a given system and branch context. | Resolved at Runtime |
| **Principal** | An authenticated subject (user or service) requesting access to a resource. | IAM Standard Term |
| **PEP** | Policy Enforcement Point — the gateway component that intercepts requests and enforces access rules. | `Auth.Gateway` |
| **PDP** | Policy Decision Point — the UMS core engine that compiles and returns the authorization graph. | `Auth.Engine` |
| **PAP** | Policy Administration Point — the UMS Administrative Web Console where rules are managed. | `Console.App` |
| **PIP** | Policy Information Point — the PostgreSQL database supplying entity attributes during evaluation. | `Auth.Database` |


