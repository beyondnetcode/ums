# Runbook RB-02: Rollback Procedure

**Applies to:** Application deployments · Database migrations · Configuration changes  
**Pre-condition:** A deployment has caused a regression detected in production or staging.  
**Goal:** Return the system to the last known stable state with minimal data loss.

---

## 1. Rollback Decision Criteria

Trigger a rollback when **any** of the following are true:

| Signal | Threshold | Check |
| :--- | :--- | :--- |
| Error rate | > 5% sustained for > 5 min | Grafana → Error Rate panel |
| Auth endpoint p99 | > 2000ms sustained | Grafana → Auth Latency panel |
| Health check failures | Any service returning non-200 | `kubectl get pods -n ums` |
| Data integrity alert | Any row-level security breach signal | Audit log alert |
| Critical bug confirmed | Bug affects core auth/authz flow | Manual decision by IC | **Do NOT rollback for:** transient spikes < 2 min · P3/P4 incidents with a known fix available in < 1 h · warnings without user impact.

---

## 2. Application Rollback (Kubernetes)

### 2.1 Check Current Rollout State

```bash
kubectl rollout history deployment/ums-api -n ums
kubectl rollout history deployment/ums-auth-engine -n ums
kubectl rollout history deployment/ums-outbox-relay -n ums
```

This lists revision numbers and change-cause annotations. Identify the last stable revision.

### 2.2 Execute Rollback

```bash
# Roll back to previous revision (most common case)
kubectl rollout undo deployment/ums-api -n ums

# Roll back to a specific revision
kubectl rollout undo deployment/ums-api -n ums --to-revision=<N>

# Monitor rollback progress
kubectl rollout status deployment/ums-api -n ums --timeout=5m
```

### 2.3 Verify Rollback

```bash
# Confirm running image
kubectl get deployment ums-api -n ums -o jsonpath='{.spec.template.spec.containers[0].image}'

# Run health check
curl -s http://ums-api:8080/health | jq .
```

---

## 3. Database Migration Rollback

> **Critical:** Not all migrations are reversible. Assess before executing.

### 3.1 Assess Migration Reversibility

| Migration Type | Reversible? | Procedure |
| :--- | :--- | :--- |
| Add column (nullable) | Yes | Drop column |
| Add index | Yes | Drop index |
| Create table | Yes | Drop table |
| Drop column | **No** | Restore from backup |
| Rename column | **No** (data risk) | Restore from backup |
| Data transformation | Depends | Restore from backup if no inverse script |
| RLS policy change | Yes | Re-apply previous policy | ### 3.2 Reversible Migration — Down Script

```bash
# Run migration down script (if framework supports it)
dotnet ef migrations script <CurrentMigration> <PreviousMigration> \
  --output rollback.sql \
  --idempotent

# Review the script before applying
cat rollback.sql

# Apply after review
/opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -i rollback.sql -v
```

### 3.3 Non-Reversible Migration — Point-in-Time Restore

```bash
# 1. Identify last clean backup before the problematic migration
# Check automated backup schedule — daily at 02:00 UTC, 6-hour transaction log backups

# 2. Restore database to point-in-time (SQL Server)
RESTORE DATABASE UMS_DB
  FROM DISK = '/backups/UMS_DB_<date>.bak'
  WITH STOPAT = '<YYYY-MM-DDTHH:MM:SS>',
       REPLACE, NORECOVERY;

RESTORE LOG UMS_DB
  FROM DISK = '/backups/UMS_DB_<date>_log.bak'
  WITH STOPAT = '<YYYY-MM-DDTHH:MM:SS>',
       RECOVERY;
```

> After a point-in-time restore, all events written after the restore point are lost. Notify stakeholders and document in the incident timeline.

### 3.4 RLS Policy Rollback

```bash
# Disable current policy
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -Q "ALTER SECURITY POLICY org_rls_policy WITH (STATE = OFF);"

# Re-apply previous policy from version control
git show <previous-commit>:src/migrations/rls/policy_v<N>.sql | \
  kubectl exec -i -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD
```

---

## 4. Configuration Rollback

### 4.1 Application Configuration

```bash
# If using Kubernetes ConfigMap
kubectl rollout undo configmap/ums-config -n ums  # Not supported natively
# Alternative: apply the previous ConfigMap from git
git show HEAD~1:k8s/configmap.yaml | kubectl apply -f -

# Restart deployments to pick up config change
kubectl rollout restart deployment/ums-api -n ums
kubectl rollout status deployment/ums-api -n ums
```

### 4.2 Kong Gateway Configuration

```bash
# List recent Kong config changes via Admin API
curl -s http://kong:8001/routes | jq '.data[] | {id, name, updated_at}'

# Roll back a specific plugin configuration
curl -X PATCH http://kong:8001/plugins/<plugin-id> \
  -H "Content-Type: application/json" \
  -d '{"config": <previous-config-json>}'
```

### 4.3 Feature Flag Rollback

```bash
# Disable a feature flag immediately (kills the new behavior)
curl -X PATCH http://ums-api:8080/admin/feature-flags/<flag-name> \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"enabled": false}'
```

---

## 5. Projection Rollback

If a CQRS projection rebuild was in progress at time of incident:

```bash
# Revert the query router to the previous projection store
curl -X POST http://ums-api:8080/admin/projections/auth-graph/rollback \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# Verify the rollback took effect
curl -s http://ums-api:8080/admin/projections/auth-graph/status | jq .
```

See [TE-06 — CQRS Projection Rebuild](../../architecture/blueprints/technical-enablers/te-06-cqrs-projection-rebuild.md) for the 24-hour rollback window.

---

## 6. Post-Rollback Verification

```
[ ] All services return 200 on /health
[ ] Error rate back to baseline (< 0.5%)
[ ] Auth p99 < 100ms
[ ] No pending outbox events older than 5 min
[ ] Cache hit rate > 80%
[ ] Confirm with a sample end-to-end auth flow (manual smoke test)
[ ] Document rollback in incident timeline with revision numbers and timestaamps
```

---

## 7. Related Documents

- [RB-01 — Incident Response](./rb-01-incident-response.md)
- [TE-06 — CQRS Projection Rebuild](../../architecture/blueprints/technical-enablers/te-06-cqrs-projection-rebuild.md)
- [ADR-0013 — Cloud Infrastructure & DR](../../architecture/adrs/0013-cloud-infrastructure-topology-dr.md)
- [ADR-0017 — Feature Flagging Strategy](../../architecture/adrs/0017-feature-flagging-strategy.md)
