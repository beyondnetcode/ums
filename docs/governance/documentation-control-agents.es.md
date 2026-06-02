# Agentes de Control de Documentacion -- Gobernanza UMS

## Resumen

La documentacion de UMS hereda los estandares de gobernanza de [Evolith Architecture Reference](https://github.com/beyondnetcode/evolith_arch32) mientras mantiene localmente la evidencia de implementacion especifica de UMS. Este documento establece las reglas y el proceso para que los agentes BMAD mantengan la calidad documental, la consistencia bilingue y la trazabilidad arquitectonica.

## Modelo de Herencia

```
Evolith (repositorio padre)
    │
    ├── Define estandares globales de documentacion
    ├── Proporciona reglas de gobernanza reutilizables
    ├── Establece patrones canonicos para la documentacion
    │
    ▼
UMS (repositorio satelite)
    │
    ├── Hereda las reglas aplicables de Evolith por referencia
    ├── Adapta las reglas al contexto de UMS cuando es necesario
    ├── Documenta la evidencia especifica de UMS
    └── Propone patrones exitosos de regreso a Evolith
```

### Clasificacion de Reglas

| Tipo de Regla | Origen | Ejemplo |
|---|---|---|
| Estadar empresarial global | Evolith | Convenciones de nombres, formato ADR, principios de Clean Architecture |
| Evidencia de implementacion aplicada | UMS | Referencias aplicadas de API, React y diseño de dominio |
| Adaptacion local | UMS | Ruteo especifico de UMS, organizacion de modulos, valores de runtime |
| Candidata a promocion | UMS → Evolith | Patrones sin dependencias de UMS que aplican a cualquier satelite |

## Reglas de Consistencia Bilingue

### Requisito Central

Todo artefacto documental que exista en ingles DEBE tener un equivalente en espanol, y viceversa. Las versiones en ingles y espanol DEBEN ser:

1. **Estructuralmente homogeneas** -- mismo orden de secciones, mismas tablas y misma jerarquia de encabezados
2. **Tecnicamente equivalentes** -- mismos conceptos, mismos enlaces, mismas referencias
3. **Naturalmente redactadas** -- no traducciones literales; usar terminologia tecnica apropiada en cada idioma

### Enlaces de Cambio de Idioma

Todos los puntos de entrada documentales DEBEN incluir enlaces de cambio de idioma:

- Paginas en ingles: `[Leer en espanol](./README.es.md)` (o la ruta equivalente)
- Paginas en espanol: `[Read in English](./README.md)` (o la ruta equivalente)

### Convencion de Nombres de Archivos

- Ingles: `<filename>.md`
- Espanol: `<filename>.es.md`

Cuando existe una version en espanol, los enlaces internos del documento espanol DEBEN apuntar a versiones `*.es.md` de otros documentos en espanol.

### Estandares de Terminologia

Usar los terminos tecnicos aceptados de forma apropiada en cada idioma:

| Ingles | Espanol | Contexto |
|---|---|---|
| ADR | ADR | Registros de decision |
| Backend | Backend / API | Segun el contexto |
| Frontend | Frontend / Web | Segun el contexto |
| Bounded Context | Bounded Context | Concepto DDD (se mantiene en ingles) |
| Aggregate | Agregado | Concepto DDD |
| Quick Access | Acceso Rapido | Pagina de estandares |
| Master Index | Indice Maestro | Hub de navegacion |
| Applied Reference | Referencia Aplicada | Implementacion especifica de UMS |

## Puntos de Entrada Documentales

UMS tiene los siguientes puntos de entrada primarios que DEBEN mantenerse sincronizados:

| Punto de Entrada | Ingles | Espanol |
|---|---|---|
| README raiz | `/README.md` | `/docs/README.es.md` |
| Estandares | `/docs/STANDARDS.md` | `/docs/STANDARDS.es.md` |
| Indice Maestro | `/docs/MASTER_INDEX.md` | `/docs/MASTER_INDEX.es.md` |
| Portal de Arquitectura | `/docs/architecture/index.md` | `/docs/architecture/index.es.md` |

## Proceso de Validacion de Agentes BMAD

### Antes de Cometer Cambios Documentales

1. **Ejecutar verificacion bilingue**
   - Verificar que las versiones en ingles y espanol tengan la misma estructura
   - Verificar que los enlaces apunten a las variantes correctas de idioma
   - Verificar que la terminologia sea apropiada para cada idioma

2. **Ejecutar validacion de enlaces**
   - Todos los enlaces internos resuelven a archivos existentes
   - Los enlaces externos a Evolith son validos y apuntan a la version correcta
   - No quedan referencias rotas

3. **Ejecutar validacion de diagramas**
   - Todos los diagramas Mermaid son sintacticamente correctos
   - Los diagramas coinciden con el idioma del documento

4. **Verificar cumplimiento de codificacion**
   - No hay mojibake ni artefactos de codificacion
   - No hay emojis ni caracteres decorativos no estandar (segun BMAD R-03 y R-14)
   - Ejecutar limpieza si es necesario: `python ../.bmad-core/scripts/cleanup_markdown_encoding.py`

### Uso de Playbooks

Usar el [Documentation Audit Playbook](../.harness/playbooks/documentation-audit-playbook.md) para:
- Verificacion de legibilidad de historias funcionales
- Validacion de sincronizacion bilingue
- Verificacion de ADR y coherencia de stack
- Verificacion de trazabilidad de diagramas

## Reglas Inheredadas de Evolith

Los agentes de UMS DEBEN hacer cumplir estas reglas de Evolith cuando aplique:

1. **R-03 y R-14 (Reglas Globales BMAD)**: sin artefactos de codificacion ni caracteres decorativos
2. **Estandar de ADR**: todas las decisiones arquitectonicas siguen la plantilla ADR de Evolith
3. **Estandar de Historias Funcionales**: la narrativa de negocio permanece legible; los detalles tecnicos van en una seccion dedicada
4. **Contrato Minimo de Catalogos de Configuracion**: `code`, `value`, `description` obligatorios
5. **Validacion de Diagramas**: la sintaxis Mermaid debe ser correcta antes del commit

## Reglas Locales de Documentacion de UMS

Estas reglas aplican especificamente a la documentacion de UMS:

1. **Referencias Aplicadas**: documentar evidencia especifica de implementacion UMS en `/docs/architecture/`
2. **Enlaces Evolith**: todos los estandares ascendentes deben enlazar al repositorio Evolith con la variante de idioma correcta
3. **Alcance del Producto**: las decisiones especificas de UMS permanecen locales; no generalizarlas a Evolith sin ADR
4. **Proceso de Promocion**: los patrones sin dependencias de UMS deben proponerse a Evolith via ADR
5. **Compuerta Documental para Cambios Complejos**: todo cambio complejo, transversal, arquitectonico o evolutivo debe incluir una evaluacion de impacto documental, un plan de actualizacion de documentacion y actualizacion sincronizada en ingles y espanol antes de considerarse completo.

## Lista de Verificacion

Antes de cualquier commit documental, verificar:

- [ ] Las versiones en ingles y espanol tienen estructura identica
- [ ] Los enlaces de cambio de idioma estan presentes y correctos
- [ ] Los enlaces internos resuelven archivos existentes
- [ ] Los enlaces externos a Evolith son validos
- [ ] No existen artefactos de codificacion (mojibake)
- [ ] No existen emojis ni caracteres decorativos
- [ ] Los diagramas Mermaid son sintacticamente validos
- [ ] La terminologia es apropiada para cada idioma
- [ ] La separacion UMS/Evolith es clara
- [ ] Los catalogos de configuracion cumplen con `code`, `value`, `description`
- [ ] Los cambios complejos o evolutivos incluyen una evaluacion de impacto documental y artefactos actualizados en ambos idiomas

## Mapa del Repositorio para Documentacion

| Area | Punto de Entrada | Proposito |
|---|---|---|
| Estandares | `docs/STANDARDS*.md` | Acceso rapido a estandares de Evolith y referencias aplicadas de UMS |
| Arquitectura | `docs/architecture/index*.md` | ADRs, blueprints, referencias aplicadas, trazabilidad |
| Gobernanza | `docs/governance/index*.md` | Vision del producto, requerimientos, documentacion de entrega |
| Control de Documentacion | `docs/governance/documentation-control-agents*.md` | Gobernanza documental, sincronizacion bilingue y flujo de validacion |
| Construccion | `docs/governance/construction/index*.md` | Diseno DDD, bounded contexts, agregados |
| Operaciones | `docs/operations/index.md` | Metricas, runbooks, documentacion operativa |
| Indice Maestro | `docs/MASTER_INDEX*.md` | Arbol documental completo |

## Referencias Externas

- [Evolith Architecture Reference](https://github.com/beyondnetcode/evolith_arch32)
- [Evolith Quick Access by Stack](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/quick-access/README.md)
- [Reglas Globales BMAD](../../.bmad-core/rules/global-rules.md)
- [Estandar de Estructuracion BMAD](../../.bmad-core/rules/structuring-standard.md)
- [AGENTS.md de UMS](../../AGENTS.md)

## Proceso de Cambio

Al actualizar documentacion:

1. Identificar si el cambio afecta ingles, espanol o ambos
2. Asegurar que ambas versiones se actualicen juntas
3. Ejecutar la lista de verificacion
4. Si se agrega un patron nuevo aplicable a otros satelites, proponerlo a Evolith via ADR
5. Actualizar MASTER_INDEX si se agregan nuevos documentos de primer nivel
6. Para cambios complejos o evolutivos, agregar o actualizar el ADR, historia funcional o nota de arquitectura correspondiente antes de cerrar el trabajo

---

**[Volver a AGENTS.md](../../AGENTS.md)** | **[Volver al Indice Maestro](./MASTER_INDEX.es.md)**
