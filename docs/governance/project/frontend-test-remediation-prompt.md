# Frontend Test Remediation Prompt

Use this prompt to start a focused remediation thread for the failing `app-web` unit tests.

```text
You are working in /Users/beyondnet/Source/ums. Follow AGENTS.md, BMAD rules, and the frontend audit playbook.

Goal:
Make `npm run test --workspace app-web` pass without deleting tests or weakening assertions.

Current failing area:
- `src/presentation/shared/layouts/NavRail.test.tsx`
- 3 failing tests:
  - renders navigation items when module is expanded
  - toggles module expansion when clicked
  - highlights active tab

Audit diagnosis:
- The test fixture mocks `NAV_MODULES` with module key `identity`.
- `NavRail` initializes expanded modules with keys `idm`, `auth`, and `sys`.
- Because the mocked module key does not match the expanded state, its members are collapsed and labels such as `Tenants` are not rendered.

Execution rules:
1. Run commands from /Users/beyondnet/Source/ums/src.
2. Prefer aligning the test fixture with production navigation keys unless product behavior says all modules must expand regardless of key.
3. Keep assertions user-facing and accessible.
4. Do not remove tests or skip the suite.
5. After the focused fix, run `npm run test --workspace app-web`.
6. Then run `npm run lint --workspace app-web` to surface remaining quality debt separately.

Deliverable:
Summarize the root cause, exact changed files, verification result, and any remaining frontend gates still failing.
```
