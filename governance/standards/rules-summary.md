# BMAD-METHOD Project Rules Summary

This document summarizes the rules configuration enforced by the AI agent harness of the project.

| ID | Title | Agents | Trigger Summary | One-Line Description | Severity |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **R-01** | Bilingual Documentation Sync | `po`, `architect`, `dev`, `qa`, `analyst` | Doc/Diagram modification | Ensure Spanish and English artifacts are 100% synced. | **Critical** |
| **R-02** | Contextual Architecture (Context7) | `architect`, `dev` | Architecture tasks | Validate Decisions and Tech Stacks against context7. | **Critical** |
| **R-03** | UTF-8 & Markdown Encoding | All Agents | Document editing | Prevent encoding bugs and ensure rendering integrity. | **Critical** |
| **R-04** | Diagram Language Consistency | `po`, `architect`, `dev`, `analyst` | Diagram editing | Keep diagram labels matched to doc language. | Warning |
| **R-05** | Tech Stack Coherence | `architect`, `dev`, `qa` | Tech reference changes | Cross-validate docs with the approved stack. | **Critical** |
| **R-06** | Separation of Concerns (UC/Story) | `po`, `architect`, `analyst` | UC/Story editing | Classify as Functional/Technical/Enabler. Split Mixed. | Warning |
| **R-07** | UC Traceability | `po`, `architect`, `dev`, `analyst` | UC modification | Sync impacted diagrams and log modification metadata. | **Critical** |
| **R-08** | Auth Flow Completeness | `po`, `architect`, `dev`, `analyst` | Auth logic/diagrams | Explicitly model Federated (IDP) & Internal paths. | **Critical** |
| **R-09** | Functional Readability | `po`, `analyst`, `architect`, `dev` | Functional story editing | Keep business narrative understandable for Product Owners and Business Analysts; move implementation details to a dedicated technical requirements section. | **Critical** |
| **R-10** | Audit Report Format | `qa`, `po`, `architect`, `analyst`, `sm` | Verification tasks | Produce a structured audit findings report. | **Critical** |
| **R-11** | PO-first Architect-second | `po`, `architect` | Combined analysis | Ensure Architect follows the Functional/PO base analysis. | Warning |
| **R-12** | Naming & Tagging | All Agents | Artifact management | Enforce structural naming, prefixes, and tagging. | Info |
| **R-13** | Parametric Catalog Standard (`code/value/description`) | `architect`, `dev`, `qa`, `analyst` | Any parameter/config/policy/workflow/flag model change | Every parameterization/configuration/catalog table MUST include `code`, `value`, `description` plus uniqueness, versioning, audit, traceability, cacheability and forward extensibility controls. | **Critical** |
| **R-14** | Functional/Technical Story Split | `po`, `analyst`, `architect`, `dev`, `qa` | New or modified Functional Story | Functional intent, flows, business rules and acceptance criteria MUST be separated from APIs, payloads, protocols, persistence, cache, security controls and implementation constraints. | **Critical** | 