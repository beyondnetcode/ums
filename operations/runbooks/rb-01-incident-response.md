# Runbook RB-01: Incident Response

**Severity levels:** P1 (Critical) · P2 (High) · P3 (Medium) · P4 (Low)  
**On-call dashboard:** Grafana → UMS Overview board  
**Escalation contact:** defined in the team's on-call rotation document

---

## 1. Severity Classification

| Severity | Definition | Response SLA | Example |
| :--- | :--- | :--- | :--- |
| **P1** | Complete service outage or data integrity risk | 15 min acknowledge / 1 h resolve | Auth endpoint down, RLS breach detected |
| **P2** | Partial degradation affecting multiple users | 30 min acknowledge / 4 h resolve | Redis unavailable, slow auth graph compile |
| **P3** | Single feature degraded, workaround exists | 2 h acknowledge / 24 h resolve | Notification delivery failing, one tenant impacted |
| **P4** | Non-urgent, cosmetic or low-impact | Next business day | Stale projection view, minor UI glitch |

---

## 2. First Responder Checklist

Execute in order. Do not skip steps.

```
[ ] 1. Acknowledge the alert in Grafana Alerting
[ ] 2. Determine severity (see table above)
[ ] 3. Open a War Room channel: #incident-YYYY-MM-DD
[ ] 4. Assign an Incident Commander (IC) — first responder owns this until escalated
[ ] 5. Post initial status to the channel: what is affected, since when, severity
[ ] 6. Start the incident timeline log (see Section 3)
[ ] 7. Run the triage checklist matching the symptom (see Section 4)
[ ] 8. If P1/P2: notify stakeholders within 15 min of acknowledge
[ ] 9. Apply fix or mitigation
[ ] 10. Confirm recovery via health checks (see Section 5)
[ ] 11. Post all-clear to channel
[ ] 12. Schedule post-mortem within 48 h (P1) or 5 days (P2)
```

---

## 3. Incident Timeline Log

Maintain a running log in the war room channel. Format:

```
[HH:MM UTC] <action or observation>

Example:
[14:03 UTC] Alert fired: auth endpoint p99 > 5000ms
[14:05 UTC] IC: @jane. Severity P2. War room: #incident-2026-05-14
[14:07 UTC] Redis cache miss rate at 100% — suspected Redis outage
[14:09 UTC] Confirmed: Redis pod OOMKilled. See RB-03 for cache recovery.
[14:22 UTC] Redis restarted. Cache warming in progress.
[14:31 UTC] p99 auth latency back to baseline (< 50ms). Monitoring 15 min.
[14:46 UTC] All-clear declared. Post-mortem scheduled 2026-05-16.
```

---

## 4. Symptom-Based Triage

### 4.1 Authentication Failures (401/403 spike)

```bash
# Check Kong gateway logs
kubectl logs -n ums -l app=kong --tail=100 | grep -E "401|403|error"

# Check UMS auth service
kubectl logs -n ums -l app=ums-api --tail=100 | grep -iE "auth|token|jwt|error"

# Verify IdP connectivity
curl -v https://<idp-domain>/.well-known/openid-configuration

# Check JWKS endpoint cache
redis-cli -h redis GET "jwks:<idp-id>"
```

**Common causes:** IdP JWKS rotation not propagated · Token TTL mismatch · Kong plugin misconfiguration

**Quick mitigation:** Flush JWKS cache: `redis-cli DEL "jwks:<idp-id>"` then force refresh.

---

### 4.2 Authorization Graph Latency (p99 > 500ms)

```bash
# Check Redis health
redis-cli PING
redis-cli INFO stats | grep -E "hit|miss|evicted"

# Check cache hit rate
redis-cli INFO stats | awk -F: '/keyspace_hits/{h=$2} /keyspace_misses/{m=$2} END{print "Hit rate: " h/(h+m)*100 "%"}'

# Check auth engine logs
kubectl logs -n ums -l app=ums-auth-engine --tail=200 | grep -iE "compile|timeout|error"
```

**If cache hit rate < 80%:** Likely Redis eviction or restart. See [RB-03 Cache Recovery](./rb-03-cache-failure-recovery.md).  
**If cache healthy but latency high:** Check SQL Server query plans on `auth_profiles` and `authorization_templates` tables.

---

### 4.3 Database Connectivity

```bash
# Check SQL Server pod
kubectl get pods -n ums -l app=sqlserver

# Test connection
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD -Q "SELECT 1"

# Check active connections
kubectl exec -n ums deploy/ums-api -- \
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD \
  -Q "SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1"
```

**If connection count maxed:** Check for connection pool exhaustion in the app. See [RB-04 Database Failover](./rb-04-database-failover.md).

---

### 4.4 Message Bus (RabbitMQ) Backlog

```bash
# Check RabbitMQ management API
curl -u guest:guest http://rabbitmq:15672/api/queues | \
  jq '.[] | {name:.name, messages:.messages, consumers:.consumers}'

# Check outbox relay logs
kubectl logs -n ums -l app=ums-outbox-relay --tail=100
```

**If outbox_events.PENDING > threshold:** Check RabbitMQ connectivity. If broker is down, events are safe in SQL — relay will catch up on recovery.

---

### 4.5 Dapr Sidecar Issues

```bash
# Check Dapr sidecar health
kubectl exec -n ums <pod-name> -c daprd -- \
  curl -s http://localhost:3500/v1.0/healthz

# Check Dapr logs
kubectl logs -n ums <pod-name> -c daprd --tail=100

# List active Dapr subscriptions
kubectl get subscriptions -n ums
```

---

## 5. Recovery Verification Checklist

After applying any fix, confirm all of the following before declaring all-clear:

```
[ ] GET /health returns 200 on all UMS services
[ ] Auth endpoint p99 < 100ms (Grafana: UMS Auth Latency panel)
[ ] Redis cache hit rate > 80% (Grafana: Cache panel)
[ ] outbox_events PENDING count trending down (Grafana: Outbox panel)
[ ] RabbitMQ queue depths returning to baseline
[ ] No new error spikes in last 10 min (Grafana: Error Rate panel)
[ ] SQL Server connections below 80% of pool limit
```

---

## 6. Post-Mortem Template

File post-mortems in `/operations/post-mortems/YYYY-MM-DD-<incident-slug>.md`:

```markdown
## Incident: <title>
**Date:** YYYY-MM-DD  
**Duration:** Xh Ym  
**Severity:** P1/P2/P3  
**IC:** @name  

### Timeline
[paste from war room log]

### Root Cause
[single precise sentence]

### Contributing Factors
- 

### What Went Well
- 

### Action Items
| Action | Owner | Due |
|--------|-------|-----|
|        |       |     |
```

---

## 7. Related Runbooks & Documents

- [RB-02 — Rollback Procedure](./rb-02-rollback-procedure.md)
- [RB-03 — Cache Failure Recovery](./rb-03-cache-failure-recovery.md)
- [RB-04 — Database Failover](./rb-04-database-failover.md)
- [Observability Strategy](../../architecture/artifacts/observability-strategy.md)
- [ADR-0011 — Fault Tolerance & Resiliency Patterns](../../architecture/adrs/0011-fault-tolerance-resiliency-patterns.md)
- [ADR-0013 — Cloud Infrastructure & DR](../../architecture/adrs/0013-cloud-infrastructure-topology-dr.md)
