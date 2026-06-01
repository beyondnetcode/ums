# @ums/sdk-express

> Part of the [UMS SDK](https://github.com/beyondnetcode/ums/tree/main/docs/sdk).

Express middleware that decodes a bearer JWT's `graph` claim into an `AuthorizationGraph` and binds it to the configured `@ums/sdk-authorization` accessor for the request lifetime.

## Install

```bash
npm install @ums/sdk-express @ums/sdk-authorization @ums/sdk-contracts
```

## Use

```ts
import express from 'express';
import {
  AsyncLocalAuthGraphAccessor, AuthorizationValidator, configureAuthorization
} from '@ums/sdk-authorization';
import { umsAuthGraph } from '@ums/sdk-express';

const accessor = new AsyncLocalAuthGraphAccessor();
configureAuthorization({ accessor, validator: new AuthorizationValidator() });

const app = express();
app.use(umsAuthGraph({ accessor, rejectExpired: true }));
```

After the middleware runs, any handler that calls `requireScope(...)` or any of the
HOFs/decorators from `@ums/sdk-authorization` can read the current graph through the
accessor without further wiring.

## License

MIT — see [LICENSE](https://github.com/beyondnetcode/ums/blob/main/src/libs/sdk/LICENSE).
