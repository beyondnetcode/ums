# ADR 0065: Prohibition of Raw GUIDs in User Interfaces

## Context and Problem Statement
In distributed architectures, GUIDs (Globally Unique Identifiers) are used extensively for technical identity, foreign keys, and database references (e.g., Tenant IDs, User IDs, Delegation IDs). Often, developers inadvertently expose these raw technical identifiers to end-users on UI views, tables, and detail panels.

This practice drastically degrades User Experience (UX), violates business language representation (DDD Ubiquitous Language), and creates a sense of an unfinished "technical prototype" instead of a polished product. Raw GUIDs have no meaning to end-users and clutter the interface.

## Decision
We establish a strict architectural and UX rule: **Raw GUIDs must NEVER be exposed or rendered in the User Interface (UI), unless explicitly requested by a specific, justified business requirement.**
This rule is adopted across the entire **Evolith** reference architecture and is strictly enforced in the **UMS** monorepo.

### Implementation Guidelines
1. **Semantic Representation:** Any technical ID must be mapped to a human-readable alias, role, username, email, or business-friendly code before rendering.
2. **Internal Usage Only:** GUIDs should only be used internally for API requests, routing, keys, state management, and payload processing.
3. **Fallback Mechanisms:** If a friendly name is unavailable, the system should display a generic, localized fallback (e.g., "User", "Delegation", "Unknown") instead of printing the raw hash or ID.
4. **Code Reviews:** Pull requests containing raw ID exposure in the presentation layer (e.g., React components) without an explicit exception will be rejected.

## Consequences

### Positive
* Significantly improves the system's UX and professionalism.
* Enforces alignment with Domain-Driven Design's Ubiquitous Language by presenting business concepts rather than database technicalities.
* Prevents potential security or enumeration leaks (even though GUIDs are secure, obfuscating database keys is a good defense-in-depth practice).

### Negative
* Requires additional frontend/backend mappings or API joins to fetch semantic labels for related entities instead of just passing foreign keys.
* Developers must be diligent in writing UI formatters for edge cases where the semantic data is not immediately available in the context.
