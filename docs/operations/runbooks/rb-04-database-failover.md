# RB-04: Database Failover

| Field | Value |
|-------|-------|
| **Runbook ID** | RB-04 |
| **Scope** | SQL Server primary failover — UMS databases |
| **Owner** | DBA / Platform On-Call |
| **Last Review** | 2026-05-15 |

---

## 1. Failover Scenarios

| Scenario | RTO Target | RPO Target | Action |
|----------|-----------|-----------|--------|
| Primary pod crash (k8s) | < 60 s | 0 (synchronous replica) | Automatic via Patroni / operator |
| AZ-level failure | < 5 min | < 30 s | Manual promotion of standby |
| Region-level DR | < 30 min | < 5 min | DR runbook activation |
| Corruption / bad migration | Variable | Point-in-time | PITR restore |

---

## 2. Automatic Failover

The UMS database cluster uses the SQL Server high-availability topology configured for the environment. Automatic failover requires no manual intervention.

```bash
# Verify current primary
kubectl get cluster ums-db-cluster -n ums-prod -o jsonpath='{.status.currentPrimary}'

# Check cluster health
kubectl describe cluster ums-db-cluster -n ums-prod | grep -A 20 "Status:"

# Check replication lag on replicas
kubectl exec -n ums-prod $(kubectl get pod -n ums-prod -l role=replica -o name | head -1) \
  -- psql -U ums_app -d ums_prod \
  -c "SELECT now() - pg_last_xact_replay_timestamp() AS replication_lag;"
```

---

## 3. Manual Promotion (If Automatic Fails)

```bash
# Step 1: Identify current standby pods
kubectl get pods -n ums-prod -l cnpg.io/cluster=ums-db-cluster

# Step 2: Promote standby to primary
kubectl cnpg promote ums-db-cluster -n ums-prod

# Step 3: Verify promotion
kubectl get cluster ums-db-cluster -n ums-prod -o jsonpath='{.status.currentPrimary}'

# Step 4: Update application connection string if needed
kubectl get secret ums-db-cluster-app -n ums-prod -o yaml | grep host
```

---

## 4. Reconnect Applications After Failover

```bash
# Restart auth API pods to force connection pool reset
kubectl rollout restart deployment/ums-auth-api -n ums-prod
kubectl rollout restart deployment/ums-outbox-relay -n ums-prod

# Verify connections established
kubectl logs -n ums-prod deploy/ums-auth-api --tail=50 | grep -i "database\|connection\|ready"

# Check active DB connections
kubectl exec -n ums-prod ums-db-cluster-1 -- psql -U ums_app -d ums_prod \
  -c "SELECT count(*), state FROM pg_stat_activity WHERE datname='ums_prod' GROUP BY state;"
```

---

## 5. Outbox Relay Recovery After Failover

During failover, the outbox relay worker may have stalled. Check and recover:

```bash
# Check outbox PENDING backlog
kubectl exec -n ums-prod ums-db-cluster-1 -- psql -U ums_app -d ums_prod -c \
  "SELECT status, count(*), min(created_at) AS oldest FROM outbox_events GROUP BY status;"

# If PENDING count is high, verify relay is processing
kubectl logs -n ums-prod deploy/ums-outbox-relay --tail=100 | grep -i "relay\|publish\|error"

# Restart relay if stuck
kubectl rollout restart deployment/ums-outbox-relay -n ums-prod
```

---

## 6. Point-in-Time Recovery (PITR)

> **Last resort.** Only if primary and all replicas are lost or data is corrupted.

```bash
# List available WAL archives / snapshots
kubectl get secret ums-db-cluster-app -n ums-prod

# Restore to point in time (UTC timestamp)
kubectl apply -f - <<EOF
apiVersion: sqlserver.failover/v1
kind: Cluster
metadata:
  name: ums-db-cluster-restored
  namespace: ums-prod
spec:
  bootstrap:
    recovery:
      source: ums-db-cluster
      recoveryTarget:
        targetTime: "2026-05-15T10:30:00Z"
  externalClusters:
    - name: ums-db-cluster
      barmanObjectStore:
        destinationPath: "s3://ums-backup/ums-db-cluster"
        s3Credentials: ...
EOF
```

After PITR completes:
1. Validate data consistency (row counts, last transaction timestamp)
2. Run `npx typeorm migration:show` to verify schema state
3. Redirect applications to restored cluster
4. Replay any lost events from Dapr / message bus DLQ

---

## 7. Multi-Tenant RLS Verification Post-Failover

```sql
-- Verify RLS policies survived failover
SELECT schemaname, tablename, policyname, cmd, qual
FROM pg_policies
WHERE schemaname = 'ums'
ORDER BY tablename, policyname;

-- Test RLS enforcement
EXEC sp_set_session_context 'TenantId', 'tenant-test-1';
SELECT count(*) FROM ums.users; -- must return only tenant-test-1 rows
```

---

## 8. Verification Checklist

- [ ] New primary confirmed via `kubectl get cluster`
- [ ] Replication lag on new replicas < 10 s within 5 minutes
- [ ] Auth API health endpoint returns 200
- [ ] Login smoke test passes
- [ ] Outbox PENDING backlog draining (< 100 events after 5 min)
- [ ] RLS policies intact on all tenant tables
- [ ] Grafana DB dashboard: connection pool normal, no error spikes

---

**[Back to Operations Index](../index.md)** | **[Back to Master Index](../../MASTER_INDEX.md)**
