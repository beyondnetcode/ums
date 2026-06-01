import { UmsErrorCodes, isSchemaVersionSupported, SchemaVersion } from '@ums/sdk-contracts';
import type { ClientAuthOutcome, ClientAuthRequest, ClientAuthResult, UmsAuthClientOptions } from './types.js';

const DEFAULT_PATH = '/api/v1/client/authenticate';
const DEFAULT_TIMEOUT_MS = 30_000;

/**
 * Typed fetch-based client for the UMS authentication endpoint.
 * Validates the server's `schemaVersion` against the SDK's compatibility window before returning
 * the deserialized graph — incompatible majors yield `AUTH_205` without exposing the payload.
 */
export class UmsAuthClient {
  private readonly baseUrl: string;
  private readonly path: string;
  private readonly fetchImpl: typeof fetch;
  private readonly timeoutMs: number;

  constructor(options: UmsAuthClientOptions) {
    if (!options.baseUrl) throw new Error('UmsAuthClient requires baseUrl');
    this.baseUrl = options.baseUrl.replace(/\/$/, '');
    this.path = options.authenticatePath ?? DEFAULT_PATH;
    this.fetchImpl = options.fetchImpl ?? globalThis.fetch;
    if (!this.fetchImpl) {
      throw new Error('No fetch implementation available — provide one via options.fetchImpl.');
    }
    this.timeoutMs = options.timeoutMs ?? DEFAULT_TIMEOUT_MS;
  }

  async authenticate(request: ClientAuthRequest, init?: { signal?: AbortSignal }): Promise<ClientAuthOutcome> {
    const url = `${this.baseUrl}${this.path}`;
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), this.timeoutMs);
    const signal = init?.signal ?? controller.signal;

    let response: Response;
    try {
      response = await this.fetchImpl(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request),
        signal
      });
    } catch (err) {
      return failure(UmsErrorCodes.IdpCallFailed, (err as Error).message);
    } finally {
      clearTimeout(timeout);
    }

    if (!response.ok) {
      const body = await safeText(response);
      const code = mapStatus(response.status);
      return failure(code, body || response.statusText);
    }

    let parsed: ClientAuthResult;
    try {
      parsed = (await response.json()) as ClientAuthResult;
    } catch (err) {
      return failure(UmsErrorCodes.AuthGraphMalformed, (err as Error).message);
    }

    if (!parsed.graph?.schemaVersion) {
      return failure(
        UmsErrorCodes.AuthGraphSchemaMissing,
        'Server response does not carry a schemaVersion field.'
      );
    }
    if (!isSchemaVersionSupported(parsed.graph.schemaVersion)) {
      return failure(
        UmsErrorCodes.AuthGraphSchemaUnsupported,
        `Server emitted schemaVersion '${parsed.graph.schemaVersion}' outside SDK range ` +
          `(${SchemaVersion.CompatibilityMinInclusive} ≤ x < ${SchemaVersion.CompatibilityMaxExclusive}).`
      );
    }

    return { ok: true, value: parsed };
  }
}

function failure(code: string, message: string): ClientAuthOutcome {
  return { ok: false, error: { code, message } };
}

async function safeText(response: Response): Promise<string> {
  try {
    return await response.text();
  } catch {
    return '';
  }
}

function mapStatus(status: number): string {
  switch (status) {
    case 400: return UmsErrorCodes.ValidationError;
    case 401: return UmsErrorCodes.InvalidCredentials;
    case 403: return UmsErrorCodes.UserNotActive;
    case 404: return UmsErrorCodes.TenantNotFound;
    case 423: return UmsErrorCodes.AccountLocked;
    default:  return UmsErrorCodes.IdpCallFailed;
  }
}
