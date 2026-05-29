# RB-01: Procedimiento de Respuesta a Incidentes

| Campo | Valor |
|-------|-------|
| **ID del Runbook** | RB-01 |
| **Alcance de Severidad** | SEV-1 (Crítico), SEV-2 (Alto) |
| **Responsable** | Plataforma / Equipo de Guardia |
| **Última Revisión** | 2026-05-15 |

---

## 1. Clasificación de Severidad

| Nivel | Definición | SLA de Respuesta | Ejemplos |
|-------|-----------|-----------------|---------|
| SEV-1 | Indisponibilidad total del servicio o brecha de datos | 15 min ack, 4 h resolución | Auth service down, DB inalcanzable, fallo masivo de login |
| SEV-2 | Funcionalidad degradada afectando ≥20% de usuarios | 30 min ack, 8 h resolución | Queries de auth GraphQL lentas, notificaciones de email fallidas |
| SEV-3 | Degradación menor, workaround disponible | Siguiente día hábil | Error de permiso de un solo usuario |
| SEV-4 | Cosmético / bajo impacto | Sprint programado | Label de UI incorrecto |

---

## 2. Ciclo de Vida del Incidente

```
Alerta Dispara
    │
    ▼
[1] Acknowledgment (ingeniero de guardia, dentro del SLA)
    │
    ▼
[2] Triage & Clasificar (SEV-1/2/3/4)
    │
    ▼
[3] Ensamblar war room (SEV-1: todos los leads; SEV-2: 2 ingenieros)
    │
    ▼
[4] Diagnosticar — collect logs, metrics, traces
    │
    ├── ¿Causa conocida? → [5a] Aplicar fix conocido (ver runbooks RB-02..RB-04)
    │
    └── ¿Causa desconocida? → [5b] Aislar → Hipótesis → Probar → Fix
    │
    ▼
[6] Resolver & Verificar (smoke tests pasan, metrics normalizan)
    │
    ▼
[7] Post-Incident Review (within 48 h for SEV-1, 5 days for SEV-2)
```

---

## 3. Comandos de Triage Inicial

```bash
# Ver estado de pods (Kubernetes)
kubectl get pods -n ums-prod -o wide

# Ver logs recientes del auth service
kubectl logs -n ums-prod deploy/ums-auth-api --tail=200 --since=10m

# Ver salud del sidecar Dapr
kubectl exec -n ums-prod deploy/ums-auth-api -c daprd -- wget -qO- http://localhost:3500/v1.0/healthz

# Ver conectividad Redis
kubectl exec -n ums-prod deploy/ums-auth-api -- redis-cli -h redis-master -p 6379 PING

# Ver conectividad DB
kubectl exec -n ums-prod deploy/ums-auth-api -- pg_isready -h db-primary -U ums_app -d ums_prod

# Ver backlog de outbox (potential relay failure)
kubectl exec -n ums-prod deploy/ums-db-pod -- psql -c \
  "SELECT status, count(*) FROM outbox_events GROUP BY status;"
```

---

## 4. Endpoints Clave de Monitoreo

| Señal | Ubicación | Umbral |
|-------|-----------|--------|
| Latencia Auth API (p99) | Grafana → UMS Auth → Request Latency | > 800 ms |
| Tasa de error de Login | Grafana → UMS Auth → Error Rate | > 1% |
| Backlog PENDING de Outbox | Grafana → UMS Events → Outbox Lag | > 500 events |
| Tasa de hit de Redis | Grafana → UMS Cache → Redis Hit Ratio | < 80% |
| Pool de conexión DB | Grafana → UMS DB → Pool Utilization | > 90% |
| Tasa de fallo de Saga | Grafana → UMS Sagas → Compensation Rate | > 5% |

---

## 5. Protocolo de Comunicación

| Fase | Acción | Canal |
|------|--------|-------|
| Alerta dispara | Page on-call | PagerDuty |
| SEV-1 declarado | Notificar team lead + CTO | Slack `#incidents-prod` |
| Impacto en usuario confirmado | Post status update | Status page |
| Cada 30 min | Progress update | Slack `#incidents-prod` |
| Resolución | Notificar stakeholders | Email + Slack |

---

## 6. Plantilla de Post-Incident Review

```markdown
## Incidente: [Título] — [Fecha]

**Duración:** X horas Y minutos
**Severidad:** SEV-X
**Impacto:** [N usuarios afectados, servicio degradado/caído]

### Línea de Tiempo
- HH:MM — Alerta disparó
- HH:MM — Acknowledged por [nombre]
- HH:MM — Causa raíz identificada
- HH:MM — Fix desplegado
- HH:MM — Servicio recuperado

### Causa Raíz
[Qué falló y por qué]

### Factores Contribuyentes
- [Factor 1]
- [Factor 2]

### Qué Salió Bien
- [Acción de respuesta buena 1]

### Items de Acción
| Acción | Responsable | Fecha Límite |
|--------|-------------|-------------|
| Agregar monitoreo para X | @ingeniero | YYYY-MM-DD |
| Arreglar causa raíz Y | @ingeniero | YYYY-MM-DD |
```

---

**[Volver al Índice de Operaciones](../index.md)** | **[Volver al Índice Maestro](../../MASTER_INDEX.md)**