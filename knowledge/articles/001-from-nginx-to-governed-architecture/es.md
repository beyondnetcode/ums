# De una Pregunta Simple sobre nginx a una Arquitectura Gobernada

*Cómo una pregunta de infraestructura reveló la forma correcta de gestionar decisiones arquitecturales entre repositorios.*

---

Hace unos días estaba revisando la infraestructura de un producto SaaS que estoy construyendo — un User Management System (UMS) — cuando surgió una pregunta aparentemente sencilla:

> "No veo los detalles de nginx en el código fuente. ¿Lo están configurando a nivel de Docker?"

Esa pregunta, que parecía trivial al principio, terminó desencadenando una cadena de decisiones arquitecturales que quiero compartir — porque la forma en que la respondimos dice mucho sobre cómo los equipos maduros deberían gestionar la evolución técnica.

---

## El punto de partida: nginx embebido en el frontend

La aplicación web de UMS usa un build Docker multi-stage. La etapa final toma los assets compilados de React y los sirve a través de nginx. El archivo `nginx.conf` vive junto al código del frontend:

```
ums/src/apps/ums.web-app/
├── nginx.conf          ← headers de seguridad, reverse proxy hacia la API
├── Dockerfile          ← copia nginx.conf dentro de la imagen
└── dist/               ← app React compilada
```

La configuración tiene dos responsabilidades:

1. **Servir archivos estáticos** — los archivos del SPA en React
2. **Reverse proxy** — enrutar las peticiones de `/api/**` y `/graphql` hacia el backend en .NET

```nginx
server {
    listen 80;

    # Headers de seguridad
    add_header X-Frame-Options "DENY" always;
    add_header Content-Security-Policy "..." always;
    add_header Strict-Transport-Security "..." always;

    # Proxy hacia el backend
    location /api/ {
        proxy_pass http://ums-api:8080;
    }

    # Fallback SPA
    location / {
        root /usr/share/nginx/html;
        try_files $uri $uri/ /index.html;
    }
}
```

**¿Está bien esto?** Sí — para un solo cliente. La configuración está versionada junto al código que sirve, los headers de seguridad están declarados explícitamente, y el build Docker multi-stage es el patrón correcto.

---

## El momento en que la pregunta cambia

Cuando mencioné que UMS está planificado como un producto SaaS con al menos una app web y una app móvil en el futuro cercano, todo el enfoque cambió.

Con un solo cliente, nginx embebido es cohesivo y simple. Con múltiples clientes:

- Los headers de seguridad se duplican por cliente o están ausentes en alguno.
- La app móvil bypasea nginx completamente — sin enforcement centralizado.
- El rate limiting, el enrutamiento por tenant y la validación de tokens no pueden compartirse.
- Cualquier cambio de política requiere reconstruir el contenedor del frontend.

nginx fue diseñado para servir archivos y proxy tráfico — no para ser un API Gateway en un SaaS multi-cliente.

---

## La evolución correcta: YARP como API Gateway centralizado

UMS ya tenía `Yarp.ReverseProxy` declarado como dependencia. YARP (Yet Another Reverse Proxy) es la librería de reverse proxy de Microsoft para ASP.NET Core — y es exactamente lo que un equipo nativo en .NET debería usar cuando necesita un gateway.

La arquitectura objetivo:

```
Internet
    │
    ▼
┌──────────────────────────────┐
│  ums.gateway  (YARP)         │  ← punto de entrada único
│  Headers de seguridad        │
│  Rate limiting por tenant    │
│  Enrutamiento por tenant     │
└────────────┬─────────────────┘
             │
    ┌────────┴──────────┐
    ▼                   ▼
┌──────────┐      ┌──────────────┐
│ ums.api  │      │ ums.web-app  │  ← nginx: solo archivos estáticos
│ :8080    │      │ :80          │
└──────────┘      └──────────────┘
                        ▲
                  ums.mobile-app
                  (cliente futuro)
```

nginx se convierte en un servidor de archivos estáticos puro — que es para lo que mejor sirve. El gateway, escrito en C#, comparte el pipeline de middleware existente, la instrumentación OpenTelemetry y el contexto de tenant de forma nativa.

**Pero aquí está la parte importante: no lo implementamos de inmediato.**

La decisión se documentó como un ADR (Architecture Decision Record) con estado *Propuesto* y un disparador explícito:

> *No implementar hasta que se inicie `ums.mobile-app` o el enrutamiento multi-cliente se convierta en un requisito de producto confirmado.*

Esto no es procrastinación. Es evitar complejidad prematura. El enfoque actual con nginx es completamente válido para el MVP con un solo cliente web. Construir el gateway antes de que exista el segundo cliente es especulación.

---

## La segunda pregunta: ¿deberíamos separar la API en tiers?

Mientras revisábamos la decisión del gateway, surgió una pregunta relacionada:

> "¿Deberían las queries y los commands vivir en tiers de API separados? ¿Un servicio para lecturas GraphQL y otro para escrituras REST?"

UMS ya los separa a nivel de protocolo — GraphQL para queries, REST para commands. La pregunta era si esa separación debería convertirse en una separación de despliegue.

La respuesta fue no — y el razonamiento importa:

**CQRS separa los *modelos* de lectura y escritura, no las *unidades de despliegue*.**

La separación ya existe en tres niveles:

| Nivel | Separación |
|---|---|
| Protocolo | GraphQL (queries) vs REST (commands) |
| Código | Handlers distintos, clientes HTTP distintos |
| Routing | `/graphql` vs `/api/v1/...` |

