# 📖 Glossary of Terms

This document establishes the standardized, non-ambiguous glossary of terms for the **User Management System (UMS)** under the **spec-driven AI strategy BMAD-METHOD**.


---

## 🏛️ UMS Unified Glossary


| Term | Definition | SSoT Schema Owner |
| :--- | :--- | :--- |
| **User (Usuario)** | A unique human operator or service account registered in the system. Has credentials and assigned Profiles. | `Identity.Users` |
| **Organization (Organización)**| A company node. Can be the primary corporate Tenant (`INTERNAL`) or an external actor such as a B2B `CLIENT` or `SUPPLIER` linked to an ERP code. | `Identity.Organizations` |
| **Sponsor User** | An internal corporate user who requests and justifies system access for an external third-party user. | `Identity.Users` |
| **External Access Request** | An auditable business ticket routing an external B2B access request through the PAP approval workflow. | `Identity.AccessRequests` |
| **Branch (Sedes)** | A physical or logical sub-unit of an Organization (e.g., *Callao Port Terminal*, *Lurin Warehouse*). Acts as the branch context for hierarchical authorization routing. | `Identity.Branches` |
| **Network (Red)** | A logical network boundary (Private Network, Public, Shared) governing access policies. | `Identity.Networks` |
| **System (Sistema)** | An independent application or sub-portal registered in the platform (e.g., Route Planner, Billing). Contains one or more Modules. | `Auth.Systems` |
| **Menu (Menú)** | A structured navigation tree of sidebars and views within a Module. | `Auth.Menus` |
| **Module (Módulo)** | A logical grouping of Menus and Options within a System. Modules organize functional areas and can have Actions attached at the module level. | `Auth.Modules` |
| **Option (Opción)** | A specific web page or UI view within a Menu. | `Auth.Options` |
| **Action (Acción)** | An executable operation or permission that can be attached at System, Module, Menu, or Option level. | `Auth.Actions` |
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

