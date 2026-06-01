import { Inject, Injectable, type NestMiddleware } from '@nestjs/common';
import type { AuthorizationGraph } from '@ums/sdk-contracts';
import { AsyncLocalAuthGraphAccessor, MemoryAuthGraphAccessor, type AuthGraphAccessor } from '@ums/sdk-authorization';
import { UMS_AUTH_GRAPH_ACCESSOR } from './tokens.js';

type ExpressRequest = {
  readonly headers: Record<string, string | string[] | undefined>;
  readonly umsAuthGraph?: AuthorizationGraph;
};

type ExpressResponse = unknown;
type NextFn = (err?: unknown) => void;

/**
 * Express-style middleware that:
 *   1. Reads a fake or pre-parsed AuthorizationGraph from `req.umsAuthGraph` (set by an upstream
 *      authenticator — production deployments will replace this with a JWT-decoding step).
 *   2. Binds the graph to the configured accessor for the duration of the request, so the
 *      `UmsAuthGuard` reads it via `AuthGraphAccessor.current()`.
 *
 * For Phase 2 we'll add JWT body parsing + a refresh call to `POST /api/v1/client/authenticate`.
 */
@Injectable()
export class AuthGraphMiddleware implements NestMiddleware<ExpressRequest, ExpressResponse> {
  constructor(@Inject(UMS_AUTH_GRAPH_ACCESSOR) private readonly accessor: AuthGraphAccessor) {}

  use(req: ExpressRequest, _res: ExpressResponse, next: NextFn): void {
    const graph = req.umsAuthGraph ?? null;

    if (this.accessor instanceof AsyncLocalAuthGraphAccessor) {
      this.accessor.run(graph, () => next());
      return;
    }
    if (this.accessor instanceof MemoryAuthGraphAccessor) {
      this.accessor.set(graph);
      next();
      return;
    }
    // Unknown accessor — fall through so consumers using custom accessors can wire their own setup.
    next();
  }
}
