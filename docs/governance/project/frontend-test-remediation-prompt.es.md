# Prompt de Remediacion de Pruebas Frontend

Usa este prompt para iniciar un thread focalizado sobre las pruebas unitarias fallidas de `app-web`.

```text
Estas trabajando en /Users/beyondnet/Source/ums. Sigue AGENTS.md, reglas BMAD y el frontend audit playbook.

Objetivo:
Hacer que `npm run test --workspace app-web` pase sin eliminar pruebas ni debilitar assertions.

Area fallida actual:
- `src/presentation/shared/layouts/NavRail.test.tsx`
- 3 pruebas fallidas:
  - renders navigation items when module is expanded
  - toggles module expansion when clicked
  - highlights active tab

Diagnostico de auditoria:
- El fixture de prueba mockea `NAV_MODULES` con module key `identity`.
- `NavRail` inicializa modulos expandidos con keys `idm`, `auth` y `sys`.
- Como la key mockeada no coincide con el estado expandido, sus miembros quedan colapsados y labels como `Tenants` no se renderizan.

Reglas de ejecucion:
1. Ejecuta comandos desde /Users/beyondnet/Source/ums/src.
2. Prefiere alinear el fixture de prueba con las keys productivas de navegacion salvo que producto diga que todos los modulos deben expandirse sin importar key.
3. Mantén assertions orientadas al usuario y accesibles.
4. No elimines pruebas ni saltes la suite.
5. Despues del fix focalizado, ejecuta `npm run test --workspace app-web`.
6. Luego ejecuta `npm run lint --workspace app-web` para exponer deuda de calidad restante por separado.

Entregable:
Resume causa raiz, archivos modificados exactos, resultado de verificacion y cualquier gate frontend que siga fallando.
```
