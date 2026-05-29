# RB-02: Procedimiento de Rollback

| Campo | Valor |
|-------|-------|
| **ID del Runbook** | RB-02 |
| **Alcance** | Rollback de despliegues de aplicación |
| **Responsable** | Plataforma / Equipo DevOps |
| **Última Revisión** | 2026-05-15 |

---

## 1. Cuándo Hacer Rollback

Haz rollback cuando todas las siguientes sean true:
- Un despliegue fue realizado dentro de las últimas 2 horas **Y**
- La tasa de error aumentó > 2x baseline **O** síntomas SEV-1/2 aparecieron **Y**
- ETA de fix forward > 30 minutos

**No** hacer rollback si:
- El incidente no está relacionado con el despliegue reciente
- Una migración de DB ya se ejecutó (hacer roll forward en su lugar; ver sección 4)

---

## 2. Rollback de Aplicación (Kubernetes)

```bash
# Ver historial de rollout
kubectl rollout history deployment/ums-auth-api -n ums-prod

# Rollback a revisión anterior
kubectl rollout undo deployment/ums-auth-api -n ums-prod

# Rollback a revisión específica
kubectl rollout undo deployment/ums-auth-api -n ums-prod --to-revision=3

# Verificar estado de rollback
kubectl rollout status deployment/ums-auth-api -n ums-prod --timeout=120s

# Confirmar imagen corriendo
kubectl get deployment ums-auth-api -n ums-prod -o jsonpath='{.spec.template.spec.containers[*].image}'
```

---

## 3. Rollback de Migración de Base de Datos

> **Advertencia:** Solo ejecutar si la migración fue destructiva (columnas dropped/renamed). Si se insertaron datos, coordinar con DBA.

```bash
# Ver versión actual de migración (TypeORM)
kubectl exec -n ums-prod deploy/ums-auth-api -- npx typeorm migration:show

# Ejecutar down migration para la última aplicada
kubectl exec -n ums-prod deploy/ums-auth-api -- npx typeorm migration:revert

# Verificar estado del schema
kubectl exec -n ums-prod deploy/ums-db-pod -- psql -U ums_app -d ums_prod \
  -c "SELECT version, name, run_on FROM migrations ORDER BY run_on DESC LIMIT 5;"
```

---

## 4. Roll Forward (Cuando Rollback No Es Posible)

Cuando una migración de DB ya se ejecutó y el rollback no es seguro:

1. Crear branch hotfix desde `main`
2. Aplicar el fix forward mínimo
3. Fast-track CI (tag `[hotfix]` en commit message)
4. Desplegar a prod via pipeline expedito
5. Documentar en post-incident review

---

## 5. Rollback de Config / Feature Flag

```bash
# Deshabilitar feature flag via CLI (si usa OpenFeature / LaunchDarkly)
ums-admin feature-flag disable --name "new-auth-flow" --env prod

# O via kubectl ConfigMap patch
kubectl patch configmap ums-feature-flags -n ums-prod \
  --type merge \
  -p '{"data":{"NEW_AUTH_FLOW_ENABLED":"false"}}'

# Restart pods para recoger cambio de config
kubectl rollout restart deployment/ums-auth-api -n ums-prod
```

---

## 6. Checklist de Verificación de Rollback

- [ ] Imagen de contenedor antigua confirmada corriendo (`kubectl get pods -o wide`)
- [ ] Auth smoke test pasa: `curl https://ums.internal/health`
- [ ] Login flow funcional (manual o automated smoke test)
- [ ] Tasa de error volvió al baseline en Grafana
- [ ] Outbox relay procesando normalmente (no spike de backlog PENDING)
- [ ] Canal de Slack del incidente actualizado con confirmación de rollback

---

## 7. Acciones Post-Rollback

1. Anotar fallo de despliegue en Grafana (`Annotations` → `Create annotation`)
2. Abrir ticket de incidente referenciando el SHA del despliegue fallido
3. No re-desplegar hasta que se identifique la causa raíz
4. Actualizar [plantilla de post-incident de RB-01](./rb-01-incident-response.md#6-post-incident-review-template)

---

**[Volver al Índice de Operaciones](../index.md)** | **[Volver al Índice Maestro](../../MASTER_INDEX.md)**