Separar en tiers de despliegue duplicaría el costo operacional — dos Dockerfiles, dos health checks, dos políticas de escalado, dos pools de conexiones — sin ningún beneficio medible a la carga actual.

Para un producto SaaS, hay una consideración adicional: en un entorno multi-tenant, las queries pesadas de GraphQL de un tenant grande podrían impactar la latencia de los commands críticos (login, provisioning). Esto es real. Pero la mitigación correcta a escala MVP es:

1. Límites de complejidad de queries GraphQL a nivel del schema
2. Timeouts diferenciados por tipo de operación
3. Rate limiting por tenant en la capa del gateway

La separación en tiers es el *camino de escalada* — no el punto de partida. Esta decisión fue documentada en el ADR-0059 con disparadores explícitos para cuándo debe revisarse.

---

## El patrón de gobernanza detrás de las decisiones

Aquí es donde la historia se vuelve más interesante para equipos que gestionan múltiples repositorios.

UMS está construido sobre una referencia de arquitectura corporativa — [arc32 Progressive Monolith](https://github.com/beyondnetcode/arc32_progresive_monolith) — que define decisiones base para todos los productos de la organización: convenciones de nombres, patrones de infraestructura, estrategia de API gateway, diseño de event bus, y más.

UMS hereda esta línea base. Pero UMS también toma sus propias decisiones que divergen de ella. La pregunta es: **¿cómo gestionas esa divergencia sin perder la referencia?**

La respuesta es un modelo de herencia de tres modos:

| Modo | Cuándo usarlo | Ejemplo UMS |
|---|---|---|
| **Adoptar** | La política base aplica tal cual | ADR-0050: Taxonomía de nombres adoptada literalmente |
| **Extender** | La política base aplica pero necesita restricciones de dominio | ADR-0052: Audit trail inmutable extendido con especificidades SQL Server |
| **Anular** | UMS diverge con justificación explícita | ADR-0059: Tier API único — CQRS a nivel de protocolo, no de despliegue |

Cada anulación debe responder tres preguntas:

1. **¿Por qué** la decisión base no aplica aquí?
2. **¿Cuál** es la alternativa con su propio análisis de trade-offs?
3. **¿Cuándo** debería revisarse esta decisión?

Esa tercera pregunta se omite con frecuencia — y es la más importante. Una anulación sin disparador de reversión se convierte en deuda invisible. Los futuros desarrolladores asumen que era el camino intencional, no una divergencia deliberada que debería reevaluarse.

---

## Cómo se ve esto en la práctica

Las decisiones de esta sesión produjeron cuatro artefactos, todos versionados en el repositorio:

**ADR-0058** — Documenta la decisión de evolucionar hacia un gateway YARP, con estado *Propuesto* y un disparador de implementación explícito.

**ADR-0059** — Documenta por qué UMS usa un tier de API único, con el análisis completo de trade-offs y los disparadores para cuándo debe revisarse.

**TE-07** — Un Technical Enabler blueprint con la ruta de migración completa: paso a paso, nivel de riesgo por paso, y la configuración simplificada de nginx que resulta.

**Actualización del Architecture Portal** — El modelo de gobernanza (Adoptar / Extender / Anular) está documentado en el Architecture Portal para que cada nuevo desarrollador entienda el contrato de herencia desde el primer día.

Y en el repositorio base arc32, se agregó una nueva sección al Child Repository Inheritance Guide usando UMS como implementación de referencia — para que los futuros equipos que arranquen productos satelite tengan un ejemplo real que seguir, no solo teoría.

---

## Conclusiones clave

**Para CTOs y Tech Leads:**

- Las decisiones arquitecturales deben documentarse con el *disparador para cambiarlas*, no solo con el razonamiento para tomarlas. Una decisión sin fecha y sin disparador se convierte en deuda invisible.
- El momento correcto para introducir complejidad de infraestructura (gateways, separaciones en tiers) es cuando un requisito específico y concreto lo exige — no cuando puedes imaginar un futuro que podría necesitarlo.
- Si mantienes una línea base de arquitectura corporativa, define modos explícitos para que los productos puedan divergir de ella. El silencio no es adopción — es riesgo.

**Para Desarrolladores Senior y Arquitectos:**

- CQRS no implica unidades de despliegue separadas. La separación a nivel de protocolo (GraphQL/REST) es una forma válida y frecuentemente suficiente de separación CQRS.
- YARP es la elección correcta para un equipo nativo en .NET que necesita un gateway programable — se integra nativamente con el pipeline de middleware, el contenedor DI y OpenTelemetry.
- nginx dentro de un contenedor de frontend es un patrón legítimo para aplicaciones de un solo cliente. En el momento en que tienes múltiples clientes, centraliza el gateway.
- El rate limiting por tenant en un gateway SaaS no es infraestructura opcional — es la primera línea de defensa contra problemas de *noisy neighbor* a escala.

---

## El repositorio

UMS y arc32 son open source. Los ADRs, Technical Enablers y documentos de gobernanza referenciados en este artículo están disponibles públicamente:

- **UMS:** https://github.com/beyondnetcode/ums
- **arc32 base:** https://github.com/beyondnetcode/arc32_progresive_monolith

---

*¿Has enfrentado decisiones similares en tu propio stack? ¿Cómo gestionas la divergencia arquitectural entre una línea base corporativa y las decisiones locales de un equipo de producto? Me interesa conocer tu enfoque.*
