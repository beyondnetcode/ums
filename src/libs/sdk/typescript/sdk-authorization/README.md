# @ums/sdk-authorization

> Part of the [UMS SDK](https://github.com/beyondnetcode/ums/tree/main/docs/sdk) — the official client integration surface for UMS.

Pure authorization validator + accessor abstraction + HOF/decorator authorization primitives for TypeScript and JavaScript. Framework-agnostic.

## Install

```bash
npm install @ums/sdk-authorization @ums/sdk-contracts
```

## Quickstart (Node + Express)

```ts
import express from 'express';
import {
  AsyncLocalAuthGraphAccessor,
  AuthorizationValidator,
  configureAuthorization,
  requireScope
} from '@ums/sdk-authorization';

const accessor = new AsyncLocalAuthGraphAccessor();
configureAuthorization({ accessor, validator: new AuthorizationValidator() });

const app = express();

// upstream middleware parses JWT body → req.umsAuthGraph
app.use((req, _res, next) => accessor.run((req as any).umsAuthGraph ?? null, () => next()));

app.post(
  '/orders/:id/approve',
  requireScope('PURCHASE_ORDER.APPROVE', async (_req, res) => {
    res.json({ ok: true });
  })
);
```

## Decorators (TS 5.0+)

```ts
import { RequiresScope } from '@ums/sdk-authorization';

class OrderService {
  @RequiresScope('PURCHASE_ORDER.APPROVE')
  async approve(id: string): Promise<void> { /* ... */ }
}
```

## Browser

```ts
import { MemoryAuthGraphAccessor } from '@ums/sdk-authorization';
const accessor = new MemoryAuthGraphAccessor();
accessor.set(graph);   // after login
accessor.clear();      // on logout
```

## See also

- [TypeScript SDK guide](https://github.com/beyondnetcode/ums/blob/main/docs/sdk/typescript/README.md)
- [TypeScript Quickstart](https://github.com/beyondnetcode/ums/blob/main/docs/sdk/typescript/quickstart.md)

## License

MIT — see [LICENSE](https://github.com/beyondnetcode/ums/blob/main/src/libs/sdk/LICENSE).
