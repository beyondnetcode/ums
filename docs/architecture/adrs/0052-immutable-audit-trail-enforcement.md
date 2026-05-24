# ADR-0052: Immutable Audit Trail — SQL Server Enforcement Strategy

## Status

Accepted

## Date

2026-05-15

## Context

Evolith ADR-0016 mandates that audit records be **physically immutable** — no UPDATE or DELETE can ever execute against audit trail tables, not even by service accounts or DBAs. UMS has a 10-column audit schema on every entity table and a dedicated event log table per bounded context. Without enforcement at the database layer, a misconfigured command, elevated-privilege script, or ORM misconfiguration can silently corrupt the audit trail.

Two layers of concern:

1. **Entity audit columns** (`created_at`, `created_by`, `modified_at`, `modified_by`, `deleted_at`, `deleted_by`, etc.) must be write-once for creation columns and append-only for modification columns — no retroactive alteration
2. **Audit event log tables** (e.g., `ums_audit.domain_events`, `ums_audit.security_events`) must be strictly insert-only; UPDATE and DELETE must be blocked at the SQL engine level regardless of caller

---

## Decision

**Enforce audit immutability at two levels: SQL Server DDL triggers on audit log tables (insert-only) and command handler delta capture for entity audit columns (application-layer before/after snapshot).**

### 1. SQL Server DDL Triggers — Audit Log Tables (Insert-Only)

All tables in the `ums_audit` schema are protected by a trigger that raises an error on any UPDATE or DELETE attempt:

```sql
-- migrations/audit/V002__audit_immutability_triggers.sql
CREATE OR ALTER TRIGGER trg_domain_events_immutable
ON ums_audit.domain_events
AFTER UPDATE, DELETE
AS
BEGIN
    RAISERROR (
        'Audit log is immutable. UPDATE and DELETE are prohibited on ums_audit.domain_events.',
        16, 1
    );
    ROLLBACK TRANSACTION;
END;
GO

CREATE OR ALTER TRIGGER trg_security_events_immutable
ON ums_audit.security_events
AFTER UPDATE, DELETE
AS
BEGIN
    RAISERROR (
        'Audit log is immutable. UPDATE and DELETE are prohibited on ums_audit.security_events.',
        16, 1
    );
    ROLLBACK TRANSACTION;
END;
GO
```

The same trigger pattern is applied to every table in `ums_audit`:

| Table | Trigger |
|---|---|
| `ums_audit.domain_events` | `trg_domain_events_immutable` |
| `ums_audit.security_events` | `trg_security_events_immutable` |
| `ums_audit.permission_changes` | `trg_permission_changes_immutable` |
| `ums_audit.document_events` | `trg_document_events_immutable` |
| `ums_audit.outbox_messages` | `trg_outbox_messages_immutable` (blocks DELETE of processed messages) |

### 2. Audit Log Table Structure

```sql
CREATE TABLE ums_audit.domain_events (
    id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    occurred_at     DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    event_type      NVARCHAR(200)    NOT NULL,   -- ums.identity.user.registered
    aggregate_type  NVARCHAR(100)    NOT NULL,
    aggregate_id    UNIQUEIDENTIFIER NOT NULL,
    tenant_id       UNIQUEIDENTIFIER NOT NULL,
    root_tenant_id  UNIQUEIDENTIFIER NOT NULL,
    actor_id        UNIQUEIDENTIFIER NULL,
    payload         NVARCHAR(MAX)    NOT NULL,   -- JSON CloudEvents envelope
    schema_version  SMALLINT         NOT NULL DEFAULT 1,
    CONSTRAINT pk_domain_events PRIMARY KEY (id, root_tenant_id)
) ON ps_tenant_scheme(root_tenant_id);  -- partition per ADR-0049
GO
```

### 3. Application-Layer Delta Capture (Entity Audit Columns)

Command handlers capture before/after state for entities using a `DeltaCapture` utility:

