# Runbook RB-04: Database Failover (SQL Server 2022)

**Component:** SQL Server 2022 — Primary data store for UMS  
**Impact of failure:** Complete service outage. No read or write operations succeed. Auth, authorization, config, and audit paths all depend on the database.  
**Recovery objective:** RTO < 30 min (P1) · RPO < 5 min (with transaction log shipping)  
**Backed by:** [ADR-0013 — Cloud Infrastructure & DR](../../architecture/adrs/0013-cloud-infrastructure-topology-dr.md) · [ADR-0041 — Authoritative Database Engine](../../architecture/adrs/0041-authoritative-database-engine-strategy.md)

---

## 1. Failure Detection

```bash
# 1. Check SQL Server pod
kubectl get pods -n ums -l app=sqlserver

# 2. Test connectivity from app pod
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -Q "SELECT GETUTCDATE() AS now, @@VERSION AS version" -t 5

# 3. Check active sessions (if reachable)
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -Q "SELECT COUNT(*) AS sessions FROM sys.dm_exec_sessions WHERE is_user_process = 1"

# 4. Check disk pressure (common cause)
kubectl exec -n ums -l app=sqlserver -- df -h /var/opt/mssql/
```

---

## 2. Common Failure Modes & Immediate Actions

### 2.1 Pod Crash / OOMKilled

```bash
# Confirm
kubectl describe pod -n ums -l app=sqlserver | grep -A5 "OOMKilled\|Reason\|Exit Code"

# Check recent logs before crash
kubectl logs -n ums -l app=sqlserver --previous --tail=100

# Restart
kubectl rollout restart statefulset/sqlserver -n ums
kubectl rollout status statefulset/sqlserver -n ums

# Verify SQL Server started
kubectl exec -n ums -l app=sqlserver -- \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD \
  -Q "SELECT name, state_desc FROM sys.databases"
```

### 2.2 Disk Full

```bash
# Check disk usage
kubectl exec -n ums -l app=sqlserver -- df -h /var/opt/mssql/

# Identify largestá files
kubectl exec -n ums -l app=sqlserver -- \
  du -sh /var/opt/mssql/data/* | sort -rh | head -20

# Emergency: Shrink transaction log (only if log is not needed for restore point)
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD -Q "
  USE UMS_DB;
  CHECKPOINT;
  DBCC SHRINKFILE (UMS_DB_log, 512);  -- shrink to 512 MB
  "

# Long-term: Expand PVC (requires cluster support)
kubectl patch pvc sqlserver-data -n ums -p '{"spec":{"resources":{"requests":{"storage":"50Gi"}}}}'
```

> **Warning:** `SHRINKFILE` on transaction log is a last resort during disk emergency only. It invalidates the log backup chain — take a full backup immediately after.

### 2.3 Connection Pool Exhaustion

```bash
# Check max connections config
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -Q "SELECT value_in_use FROM sys.configurations WHERE name = 'max connections'"

# Find blocking queries
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -Q "
  SELECT blocking_session_id, session_id, wait_type, wait_time, status, command,
         SUBSTRING(st.text, (r.statement_start_offset/2)+1, 200) AS query_text
  FROM sys.dm_exec_requests r
  CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) st
  WHERE blocking_session_id > 0
  ORDER BY wait_time DESC
  "

# Kill a blocking session (use carefully — confirm with DBA or IC)
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -Q "KILL <session_id>"
```

---

## 3. Failover to Replica (If Configured)

If a hot standby replica is available (SQL Server Always On or log shipping target):

```bash
# Step 1: Confirm replica is caught up
# On replica:
kubectl exec -n ums -l app=sqlserver-replica -- \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD \
  -Q "SELECT secondary_lag_seconds FROM sys.dm_hadr_database_replica_states"

# Step 2: Initiate manual failover (Always On AG)
kubectl exec -n ums -l app=sqlserver -- \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD \
  -Q "ALTER AVAILABILITY GROUP UMS_AG FAILOVER"

# Step 3: Update connection string in Kubernetes secret
kubectl patch secret ums-db-secret -n ums \
  -p '{"stringData":{"connection-string":"Server=sqlserver-replica;Database=UMS_DB;..."}}'

# Step 4: Restart app to pick up new connection string
kubectl rollout restart deployment/ums-api -n ums
kubectl rollout restart deployment/ums-auth-engine -n ums
kubectl rollout restart deployment/ums-outbox-relay -n ums
```

