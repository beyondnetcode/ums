# ADR-0076: Almacenamiento UTC, Detección de Timezone del Browser y Resolución de Idioma

**Estado:** Aceptado  
**Fecha:** 2026-06-02  
**Contexto:** Plataforma Evolith — Estándar Raíz (aplica a todos los sistemas hijos: UMS y productos futuros)  
**Reemplaza:** Ninguno  
**Relacionado:** ADR-0052 (Inmutabilidad del Audit Trail), ADR-0056 (Clean Architecture Frontend)

---

## Contexto

Los sistemas distribuidos que abarcan múltiples países y zonas horarias deben manejar fechas, horas y preferencias de idioma de manera consistente en todas las capas. Sin un estándar explícito, los equipos toman decisiones incompatibles: algunos almacenan hora local, otros usan formateo por defecto del browser, otros codifican identificadores de idioma en duro. Cuando los datos cruzan límites de sistemas o se muestran a usuarios en zonas horarias distintas, estas inconsistencias generan errores de visualización, discrepancias en auditorías y fallos de cumplimiento.

Tres preocupaciones independientes deben abordarse juntas porque interactúan en la inicialización de la sesión:

1. **Almacenamiento de fechas y horas**: ¿La base de datos almacena UTC u hora local?
2. **Detección de timezone**: ¿Cómo sabe el sistema dónde está el usuario y qué zona horaria aplicar al mostrar timestamps?
3. **Resolución de idioma**: ¿Qué idioma debe usar la UI y cuál es la cadena de prioridad cuando múltiples fuentes proveen una preferencia?

Todos los sistemas hijos de Evolith (actual: UMS; futuros: facturación, logística, operaciones portuarias, integraciones ERP) deben seguir el mismo estándar para que los trails de auditoría, delegaciones y eventos entre sistemas sean interpretables sin ambigüedad temporal.

---

## Decisiones

### D1 — Todas las fechas se almacenan y transmiten en UTC

**Regla:** Toda fecha o timestamp almacenado en cualquier tabla de base de datos, evento de dominio, mensaje outbox o cuerpo de respuesta API **debe** ser UTC.