```csharp
// src/UMS.Application/Audit/DeltaCapture.cs
public static class DeltaCapture
{
    public static AuditDelta Capture<T>(T before, T after, string actorId)
        where T : class
    {
        var beforeJson = JsonSerializer.Serialize(before, AuditSerializerOptions.Default);
        var afterJson  = JsonSerializer.Serialize(after,  AuditSerializerOptions.Default);

        return new AuditDelta(
            Before:      beforeJson,
            After:       afterJson,
            ActorId:     actorId,
            OccurredAt:  DateTimeOffset.UtcNow,
            HasChanges:  beforeJson != afterJson
        );
    }
}

public sealed record AuditDelta(
    string Before,
    string After,
    string ActorId,
    DateTimeOffset OccurredAt,
    bool HasChanges);
```

Usage in a command handler:

```csharp
public async Task<Result<Unit>> Handle(BlockUserCommand cmd, CancellationToken ct)
{
    var user = await _repo.GetByIdAsync(cmd.UserId, ct);
    var before = user.ToAuditSnapshot();

    var result = user.Block(cmd.Reason);
    if (!result.IsSuccess) return result;

    var after = user.ToAuditSnapshot();
    var delta = DeltaCapture.Capture(before, after, cmd.ActorId);

    if (delta.HasChanges)
    {
        await _auditRepo.RecordAsync(new DomainEventRecord(
            EventType:     "ums.identity.user.blocked",
            AggregateType: nameof(UserAccount),
            AggregateId:   user.Id,
            TenantId:      user.TenantId,
            Payload:       delta.After,
            ActorId:       delta.ActorId
        ), ct);
    }

    await _repo.UpdateAsync(user, ct);
    await _unitOfWork.CommitAsync(ct);
    return Result<Unit>.Ok(Unit.Value);
}
```

### 4. Entity Audit Columns — Write Rules

Every entity table inherits the 10-column audit schema. EF Core configurations enforce the write rules:

```csharp
// Example: UserAccountConfiguration.cs
builder.Property(e => e.CreatedAt)
    .ValueGeneratedOnAdd()
    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);  // never overwrite

builder.Property(e => e.CreatedBy)
    .ValueGeneratedOnAdd()
    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

builder.Property(e => e.ModifiedAt)
    .ValueGeneratedOnAddOrUpdate();

builder.Property(e => e.RowVersion)
    .IsRowVersion();  // optimistic concurrency
```

### 5. Soft Delete — No Physical DELETE

UMS uses soft deletes exclusively. Physical DELETE is never issued against entity tables. Hard-delete stored procedures are reserved for GDPR purge workflows and are:

1. Logged to `ums_audit.security_events` before execution
2. Require dual-authorization (two service accounts with GDPR role)
3. Only available for PII fields, not entire rows

### 6. CI Validation

A migration lint step verifies that no new migration adds DROP TRIGGER, DISABLE TRIGGER, or ALTER TABLE ... DROP on audit tables:

```yaml
# .github/workflows/audit-guard.yml
- name: Audit trigger guard
  run: |
    grep -rn "DISABLE TRIGGER\|DROP TRIGGER\|DROP TABLE.*ums_audit" migrations/ \
      && { echo "VIOLATION: audit immutability guard triggered"; exit 1; } \
      || echo "OK: no audit trigger modifications found"
```

---

## Consequences

### Positive

- Physical immutability guaranteed at the SQL engine level — no ORM bug, no elevated-privilege query can alter audit rows
- Delta capture gives a complete before/after snapshot for every state transition
- Trigger errors surface immediately in integration tests (Testcontainers) — not only in production
- Soft-delete discipline means no accidental data loss from application-layer mistakes

### Negative

- Triggers add a small per-insert overhead on `ums_audit` tables (negligible for append-only workloads)
- GDPR purge workflow requires a separate out-of-band process; cannot use standard EF Core delete
- Every new `ums_audit` table requires its own trigger — enforced by migration convention checklist

---

**[ADR Registry](./index.md)** | **[Evolith ADR-0016](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/core/0016-immutable-business-audit-trail.md)**