---

## 4. Point-in-Time Restore (Last Resort)

Use when the primary is unrecoverable and no live replica is available.

```bash
# Step 1: List available backups
ls -lh /backups/ | grep UMS_DB

# Step 2: Restore full backup
kubectl exec -n ums -l app=sqlserver -- \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "
  RESTORE DATABASE UMS_DB
    FROM DISK = '/backups/UMS_DB_full_<date>.bak'
    WITH NORECOVERY, REPLACE;
  "

# Step 3: Apply differential backup (if available)
kubectl exec -n ums -l app=sqlserver -- \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "
  RESTORE DATABASE UMS_DB
    FROM DISK = '/backups/UMS_DB_diff_<date>.bak'
    WITH NORECOVERY;
  "

# Step 4: Apply transaction log backups up to desired point
kubectl exec -n ums -l app=sqlserver -- \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "
  RESTORE LOG UMS_DB
    FROM DISK = '/backups/UMS_DB_log_<date>.bak'
    WITH STOPAT = '<YYYY-MM-DDTHH:MM:SS>', RECOVERY;
  "

# Step 5: Verify database is online
kubectl exec -n ums -l app=sqlserver -- \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD \
  -Q "SELECT name, state_desc FROM sys.databases WHERE name = 'UMS_DB'"
```

---

## 5. Post-Recovery Steps

```bash
# 1. Re-enable Row-Level Security policies (verify they survived restore)
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -Q "SELECT name, is_enabled FROM sys.security_policies"

# 2. Verify RLS is active
# Expected: all org_rls_* policies show is_enabled = 1

# 3. Flush Redis cache (may contain stale data if restore rolled back writes)
kubectl exec -n ums -l app=redis -- \
  redis-cli --scan --pattern "auth:graph:*" | xargs redis-cli DEL
kubectl exec -n ums -l app=redis -- \
  redis-cli --scan --pattern "cfg:sys:*" | xargs redis-cli DEL

# 4. Check outbox relay — events written after the restore point are lost
# Communicate data loss scope to stakeholders
kubectl logs -n ums -l app=ums-outbox-relay --tail=50

# 5. Run application health checks
curl -s http://ums-api:8080/health | jq .
```

---

## 6. Recovery Verification Checklist

```
[ ] SQL Server pod Running and Ready
[ ] Test query succeeds: SELECT 1
[ ] All RLS policies enabled (sys.security_policies)
[ ] Application health endpoints return 200
[ ] Auth flow works end-to-end (manual smoke test)
[ ] Redis cache flushed and repopulating
[ ] Outbox relay processing PENDING events
[ ] Data loss scope documented and communicated to stakeholders
[ ] Full backup initiated immediately after recovery
```

---

## 7. Backup Schedule Reference

| Backup Type | Frequency | Retention | Location |
| :--- | :--- | :--- | :--- |
| Full backup | Daily at 02:00 UTC | 30 days | `/backups/full/` |
| Differential backup | Every 6 hours | 7 days | `/backups/diff/` |
| Transaction log | Every 15 min | 48 hours | `/backups/log/`
## 8. Related Documents

- [RB-01 — Incident Response](./rb-01-incident-response.md)
- [RB-03 — Cache Failure Recovery](./rb-03-cache-failure-recovery.md)
- [TE-03 — Enforce Organization RLS](../../architecture/blueprints/technical-enablers/te-03-enforce-organization-rls-sql-server.md)
- [ADR-0013 — Cloud Infrastructure & DR](../../architecture/adrs/0013-cloud-infrastructure-topology-dr.md)
- [ADR-0041 — Authoritative Database Engine](../../architecture/adrs/0041-authoritative-database-engine-strategy.md)
- [ADR-0044 — Configurable Security Persistence (RLS)](../../architecture/adrs/0044-delegated-admin-and-approvals.md)
