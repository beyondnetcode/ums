# ADR-0066: Contrato de Errores Accionables para Usuarios y Diagnostico Correlacionado

**Estado:** Aceptado  
**Fecha:** 2026-05-26  
**Responsable:** Arquitectura  
**Disposicion Evolith:** Propuesto para adopcion como estandar universal de Evolith  
**Relacionados:**
- [ADR-0053: Observabilidad con OpenTelemetry](./0053-opentelemetry-observability.md)
- [ADR-0055: Patron Hibrido GraphQL/REST](./0055-graphql-rest-hybrid-api.md)
- [ADR-0057: Estado con Zustand y TanStack Query](./0057-zustand-tanstack-query-state.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.es.md)
- [ADR-0062: Configuracion Serilog Segura de PII](./0062-pii-safe-serilog-configuration.es.md)
- [Arquitectura de Notificaciones y Feedback](../blueprints-es/notification-feedback-architecture.md)

---

## Contexto

Una revision de calidad del flujo de registro de modulos de System Suite revelo dos modos de falla opuestos:

1. Mensajes tecnicos del API, detalles de excepcion y datos relacionados con la pila podian llegar al navegador.
2. Al suprimir todos los mensajes del backend, las fallas esperadas de validacion quedaron demasiado genericas para que el usuario corrigiera la entrada.

Por ejemplo, un usuario que envia una descripcion de modulo mayor al limite permitido debe saber como corregir sus datos. Ese usuario no debe ver un namespace, nombre de clase, detalle de base de datos, mensaje de excepcion ni stack trace. El equipo de soporte si necesita una referencia que localice el evento tecnico completo en Serilog y Grafana Loki.

La decision aplica a todos los comandos iniciados por usuarios en UMS, no solo al registro de modulos.

## Decision

UMS adopta un contrato de errores con dos canales:

1. **Canal de feedback para usuario:** expone solo informacion de negocio o validacion aprobada y accionable.
2. **Canal de diagnostico:** conserva detalles tecnicos en logs estructurados y telemetria, correlacionados mediante un identificador de error generado por el servidor.

### 1. Clasificacion de Errores

| Clase de error | Ejemplo | Contenido visible para usuario | Logging tecnico |
|---|---|---|---|
| Falla de validacion | Longitud maxima excedida | Correccion accionable y codigo de seguimiento | Evento estructurado opcional |
| Conflicto de negocio | Codigo de modulo duplicado | Razon segura de negocio y codigo de seguimiento | Evento estructurado cuando aporte valor operativo |
| Autorizacion o autenticacion | Acceso rechazado | Declaracion segura de acceso y codigo de seguimiento | Evento estructurado con tratamiento de seguridad |
| Infraestructura o falla inesperada | Base de datos, resolver, excepcion no controlada | Guia generica de reintento y codigo de seguimiento | Excepcion completa sanitizada con correlacion |

### 2. Payload Publico de Error

