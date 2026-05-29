# RB-03: Recuperación de Fallo de Cache

| Campo | Valor |
|-------|-------|
| **ID del Runbook** | RB-03 |
| **Alcance** | Fallo de cache Redis — auth graph y session cache de UMS |
| **Responsable** | Plataforma / Equipo de Guardia |
| **Última Revisión** | 2026-05-15 |

---

## 1. Modos de Fallo

| Modo | Síntoma | Causa Probable |
|------|---------|---------------|
| Redis pod crash | Auth latency spikes, cache miss 100% | OOM kill, pod eviction |
| Redis network partition | Connection timeouts en app logs | Network policy change, k8s DNS |
| Redis data corruption | Auth errors inesperados, graph stale | RDB restore from bad snapshot |
| Sentinel failover in progress | Fallos intermitentes, gap de 5-30 s | Primary failover election |

---

## 2. Diagnóstico

```bash
# Ver estado del pod Redis
kubectl get pods -n ums-prod -l app=redis

# Ver logs de Redis
kubectl logs -n ums-prod deploy/redis-master --tail=100

# Testear conectividad desde app pod
kubectl exec -n ums-prod deploy/ums-auth-api -- redis-cli -h redis-master -p 6379 PING

# Ver uso de memoria de Redis
kubectl exec -n ums-prod deploy/redis-master -- redis-cli INFO memory | grep used_memory_human

# Ver clientes conectados
kubectl exec -n ums-prod deploy/redis-master -- redis-cli CLIENT LIST | wc -l

# Ver ratio de cache hit
kubectl exec -n ums-prod deploy/redis-master -- redis-cli INFO stats \
  | grep -E "keyspace_hits|keyspace_misses"
```

---

## 3. Pasos de Recuperación

### 3a. Redis Pod Crashed — Restart

```bash
# Delete pod para forzar restart (Deployment recreará)
kubectl delete pod -n ums-prod -l app=redis-master

# Esperar a que pod esté ready
kubectl wait pod -n ums-prod -l app=redis-master --for=condition=Ready --timeout=120s

# Verificar replicación (si Sentinel / cluster)
kubectl exec -n ums-prod deploy/redis-master -- redis-cli INFO replication
```

### 3b. Aplicación Failover a DB (Cache-Aside Degraded Mode)

El repositorio de auth graph de UMS caeback a queries de DB cuando Redis no está disponible (circuit breaker pattern).

```bash
# Habilitar flag de modo degradado para skip cache reads
kubectl patch configmap ums-feature-flags -n ums-prod \
  --type merge \
  -p '{"data":{"REDIS_CACHE_ENABLED":"false"}}'

# Restart pods para recoger flag
kubectl rollout restart deployment/ums-auth-api -n ums-prod

# Monitorear pool de conexión DB — esperar mayor utilización
kubectl exec -n ums-prod deploy/ums-db-pod -- psql -c \
  "SELECT count(*) FROM pg_stat_activity WHERE datname='ums_prod';"
```

> **Nota:** DB fallback es seguro pero aumenta carga de DB ~3x. Notificar a DBA si outage de Redis excede 30 minutos.

### 3c. Flush Cache Corrupto (parcial o full)

```bash
# Flush solo keys de auth graph (pattern match)
kubectl exec -n ums-prod deploy/redis-master -- redis-cli \
  --scan --pattern "auth:graph:*" | xargs redis-cli DEL

# Full flush (último recurso — todas las caches cold start)
kubectl exec -n ums-prod deploy/redis-master -- redis-cli FLUSHDB ASYNC
```

---

## 4. Re-habilitar Cache Después de Recuperación

```bash
# Re-habilitar Redis cache
kubectl patch configmap ums-feature-flags -n ums-prod \
  --type merge \
  -p '{"data":{"REDIS_CACHE_ENABLED":"true"}}'

kubectl rollout restart deployment/ums-auth-api -n ums-prod

# Verificar que ratio de hit está recuperándose (debe alcanzar > 80% dentro de 10 min)
watch -n 30 "kubectl exec -n ums-prod deploy/redis-master -- redis-cli INFO stats \
  | grep -E 'keyspace_hits|keyspace_misses'"
```

---

## 5. Warm-up Después de Full Flush

Para tenants críticos, pre-warm el cache de auth graph:

```bash
# Trigger projection warm-up via admin API
curl -X POST https://ums.internal/admin/cache/warm-up \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"tenants": ["tenant-1", "tenant-2"]}'
```

---

## 6. Checklist de Verificación

- [ ] Redis pod Running and Ready
- [ ] `PING` retorna `PONG` desde app pod
- [ ] Auth graph cache hit ratio > 80% dentro de 10 min
- [ ] No hay errores de conexión Redis en logs de auth API
- [ ] Flag `REDIS_CACHE_ENABLED` seteado a `true`
- [ ] Dashboard de Redis en Grafana muestra memoria normal y métricas de conexión

---

**[Volver al Índice de Operaciones](../index.md)** | **[Volver al Índice Maestro](../../MASTER_INDEX.md)**