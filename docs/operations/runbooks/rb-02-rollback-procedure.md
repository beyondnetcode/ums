# RB-02: Rollback Procedure

| Field | Value |
|-------|-------|
| **Runbook ID** | RB-02 |
| **Scope** | Application deployment rollback |
| **Owner** | Platform / DevOps Team |
| **Last Review** | 2026-05-15 |

---

## 1. When to Roll Back

Roll back when all of the following are true:
- A deployment was made within the last 2 hours **AND**
- Error rate increased > 2x baseline **OR** SEV-1/2 symptoms appeared **AND**
- Forward fix ETA > 30 minutes

Do **not** roll back if:
- The incident is unrelated to the recent deployment
- A DB migration has already run (roll forward instead; see section 4)

---

## 2. Application Rollback (Kubernetes)

```bash
# Check rollout history
kubectl rollout history deployment/ums-auth-api -n ums-prod

# Rollback to previous revision
kubectl rollout undo deployment/ums-auth-api -n ums-prod

# Rollback to specific revision
kubectl rollout undo deployment/ums-auth-api -n ums-prod --to-revision=3

# Verify rollback status
kubectl rollout status deployment/ums-auth-api -n ums-prod --timeout=120s

# Confirm running image
kubectl get deployment ums-auth-api -n ums-prod -o jsonpath='{.spec.template.spec.containers[*].image}'
```

---

## 3. Database Migration Rollback

> **Warning:** Only execute if the migration was destructive (dropped/renamed columns). If data was inserted, coordinate with DBA.

```bash
# Check current migration version (TypeORM)
kubectl exec -n ums-prod deploy/ums-auth-api -- npx typeorm migration:show

# Run down migration for last applied
kubectl exec -n ums-prod deploy/ums-auth-api -- npx typeorm migration:revert

# Verify schema state
kubectl exec -n ums-prod deploy/ums-db-pod -- psql -U ums_app -d ums_prod \
  -c "SELECT version, name, run_on FROM migrations ORDER BY run_on DESC LIMIT 5;"
```

---

## 4. Roll Forward (When Rollback Is Not Possible)

When a DB migration has already run and rollback is not safe:

1. Create a hotfix branch from `main`
2. Apply the minimal forward fix
3. Fast-track CI (tag `[hotfix]` in commit message)
4. Deploy to prod via expedited pipeline
5. Document in post-incident review

---

## 5. Config / Feature Flag Rollback

```bash
# Disable feature flag via CLI (if using OpenFeature / LaunchDarkly)
ums-admin feature-flag disable --name "new-auth-flow" --env prod

# Or via kubectl ConfigMap patch
kubectl patch configmap ums-feature-flags -n ums-prod \
  --type merge \
  -p '{"data":{"NEW_AUTH_FLOW_ENABLED":"false"}}'

# Restart pods to pick up config change
kubectl rollout restart deployment/ums-auth-api -n ums-prod
```

---

## 6. Rollback Verification Checklist

- [ ] Old container image confirmed running (`kubectl get pods -o wide`)
- [ ] Auth smoke test passes: `curl https://ums.internal/health`
- [ ] Login flow functional (manual or automated smoke test)
- [ ] Error rate returned to baseline in Grafana
- [ ] Outbox relay processing normally (no PENDING backlog spike)
- [ ] Incident Slack channel updated with rollback confirmation

---

## 7. Post-Rollback Actions

1. Annotate deployment failure in Grafana (`Annotations` → `Create annotation`)
2. Open incident ticket referencing failed deployment SHA
3. Do not re-deploy until root cause is identified
4. Update [RB-01 post-incident template](./rb-01-incident-response.md#6-post-incident-review-template)

---

**[Back to Operations Index](../index.md)** | **[Back to Master Index](../../MASTER_INDEX.md)**
