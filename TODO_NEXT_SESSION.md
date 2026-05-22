# UMS - Plan de Trabajo: Sesion Siguiente

**Ultima sesion:** 21 de mayo 2026
**Proxima sesion:** Por definir

---

## Resumen de la Sesion Anterior

### Corregido (Auditoria P0/P1/P2)
- Focus trap en M3Dialog/M3Drawer (useFocusTrap hook)
- CSP: eliminado unsafe-eval de index.html y Dockerfile
- i18n store independiente (separado de devTools.store)
- README expandido (EN + ES, 137 lineas c/u)
- React.memo en componentes puros
- Labels accesibles en selects (aria-label)
- Build optimization Vite (manualChunks, esbuild minify)
- Barrel exports (components, hooks, stores)
- .dockerignore + HEALTHCHECK en Dockerfile
- Testing: 79 tests passing (de 18 a 79)
- Documentacion: ADR-0055, ADR-0056, ADR-0057, diagramas Mermaid, CONTRIBUTING.md
- Score general: 7.0 -> ~8.5/10

### Nuevo Agregado: UserAccount (completo frontend)
- Domain: schemas Zod, modelos, constantes
- Infrastructure: GraphQL queries, REST service
- Application: TanStack Query hooks, dashboard orchestration hook
- Presentation: DashboardScreen, ListPanel, DetailPanel, ProfileCard, Form
- Routing: /users lazy-loaded, navegacion configurada
- i18n: +40 claves EN + +40 claves ES
- Build success: UserAccountDashboardScreen code-split (17.29 kB)
- Dev server: port 5173, HTTP 200 OK

---

## Pendientes para Proxima Sesion

### CRITICO / MUST DO

#### 1. Conectar UserAccount con datos reales
**Estado actual:** El formulario usa un tenantId hardcoded (f3e2d1c0-...).
**Que hacer:**
- [ ] Pasar el tenantId seleccionado al crear un UserAccount
- [ ] Agregar filtro por tenant en el dashboard (tenantId prop)
- [ ] Opcion A: Selector de tenant en la UI
- [ ] Opcion B: Usar el X-Tenant-Id del contexto de dev

**Archivos afectados:**
- presentation/identity/user-account/components/UserAccountForm.tsx (linea ~34)
- application/identity/hooks/use-user-account-dashboard.ts

#### 2. Implementar seed data para UserAccount
**Estado actual:** No existen UserAccounts en la base de datos dev. La lista aparece vacia.
**Que hacer:**
- [ ] Crear seed data en DevDataSeeder.cs (backend)
- [ ] Mismo patron que los tenants: 3-5 cuentas con emails variados
- [ ] Incluir estados: Pending, Active, Blocked
- [ ] Re-run migration o seeder antes de probar

**Archivo:** Ums.Infrastructure/Persistence/DevDataSeeder.cs

#### 3. Tests para UserAccount
**Estado actual:** 0 tests para UserAccount.
**Que hacer:**
- [ ] user-account.schema.test.ts - Validar schemas Zod
- [ ] use-user-account.test.tsx - Hooks TanStack Query
- [ ] UserAccountListPanel.test.tsx - Render + seleccion
- [ ] UserAccountProfileCard.test.tsx - Acciones (activate/block/restore)

**Patron:** Copiar estructura de *.test.ts existentes en stores/ y hooks/.

---

### ALTA PRIORIDAD / SHOULD DO

#### 4. UserAccount: Inline Edit Mode (como TenantProfileCard)
**Estado actual:** ProfileCard es solo lectura.
**Que hacer:**
- [ ] Doble click para editar email/categoria
- [ ] useInlineEdit<UserAccount> para draft state
- [ ] Botones Save/Discard
- [ ] Validacion antes de guardar

**Referencia:** presentation/identity/tenant/components/TenantProfileCard.tsx

#### 5. UserAccount: Block Reason Dialog Mejorado
**Estado actual:** Dialog muestra solo confirm generico + reason.
**Que hacer:**
- [ ] Campo blockReason dentro del dialog (no en dashboard)
- [ ] Validacion: reason no vacio antes de confirmar
- [ ] Mostrar reason en detail panel despues de bloquear

#### 6. ESLint warnings pendientes
**Estado actual:** 7 warnings (non-null assertions).
**Que hacer:**
- [ ] Reemplazar ! con optional chaining o guards en DataGrid.tsx
- [ ] Reemplazar ! en IdpPanel.tsx
- [ ] Verificar que no haya regresiones

---

### MEDIA / NICE TO HAVE

#### 7. Virtualizacion para listas grandes
**Cuando:** Cuando haya 100+ user accounts.
**Como:** Instalar @tanstack/react-virtual o react-window.

#### 8. Skip Navigation Link (a11y)
**Que:** Enlace "Skip to main content" visible al tab focus.
**Donde:** MainLayout.tsx, antes del primer elemento renderizado.

#### 9. Formateo de fechas/numeros por locale
**Estado actual:** Fechas sin formatear.
**Que hacer:** Crear formatDate(date, locale) y formatNumber(value, locale) en application/utils/.

#### 10. E2E Setup (Playwright)
**Que:** Instalar Playwright + primer test critico (login -> navigate -> select tenant).
**Cuando:** Despues de tener seed data funcional.

---

## Comandos Utiles

```bash
# Antes de empezar
cd src/apps/ums.web-app
npx tsc --noEmit          # Type check
npx eslint "src/**/*.{ts,tsx}"  # Lint
npx vitest run            # Tests (79/79)
npx vite                  # Dev server

# Backend
cd ../ums.api-dotnet
dotnet build              # Build backend
dotnet run                # Run API

# Commit
git add -A && git commit -m "feat(user-account): implement..."
```
