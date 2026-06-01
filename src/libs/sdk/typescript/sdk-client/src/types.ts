import type { AuthorizationGraph } from '@ums/sdk-contracts';

export interface ClientAuthRequest {
  readonly tenantCode: string;
  readonly username: string;
  readonly password: string;
  readonly format?: 'JSON' | 'XML' | 'YAML' | 'CSV';
}

export interface ClientAuthResult {
  readonly token: string;
  readonly tokenType: string;
  readonly expiresIn: number;
  readonly issuedAt: string;
  readonly format: string;
  readonly graph: AuthorizationGraph;
  readonly requestId: string;
}

export interface UmsAuthClientOptions {
  readonly baseUrl: string;
  readonly authenticatePath?: string;   // default: '/api/v1/client/authenticate'
  readonly fetchImpl?: typeof fetch;    // for testing or environments without global fetch
  readonly timeoutMs?: number;
}

export interface ClientAuthFailure {
  readonly code: string;
  readonly message: string;
}

export type ClientAuthOutcome =
  | { readonly ok: true; readonly value: ClientAuthResult }
  | { readonly ok: false; readonly error: ClientAuthFailure };