- **Backend (C#):** Usar `DateTime.UtcNow` o `DateTimeOffset.UtcNow`. Nunca usar `DateTime.Now`.
- **Convención de nombres de columnas:** Sufijo `UtcNow` al nivel de dominio y `Utc` al nivel de persistencia (ej: `CreatedAtUtc`, `DeletedAtUtc`).
- **EF Core:** Registrar un `ValueConverter<DateTime, DateTime>` en todas las propiedades `DateTime` que fuerce `DateTimeKind.Utc` en lectura, previniendo el almacenamiento silencioso en hora local en SQLite (que no almacena información de zona horaria).
- **Respuestas API:** Las cadenas ISO 8601 deben incluir el sufijo `Z` (ej: `"2026-06-02T15:30:00Z"`) para hacer UTC explícito para los consumidores.
- **Frontend:** Parsear cadenas ISO con `new Date(isoString)` — JavaScript siempre interpreta cadenas con sufijo `Z` como UTC internamente. Nunca aplicar offsets manualmente a valores UTC antes de almacenar.

**Justificación:** UTC es el único ancla sin ambigüedad para sistemas distribuidos. Las horas locales introducen brechas por horario de verano, ambigüedad de reloj de pared e inconsistencia entre datacenters. Almacenar UTC y convertir en el momento de visualización es el estándar de la industria (IANA, RFC 3339, ISO 8601).

---

### D2 — El timezone del browser se detecta al inicio de sesión y se almacena en la sesión

**Regla:** Al hacer login, el frontend detecta el timezone IANA del browser usando `Intl.DateTimeFormat().resolvedOptions().timeZone` y lo almacena en el estado de sesión autenticado. Se envía al backend como header `X-Timezone` en cada llamada API.

- **Detección:** `Intl.DateTimeFormat().resolvedOptions().timeZone` retorna un identificador IANA (ej: `"America/Lima"`, `"Europe/Madrid"`). Soportado en todos los browsers modernos.
- **Cadena de fallback:**
  1. Timezone IANA detectado por el browser (primario — refleja la ubicación real del usuario)
  2. Parámetro de tenant `UI_TIMEZONE_DEFAULT` (configurado por el admin del tenant, ej: `"America/Lima"`)
  3. Default del sistema hardcodeado `"UTC"` (último recurso únicamente)
- **Precedencia:** La detección del browser siempre gana sobre la configuración del tenant. El parámetro del tenant sirve solo como default cuando la detección falla o retorna `undefined`.
- **Backend:** El header `X-Timezone` se valida contra la base de datos de zonas horarias IANA y se almacena en el contexto del request. Se usa para formateo de fechas del lado del servidor (ej: generación de reportes, timestamps en emails).
- **Visualización:** Todos los valores de fecha/hora mostrados al usuario se convierten de su valor UTC almacenado a la zona horaria de la sesión usando `Intl.DateTimeFormat` con una opción `timeZone` explícita.

**Justificación:** El browser conoce la zona horaria real del usuario sin requerir configuración manual. El parámetro del tenant cubre casos borde (terminales kiosko, sesiones SSO desde browsers externos) mientras sigue siendo el default sensato para la mayoría de usuarios en la misma región que el tenant.

---

### D3 — La resolución de idioma sigue una cadena de prioridad estricta

**Regla:** El idioma de la UI se resuelve en la inicialización de la sesión en este orden:

| Prioridad | Fuente | Mecanismo |
|---|---|---|
| 1 (más alta) | Header HTTP `Accept-Language` del browser | Leído por `CultureMiddleware` del backend, validado contra locales soportados |
| 2 | Parámetro de tenant `UI_LANGUAGE_DEFAULT` | Leído desde el caché de configuración en memoria al hacer login |
| 3 (más baja) | Default hardcodeado de la plataforma `"es"` | Fallback final |

- **`CultureMiddleware` del backend** lee `Accept-Language`, extrae el código de idioma primario (primeros 2 caracteres, en minúsculas: `"es-PE"` → `"es"`), lo valida contra la lista de locales soportados y establece `CultureInfo.CurrentCulture` para el request.
- **Respuesta de login:** El idioma resuelto se incluye en `LoginSuccessResponse.Language` Y en `SessionParameters.DefaultLanguage`.
- **Store i18n del frontend:** Al hacer login exitoso, el frontend lee `sessionParameters.defaultLanguage` de la respuesta y llama `useI18nStore.setLanguage(lang)` para inicializar el idioma activo de la UI para toda la sesión.
- **Idiomas soportados:** Los sistemas declaran explícitamente su lista de locales soportados. Los códigos de idioma no soportados caen al siguiente nivel de prioridad. Para UMS: `["es", "en"]`.
- **Formateo de fechas:** Todas las llamadas a `formatDate`, `formatDateTime` y `formatRelativeTime` **deben** recibir el locale activo del store i18n. Las funciones no deben usar un locale hardcodeado.

**Justificación:** La preferencia del browser refleja lo que el usuario configuró a nivel de SO. Respetarla no requiere configuración manual y cubre la mayoría de casos. El admin del tenant puede controlar el fallback para entornos donde la configuración del browser no es representativa (kioscos compartidos, dispositivos gestionados).

---

## Consecuencias

### Positivas
- El almacenamiento UTC elimina toda ambigüedad por DST, cambios de reloj e inconsistencia entre datacenters.
- Las sesiones siempre muestran fechas en la zona horaria real del usuario sin configuración.
- La inicialización del idioma es automática; los usuarios ven el sistema en su idioma preferido desde el primer login.
- Los trails de auditoría entre sistemas (UMS → facturación → logística) comparten el mismo marco de referencia temporal.

### Negativas / Trade-offs
- Los convertidores UTC de EF Core añaden complejidad menor a la configuración del DbContext.
- La detección de timezone del browser requiere la API `Intl` (disponible en todos los browsers modernos; no es una restricción real).
- El header `X-Timezone` añade un overhead menor a cada request (una cadena, negligible).
- La conversión de visualización (UTC → local) debe aplicarse consistentemente; olvidarla en cualquier componente es un bug silencioso. Las puertas de code review deben verificar la visualización directa de fechas UTC sin conversión.

---

## Checklist de Implementación (por sistema hijo)

- [ ] Todas las propiedades `DateTime` en entidades de dominio usan `DateTime.UtcNow`.
- [ ] EF Core registra `ValueConverter` UTC para propiedades `DateTime`.
- [ ] Las respuestas API usan ISO 8601 con sufijo `Z`.
- [ ] `CultureMiddleware` lee `Accept-Language` y valida contra locales soportados.
- [ ] La respuesta de login incluye `Language` (resuelto) y `SessionParameters.DefaultTimezone`.
- [ ] El frontend detecta `Intl.DateTimeFormat().resolvedOptions().timeZone` al hacer login.
- [ ] El frontend almacena el timezone en la sesión y envía el header `X-Timezone`.
- [ ] El store i18n del frontend se inicializa desde `sessionParameters.defaultLanguage` al hacer login.
- [ ] Todas las llamadas a `formatDate`/`formatDateTime` pasan el locale activo y el timezone de la sesión.

---

## Referencias

- [Base de Datos de Zonas Horarias IANA](https://www.iana.org/time-zones)
- [RFC 3339 — Fecha y Hora en Internet](https://tools.ietf.org/html/rfc3339)
- [ISO 8601 — Formato de fecha y hora](https://www.iso.org/iso-8601-date-and-time-format.html)
- [MDN: Intl.DateTimeFormat](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DateTimeFormat)
- UMS ADR-0052: Inmutabilidad del Audit Trail
- UMS ADR-0056: Clean Architecture Frontend
