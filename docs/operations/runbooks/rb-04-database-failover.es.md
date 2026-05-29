# RB-04: Failover de Base de Datos

| Campo | Valor |
|-------|-------|
| **ID del Runbook** | RB-04 |
| **Alcance** | PostgreSQL / SQL Server primary failover — bases de datos de UMS |
| **Responsable** | DBA / Plataforma On-Call |
| **Última Revisión** | 2026-05-15 |

---

## 1. Escenarios de Failover

| Escenario | RTO Objetivo | RPO Objetivo | Acción |
|-----------|--------------|--------------|---------|
| Primary pod crash (k8s) | < 60 s | 0 (replica sincrónica) | Automático via Patroni / operator |
| AZ-level failure | < 5 min | < 30 s | Promoción manual de standby |
| Region-level DR | < 30 min | < 5 min | Activación de runbook DR |
| Corruption / bad migration | Variable | Point-in-time | Restauración PITR |

---

## 2. Failover Automático (Patroni / CloudNativePG)

El cluster de base de datos de UMS usa CloudNativePG (o Patroni) para elección automática de primary. El failover automático no requiere intervención manual.

```bash
# Verificar primary actual
kubectl get cluster ums-db-cluster -n ums-prod -o jsonpath='{.status.currentPrimary}'

# Ver salud del cluster
kubectl describe cluster ums-db-cluster -n ums-prod | grep -A 20 "Status:"

# Ver replication lag en replicas
kubectl exec -n ums-prod $(kubectl get pod -n ums-prod -l role=replica -o name | head -1) \
  -- psql -U ums_app -d ums_prod \
  -c "SELECT now() - pg_last_xact_replay_timestamp() AS replication_lag;"
```

---

## 3. Promoción Manual (Si Automático Falla)

```bash
# Paso 1: Identificar pods standby actuales
kubectl get pods -n ums-prod -l cnpg.io/cluster=ums-db-cluster

# Paso 2: Promover standby a primary
kubectl cnpg promote ums-db-cluster -n ums-prod

# Paso 3: Verificar promoción
kubectl get cluster ums-db-cluster -n ums-prod -o jsonpath='{.status.currentPrimary}'

# Paso 4: Actualizar connection string de aplicación si es necesario
kubectl get secret ums-db-cluster-app -n ums-prod -o yaml | grep host
```

---

## 4. Reconectar Aplicaciones Después de Failover

```bash
# Restart auth API pods para forzar reset de connection pool
kubectl rollout restart deployment/ums-auth-api -n ums-prod
kubectl rollout restart deployment/ums-outbox-relay -n ums-prod

# Verificar conexiones establecidas
kubectl logs -n ums-prod deploy/ums-auth-api --tail=50 | grep -i "database\|connection\|ready"

# Ver conexiones DB activas
kubectl exec -n ums-prod ums-db-cluster-1 -- psql -U ums_app -d ums_prod \
  -c "SELECT count(*), state FROM pg_stat_activity WHERE datname='ums_prod' GROUP BY state;"
```

---

## 5. Recuperación de Outbox Relay Después de Failover

Durante el failover, el worker de outbox relay puede haberse estancado. Verificar y recuperar:

```bash
# Ver backlog PENDING de outbox
kubectl exec -n ums-prod ums-db-cluster-1 -- psql -U ums_app -d ums_prod -c \
  "SELECT status, count(*), min(created_at) AS oldest FROM outbox_events GROUP BY status;"

# Si conteo PENDING es alto, verificar que relay está procesando
kubectl logs -n ums-prod deploy/ums-outbox-relay --tail=100 | grep -i "relay\|publish\|error"

# Restart relay si está atascado
kubectl rollout restart deployment/ums-outbox-relay -n ums-prod
```

---

## 6. Point-in-Time Recovery (PITR)

> **Último recurso.** Solo si se perdió el primary y todas las replicas o los datos están corrupto.

```bash
# Listar WAL archives / snapshots disponibles
kubectl cnpg backup list ums-db-cluster -n ums-prod

# Restaurar a point in time (timestamp UTC)
kubectl apply -f - <<EOF
apiVersion: postgresql.cnpg.io/v1
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

Después de que PITR completa:
1. Validar consistencia de datos (row counts, last transaction timestamp)
2. Ejecutar `npx typeorm migration:show` para verificar estado del schema
3. Redirigir aplicaciones a cluster restaurado
4. Replay cualquier evento perdido de Dapr / message bus DLQ

---

## 7. Verificación de RLS Multi-Tenant Post-Failover

```sql
-- Verificar que RLS policies sobrevivieron el failover
SELECT schemaname, tablename, policyname, cmd, qual
FROM pg_policies
WHERE schemaname = 'ums'
ORDER BY tablename, policyname;

-- Testear enforcement de RLS
EXEC sp_set_session_context 'TenantId', 'tenant-test-1';
SELECT count(*) FROM ums.users; -- debe retornar solo filas de tenant-test-1
```

---

## 8. Checklist de Verificación

- [ ] New primary confirmado via `kubectl get cluster`
- [ ] Replication lag en nuevas replicas < 10 s dentro de 5 minutos
- [ ] Auth API health endpoint retorna 200
- [ ] Login smoke test pasa
- [ ] Outbox PENDING backlog drenando (< 100 events después de 5 min)
- [ ] RLS policies intactas en todas las tablas de tenant
- [ ] Dashboard de DB en Grafana: connection pool normal, sin spikes de error

---

**[Volver al Índice de Operaciones](../index.md)** | **[Volver al Índice Maestro](../../MASTER_INDEX.md)**