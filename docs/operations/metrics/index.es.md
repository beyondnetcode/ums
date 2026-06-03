# Panel de Metricas de Soluciones

> **Idioma:** [English](./index.md) | [Espanol](./index.es.md)  
> **Ultima Actualizacion:** 2026-05-28
> **Generado Por:** Pipeline de Metricas Automatizado  
> **Indice de Arquitectura:** [Portal de Arquitectura](../../architecture/index.es.md)

---

## Resumen

Este documento proporciona una vista consolidada de las metricas de ingenieria en todas las soluciones UMS, organizadas por tipo de solucion y categoria de metrica. Cada seccion utiliza tablas colapsables para una navegacion rapida.

### Inventario de Soluciones

| Solucion | Tipo | Tecnologia | Estado |
|----------|------|------------|--------|
| `ums.api` | API | .NET 10, C# | Activo |
| `ums.web-app` | Web | React 18, TypeScript, Vite | Activo |
| `shell/*` | Librerias | .NET 10, C# (DDD, Factory, AOP, Bootstrapper) | Activo |
| `tests/load` | Pruebas | k6 | Activo |
| `tests/*` | Pruebas | xUnit, Vitest, Playwright | Activo |

---

## Navegacion Rapida

<details>
<summary><strong>1. Metricas de API (ums.api)</strong></summary>

