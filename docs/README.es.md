# UMS - Sistema Empresarial de Gestión de Usuarios

UMS es un monolito modular para identidad, autorización, configuración, aprobaciones, cumplimiento, IGA y auditoría. El repositorio sigue la línea base de Evolith y documenta la evidencia aplicada de UMS, las decisiones locales y las desviaciones justificadas mediante ADRs.

## Idioma

- [Read in English](./README.md)
- [Leer en Español](./README.es.md)
- [README raíz](../README.md)
- [Master Index](./MASTER_INDEX.md)
- [Índice Maestro](./MASTER_INDEX.es.md)

## Rutas Rápidas

| Necesidad | Abrir esto |
|---|---|
| Estándares | [STANDARDS.md](./STANDARDS.md) |
| Estándares en español | [STANDARDS.es.md](./STANDARDS.es.md) |
| Mapa completo de documentación en inglés | [MASTER_INDEX.md](./MASTER_INDEX.md) |
| Mapa completo de documentación en español | [MASTER_INDEX.es.md](./MASTER_INDEX.es.md) |
| Navegación corta por equipo u objetivo | [Navegación Rápida](./governance/quick-navigation.es.md) |
| Portal de arquitectura | [Portal de Arquitectura](./architecture/index.es.md) |
| Portal de gobernanza | [Portal de Gobernanza](./governance/index.es.md) |

## Vista General

| Área | Decisión autoritativa |
|---|---|
| Backend | .NET 10, SQL Server 2022, EF Core |
| Frontend | React 18, Vite, TypeScript |
| Monorepo | Nx, npm workspaces |
| Método de entrega | BMAD-METHOD, Arquitectura Limpia, DDD |
| Multi-tenancy | Filtrado de tenant en la capa de aplicación como mecanismo principal y SQL Server RLS como failsafe secundario |

## Flujos Locales

Todos los comandos técnicos deben ejecutarse desde `src/`.

### Frontend

```bash
cd src
npm install
npx nx run app-web:dev
```

### Backend

```bash
cd src/apps/ums.api
dotnet build
dotnet run
```

### Validación y contexto

```bash
cd src
python ../.bmad-core/scripts/validate_docs_consistency.py README.md docs/
```

## Mapa Documental

| Portal | Propósito |
|---|---|
| [Estándares](./STANDARDS.md) | Referencias aplicadas UMS y estándares Evolith |
| [Estándares en español](./STANDARDS.es.md) | Referencias aplicadas UMS y estándares Evolith en español |
| [Master Index](./MASTER_INDEX.md) | Mapa de ciclo de vida de la documentación en inglés |
| [Índice Maestro](./MASTER_INDEX.es.md) | Mapa de ciclo de vida de la documentación en español |
| [Portal de Gobernanza](./governance/index.es.md) | Dirección de producto, requerimientos, backlog y construcción |
| [Portal de Arquitectura](./architecture/index.es.md) | Arquitectura, planos, ADRs y trazabilidad |
| [Portal de Operaciones](./operations/index.es.md) | Runbooks, métricas y guía operativa |

## Notas de Gobernanza

- La documentación bilingüe debe mantenerse sincronizada.
- Los archivos Markdown deben permanecer limpios, profesionales y sin iconos decorativos.
- Las decisiones de arquitectura deben seguir alineadas con los ADRs aprobados y con la línea base de Evolith.
- La documentación raíz debe mantenerse corta y fácil de navegar. El detalle vive en `docs/`.

## Licencia

Consulta [LICENSE](../LICENSE) y [NOTICE](../NOTICE).
