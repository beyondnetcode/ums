# Estrategia de Pruebas de Rendimiento (SaaS de Alta Disponibilidad)

## 1. Justificación y Alcance
UMS está diseñado como una plataforma SaaS empresarial de Alta Disponibilidad (HA). Ante la ausencia de un SLA base fijo, debemos anticipar escenarios altamente complejos y de estrés extremo. El alcance de las pruebas de rendimiento cubre integralmente todas las capas:

- **Web (Frontend):** Garantizar que la SPA se mantenga responsiva, maneje correctamente los escenarios `429 Too Many Requests` y gestione sin problemas la validación de caché durante alta concurrencia.
- **API (Backend):** Estresar el Monolito Modular en .NET 10, evaluando el agotamiento del pool de hilos, la resolución de GraphQL bajo grafos jerárquicos profundos y el procesamiento masivo y concurrente de JWT.
- **BD (Base de Datos):** Evaluar los límites de PostgreSQL, apuntando específicamente a la sobrecarga de la Seguridad a Nivel de Fila (RLS), la contención de bloqueos (locks) y la resiliencia en failover.

## 2. Definición de Escenarios Complejos
Diseñaremos pruebas dirigidas a los siguientes casos límite de alta complejidad:

- **Concurrencia Masiva y Profundidad Jerárquica:** 500,000 sesiones activas distribuidas en 1,500 inquilinos distintos. Los inquilinos tendrán jerarquías organizacionales profundas (hasta 10 niveles) que requerirán resolución compleja de grafos de permisos.
- **El Evento "Estampida de Caché" (Cache Stampede):** Una actualización global de permisos fuerza la expiración de las ACLs en caché para 50,000 usuarios concurrentes. El sistema debe evitar una sobrecarga masiva (thundering herd) hacia PostgreSQL.
- **Prueba de Estrés contra Fugas de Datos (Data Leakage):** Llevar la utilización de CPU y Memoria al 95% mientras se ejecutan consultas cruzadas concurrentes para garantizar que el filtro de la aplicación y el RLS (Row-Level Security) no filtren ni enruten datos erróneamente bajo inanición severa de hilos.
- **Tráfico de Ráfaga en Reportes:** Los 10 inquilinos de mayor tamaño ejecutan simultáneamente consultas pesadas de reportes (vía GraphQL) durante horas pico de transaccionalidad.

## 3. Fases de Prueba

### Fase 1: Pruebas de Carga Distribuidas (Clúster K6)
- **Objetivo:** Establecer el rendimiento base de la API en .NET bajo una resolución de cargas complejas.
- **Ejecución:** Nodos geográficos distribuidos en K6 simulando emisión de tokens y obtención de jerarquías vía GraphQL.
- **Métrica Esperada:** Asegurar un encolamiento elegante (`429 Too Many Requests`) antes de que se agote el pool de conexiones a la base de datos.

### Fase 2: Estrés de Base de Datos y RLS
- **Objetivo:** Identificar el punto de quiebre de las políticas PostgreSQL row-level security bajo un bloqueo intenso.
- **Ejecución:** JMeter generando operaciones simultáneas de ESCRITURA de alto volumen junto con operaciones masivas de LECTURA en la BD del mismo inquilino.
- **Métrica Esperada:** Cero interbloqueos (deadlocks) y una sobrecarga máxima de 15% en la ejecución de la capa RLS.

### Fase 3: Ingeniería del Caos y Resiliencia
- **Objetivo:** Verificar las capacidades de Alta Disponibilidad durante catástrofes.
- **Ejecución:** Terminar aleatoriamente los nodos de Redis (provocando estampida de caché) y forzar el failover de PostgreSQL mientras 10,000 solicitudes están en vuelo.
- **Métrica Esperada:** La aplicación debe recuperarse automáticamente en menos de 30 segundos, manteniendo una disponibilidad del 99.99% sin comprometer las fronteras de datos de los inquilinos.

## 4. Acciones Pendientes
- Aprovisionar un entorno de Staging que refleje los recursos de Producción (Clúster Redis, PostgreSQL HA replica).
- Crear scripts de escenarios de carga en K6 incorporando JWT dinámicos y cabeceras `X-Tenant-Id`.
- Establecer paneles de monitoreo en Grafana/Prometheus para rastrear específicamente la latencia añadida por el RLS.
