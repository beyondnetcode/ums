# RB-01: Incident Response Procedure

| Field | Value |
|-------|-------|
| **Runbook ID** | RB-01 |
| **Severity Scope** | SEV-1 (Critical), SEV-2 (High) |
| **Owner** | Platform / On-Call Team |
| **Last Review** | 2026-05-15 |

---

## 1. Severity Classification

| Level | Definition | Response SLA | Examples |
|-------|-----------|-------------|---------|
| SEV-1 | Total service unavailability or data breach | 15 min acknowledgment, 4 h resolution | Auth service down, DB unreachable, mass login failure |
| SEV-2 | Degraded functionality affecting ≥20% users | 30 min acknowledgment, 8 h resolution | Slow auth graph queries, failed email notifications |
| SEV-3 | Minor degradation, workaround available | Next business day | Single user permission error |
| SEV-4 | Cosmetic / low-impact | Scheduled sprint | UI label wrong |

---

## 2. Incident Lifecycle

```
Alert Fires
    │
    ▼
[1] Acknowledge (on-call engineer, within SLA)
    │
    ▼
[2] Triage & Classify (SEV-1/2/3/4)
    │
    ▼
[3] Assemble war room (SEV-1: all leads; SEV-2: 2 engineers)
    │
    ▼
[4] Diagnose — collect logs, metrics, traces
    │
    ├── Known cause? → [5a] Apply known fix (see runbooks RB-02..RB-04)
    │
    └── Unknown cause? → [5b] Isolate → Hypothesis → Test → Fix
    │
    ▼
[6] Resolve & Verify (smoke tests pass, metrics normalize)
    │
    ▼
[7] Post-Incident Review (within 48 h for SEV-1, 5 days for SEV-2)
```

---

## 3. Initial Triage Commands

```bash
# Check pod status (Kubernetes)
kubectl get pods -n ums-prod -o wide

# Check recent logs for auth service
kubectl logs -n ums-prod deploy/ums-auth-api --tail=200 --since=10m

# Check Dapr sidecar health
kubectl exec -n ums-prod deploy/ums-auth-api -c daprd -- wget -qO- http://localhost:3500/v1.0/healthz

# Check Redis connectivity
kubectl exec -n ums-prod deploy/ums-auth-api -- redis-cli -h redis-master -p 6379 PING

# Check DB connectivity
kubectl exec -n ums-prod deploy/ums-auth-api -- pg_isready -h db-primary -U ums_app -d ums_prod

# Check outbox backlog (potential relay failure)
kubectl exec -n ums-prod deploy/ums-db-pod -- psql -c \
  "SELECT status, count(*) FROM outbox_events GROUP BY status;"
```

---

## 4. Key Monitoring Endpoints

| Signal | Location | Threshold |
|--------|----------|-----------|
| Auth API latency (p99) | Grafana → UMS Auth → Request Latency | > 800 ms |
| Login error rate | Grafana → UMS Auth → Error Rate | > 1% |
| Outbox PENDING backlog | Grafana → UMS Events → Outbox Lag | > 500 events |
| Redis hit rate | Grafana → UMS Cache → Redis Hit Ratio | < 80% |
| DB connection pool | Grafana → UMS DB → Pool Utilization | > 90% |
| Saga failure rate | Grafana → UMS Sagas → Compensation Rate | > 5% |

---

## 5. Communication Protocol

| Phase | Action | Channel |
|-------|--------|---------|
| Alert fires | Page on-call | PagerDuty |
| SEV-1 declared | Notify team lead + CTO | Slack `#incidents-prod` |
| User impact confirmed | Post status update | Status page |
| Every 30 min | Progress update | Slack `#incidents-prod` |
| Resolution | Notify stakeholders | Email + Slack |

---

## 6. Post-Incident Review Template

```markdown
## Incident: [Title] — [Date]

**Duration:** X hours Y minutes
**Severity:** SEV-X
**Impact:** [N users affected, service degraded/down]

### Timeline
- HH:MM — Alert fired
- HH:MM — Acknowledged by [name]
- HH:MM — Root cause identified
- HH:MM — Fix deployed
- HH:MM — Service recovered

### Root Cause
[What failed and why]

### Contributing Factors
- [Factor 1]
- [Factor 2]

### What Went Well
- [Good response action 1]

### Action Items
| Action | Owner | Due Date |
|--------|-------|----------|
| Add monitoring for X | @engineer | YYYY-MM-DD |
| Fix root cause Y | @engineer | YYYY-MM-DD |
```

---

**[Back to Operations Index](../index.md)** | **[Back to Master Index](../../MASTER_INDEX.md)**
