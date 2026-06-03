# UMS - Sistema Empresarial de Gestion de Usuarios

UMS es un monolito modular para identidad, autorizacion, configuracion, aprobaciones, cumplimiento, IGA y auditoria. El repositorio sigue la linea base de Evolith y documenta cualquier desviacion local mediante ADRs y el hub documental del proyecto.

## Idioma

- Portal documental en ingles: [README](./README.md)
- Resumen del repositorio: [Root README](../README.md)
- Indice maestro en ingles: [Indice Maestro](./MASTER_INDEX.md)
- Indice maestro en espanol: [Indice Maestro](./MASTER_INDEX.es.md)

## Rutas Rapidas

| Necesidad | Abrir esto |
|---|---|
| Estandares y referencias upstream de Evolith | [Acceso Rapido a Estandares](./STANDARDS.md) |
| Mapa completo de documentacion en ingles | [Indice Maestro](./MASTER_INDEX.md) |
| Mapa completo de documentacion en espanol | [Indice Maestro](./MASTER_INDEX.es.md) |
| Navegacion corta por equipo u objetivo | [Navegacion Rapida](./governance/quick-navigation.es.md) |
| Portal de arquitectura | [Portal de Arquitectura](./architecture/index.es.md) |
| Portal de gobernanza | [Portal de Gobernanza](./governance/index.es.md) |

## Vista General

| Area | Decision autoritativa |
|---|---|
| Backend | .NET 10, SQL Server 2022, EF Core |
| Frontend | React 18, Vite, TypeScript |
| Monorepo | Nx, npm workspaces |
| Metodo de entrega | BMAD-METHOD, Arquitectura Limpia, DDD |
| Multi tenancy | Filtrado de tenant en la capa de aplicacion como mecanismo principal y SQL Server RLS como failsafe secundario |

## Flujos Locales

Todos los comandos tecnicos deben ejecutarse desde `src/`.

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

### Validacion y contexto

```bash
cd src
python ../.bmad-core/scripts/validate_docs_consistency.py README.md docs/
```

## Mapa Documental

| Portal | Proposito |
|---|---|
| [Acceso Rapido a Estandares](./STANDARDS.md) | Referencias aplicadas UMS y estandares Evolith |
| [Indice Maestro](./MASTER_INDEX.md) | Mapa de ciclo de vida de la documentacion en ingles |
| [Indice Maestro](./MASTER_INDEX.es.md) | Mapa de ciclo de vida de la documentacion en espanol |
| [Portal de Gobernanza](./governance/index.es.md) | Direccion de producto, requerimientos, backlog y construccion |
| [Portal de Arquitectura](./architecture/index.es.md) | Arquitectura, planos, ADRs y trazabilidad |
| [Portal de Operaciones](./operations/index.es.md) | Runbooks, metricas y guia operativa |

## Notas de Gobernanza

- La documentacion bilingue debe mantenerse sincronizada.
- Los archivos Markdown deben permanecer limpios, profesionales y sin iconos decorativos.
- Las decisiones de arquitectura deben seguir alineadas con los ADRs aprobados y con la linea base de Evolith.
- La documentacion raiz debe mantenerse corta y facil de navegar. El detalle vive en `docs/`.

## Licencia

Consulta [LICENSE](../LICENSE) y [NOTICE](../NOTICE).
