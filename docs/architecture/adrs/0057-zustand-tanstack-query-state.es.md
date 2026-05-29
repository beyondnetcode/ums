# ADR-0057: Gestión de Estado con Zustand + TanStack Query

| Campo | Valor |
|---|---|
| **Estado** | Aceptado |
| **Fecha** | 2026-05-21 |
| **Contexto** | UMS Web App — Estrategia de Gestión de Estado |
| **Decisores** | Equipo de Arquitectura |

## Problema

Las aplicaciones React necesitan gestionar dos tipos fundamentalmente diferentes de estado:
1. **Estado de servidor**: Datos de APIs (tenants, branches, users) — necesita caching, invalidación, deduplicación
2. **Estado de cliente**: Estado de UI (tema, idioma, notificaciones, sesión de auth) — necesita reactividad, persistencia

Usar una única solución para ambos lleva a over-engineering (estado de servidor en Redux) o under-engineering (estado de cliente con lógica de fetch manual).

## Decisión

Usar un enfoque de **estrategia dual**:

### Estado de Servidor: TanStack Query (React Query)

```typescript
// Las queries son cacheadas, deduplicadas, y auto-invalidadas
const { data, isLoading } = useQuery({
  queryKey: ['tenants', page, filters],
  queryFn: () => tenantService.getTenants(page, filters),
  staleTime: 30_000,
});

// Las mutaciones invalidan queries y muestran notificaciones
const createMutation = useNotifiedMutation({
  mutationFn: (data) => tenantService.createTenant(data),
  invalidateKeys: [['tenants']],
  successNotif: () => ({ title: 'Created', message: 'Tenant created' }),
  errorNotif: (err) => ({ title: 'Error', message: getHttpErrorMessage(err) }),
});
```

### Estado de Cliente: Zustand

```typescript
// Gestión de estado simple, rápida, TypeScript-first
export const useThemeStore = create<ThemeState>()(
  persist(
    (set) => ({
      isDarkMode: true,
      toggleDarkMode: () => set((s) => ({ isDarkMode: !s.isDarkMode })),
    }),
    { name: 'ums-theme' },
  ),
);
```

### Inventario de Stores

| Store | Propósito | Persistencia |
|---|---|---|
| `auth.store` | Sesión de usuario, estado de autenticación | No (solo sesión) |
| `theme.store` | Preferencia de modo oscuro/claro | Sí (localStorage) |
| `notification.store` | Notificaciones in-app (cap: 50) | No (solo sesión) |
| `i18n.store` | Idioma activo (`en`/`es`) | No (sincroniza con `document.lang`) |
| `devTools.store` | Overrides solo-dev (suplantación de usuario) | No (solo dev) |

### Reglas

1. **Los datos de servidor van a través de TanStack Query**: Nunca almacenar respuestas de API en Zustand.
2. **El estado de UI va a través de Zustand**: Tema, idioma, notificaciones, modal abierto/cerrado.
3. **Sin manipulación de DOM en stores**: Los stores son estado puro. Los componentes manejan los efectos secundarios de DOM.
4. **Única fuente de verdad**: Cada pieza de estado vive en exactamente un store.
5. **Las dev tools están aisladas**: `devTools.store` es solo desarrollo; el código de producción usa `i18n.store` y `auth.store`.

### Patrón useNotifiedMutation

Todas las mutaciones siguen el mismo patrón vía `useNotifiedMutation`:
1. Ejecutar función de mutación
2. Invalidar query keys relevantes
3. Mostrar notificación de éxito
4. Mostrar notificación de error en fallo

## Consecuencias

**Positivas:**
- Cacheo y deduplicación automáticos de datos de servidor
- Estado de cliente simple, sin boilerplate
- Separación clara de concerns
- Middleware `persist` para localStorage sin código custom
- `useNotifiedMutation` elimina boilerplate de mutaciones

**Negativas:**
- Dos librerías que aprender y mantener
- Gestión de query keys requiere disciplina
- Los stores de Zustand no son serializables por default (excepto con `persist`)

## Implementación

- `src/application/stores/` — Todos los stores de Zustand
- `src/application/hooks/use-notified-mutation.ts` — Factory de mutaciones
- `src/infrastructure/http/` — Clientes HTTP y GraphQL
- `vitest.config.ts` — Thresholds de coverage para código de estado

## Relacionados

- ADR-0055: Patrón de API Híbrida GraphQL/REST
- ADR-0056: Límites de Capas de Clean Architecture