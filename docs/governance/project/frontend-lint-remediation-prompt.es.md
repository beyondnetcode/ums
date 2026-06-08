# Prompt de Remediacion de Lint Frontend

Usa este prompt para iniciar un thread focalizado de remediacion del gate lint actual de `app-web`.

```text
Estas trabajando en /Users/beyondnet/Source/ums. Sigue AGENTS.md, reglas BMAD y el frontend audit playbook.

Objetivo:
Hacer que `npm run lint --workspace app-web` pase sin deshabilitar reglas, eliminar pruebas, debilitar TypeScript ni saltar checks de React hooks.

Contexto de la auditoria:
- El frontend compila correctamente.
- El gate lint falla con 315 errores y 29 warnings.
- Las categorias incluyen no-explicit-any, imports/variables sin uso, no-console, formato Prettier, reglas de React hooks, refs durante render, setState sincronico en effects y hooks condicionales.
- Archivos de alto riesgo identificados:
  - src/apps/ums.web-app/src/application/hooks/use-notified-mutation.ts
  - src/apps/ums.web-app/src/application/hooks/use-focus-trap.ts
  - src/apps/ums.web-app/src/application/hooks/use-local-overrides.ts
  - src/apps/ums.web-app/src/application/authorization/hooks/use-system-suite-dashboard.ts
  - src/apps/ums.web-app/src/application/identity/hooks/use-tenant-dashboard.ts
  - src/apps/ums.web-app/src/application/identity/hooks/use-delegation-dashboard.ts

Reglas de ejecucion:
1. Ejecuta todos los comandos tecnicos desde /Users/beyondnet/Source/ums/src.
2. No deshabilites reglas ESLint globales ni locales salvo que un ADR existente lo permita explicitamente.
3. Reemplaza `any` por tipos locales precisos, `unknown` o genericos.
4. Mantén componentes compartidos libres de reglas de dominio.
5. Corrige violaciones de React hooks cambiando flujo de control, estado derivado o effects, no suprimiendo reglas.
6. Mantén el comportamiento UI salvo que una prueba demuestre que el comportamiento actual esta roto.
7. Despues de cada lote, ejecuta `npm run lint --workspace app-web`.
8. Cuando lint pase, ejecuta `npm run build --workspace app-web` y `npm run test --workspace app-web`.

Entregable:
Entrega un resumen conciso de archivos modificados, riesgo restante y salida de verificacion.
```
