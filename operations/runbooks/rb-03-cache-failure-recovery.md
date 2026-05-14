# Runbook RB-03: Cache Failure Recovery (Redis)

**Component:** Redis Cache (Authorization Graph store + Config cache)  
**Impact of failure:** Auth graph compilation falls back to SQL Server — latency increases from ~5ms to ~50–200ms. No data loss. Service remains functional with degraded performance.  
**Backed by:** [ADR-0014 — Distributed Caching Strategy](../../architecture/adrs/0014-distributed-caching-strategy-redis.md) · [TE-01 — Build Authorization Graph](../../architecture/blueprints/technical-enablers/te-01-build-authorization-graph.md)

---

## 1. Failure Modes

| Mode | Symptom | Grafana Signal |
| :--- | :--- | :--- |
| Redis pod OOMKilled | Cache miss rate 100%, latency spike | `redis_memory_used_bytes` near limit |
| Redis pod crash/restart | Connection refused errors in app logs | Pod restarts counter |
| Network partition | Intermittent cache misses | `redis_connected_clients` drops |
| Key eviction storm | Gradual latency increase | `evicted_keys` counter rising |
| Corrupted data | Auth graph returning incorrect permissions | Auth validation errors in app
## 2. Immediate Triage

```bash
# 1. Check Redis pod status
kubectl get pods -n ums -l app=redis

# 2. Check Redis connectivity
kubectl exec -n ums deploy/ums-api -- redis-cli -h redis PING
# Expected: PONG

# 3. Check memory usage
kubectl exec -n ums -l app=redis -- redis-cli INFO memory | \
  grep -E "used_memory_human|maxmemory_human|mem_fragmentation_ratio"

# 4. Check hit/miss rates
kubectl exec -n ums -l app=redis -- redis-cli INFO stats | \
  grep -E "keyspace_hits|keyspace_misses|evicted_keys"

# 5. Check connected clients
kubectl exec -n ums -l app=redis -- redis-cli INFO clients | grep connected_clients
```

---

## 3. Recovery Procedures

### 3.1 Redis Pod OOMKilled

```bash
# Confirm cause
kubectl describe pod -n ums -l app=redis | grep -A5 "OOMKilled\|Reason"

# Restart Redis pod (data loss — see warming procedure below)
kubectl delete pod -n ums -l app=redis

# Monitor restart
kubectl rollout status statefulset/redis -n ums

# Confirm healthy
kubectl exec -n ums -l app=redis -- redis-cli PING
```

**After restart — cache warming:** Redis starts empty. The fallback path (SQL Server) handles requests automatically. Auth graph cache repopulates organically as users authenticate. No manual intervention needed unless you want to pre-warm for high-traffic contexts.

**Optional pre-warm for critical tenants:**

```bash
# Trigger pre-warm via Admin API (invalidates and recompiles graphs for specified org)
curl -X POST http://ums-api:8080/admin/cache/warm \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"org_ids": ["<org-id-1>", "<org-id-2>"]}'
```

---

### 3.2 Redis Crash / Connection Refused

```bash
# Check if pod is running but not accepting connections
kubectl logs -n ums -l app=redis --tail=50

# Possible: Redis is in AOF recovery mode (replaying append-only log)
# Wait for recovery to complete before restarting:
kubectl exec -n ums -l app=redis -- redis-cli DEBUG SLEEP 0  # Noop — forces a response if alive

# If unresponsive for > 2 min, force restart:
kubectl rollout restart statefulset/redis -n ums
kubectl rollout status statefulset/redis -n ums
```

---

### 3.3 Key Eviction Storm

Eviction means Redis is running out of memory and dropping keys — the `maxmemory-policy` (`allkeys-lru` by default) is evicting auth graphs faster than they are compiled.

```bash
# Check current maxmemory setting
kubectl exec -n ums -l app=redis -- redis-cli CONFIG GET maxmemory

# Check eviction policy
kubectl exec -n ums -l app=redis -- redis-cli CONFIG GET maxmemory-policy

# Temporary increase maxmemory (does not persist across restarts)
kubectl exec -n ums -l app=redis -- redis-cli CONFIG SET maxmemory 2gb

# Identify the largestá keys consuming memory
kubectl exec -n ums -l app=redis -- redis-cli --bigkeys
```

**Permanent fix:** Update the Redis StatefulSet memory limit and `maxmemory` in the ConfigMap. Submit as a change requestá.

---

### 3.4 Corrupted Cache Data

If auth graphs are returning incorrect permissions (not just slow — actually wrong), the cache may contain stale or corrupted data.

```bash
# Flush only auth graph keys (targeted, not full flush)
kubectl exec -n ums -l app=redis -- \
  redis-cli --scan --pattern "auth:graph:*" | \
  xargs redis-cli DEL

# Flush config cache keys
kubectl exec -n ums -l app=redis -- \
  redis-cli --scan --pattern "cfg:sys:*" | \
  xargs redis-cli DEL

# Flush JWKS cache
kubectl exec -n ums -l app=redis -- \
  redis-cli --scan --pattern "jwks:*" | \
  xargs redis-cli DEL

# Verify flush
kubectl exec -n ums -l app=redis -- redis-cli DBSIZE
```

> **Do NOT use `FLUSHALL` unless instructed by the IC.** It drops all keys including session tokens, causing active user sessions to be invalidated.

---

## 4. Cache Key Reference

| Pattern | Content | TTL | Eviction Impact |
| :--- | :--- | :--- | :--- |
| `auth:graph:{user_id}` | Compiled permission graph (JSON) | 1 hour | Fallback to SQL — latency only |
| `cfg:sys:{system_id}:{ctx_hash}` | Effective system configuration | 5 min | Fallback to config resolution — latency only |
| `jwks:{idp_id}` | IdP public keys for token validation | 15 min | Fallback to IdP endpoint — latency + IdP dependency |
| `session:{token_id}` | Active session metadata | Per-session TTL | Session invalidated — user must re-authenticate
## 5. Recovery Verification

```
[ ] redis-cli PING returns PONG
[ ] Cache hit rate recovering (Grafana: Redis Hit Rate panel)
[ ] Auth p99 returning to < 50ms
[ ] No auth:graph:* key returning incorrect permission sets (spot check 2-3 users)
[ ] evicted_keys counter stable (not continuously rising)
```

---

## 6. Related Documents

- [RB-01 — Incident Response](./rb-01-incident-response.md)
- [TE-01 — Build Authorization Graph](../../architecture/blueprints/technical-enablers/te-01-build-authorization-graph.md)
- [ADR-0014 — Distributed Caching Strategy](../../architecture/adrs/0014-distributed-caching-strategy-redis.md)
- [ADR-0011 — Fault Tolerance & Resiliency Patterns](../../architecture/adrs/0011-fault-tolerance-resiliency-patterns.md)
