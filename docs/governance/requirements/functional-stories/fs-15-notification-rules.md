# Functional Story 15: Configure Expiration Notification Rules

## 1. Business Purpose

Compliance administrators need to warn users before required documents expire, so users can renew them on time and avoid unnecessary access restrictions.

---

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Compliance Administrator** | Defines notification timing and delivery channels. |
| **User** | Receives renewal reminders. |
| **Compliance Engine** | Applies active rules when documents approach expiration.
## 3. Business Preconditions

- The document type exists.
- The administrator has permission to manage notification rules.

---

## 4. Main Functional Flow

1. The administrator selects the document type that requires reminders.
2. The administrator defines how many days before expiration the user should be notified.
3. The administrator selects one or more delivery channels.
4. The administrator provides a clear description of the rule and its business impact.
5. The system saves the rule and makes it available for compliance processing.
6. Users receive reminders according to the active rules.

---

## 5. Alternative Flows and Exceptions

### A. Duplicate Notification Rule

If an identical rule already exists for the same document, tenant, timing, and channel, the system prevents duplication to avoid notification spam.

---

## 6. Business Rules

1. Notification rules must be configurable by tenant and document type.
2. Multiple reminder steps may exist for the same document type.
3. Every rule must include `code`, `value`, and `description`.
4. The system must preserve traceability of sent notifications.

---

## 7. Acceptance Criteria

1. An administrator can configure at least three reminder levels.
2. Duplicate rules are blocked.
3. Users receive reminders before expiration when rules apply.
4. Notification behavior can vary by tenant.

---

## 8. Technical Requirements

> [!NOTE]
> En la implementación real de C# (base de código), `NotificationRule` es una Entidad hija encapsulada dentro del Agregado **[DocumentType](file:///d:/Users/aarroyo/personal/sources/ums/src/apps/app-api-dotnet/Ums.Domain/Approvals/DocumentType/DocumentType.cs)**, bajo el espacio de nombres unificado **[Ums.Domain.Approvals](file:///d:/Users/aarroyo/personal/sources/ums/src/apps/app-api-dotnet/Ums.Domain/Approvals/)**.

- Persist rules as part of the `DocumentType` Aggregate Root.
- Mandatory fields: `Code`, `Value` (JSON containing timing/channels), `Description`.
- Enforce uniqueness by `Code`, `TenantId`, and `DocumentTypeId`.
- Record notification delivery traceability.
- Support cache invalidation when notification rules change.

---

## 9. Traceability

- Entities: `DocumentType` (AR), `NotificationRule` (Child Entity), `UserDocument` (AR)
- ADRs: ADR-0045, ADR-0016
- Related Stories: FS-11, FS-16
