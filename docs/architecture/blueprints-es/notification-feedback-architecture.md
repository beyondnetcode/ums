# Arquitectura de Notificaciones y Feedback

**Tipo:** Blueprint de Arquitectura  
**Estado:** Aceptado · Implementado 2026-05-26  
**Runtime:** React 18 · TypeScript · TanStack Query v5 · Zustand · Axios  
**Ubicación en código:** `src/apps/ums.web-app/src`

---

## Propósito

Toda mutación iniciada por el usuario (crear, actualizar, eliminar, cambiar estado) debe
producir feedback visible y accionable. Este blueprint define cómo UMS expone errores de negocio
y confirmaciones a través de un **sistema de visibilidad dual**: un toast efímero para
visibilidad inmediata, y un panel de notificaciones persistente para historial y auditoría.

El diseño resuelve tres modos de fallo específicos:

| Modo de fallo | Síntoma | Causa raíz |
|---|---|---|
| 400/409 silencioso | El usuario reintenta sin saber que la acción falló | El error llega al store pero el drawer está cerrado |
| "Algo salió mal" genérico | El usuario no puede actuar sobre el error | El campo `detail` del backend no se extrae |
| Boilerplate duplicado | Cada hook reimplementa el cableado éxito/error | No existe una fábrica de mutaciones compartida |

---

## Visión General de la Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                    Capa de Presentación                      │
│                                                             │
│   Componente llama mutateAsync()                            │
│        │                                                    │
│        ▼                                                    │
│   useNotifiedMutation ──► mutationFn (llamada al servicio)  │
│        │           ◄── onSuccess / onError (TanStack Query) │
│        │                                                    │
│        ▼                                                    │
│   getHttpErrorMessage(error, fallback)                      │
│        │  REST:    response.data.detail                     │
│        │           response.data.message                    │
│        │           error.message                            │
│        │  GraphQL: graphQLErrors[0].message                 │
│        │                                                    │
│        ▼                                                    │
│   useNotificationStore.addNotification()  ◄── Zustand       │
│        │                                                    │
│        ├──► ToastQueue (nueva notificación detectada)       │
│        │         flotante, auto-descarte, inferior-derecha  │
│        │         error: 6 s · warning: 5 s                  │
│        │         info: 4 s · success: 3,5 s                 │
│        │                                                    │
│        └──► Badge en Bell de NotificationCenter             │
│                  M3Drawer, historial completo, máx 50       │
└─────────────────────────────────────────────────────────────┘
```

---

## Inventario de Componentes

### `getHttpErrorMessage(error, fallback)` — Extracción de Error
**Archivo:** `src/application/errors/http-error.ts`

Normaliza cualquier valor lanzado en un string legible por humanos. Lee, en orden de prioridad:

1. `error.graphQLErrors[0].message` — error de HotChocolate
2. `error.response.data.detail` — RFC 7807 Problem Details (campo `detail`)
3. `error.response.data.message` — campo legacy o personalizado
4. `error.message` — error de red de Axios
5. `fallback` — el string estático provisto por el hook que llama

Este es el **único** lugar en el codebase que sabe leer un error del backend. Ningún componente
ni hook debe parsear `error.response` directamente.

---

### `useNotifiedMutation` — Fábrica de Mutaciones
**Archivo:** `src/application/hooks/use-notified-mutation.ts`

```typescript
useNotifiedMutation<TData, TVariables>({
  mutationFn,       // (variables) => Promise<TData>
  invalidateKeys,   // QueryKey[] a invalidar en éxito
  successNotif,     // (data: TData) => { title, message, type? }
  errorNotif,       // (error: unknown) => { title, message, type? }
  options?,         // opciones TanStack extra (excepto onSuccess/onError)
})
```

`onError` siempre llama a `getHttpErrorMessage(error, notif.message)` — el `errorNotif` solo
necesita proveer el **título** y un **mensaje de fallback**. El detalle real del error del
backend se extrae automáticamente y se muestra en el toast y en el drawer.

**Regla:** Toda mutación en UMS se crea a través de esta fábrica. `useMutation` directo no
está permitido en hooks de feature.

---

### `useNotificationStore` — Estado Centralizado
**Archivo:** `src/application/stores/notification.store.ts`

Store Zustand. Fuente de verdad única para notificaciones. Máximo 50 entradas (evicción FIFO).

```typescript
interface AppNotification {
  id: string;          // crypto.randomUUID()
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: string;   // ISO-8601
  read: boolean;
}
```

---

### `ToastQueue` — Visibilidad Efímera
**Archivo:** `src/presentation/shared/components/ToastQueue.tsx`

Se suscribe al store de notificaciones usando un `seenRef` Set para detectar nuevas entradas
sin re-renderizar en cada mark-as-read. Renderiza una columna apilada de `ToastItem` en
`fixed bottom-6 right-6 z-[100]`. Cada toast:

- Entra deslizándose desde la derecha (`translate-x-0 opacity-100`)
- Se descarta automáticamente tras el delay específico del tipo
- Puede descartarse manualmente con el botón ×
- Tope de pila: **4 toasts visibles** (las entradas adicionales siguen en `NotificationCenter`)

Delays de auto-descarte:

| Tipo | Delay | Razonamiento |
|---|---|---|
| `error` | 6 s | El usuario debe leer y decidir qué hacer |
| `warning` | 5 s | Puede requerir acción |
| `info` | 4 s | Informativo, baja urgencia |
| `success` | 3,5 s | Confirmación, no requiere acción |

---

### `NotificationCenter` — Historial Persistente
**Archivo:** `src/presentation/shared/components/NotificationCenter.tsx`

M3Drawer abierto por el ícono Bell en `TopAppBar`. Muestra todas las notificaciones en
orden cronológico inverso. Provee marcar-todas-como-leídas y limpiar-todo. El conteo
de no leídas se muestra como badge pulsante en el Bell.

---

## Flujo de Datos — Error de Negocio (400)

```
1. El usuario hace clic en "Agregar Módulo" con descripción vacía
2. handleAddModule() llama addModuleMutation.mutateAsync(payload)
3. httpClient.POST /system-suites/{id}/modules → HTTP 400
   Body: { "detail": "Description: is required", "status": 400, ... }
