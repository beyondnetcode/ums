# BMAD-METHOD Enforceable Agent Instructions

This document establishes the mandatory rule configuration for the AI agent harness of this project. All agents MUST adhere to these instructions under the defined trigger conditions.

---

### R-01: Bilingual Documentation Sync
*   **Scope**: `po`, `architect`, `dev`, `qa`, `analyst`
*   **Trigger Condition**: Any document, user story, use case, or diagram creation or update.
*   **Instruction**: Every document, user story, use case, and diagram must be maintained in both Spanish and English simultaneously. When content is created or updated in one language, the equivalent artifact in the other language must be updated in the same execution. Never leave a language version outdated or out of sync.

---

### R-02: Contextual Architecture Awareness (Context7)
*   **Scope**: `architect`, `dev`
*   **Trigger Condition**: Any task involving architecture, tech stack references, diagrams, or technical decisions.
*   **Instruction**: Before executing any task involving architecture, tech stack references, diagrams, or technical decisions, always resolve and load the current project context using context7. Never make architectural assumptions from memory. Always validate against live context.

---

### R-03: UTF-8 and Markdown Encoding Integrity
*   **Scope**: `po`, `architect`, `dev`, `qa`, `sm`, `analyst`
*   **Trigger Condition**: Document generation or modification.
*   **Instruction**: All generated or modified documents must be 100% UTF-8 clean. No special characters, corrupted glyphs, or encoding artifacts that could break Markdown rendering are permitted. Verify encoding integrity before finalizing any document output.

---

### R-04: Diagram Language Consistency
*   **Scope**: `po`, `architect`, `dev`, `analyst`
*   **Trigger Condition**: Creation or update of flow diagrams, sequence diagrams, architecture diagrams, or ERDs.
*   **Instruction**: All diagram element labels must match the language of the document they belong to. Spanish document = Spanish labels. English document = English labels. Cross-language diagrams are not permitted unless explicitly requested.

---

### R-05: Tech Stack Coherence Enforcement
*   **Scope**: `architect`, `dev`, `qa`
*   **Trigger Condition**: Modification of technical references in functional or architectural documentation.
*   **Instruction**: Any technical reference in functional or architectural documentation must be validated against the approved project tech stack. Flag undeclared technologies and missing stack components from relevant documentation.

---

### R-06: UC and Story Separation of Concerns
*   **Scope**: `po`, `architect`, `analyst`
*   **Trigger Condition**: Creation or update of UCs and stories.
*   **Instruction**: Classify all UCs and stories before creation or update:
    *   `FUNCTIONAL`: business behavior, user goals, operational rules only.
    *   `TECHNICAL`: system behavior, infrastructure, integrations, architecture.
    *   `TECHNICAL_ENABLER`: NFRs, security, performance, architectural decisions.
    MIXED stories must be split before backlog acceptance. Never merge business logic with implementation details in a single UC.

---

### R-07: UC Traceability on Diagram Updates
*   **Scope**: `po`, `architect`, `dev`, `analyst`
*   **Trigger Condition**: UC creation or update.
*   **Instruction**: When a UC is created or updated, all impacted diagrams must be identified and updated in the same execution. Every diagram modification must log: document name, diagram type, what changed, and the UC ID that triggered the change.

---

### R-08: Authentication Flow Completeness
*   **Scope**: `po`, `architect`, `dev`, `analyst`
*   **Trigger Condition**: Any diagram or document including login or authentication flows.
*   **Instruction**: Any diagram or document including login or authentication flows must explicitly represent both paths: IDP (federated) and Internal (native). The decision point must be clearly visible. Single-path auth diagrams are incomplete and must be flagged.

---

### R-09: Functional Readability Standard
*   **Scope**: `po`, `analyst`
*   **Trigger Condition**: Creation or update of functional documentation.
*   **Instruction**: All functional documentation must be written in plain language accessible to non-technical stakeholders. Technical jargon must not appear in FUNCTIONAL artifacts. If a technical concept is essential, include a plain-language summary.

---

### R-10: Audit Report Format on Verification Tasks
*   **Scope**: `qa`, `po`, `architect`, `analyst`, `sm`
*   **Trigger Condition**: Any verification or quality audit task.
*   **Instruction**: Any verification or quality audit task must produce a structured report per issue:
    *   Affected document or artifact
    *   Location (section, diagram name, line, or UC ID)
    *   Issue type (encoding / language / stack / diagram / story-mix)
    *   Severity (critical / warning / info)
    *   Recommended fix
    No verification task is complete without this report.

---

### R-11: PO-first Architect-second Execution Order
*   **Scope**: `po`, `architect`
*   **Trigger Condition**: Tasks requiring both functional and architectural analysis.
*   **Instruction**: For tasks requiring both functional and architectural analysis, always execute:
    1. PO agent — functional lens, business impact, readability.
    2. Architect agent — technical lens, system coherence, stack alignment, diagrams.
    The Architect phase must build upon PO phase output. Never reverse or parallelize this order unless explicitly instructed.

### R-12: Naming and Tagging Convention Enforcement
*   **Scope**: `po`, `architect`, `dev`, `analyst`, `sm`
*   **Trigger Condition**: UC, story, epic, or technical enabler creation or update.
*   **Instruction**: All UCs, stories, epics, and technical enablers must follow the project naming convention and tagging taxonomy. Apply category prefixes and tags on creation and update. Flag untagged or incorrectly named artifacts before merge.

---

### R-13: Enterprise Structuring Standard
*   **Scope**: `po`, `architect`, `dev`, `qa`, `sm`, `analyst`
*   **Trigger Condition**: Repository initialization, new module creation, or documentation refactoring.
*   **Instruction**: All directory creation, module scaffolding, and file naming must strictly follow the `structuring-standard.md` (R-13). Any deviation requires an explicit ADR.

---

### R-14: Enterprise Documentation Professionalism
*   **Scope**: `po`, `architect`, `dev`, `qa`, `analyst`
*   **Trigger Condition**: Creation or modification of any Markdown (.md) file.
*   **Instruction**: Documentation must maintain a clean, professional enterprise visual standard. The use of emojis, UTF-8 icons, or non-standard decorative characters is strictly forbidden in headings, tables, or body text. Prioritize structured plain text, standard Markdown headings, clean tables, and simple lists. This applies to all supported languages.
