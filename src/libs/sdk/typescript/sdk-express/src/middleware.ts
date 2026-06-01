import type { AuthorizationGraph } from '@ums/sdk-contracts';
import { isSchemaVersionSupported } from '@ums/sdk-contracts';
import {
  type AuthGraphAccessor,
  AsyncLocalAuthGraphAccessor,
  MemoryAuthGraphAccessor
} from '@ums/sdk-authorization';

export interface UmsAuthGraphOptions {
  readonly accessor: AuthGraphAccessor;
  /** JWT body claim that carries the serialized graph. Default: 'graph'. */
  readonly jwtBodyClaim?: string;
  /** When true, reject with 401 + AUTH_201 if validUntil is in the past. */
  readonly rejectExpired?: boolean;
  /** When true (default), reject with 401 + AUTH_204/AUTH_205 on missing or incompatible schemaVersion. */
  readonly rejectIncompatible?: boolean;
}

/**
 * Express middleware. Reads the bearer JWT, decodes its body, extracts the `graph` claim,
 * validates schema compatibility (and optionally expiry), and binds the graph to the configured
 * accessor for the request lifetime. Downstream handlers protected by `requireScope`/decorators
 * see the graph through `accessor.current()`.
 */
export function umsAuthGraph(options: UmsAuthGraphOptions) {
  const accessor = options.accessor;
  const claim = options.jwtBodyClaim ?? 'graph';
  const rejectExpired = options.rejectExpired === true;
  const rejectIncompatible = options.rejectIncompatible !== false;

  return function umsAuthGraphMiddleware(
    req: { headers: Record<string, string | string[] | undefined> },
    res: ExpressResponse,
    next: (err?: unknown) => void
  ): void {
    const token = extractBearer(req);
    if (token === null) return runWith(null, next);

    let graph: AuthorizationGraph | null = null;
    try {
      graph = decodeGraph(token, claim);
    } catch {
      return runWith(null, next);
    }
    if (graph === null) return runWith(null, next);

    if (!graph.schemaVersion) {
      if (rejectIncompatible) return reject(res, 'AUTH_204', 'AuthorizationGraph payload does not declare a schemaVersion.');
      return runWith(null, next);
    }
    if (!isSchemaVersionSupported(graph.schemaVersion)) {
      if (rejectIncompatible)
        return reject(res, 'AUTH_205', `schemaVersion '${graph.schemaVersion}' is outside SDK compatibility range.`);
      return runWith(null, next);
    }
    if (rejectExpired && new Date(graph.validUntil).getTime() <= Date.now()) {
      return reject(res, 'AUTH_201', 'AuthorizationGraph has expired.');
    }

    return runWith(graph, next);
  };

  function runWith(graph: AuthorizationGraph | null, next: (err?: unknown) => void): void {
    if (accessor instanceof AsyncLocalAuthGraphAccessor) {
      accessor.run(graph, () => next());
      return;
    }
    if (accessor instanceof MemoryAuthGraphAccessor) {
      accessor.set(graph);
      next();
      return;
    }
    // Custom accessor — caller is responsible for plumbing.
    next();
  }
}

interface ExpressResponse {
  status: (code: number) => { json: (body: unknown) => void };
}

function extractBearer(req: { headers: Record<string, string | string[] | undefined> }): string | null {
  const raw = req.headers['authorization'];
  const header = Array.isArray(raw) ? raw[0] : raw;
  if (!header) return null;
  const m = /^Bearer\s+(.+)$/i.exec(header);
  return m?.[1]?.trim() ?? null;
}

function decodeGraph(token: string, claim: string): AuthorizationGraph | null {
  const parts = token.split('.');
  if (parts.length < 2) return null;
  const payload = JSON.parse(base64UrlDecode(parts[1]!));
  const value = payload?.[claim];
  if (value && typeof value === 'object') return value as AuthorizationGraph;
  if (typeof value === 'string') return JSON.parse(value) as AuthorizationGraph;
  return null;
}

function base64UrlDecode(input: string): string {
  let normalized = input.replace(/-/g, '+').replace(/_/g, '/');
  const pad = normalized.length % 4;
  if (pad === 2) normalized += '==';
  else if (pad === 3) normalized += '=';
  else if (pad === 1) throw new Error('Invalid base64url length');
  if (typeof Buffer !== 'undefined') return Buffer.from(normalized, 'base64').toString('utf-8');
  // Browser fallback:
  return decodeURIComponent(
    atob(normalized)
      .split('')
      .map((c) => `%${c.charCodeAt(0).toString(16).padStart(2, '0')}`)
      .join('')
  );
}

function reject(res: ExpressResponse, code: string, message: string): void {
  res.status(401).json({ code, message });
}