- [1.1 Metricas de Codificacion](#11-metricas-de-codificacion)
- [1.2 Metricas de Seguridad](#12-metricas-de-seguridad)
- [1.3 Metricas de Calidad](#13-metricas-de-calidad)
- [1.4 Metricas de Pruebas](#14-metricas-de-pruebas)
- [1.5 Metricas de Uso de IA](#15-metricas-de-uso-de-ia)

</details>

<details>
<summary><strong>2. Metricas Web (ums.web-app)</strong></summary>

- [2.1 Metricas de Codificacion](#21-metricas-de-codificacion)
- [2.2 Metricas de Seguridad](#22-metricas-de-seguridad)
- [2.3 Metricas de Calidad](#23-metricas-de-calidad)
- [2.4 Metricas de Pruebas](#24-metricas-de-pruebas)
- [2.5 Metricas de Uso de IA](#25-metricas-de-uso-de-ia)

</details>

<details>
<summary><strong>3. Metricas de Librerias (shell/*)</strong></summary>

- [3.1 Metricas de Codificacion](#31-metricas-de-codificacion)
- [3.2 Metricas de Seguridad](#32-metricas-de-seguridad)
- [3.3 Metricas de Calidad](#33-metricas-de-calidad)
- [3.4 Metricas de Pruebas](#34-metricas-de-pruebas)

</details>

<details>
<summary><strong>4. Metricas de Suites de Pruebas</strong></summary>

- [4.1 Pruebas Unitarias](#41-pruebas-unitarias)
- [4.2 Pruebas de Integracion](#42-pruebas-de-integracion)
- [4.3 Pruebas de Carga](#43-pruebas-de-carga)
- [4.4 Pruebas E2E](#44-pruebas-e2e)

</details>

<details>
<summary><strong>5. Metricas Agregadas por Categoria</strong></summary>

- [5.1 Resumen de Codificacion](#51-resumen-de-codificacion)
- [5.2 Resumen de Seguridad](#52-resumen-de-seguridad)
- [5.3 Resumen de Calidad](#53-resumen-de-calidad)
- [5.4 Resumen de Pruebas](#54-resumen-de-pruebas)
- [5.5 Resumen de Uso de IA](#55-resumen-de-uso-de-ia)

</details>

---

## 1. Metricas de API (ums.api)

### 1.1 Metricas de Codificacion

<details>
<summary>Ver Metricas de Codificacion de API</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Lineas Totales de Codigo | *auto* | - | - |
| Archivos Fuente C# | *auto* | - | - |
| Complejidad Ciclomatica (prom) | *auto* | < 10 | - |
| Complejidad Ciclomatica (max) | *auto* | < 20 | - |
| Indice de Mantenibilidad | *auto* | > 60 | - |
| Ratio de Deuda Tecnica | *auto* | < 5% | - |
| Violaciones de Pureza de Dominio | *auto* | 0 | - |
| Advertencias de Referencia Nula | *auto* | 0 | - |

**Puerta de Pureza de Dominio:** El proyecto `Ums.Domain` debe contener cero referencias NuGet. Verificado por compilacion.

</details>

### 1.2 Metricas de Seguridad

<details>
<summary>Ver Metricas de Seguridad de API</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Vulnerabilidades NuGet (Criticas) | *auto* | 0 | - |
| Vulnerabilidades NuGet (Altas) | *auto* | 0 | - |
| Vulnerabilidades NuGet (Medias) | *auto* | < 5 | - |
| Alertas CodeQL (Criticas) | *auto* | 0 | - |
| Alertas CodeQL (Altas) | *auto* | 0 | - |
| Cobertura de Auth (endpoints) | *auto* | 100% | - |
| Cobertura de Filtro de Tenant | *auto* | 100% | - |
| Violaciones de Logging PII | *auto* | 0 | - |
| Violaciones CSP | *auto* | 0 | - |

**Puertas de Seguridad:**
- Auditoria NuGet se ejecuta en cada compilacion (`dotnet nuget verify`)
- Analisis estatico CodeQL se ejecuta en CI
- ADR-0062 aplica configuracion Serilog segura para PII
- ADR-0052 aplica trazabilidad de auditoria inmutable

</details>

### 1.3 Metricas de Calidad

<details>
<summary>Ver Metricas de Calidad de API</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Cobertura de Codigo - Dominio | *auto* | >= 85% | - |
| Cobertura de Codigo - Aplicacion | *auto* | >= 75% | - |
| Cobertura de Codigo - Combinada | *auto* | >= 80% | - |
| Advertencias de Compilacion | *auto* | 0 | - |
| Errores de Compilacion | *auto* | 0 | - |
| Puerta de Calidad SonarQube | *auto* | Aprobada | - |
| Importaciones No Usadas | *auto* | 0 | - |
| Lineas de Codigo Muerto | *auto* | 0 | - |

**Herramientas de Cobertura:** coverlet.collector + dotnet-reportgenerator-globaltool  
**Ejecutor:** `./coverage.sh` (local) / `./coverage.sh --ci` (aplica umbrales)

</details>

### 1.4 Metricas de Pruebas

<details>
<summary>Ver Metricas de Pruebas de API</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Pruebas Unitarias - Total | *auto* | - | - |
| Pruebas Unitarias - Aprobadas | *auto* | 100% | - |
| Pruebas Unitarias - Fallidas | *auto* | 0 | - |
| Pruebas Unitarias - Omitidas | *auto* | 0 | - |
| Pruebas de Integracion - Total | *auto* | - | - |
| Pruebas de Integracion - Aprobadas | *auto* | 100% | - |
| Pruebas de Contrato - Total | *auto* | - | - |
| Tiempo Promedio de Ejecucion | *auto* | < 5 min | - |
| Ratio de Piramide (Unit/Int/E2E) | *auto* | 70/20/10 | - |

**Proyectos de Pruebas:**
- `Ums.Domain.Test` - Invariantes de dominio, maquinas de estado, contratos de eventos
- `Ums.Application.Test` - Manejadores de comandos, guardas de auth, guardas de no encontrado
- `Ums.Presentation.IntegrationTest` - Endpoints API, EF Core, Testcontainers
- `Ums.ContractTest` - Validacion de contratos API

</details>

### 1.5 Metricas de Uso de IA

<details>
<summary>Ver Metricas de Uso de IA (API)</summary>

| Modelo | Herramienta | Cantidad de Uso | Lineas Generadas | Tasa de Aceptacion |
|--------|-------------|-----------------|------------------|-------------------|
| *auto* | *auto* | *auto* | *auto* | *auto* |
| *auto* | *auto* | *auto* | *auto* | *auto* |

**Herramientas de IA Rastreadas:**
- GitHub Copilot
- Cursor
- Claude Code
- opencode

**Recoleccion de Metricas:** Rastreado mediante etiquetas en mensajes de commit y marcadores de codigo asistido por IA.

</details>

---

## 2. Metricas Web (ums.web-app)

### 2.1 Metricas de Codificacion

<details>
<summary>Ver Metricas de Codificacion Web</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Lineas Totales de Codigo | *auto* | - | - |
| Archivos Fuente TypeScript | *auto* | - | - |
| Componentes React | *auto* | - | - |
| Complejidad Ciclomatica (prom) | *auto* | < 10 | - |
| Errores ESLint | *auto* | 0 | - |
| Advertencias ESLint | *auto* | < 10 | - |
| Violaciones Prettier | *auto* | 0 | - |
| Exportaciones No Usadas | *auto* | 0 | - |
| Tamano del Bundle (prod) | *auto* | < 500KB | - |

**Linting:** ESLint + TypeScript strict mode + Prettier  
**Compilacion:** Vite 5 con tree-shaking

</details>

### 2.2 Metricas de Seguridad

<details>
<summary>Ver Metricas de Seguridad Web</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Vulnerabilidades npm Audit (Criticas) | *auto* | 0 | - |
| Vulnerabilidades npm Audit (Altas) | *auto* | 0 | - |
| Vulnerabilidades npm Audit (Medias) | *auto* | < 5 | - |
| Violaciones CSP | *auto* | 0 | - |
| Cobertura Prevencion XSS | *auto* | 100% | - |
| Cobertura Token CSRF | *auto* | 100% | - |
| Violaciones de Fijacion de Dependencias | *auto* | 0 | - |
| Fugas de Secretos | *auto* | 0 | - |

**Puertas de Seguridad:**
- `npm audit` se ejecuta en CI en cada PR
- Esquemas Zod para validacion en tiempo de ejecucion
- Encabezados CSP mediante configuracion de Nginx

</details>

### 2.3 Metricas de Calidad

<details>
<summary>Ver Metricas de Calidad Web</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Cobertura de Codigo - Lineas | *auto* | >= 60% | - |
| Cobertura de Codigo - Ramas | *auto* | >= 50% | - |
| Cobertura de Codigo - Funciones | *auto* | >= 60% | - |
| Cobertura de Codigo - Sentencias | *auto* | >= 60% | - |
| Errores TypeScript Strict | *auto* | 0 | - |
| Variables No Usadas | *auto* | 0 | - |
| Ratio de Reuso de Componentes | *auto* | > 70% | - |

**Herramientas de Pruebas:** Vitest + React Testing Library  
**Herramientas de Cobertura:** Cobertura integrada Vitest (v8)

</details>

### 2.4 Metricas de Pruebas

<details>
<summary>Ver Metricas de Pruebas Web</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Pruebas Unitarias - Total | *auto* | - | - |
| Pruebas Unitarias - Aprobadas | *auto* | 100% | - |
| Pruebas Unitarias - Fallidas | *auto* | 0 | - |
| Pruebas E2E - Total | *auto* | - | - |
| Pruebas E2E - Aprobadas | *auto* | 100% | - |
| Pruebas E2E - Fallidas | *auto* | 0 | - |
| Pruebas de Componentes - Total | *auto* | - | - |
| Tiempo Promedio de Ejecucion | *auto* | < 3 min | - |

**Frameworks de Pruebas:**
- Vitest - Pruebas unitarias y de componentes
- Playwright - Pruebas E2E (planificado)
- React Testing Library - Pruebas de comportamiento de componentes

</details>

### 2.5 Metricas de Uso de IA

<details>
<summary>Ver Metricas de Uso de IA (Web)</summary>

| Modelo | Herramienta | Cantidad de Uso | Lineas Generadas | Tasa de Aceptacion |
|--------|-------------|-----------------|------------------|-------------------|
| *auto* | *auto* | *auto* | *auto* | *auto* |
| *auto* | *auto* | *auto* | *auto* | *auto* |

**Herramientas de IA Rastreadas:**
- GitHub Copilot
- Cursor
- Claude Code
- opencode

</details>

---

## 3. Metricas de Librerias (shell/*)

### 3.1 Metricas de Codificacion

<details>
<summary>Ver Metricas de Codificacion de Librerias</summary>

| Libreria | LOC | Archivos | Complejidad | Mantenibilidad |
|----------|-----|----------|-------------|----------------|
| BeyondNetCode.Shell.Ddd | *auto* | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Factory | *auto* | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Aop | *auto* | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Bootstrapper | *auto* | *auto* | *auto* | *auto* |

**Estandares de Librerias:**
- ADR-0054: Aislamiento de Librerias Shell
- Cero dependencias externas (kernel puro)
- Promovidas via NuGet despues de puerta de estabilidad

</details>

### 3.2 Metricas de Seguridad

<details>
<summary>Ver Metricas de Seguridad de Librerias</summary>

| Metrica | Valor | Umbral | Estado |
|---------|-------|--------|--------|
| Vulnerabilidades NuGet | *auto* | 0 | - |
| Bloques de Codigo Unsafe | *auto* | 0 | - |
| Uso de Reflexion | *auto* | Documentado | - |
| Carga Dinamica de Ensamblados | *auto* | 0 | - |

</details>

### 3.3 Metricas de Calidad

<details>
<summary>Ver Metricas de Calidad de Librerias</summary>

| Libreria | Cobertura | Advertencias Compilacion | Estabilidad API |
|----------|-----------|--------------------------|-----------------|
| BeyondNetCode.Shell.Ddd | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Factory | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Aop | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Bootstrapper | *auto* | *auto* | *auto* |

</details>

### 3.4 Metricas de Pruebas

<details>
<summary>Ver Metricas de Pruebas de Librerias</summary>

| Libreria | Pruebas Unitarias | Pruebas Integracion | Tiempo Prom Ejec |
|----------|-------------------|---------------------|------------------|
| BeyondNetCode.Shell.Ddd | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Factory | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Aop | *auto* | *auto* | *auto* |
| BeyondNetCode.Shell.Bootstrapper | *auto* | *auto* | *auto* |

</details>

---

## 4. Metricas de Suites de Pruebas

### 4.1 Pruebas Unitarias

<details>
<summary>Ver Resumen de Pruebas Unitarias</summary>

| Solucion | Total | Aprobadas | Fallidas | Omitidas | Cobertura |
|----------|-------|-----------|----------|----------|-----------|
| ums.api (Dominio) | *auto* | *auto* | *auto* | *auto* | *auto* |
| ums.api (Aplicacion) | *auto* | *auto* | *auto* | *auto* | *auto* |
| ums.web-app | *auto* | *auto* | *auto* | *auto* | *auto* |
| shell/* | *auto* | *auto* | *auto* | *auto* | *auto* |

</details>

### 4.2 Pruebas de Integracion

<details>
<summary>Ver Resumen de Pruebas de Integracion</summary>

| Solucion | Total | Aprobadas | Fallidas | Testcontainers | Tiempo Prom Ejec |
|----------|-------|-----------|----------|----------------|------------------|
| ums.api (Presentacion) | *auto* | *auto* | *auto* | Si | *auto* |
| ums.api (Contrato) | *auto* | *auto* | *auto* | No | *auto* |

</details>

### 4.3 Pruebas de Carga

<details>
<summary>Ver Resumen de Pruebas de Carga</summary>

| Endpoint | VUs | Duracion | p95 (ms) | p99 (ms) | Fallos | RPS |
|----------|-----|----------|----------|----------|--------|-----|
| *auto* | *auto* | *auto* | *auto* | *auto* | *auto* | *auto* |

**Umbrales (desde config k6):**
- p95 < 500ms
- p99 < 1500ms
- Tasa de fallos < 1%
- Rendimiento > 100 RPS

**Documentacion:** [README de Pruebas de Carga](../../../tests/load/README.md)

</details>

### 4.4 Pruebas E2E

<details>
<summary>Ver Resumen de Pruebas E2E</summary>

| Escenario | Navegador | Estado | Duracion | Captura |
|-----------|-----------|--------|----------|---------|
| *auto* | *auto* | *auto* | *auto* | *auto* |

**Framework:** Playwright (planificado)  
**Navegadores:** Chromium, Firefox, WebKit

</details>

---

## 5. Metricas Agregadas por Categoria

### 5.1 Resumen de Codificacion

<details>
<summary>Ver Resumen de Codificacion en Todas las Soluciones</summary>

| Solucion | LOC | Archivos | Complejidad Prom | Complejidad Max | Deuda Tecnica |
|----------|-----|----------|------------------|-----------------|---------------|
| ums.api | *auto* | *auto* | *auto* | *auto* | *auto* |
| ums.web-app | *auto* | *auto* | *auto* | *auto* | *auto* |
| shell/* | *auto* | *auto* | *auto* | *auto* | *auto* |
| **Total** | **auto** | **auto** | **auto** | **auto** | **auto** |

</details>

### 5.2 Resumen de Seguridad

<details>
<summary>Ver Resumen de Seguridad en Todas las Soluciones</summary>

| Solucion | Vulns Criticas | Vulns Altas | Vulns Medias | Alertas CodeQL | Cobertura Auth |
|----------|---------------|-------------|--------------|----------------|----------------|
| ums.api | *auto* | *auto* | *auto* | *auto* | *auto* |
| ums.web-app | *auto* | *auto* | *auto* | *auto* | *auto* |
| shell/* | *auto* | *auto* | *auto* | *auto* | N/A |
| **Total** | **auto** | **auto** | **auto** | **auto** | **auto** |

</details>

### 5.3 Resumen de Calidad

<details>
<summary>Ver Resumen de Calidad en Todas las Soluciones</summary>

| Solucion | Cobertura | Advertencias Compilacion | Errores Lint | Puerta Sonar |
|----------|-----------|--------------------------|--------------|--------------|
| ums.api (Dominio) | *auto* | *auto* | N/A | *auto* |
| ums.api (Aplicacion) | *auto* | *auto* | N/A | *auto* |
| ums.web-app | *auto* | *auto* | *auto* | *auto* |
| shell/* | *auto* | *auto* | N/A | *auto* |

</details>

### 5.4 Resumen de Pruebas

<details>
<summary>Ver Resumen de Pruebas en Todas las Soluciones</summary>

| Solucion | Unitarias | Integracion | E2E | Carga | Cobertura Total |
|----------|-----------|-------------|-----|-------|-----------------|
| ums.api | *auto* | *auto* | N/A | N/A | *auto* |
| ums.web-app | *auto* | N/A | *auto* | N/A | *auto* |
| shell/* | *auto* | N/A | N/A | N/A | *auto* |
| tests/load | N/A | N/A | N/A | *auto* | N/A |
| **Total** | **auto** | **auto** | **auto** | **auto** | **auto** |

</details>

### 5.5 Resumen de Uso de IA

<details>
<summary>Ver Resumen de Uso de IA en Todas las Soluciones</summary>

| Modelo | Uso Total | Lineas Generadas | Tasa de Aceptacion | Solucion Principal |
|--------|-----------|------------------|-------------------|-------------------|
| *auto* | *auto* | *auto* | *auto* | *auto* |
| *auto* | *auto* | *auto* | *auto* | *auto* |

**Desglose por Herramienta:**

| Herramienta | Cantidad de Uso | Modelos Usados | Duracion Prom Ses |
|-------------|-----------------|----------------|-------------------|
| *auto* | *auto* | *auto* | *auto* |
| *auto* | *auto* | *auto* | *auto* |

</details>

---

## Trazabilidad de Arquitectura

Todas las metricas en este documento se remontan a los siguientes activos de arquitectura:

| Activo | Enlace | Relevancia |
|--------|--------|------------|
| Portal de Arquitectura | [index.es.md](../../architecture/index.es.md) | Referencia principal de arquitectura |
| Registro ADR | [adrs/index.es.md](../../architecture/adrs/index.es.md) | Trazabilidad de decisiones |
| Matriz de Trazabilidad | [traceability-matrix.md](../../architecture/traceability-matrix.md) | Mapeo FS-a-ADR-a-TE |
| Portal de Diseno DDD | [ddd-design/index.es.md](../../governance/construction/ddd-design/index.es.md) | Metricas de modelo de dominio |
| Rastreador de Implementacion API | [api-aggregate-implementation-tracker.md](../../governance/project/api-aggregate-implementation-tracker.md) | Completitud por agregado |
| Estrategia de Cobertura | [coverage-strategy.md](../../../src/apps/ums.api/docs/testing/coverage-strategy.md) | Umbrales de cobertura |

---

## Automatizacion

Este documento se actualiza automaticamente despues de cada commit a las ramas `main` o `develop`. El proceso de actualizacion:

1. Ejecuta herramientas de analisis estatico (CodeQL, ESLint, dotnet build)
2. Ejecuta suites de pruebas y recolecta cobertura
3. Agrega metricas de artefactos CI
4. Actualiza este documento con valores actuales
5. Realiza commit de cambios con etiqueta `[ci skip]`

**Script de Actualizacion:** `src/scripts/update-metrics.sh`  
**Workflow:** `.github/workflows/update-metrics.yml`

---

## Cumplimiento BMAD

Este documento cumple con las siguientes reglas BMAD:

| Regla | Cumplimiento |
|-------|--------------|
| R-01: Sincronizacion de Documentacion Bilingue | Espejo en espanol mantenido |
| R-03: Integridad de Codificacion UTF-8 | Validado |
| R-14: Profesionalismo de Documentacion | Sin emojis ni caracteres decorativos |
| R-13: Estandar de Estructuracion Empresarial | Sigue taxonomia de nombres |

---

**[Volver al Portal de Operaciones](../index.es.md)** | **[Volver al Indice Maestro](../../MASTER_INDEX.es.md)** | **[Volver al Portal de Arquitectura](../../architecture/index.es.md)**
