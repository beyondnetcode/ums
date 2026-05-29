# ADR-0064: Taxonomía Lean Root Repository

**Estado:** Aceptado
**Fecha:** 2026-05-24
**Responsable de Decisión:** Arquitectura

---

## Contexto

Los monorepos empresariales frecuentemente sufren de bloating en el directorio raíz. Con el tiempo, directorios de tests (`tests/`), scripts (`scripts/`), archivos de configuración (`NuGet.Config`), y artículos de conocimiento dispersos (`knowledge/`) se acumulan en el workspace raíz. Esto crea sobrecarga cognitiva para nuevos ingenieros y oscurece los puntos de entrada primarios (`README.md`, `docs/MASTER_INDEX.md`).

En UMS, la falta de una taxonomía raíz estricta permitió que concerns técnicos y de gobernanza se mezclaran en el nivel superior, creando una experiencia de desarrollador subóptima y empujando documentación crítica debajo del fold en la interfaz web de GitHub.

## Decisión

**Adoptar el patrón arquitectónico "Lean Root" (o Clean Root), enforce una dicotomía binaria estricta en la raíz del repositorio: el Technical Engine (`src/`) versus el Knowledge Hub (`docs/`).**

1. **`src/` (Technical Engine):** Todo el código ejecutable, tests, scripts de load testing, migraciones de base de datos, scripts de utilidad CI/CD, y configuraciones específicas de lenguaje (ej., `NuGet.Config`) DEBE residir dentro de `src/` o sus subdirectorios.
2. **`docs/` (Knowledge Hub):** Toda la documentación empresarial, blueprints arquitectónicos, requerimientos, y READMEs traducidos (`README.es.md`) DEBE residir dentro de `docs/`.
3. **Excepciones BMAD:** Las instrucciones de agentes IA (`AGENTS.md`) y archivos estándar open-source (`CHANGELOG.md`, `LICENSE`, `README.md`) son las ÚNICAS excepciones permitidas para permanecer en la raíz, cumpliendo estrictamente con los estándares estructurales de la metodología BMAD.

## Consecuencias

### Positivas

- **Carga Cognitiva Reducida:** El directorio raíz es instantáneamente escaneable. Los ingenieros saben exactamente a dónde ir para código (`src/`) vs. teoría (`docs/`).
- **Discoverabilidad Mejorada:** El `README.md` del repositorio y los links de navegación se muestran prominentemente "above the fold" en GitHub sin requerir scrolling past docenas de carpetas.
- **Claridad Arquitectónica:** Refuerza los bounded contexts no solo en código, sino en la gestión del repositorio.

### Trade-offs

- Los desarrolladores ejecutando scripts o tests que previamente se ejecutaban desde la raíz deben ahora cambiar su directorio de trabajo a `src/` o actualizar sus paths de comando.
- Ciertos archivos de configuración (como `NuGet.Config`) deben ser explícitamente targeted o dependerse vía mecanismos de herencia estándar desde dentro de `src/`.

## Cumplimiento

- El linter estructural del pipeline CI y los AI-Agents de BMAD enforce esta dicotomía flaggando cualquier nuevo directorio de nivel superior que viole la separación `src/` vs `docs/`.