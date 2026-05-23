# GDPR Backup Retention & Erasure Policy

**Owner:** Data Protection Officer  
**Applies to:** All SQL Server backups containing UMS data  
**Regulation:** GDPR Article 17 (Right to Erasure), Article 5(1)(e) (Storage Limitation)

---

## 1. The Problem

When `DELETE /api/v1/user-accounts/{id}` is called:

1. The **primary database** is updated immediately: `IsDeleted=true`, email anonymized to `gdpr_del_{sha256}@anonymized.invalid`, `IdentityReference=NULL`.
2. The **outbox** emits `UserDeletedEvent` which revokes active tokens within seconds.
3. **Backups** taken *before* the deletion still contain the original PII in plaintext.

GDPR Art. 17 requires that personal data be erased "without undue delay" — courts have interpreted this to include backup media.

---

## 2. Backup Retention Schedule

| Backup type | Retention period | Destruction method |
|---|---|---|
| Full backup (daily) | **30 days** | Automatic SQL Server backup expiry |
| Differential backup | **7 days** | Automatic expiry |
| Transaction log backup | **72 hours** | Automatic expiry |
| Off-site / cold storage | **30 days** | Explicit deletion after 30 days |
| Dev/test database copies | **14 days** | Destroy after sprint; never copy to personal devices |

**Justification:** A 30-day retention window balances operational recovery needs against the GDPR storage limitation principle. Beyond 30 days, no recovery window is offered at standard SLA.

---

## 3. Post-Erasure Verification Procedure

After processing a Right-to-Erasure request (`UserDeletedEvent` received):

```sql
-- 1. Verify primary DB anonymization
SELECT Id, Email, IdentityReference, IsDeleted, AnonymizedAtUtc
FROM [ums_identity].[UserAccounts]
WHERE Id = '<user_guid>';
-- Expected: Email LIKE 'gdpr_del_%@anonymized.invalid', IsDeleted = 1, AnonymizedAtUtc IS NOT NULL

-- 2. Verify no PII in audit records
SELECT AffectedEntityId, WhatChanged
FROM [ums_audit].[AuditRecords]
WHERE AffectedEntityId = '<user_guid>'
ORDER BY OccurredAtUtc DESC;
-- Review: WhatChanged should NOT contain original email after anonymization timestamp
```

---

## 4. Backup Purge Process

For **standard erasure requests** (user self-service or admin via API):

- The 30-day backup retention naturally purges old backups.
- No manual intervention is required for standard requests.
- The DPA obligation is met when the last backup containing the PII ages out.

For **urgent erasure requests** (legal order, DPA investigation):

1. Identify all backup sets taken before `AnonymizedAtUtc`.
2. Delete those specific backup sets from:
   - Azure Blob Storage backup container
   - On-premises tape/disk backup system
3. Document in the erasure log (see section 5).
4. Respond to the requestor within **72 hours** of the request.

### Automation (recommended)

Configure Azure Backup or SQL Server Maintenance Plans to auto-expire backups at 30 days:

```sql
-- SQL Server Agent job: expire backups older than 30 days
EXEC msdb.dbo.sp_delete_backuphistory @oldest_date = DATEADD(day, -30, GETDATE());
```

---

## 5. Erasure Log (required by GDPR Art. 30)

Maintain a record of all erasure operations in a dedicated audit table or external system:

| Field | Value |
|---|---|
| `request_id` | UUID of the erasure request |
| `user_id` | Internal GUID (not email — already anonymized) |
| `requested_at` | Timestamp of DELETE API call |
| `anonymized_at` | `AnonymizedAtUtc` from UserAccountRecord |
| `backups_purged_by` | Date when last relevant backup expires |
| `confirmed_by` | DPO sign-off |

---

## 6. Responsibilities

| Role | Responsibility |
|---|---|
| **DPO** | Sign off on erasure log; respond to Data Subject Access Requests |
| **DBA** | Configure backup expiry; execute urgent backup deletions |
| **Engineering** | Maintain `AnonymizedAtUtc` field; verify no PII in logs (HARDENING-04) |
| **DevOps** | Enforce backup retention rules in CI/CD and IaC scripts |

---

## 7. Known Gaps & Remediation Plan

| Gap | Risk | Mitigation |
|---|---|---|
| Log files may contain PII if logged before anonymization | Medium | HARDENING-04 (PiiSanitizerEnricher) prevents email in Serilog output. Verify EF Core slow-query logs are at Warning level. |
| Dev/test DBs may be restored from backups with PII | High | Policy: dev/test restores must use anonymized dumps. Add `AnonymizeDevDb.sql` script to CI pipeline. |
| Audit records contain `WhatChanged` which may have old email | Low | `WhatChanged` stores field names not values in current implementation. Verify on schema change. |
| Third-party analytics / data warehouse | Medium | If UMS data is synced to a warehouse, apply the same retention + anonymization policy there. |
