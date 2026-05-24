# Dev DB Anonymization Pipeline

**Owner:** Engineering  
**Applies to:** Any developer or CI job that restores a production-like backup for local development or test environments  
**Related:** [GDPR Backup Retention Policy](./gdpr-backup-retention-policy.md)

---

## Purpose

The production UMS database contains real PII (emails, phone numbers, IdP references).  
GDPR Art. 25 (data protection by design) requires that PII be removed or masked before any copy of the database is handed to a developer.

This pipeline:
1. Restores a backup into an isolated database with a dev-specific name.
2. Replaces all PII with deterministic, realistic-looking fake data.
3. Verifies no real email addresses remain.

---

## Quick Start

```bash
# Full pipeline — restore from backup + anonymize
./scripts/anonymize-dev-db.sh \
  --server  localhost \
  --user    sa \
  --password '<SA_PASSWORD>' \
  --source  ums \
  --target  ums_dev_$(date +%Y%m%d) \
  --backup-file /tmp/ums_backup.bak

# Skip the restore step (target DB already exists)
./scripts/anonymize-dev-db.sh \
  --server  localhost \
  --user    sa \
  --password '<SA_PASSWORD>' \
  --target  ums_dev_20260523 \
  --skip-restore
```

### Environment variables (CI-friendly)

```bash
export UMS_SQL_SERVER=localhost
export UMS_SQL_USER=sa
export UMS_SQL_PASSWORD=<secret>
export UMS_SOURCE_DB=ums
export UMS_TARGET_DB=ums_dev_20260523
./scripts/anonymize-dev-db.sh
```

---

## What Gets Anonymized

| Table | Column(s) | Replacement |
|---|---|---|
| `ums_identity.UserAccounts` | `Email` | `dev_{sha256_12}@dev.invalid` |
| `ums_identity.UserAccounts` | `PhoneNumber` | `+1555{checksum_7digits}` |
| `ums_identity.UserAccounts` | `IdentityReference` | `dev-idp|{sha256_16}` |
| `ums_identity.UserAccounts` | `DisplayName` | `DevUser {row_number}` |
| `ums_identity.Tenants` | `ContactEmail` | `dev-tenant-{guid}@dev.invalid` |
| `ums_configuration.IdpConfigurations` | `MetadataJson` | `{"authority":"https://login.dev.invalid/common","masked":true}` |
| `ums_configuration.IdpConfigurations` | `SecretVaultPath` | `kv/dev/stub` |
| `ums_audit.AuditRecords` | `WhatChanged` | Email patterns replaced with `<email-masked>` |

Already-deleted rows (`IsDeleted = 1`) retain their GDPR anonymization (`gdpr_del_*@anonymized.invalid`) and are not touched.

---

## Safety Guards

1. **Name safelist**: The script refuses to run if `--target` matches `ums`, `ums_prod`, `ums_staging`, or `ums_preprod`.  
2. **Transaction**: All UPDATEs run inside a single transaction — failure leaves no partial state.  
3. **Post-run verification**: The shell script counts rows where `Email` does not match the dev/anonymized patterns and exits with code 3 if any are found.  
4. **Audit log**: Every run inserts a row into `ums_audit.AnonymizationRuns` (table is created lazily if absent).

---

## Using the Anonymized DB in Development

Update `appsettings.Development.json` or your local `secrets.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ums_dev_20260523;User Id=sa;Password=<pwd>;TrustServerCertificate=True"
  }
}
```

---

## CI Integration

Add a step to your environment-provisioning job:

```yaml
- name: Anonymize dev DB
  env:
    UMS_SQL_SERVER: ${{ vars.DEV_SQL_SERVER }}
    UMS_SQL_USER:   sa
    UMS_SQL_PASSWORD: ${{ secrets.DEV_SA_PASSWORD }}
    UMS_TARGET_DB:  ums_dev_${{ github.run_id }}
  run: |
    ./scripts/anonymize-dev-db.sh \
      --backup-file ${{ steps.restore.outputs.bak_path }}
```

---

## Known Limitations

| Limitation | Mitigation |
|---|---|
| `REGEXP_REPLACE` in audit table cleanup requires SQL Server 2022 R2+ | On older versions the audit `WhatChanged` masking step is skipped (field values are field names in current schema — safe) |
| `DBCC CLONEDATABASE` copies schema + statistics only, not data | Use `--backup-file` for a full data copy |
| The script does not anonymize data warehouse replicas | Apply separately if UMS data is synced externally |
