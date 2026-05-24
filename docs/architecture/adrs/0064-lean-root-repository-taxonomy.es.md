# ADR-0064: Taxonomía de Repositorio "Lean Root"

**Estado:** Aceptado  
**Fecha:** 2026-05-24  
**Propietario de la Decisión:** Arquitectura  

---

## Contexto

Los monorepos corporativos frecuentemente sufren de una sobrecarga en su directorio raíz. Con el tiempo, directorios de pruebas (`tests/`), scripts (`scripts/`), archivos de configuración (`NuGet.Config`) y artículos de conocimiento dispersos (`knowledge/`) se acumulan en el espacio de trabajo principal. Esto genera una sobrecarga cognitiva para los nuevos ingenieros y oculta los puntos de entrada principales (`README.md`, `docs/MASTER_INDEX.md`).

En UMS, la falta de una taxonomía estricta en la raíz permitía que preocupaciones técnicas y de gobernanza se mezclaran en el nivel superior, creando una experiencia de desarrollo subóptima y empujando documentación crítica hacia abajo en la interfaz web de GitHub.

## Decisión

**Adoptar el patrón arquitectónico "Lean Root" (o Raíz Limpia), imponiendo una estricta dicotomía binaria en la raíz del repositorio: el Motor Técnico (`src/`) versus el Centro de Conocimiento (`docs/`).**

1. **`src/` (Motor Técnico):** Todo el código ejecutable, pruebas, scripts de pruebas de carga, migraciones de bases de datos, scripts de utilidades CI/CD y configuraciones específicas de lenguaje (ej., `NuGet.Config`) DEBEN residir dentro de `src/` o sus subdirectorios.
2. **`docs/` (Centro de Conocimiento):** Toda la documentación corporativa, planos arquitectónicos, requerimientos y READMES traducidos (`README.es.md`) DEBEN residir dentro de `docs/`.
3. **Excepciones BMAD:** Las instrucciones para agentes de IA (`AGENTS.md`) y los archivos estándar de código abierto (`CHANGELOG.md`, `LICENSE`, `README.md`) son las ÚNICAS excepciones permitidas para permanecer en la raíz, cumpliendo estrictamente con los estándares estructurales de la metodología BMAD.

## Consecuencias

### Positivas

- **Reducción de la Carga Cognitiva:** El directorio raíz es escaneable al instante. Los ingenieros saben exactamente a dónde ir para buscar código (`src/`) vs. teoría (`docs/`).
- **Mejora en la Descubribilidad:** El `README.md` del repositorio y sus enlaces de navegación se muestran de manera prominente en la primera vista de GitHub sin necesidad de desplazarse hacia abajo pasando decenas de carpetas.
- **Claridad Arquitectónica:** Refuerza los contextos limitados (bounded contexts) no solo en el código, sino en la gestión del repositorio.

### Compensaciones (Trade-offs)

- Los desarrolladores que ejecuten scripts o pruebas que antes se ejecutaban desde la raíz, ahora deben cambiar su directorio de trabajo a `src/` o actualizar las rutas de sus comandos.
- Ciertos archivos de configuración (como `NuGet.Config`) deben ser referenciados explícitamente o dependientes de los mecanismos estándar de herencia desde dentro de `src/`.

## Cumplimiento

- El linter estructural del pipeline CI y los agentes de IA de BMAD harán cumplir esta dicotomía al marcar cualquier nuevo directorio de nivel superior que viole la separación entre `src/` y `docs/`.