Las fallas de comandos REST usan Problem Details RFC 7807. El payload publico puede incluir:

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
  "errorId": "0cd26dd6-d50e-4b3c-a662-8098a87569a4",
  "traceId": "<distributed-trace-id>"
}
```

Reglas:

- `errorId` es un GUID generado por el servidor una sola vez por solicitud fallida. Es obligatorio en la respuesta, el header (`X-Error-Id`) y el evento Serilog correspondiente.
- `traceId` y `X-Correlation-Id` permanecen como identificadores de trazabilidad distribuida y no reemplazan a `errorId`.
- `userMessage`, cuando esta presente, es explicitamente seguro para presentacion.
- `errorCode`, `messageKey` y `messageParameters` son el contrato objetivo para clientes localizados.
- `detail` nunca debe contener stack traces, tipos de excepcion, namespaces, rutas de codigo fuente, sentencias SQL, tokens, secretos ni PII.
- Los clientes no deben mostrar `detail` arbitrario, mensajes GraphQL crudos ni mensajes nativos de excepcion. Solo muestran campos aprobados o un fallback local.

La implementacion actual de UMS produce `userMessage` mediante recursos de localizacion del API seleccionados por `X-Language` o `Accept-Language`. El soporte estable de `messageKey` sigue siendo la evolucion para clientes mas ricos.

### 3. Limite GraphQL

GraphQL se usa para lecturas segun ADR-0055. Los errores GraphQL expuestos al cliente usan mensajes genericos seguros localizados y transportan `errorId`. Las excepciones de resolvers se registran mediante Serilog con el mismo `errorId`, pero no se serializan al navegador.

### 4. Observabilidad y Logging

- Cada respuesta REST o GraphQL fallida obtiene un nuevo GUID `errorId` generado por el servidor.
- Serilog registra `ErrorId` para fallas esperadas y excepciones sanitizadas completas con `ErrorId` para fallas inesperadas; Loki consulta esa propiedad.
- `CorrelationId` y `TraceId` continuan enlazando operaciones distribuidas independientemente del `ErrorId` visible para soporte.
- Las reglas de logging seguro de PII de ADR-0062 aplican antes de cualquier sink.
- Un especialista de soporte usa el codigo visible para localizar el evento detallado del backend.

### 5. Ciclo de Vida de la Notificacion

El feedback para usuario se entrega mediante el mecanismo centralizado de notificaciones:

- El feedback de validacion y negocio es accionable y puede incluir el codigo de seguimiento.
- La expiracion automatica del toast elimina solo la presentacion efimera; el historial puede permanecer disponible.
- El descarte manual mediante el control de cierre es una accion explicita del usuario y elimina la entrada correspondiente del estado activo de notificaciones.

### 6. Ejemplos Requeridos de Mensaje

| Categoria | Ejemplo seguro visible para usuario |
|---|---|
| Validacion | `No se pudo registrar el modulo porque el campo Codigo tiene un formato invalido. Use solo letras, numeros y guion bajo, por ejemplo REPORTS_01. Valor ingresado: DDDD-!.` |
| Regla de negocio | `No se pudo registrar el modulo porque ya existe otro modulo con el mismo Codigo. Ingrese un codigo diferente.` |
| Autorizacion | `No se pudo completar la operacion porque no tiene permisos suficientes. Solicite acceso al administrador.` |
| Inesperado | `No se pudo completar la operacion debido a un error inesperado. Intente nuevamente mas tarde.` |

Cada falla mostrada agrega: `Si necesitas mas detalles, consulta con el administrador e indica este ID de error: {errorId}.`

## Alternativas Rechazadas

### Mostrar todos los valores `detail` del backend

Rechazada. Es conveniente, pero permite que detalles de implementacion o PII escapen cuando un endpoint esta mal configurado.

### Mostrar solo mensajes genericos

Rechazada. Impide que los usuarios corrijan condiciones esperadas de negocio o validacion, aumentando reintentos y solicitudes de soporte.

### Mostrar detalles de pila solo en desarrollo o QA

Rechazada. QA y ambientes compartidos siguen siendo superficies de usuario y pueden contener datos similares a produccion.

## Consecuencias

### Positivas

- Los usuarios reciben informacion suficiente para corregir fallas esperadas y seguras.
- Los diagnosticos tecnicos permanecen disponibles para soporte sin exponerse en la interfaz.
- REST, GraphQL, logging y notificaciones frontend comparten un contrato auditable.
- El contrato puede evolucionar desde texto `userMessage` servido por backend hacia claves de mensaje localizadas en cliente.

### Compromisos

- Los limites del backend deben clasificar que fallas son seguras para publicar.
- Nuevas reglas de validacion requieren texto seguro de presentacion o metadata de localizacion.
- Las pruebas deben cubrir tanto salida accionable como ausencia de filtracion tecnica.

## Mapeo de Implementacion

| Preocupacion | Implementacion UMS |
|---|---|
| Mapeo seguro REST | `Ums.Presentation/Extensions/ResultExtensions.cs` |
| Generacion de Error ID | `Ums.Presentation/Extensions/UserFacingErrorContext.cs` |
| Excepciones no controladas | `Ums.Presentation/Middleware/GlobalExceptionHandler.cs` |
| Sanitizacion GraphQL | `Ums.Presentation/GraphQL/SafeGraphQlErrorFilter.cs` |
| Recursos de localizacion | `Ums.Globalization/Resources/{language}/domain-errors.json` |
| Extraccion segura web | `src/application/errors/http-error.ts` |
| Composicion de notificacion | `src/application/hooks/use-notified-mutation.ts` |
| Descarte manual | `src/application/stores/notification.store.ts` y `src/presentation/shared/components/ToastQueue.tsx` |

## Requisitos de Verificacion

- Las pruebas de integracion afirman campos de validacion accionables, `errorId` generado por servidor, header `X-Error-Id` coincidente y `traceId`.
- Las pruebas de seguridad afirman que stack traces, nombres de excepcion y mensajes internos crudos estan ausentes.
- Las pruebas frontend afirman que solo mensajes aprobados y codigos de seguimiento se renderizan.
- Las pruebas de notificaciones afirman que el cierre manual elimina la notificacion seleccionada.

---

**[Registro ADR](./index.md)** | **[Blueprint de Notificaciones y Feedback](../blueprints-es/notification-feedback-architecture.md)**