4. TanStack Query captura el AxiosError → onError(error)
5. useNotifiedMutation.onError:
     title   = errorNotif(error).title         → "Error al Registrar Módulo"
     message = getHttpErrorMessage(error, ...)  → "Description: is required"
6. addNotification({ title, message, type: 'error' })
7. ToastQueue detecta nueva entrada (ID no vista) → renderiza ToastItem (6 s)
8. Badge Bell incrementa en 1 (pulsando)
9. El usuario lee el toast: "Description: is required" → corrige el formulario
10. El toast se auto-descarta tras 6 s
```

---

## Puntos de Extensión

### Agregar una nueva mutación
```typescript
export const useAddMenu = (systemSuiteId: string, moduleId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (payload: AddMenuPayload) =>
      systemSuiteService.addMenu(systemSuiteId, moduleId, payload),
    invalidateKeys: [['system-suites', systemSuiteId]],
    successNotif: () => ({ title: t.notifMenuAdded, message: t.notifMenuAddedMsg }),
    errorNotif:   () => ({ title: t.notifMenuAddFailed, message: t.notifMenuAddFailedMsg }),
  });
};
```

### Extender la extracción de errores
Si el backend agrega un array `errors` (errores de campo de FluentValidation), actualizar
**solo** `getHttpErrorMessage`:

```typescript
// src/application/errors/http-error.ts
const fieldErrors = data?.errors;
if (fieldErrors && typeof fieldErrors === 'object') {
  const messages = Object.values(fieldErrors).flat().filter(Boolean);
  if (messages.length) return messages.join(' · ');
}
```

Ninguna otra capa requiere cambios.

### Cambiar duración del toast
Actualizar `DISMISS_DELAY_MS` en `ToastQueue.tsx`. Ningún otro archivo se ve afectado.

---

## Contrato con el Backend

El `GlobalExceptionHandler` y `Result.ToNoContent()` del backend deben retornar
RFC 7807 Problem Details para todas las respuestas 4xx:

```json
{
  "type":     "https://httpstatuses.io/400",
  "title":    "Bad Request",
  "status":   400,
  "detail":   "Description: is required",
  "instance": "/api/v1/system-suites/{id}/modules",
  "traceId":  "..."
}
```

`getHttpErrorMessage` lee `response.data.detail`. Si el backend retorna una forma diferente,
actualizar `asHttpError()` en `http-error.ts` — no los hooks ni los componentes.

---

## Testing

| Capa | Qué testear | Herramienta |
|---|---|---|
| `getHttpErrorMessage` | Extrae `detail`, `message`, fallback correcto para todas las formas de error | Vitest unit |
| `useNotifiedMutation` | Llama `addNotification` con título correcto y mensaje extraído en error | `renderHook` + MSW |
| `useNotificationStore` | `addNotification` prepende, tope en 50, `markAsRead` cambia flag | Vitest unit |
| `ToastQueue` | Renderiza con nueva notificación, auto-descarta tras delay, × remueve inmediatamente | RTL + fake timers |
| `NotificationCenter` | Conteo badge, drawer abre, marcar-todas-como-leídas limpia badge | RTL |

---

## Documentos Relacionados

- [Flujo de Arquitectura de Observabilidad](./observability-architecture-flow.md)  
- [Arquitectura de Shell Libraries](./shell-library-architecture.md)

---

**[Volver al Índice de Blueprints](./index.md)** | **[English version](../../architecture/blueprints/notification-feedback-architecture.md)**
