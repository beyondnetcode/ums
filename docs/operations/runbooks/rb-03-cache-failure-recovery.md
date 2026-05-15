# RB-03: Cache Failure Recovery

| Field | Value |
|-------|-------|
| **Runbook ID** | RB-03 |
| **Scope** | Redis cache failure — UMS auth graph and session cache |
| **Owner** | Platform / On-Call Team |
| **Last Review** | 2026-05-15 |

---

## 1. Failure Modes

| Mode | Symptom | Likely Cause |
|------|---------|-------------|
| Redis pod crash | Auth latency spikes, cache miss 100% | OOM kill, pod eviction |
| Redis network partition | Connection timeouts in app logs | Network policy change, k8s DNS |
| Redis data corruption | Unexpected auth errors, stale graph | RDB restore from bad snapshot |
| Sentinel failover in progress | Intermittent failures, 5-30 s gap | Primary failover election |

---

## 2. Diagnosis

```bash
# Check Redis pod status
kubectl get pods -n ums-prod -l app=redis

# Check Redis logs
kubectl logs -n ums-prod deploy/redis-master --tail=100

# Test connectivity from app pod
kubectl exec -n ums-prod deploy/ums-auth-api -- redis-cli -h redis-master -p 6379 PING

# Check Redis memory usage
kubectl exec -n ums-prod deploy/redis-master -- redis-cli INFO memory | grep used_memory_human

# Check connected clients
kubectl exec -n ums-prod deploy/redis-master -- redis-cli CLIENT LIST | wc -l

# Check cache hit ratio
kubectl exec -n ums-prod deploy/redis-master -- redis-cli INFO stats \
  | grep -E "keyspace_hits|keyspace_misses"
```

---

## 3. Recovery Steps

### 3a. Redis Pod Crashed — Restart

```bash
# Delete pod to force restart (Deployment will recreate)
kubectl delete pod -n ums-prod -l app=redis-master

# Wait for pod to be ready
kubectl wait pod -n ums-prod -l app=redis-master --for=condition=Ready --timeout=120s

# Verify replication (if Sentinel / cluster)
kubectl exec -n ums-prod deploy/redis-master -- redis-cli INFO replication
```

### 3b. Application Failover to DB (Cache-Aside Degraded Mode)

The UMS auth graph repository falls back to DB queries when Redis is unavailable (circuit breaker pattern).

```bash
# Enable degraded-mode flag to skip cache reads
kubectl patch configmap ums-feature-flags -n ums-prod \
  --type merge \
  -p '{"data":{"REDIS_CACHE_ENABLED":"false"}}'

# Restart pods to pick up flag
kubectl rollout restart deployment/ums-auth-api -n ums-prod

# Monitor DB connection pool — expect higher utilization
kubectl exec -n ums-prod deploy/ums-db-pod -- psql -c \
  "SELECT count(*) FROM pg_stat_activity WHERE datname='ums_prod';"
```

> **Note:** DB fallback is safe but increases DB load ~3x. Notify DBA if Redis outage exceeds 30 minutes.

### 3c. Flush Corrupted Cache (partial or full)

```bash
# Flush only auth graph keys (pattern match)
kubectl exec -n ums-prod deploy/redis-master -- redis-cli \
  --scan --pattern "auth:graph:*" | xargs redis-cli DEL

# Full flush (last resort — all cache cold start)
kubectl exec -n ums-prod deploy/redis-master -- redis-cli FLUSHDB ASYNC
```

---

## 4. Re-enable Cache After Recovery

```bash
# Re-enable Redis cache
kubectl patch configmap ums-feature-flags -n ums-prod \
  --type merge \
  -p '{"data":{"REDIS_CACHE_ENABLED":"true"}}'

kubectl rollout restart deployment/ums-auth-api -n ums-prod

# Verify hit ratio recovering (should reach > 80% within 10 min)
watch -n 30 "kubectl exec -n ums-prod deploy/redis-master -- redis-cli INFO stats \
  | grep -E 'keyspace_hits|keyspace_misses'"
```

---

## 5. Warm-up After Full Flush

For critical tenants, pre-warm the auth graph cache:

```bash
# Trigger projection warm-up via admin API
curl -X POST https://ums.internal/admin/cache/warm-up \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"tenants": ["tenant-1", "tenant-2"]}'
```

---

## 6. Verification Checklist

- [ ] Redis pod Running and Ready
- [ ] `PING` returns `PONG` from app pod
- [ ] Auth graph cache hit ratio > 80% within 10 min
- [ ] No Redis connection errors in auth API logs
- [ ] `REDIS_CACHE_ENABLED` flag set to `true`
- [ ] Grafana Redis dashboard shows normal memory and connection metrics

---

**[Back to Operations Index](../index.md)** | **[Back to Master Index](../../MASTER_INDEX.md)**
