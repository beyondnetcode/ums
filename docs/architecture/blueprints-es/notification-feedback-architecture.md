# Arquitectura de Notificaciones y Feedback

**Tipo:** Blueprint de Arquitectura  
**Estado:** Aceptado, enmendado 2026-05-26
**Runtime:** React 18, TypeScript, TanStack Query v5, Zustand, Axios
**Ubicacion en codigo:** `src/apps/ums.web-app/src`
**Decision rectora:** [ADR-0066: Contrato de Errores Accionables para Usuarios y Diagnostico Correlacionado](../adrs/0066-actionable-user-error-contract.es.md)

---

## Proposito

Todo comando iniciado por el usuario debe producir feedback visible, accionable y seguro. UMS usa un sistema de visibilidad dual:

- Un toast efimero comunica exito o falla inmediata.
- Un centro de notificaciones conserva feedback relevante hasta que expire o el usuario lo elimine explicitamente.

El feedback no es una superficie de diagnostico tecnico. El navegador recibe solamente contenido aprobado para usuarios y un GUID `errorId` generado por el servidor que soporte puede buscar en Serilog y Grafana Loki.

## Problemas Resueltos

| Modo de falla | Impacto para usuario | Respuesta estandar |
|---|---|---|
| Falla silenciosa de comando | El usuario repite una operacion sin saber que fallo | Emitir siempre una notificacion de exito o error |
| Falla de validacion generica | El usuario no puede corregir su entrada | Presentar un `userMessage` aprobado y accionable |
| Filtracion de detalle tecnico | El usuario ve pila, clase, namespace, SQL o internos | Mostrar solo campos aprobados; mantener diagnosticos en logs |
| Correlacion perdida | Soporte no encuentra la falla reportada | Incluir `errorId` generado por servidor en respuesta, header y log |
| Control de cierre inefectivo | El mensaje descartado permanece en historial activo | El cierre manual elimina la entrada de notificacion |

## Reglas Centrales

### 1. Un Solo Limite Seguro de Extraccion

`getHttpErrorMessage(error, fallback)` es la unica funcion cliente que elige contenido de error visible para usuario. Los componentes y hooks de feature no deben mostrar campos crudos de la respuesta.

Fuentes permitidas para presentacion:

1. `userMessage` REST, aprobado explicitamente por el limite del API.
2. Un mensaje localizado derivado de `messageKey` y parametros seguros, cuando este implementado.
3. Un fallback local propiedad de la funcionalidad.

Fuentes prohibidas para presentacion:

- `detail` o `message` REST arbitrarios.
- Mensajes GraphQL crudos.
- Mensajes de excepcion JavaScript o Axios.
- Stack traces, namespaces, nombres de clases, detalles SQL, tokens, secretos o PII.

### 2. Errores Esperados Accionables

Las fallas de validacion y reglas de negocio pueden mostrarse cuando el API las ha clasificado como seguras. Ejemplos:

- "No se pudo registrar el modulo porque el campo Descripcion excede el maximo de 500 caracteres. Reduzca su longitud e intente nuevamente."
- "No se pudo registrar el modulo porque ya existe otro modulo con el mismo Codigo. Ingrese un codigo diferente."

Una falla inesperada de infraestructura o programacion siempre recibe guia generica mas el codigo de seguimiento.

### 3. Flujo Centralizado de Notificaciones

Todas las mutaciones de feature usan `useNotifiedMutation`. Este ejecuta la operacion, invalida cache en exito, selecciona feedback aprobado y agrega una notificacion al unico store Zustand.

| Responsabilidad | Componente |
|---|---|
| Extraccion segura de error | `src/application/errors/http-error.ts` |
| Wrapper de notificaciones de comando | `src/application/hooks/use-notified-mutation.ts` |
| Estado de notificaciones | `src/application/stores/notification.store.ts` |
| Presentacion efimera | `src/presentation/shared/components/ToastQueue.tsx` |
| Presentacion de historial | `src/presentation/shared/components/NotificationCenter.tsx` |

### 4. Visibilidad del Codigo de Seguimiento

Cuando una operacion fallida incluye `errorId`, la notificacion contiene una guia de soporte como:

```text
Si necesitas mas detalles, consulta con el administrador e indica este ID de error: <errorId>.
```

El codigo es seguro para mostrar porque es una referencia de soporte, no una descripcion interna de excepcion. Debe correlacionar con el contexto de logs del servidor.

### 5. Semantica de Descarte

| Interaccion del usuario | Toast | Centro de notificaciones |
|---|---|---|
| Timeout automatico | Se elimina de la presentacion efimera | Puede permanecer en historial |
| Boton de cierre manual | Se elimina inmediatamente | Se elimina la entrada activa correspondiente |
| Limpiar todo en el centro | Se elimina si estaba presente | Se eliminan todas las entradas |

El descarte manual representa la intencion explicita del usuario de remover ese feedback, no solo ocultar su animacion.

## Contrato con el Backend

Los comandos REST retornan Problem Details RFC 7807. Una respuesta segura de validacion puede tomar esta forma:

```json
{
  "type": "https://httpstatuses.io/422",
  "title": "Validation Error",
  "status": 422,
  "detail": "No se pudo completar la operacion porque existen campos invalidos.",
  "userMessage": "No se pudo registrar el modulo porque el campo Codigo tiene un formato invalido. Use solo letras, numeros y guion bajo, por ejemplo REPORTS_01. Valor ingresado: DDDD-!.",
  "errorCode": "validation.invalid_format",
  "messageKey": "system_suite.module.code_invalid_format",
  "messageParameters": { "invalidValue": "DDDD-!" },
  "errorId": "<server-generated-guid>",
  "traceId": "<distributed-trace-id>"
}
```

Requisitos del contrato:

- `detail` es seguro, pero no es automaticamente presentable.
- `userMessage` es presentable solo porque el limite del API lo publica explicitamente como seguro.
- `errorCode`, `messageKey` y parametros son la evolucion multilenguaje preferida.
- `errorId` se genera una sola vez por solicitud fallida y aparece en payload, `X-Error-Id` y el evento Serilog que usa soporte.
- `traceId` permanece disponible para diagnostico distribuido y es distinto de `errorId`.
- Los diagnosticos tecnicos se registran en backend y nunca se serializan para presentacion.

Las consultas GraphQL siguen la misma regla de seguridad: errores no controlados de resolver retornan contenido seguro al cliente y conservan diagnosticos en logs.

## Implementacion Actual de UMS

UMS actualmente soporta:

- `userMessage` para feedback aprobado de validacion y conflicto de negocio REST.
- `errorId` generado por servidor en payloads REST y `X-Error-Id`, priorizado por el cliente web para soporte.
- Mensajes seguros localizados de error GraphQL con `errorId` registrado en el log correspondiente del servidor.
- Eliminacion de una notificacion del estado activo cuando el usuario presiona su boton de cierre.

La mejora objetivo es introducir valores estables `errorCode` o `messageKey` y localizacion del lado del cliente, en lugar de servir el texto final del idioma desde el API.

## Verificacion

| Capa | Verificacion requerida |
|---|---|
| API | Una validacion esperada expone feedback accionable seguro, GUID `errorId`, `X-Error-Id` coincidente y `traceId` |
| API | Errores inesperados no exponen stack trace, tipo de excepcion, namespace, SQL, secretos ni PII |
| Extraccion web | Solo se muestran mensajes aprobados o fallback local |
| Wrapper de mutacion | Se incluye la guia localizada de soporte con `errorId` cuando existe |
| Cola de toasts | El cierre manual elimina la notificacion del estado activo |

## Documentos Relacionados

- [ADR-0066: Contrato de Errores Accionables para Usuarios y Diagnostico Correlacionado](../adrs/0066-actionable-user-error-contract.es.md)
- [ADR-0053: Observabilidad con OpenTelemetry](../adrs/0053-opentelemetry-observability.md)
- [ADR-0062: Configuracion Serilog Segura de PII](../adrs/0062-pii-safe-serilog-configuration.es.md)

---

**[Volver al Indice de Blueprints](./index.md)** | **[English version](../blueprints/notification-feedback-architecture.md)**
