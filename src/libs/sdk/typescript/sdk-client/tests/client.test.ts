import { describe, it, expect } from 'vitest';
import { AuthGraphBuilder } from '@ums/sdk-testing';
import { UmsAuthClient, type ClientAuthOutcome } from '../src/index.js';

function makeFetch(status: number, body: unknown): typeof fetch {
  return async () =>
    new Response(typeof body === 'string' ? body : JSON.stringify(body), {
      status,
      headers: { 'content-type': 'application/json' }
    });
}

describe('UmsAuthClient', () => {
  it('returns Result.success on happy path', async () => {
    const graph = AuthGraphBuilder.forTenant('LOGISTICS_CORE')
      .withScope('PURCHASE_ORDER.VIEW')
      .build();

    const client = new UmsAuthClient({
      baseUrl: 'https://ums.example.com',
      fetchImpl: makeFetch(200, {
        token: 'TOKEN.PLACEHOLDER.SIG',
        tokenType: 'Bearer',
        expiresIn: 3600,
        issuedAt: new Date().toISOString(),
        format: 'JSON',
        graph,
        requestId: 'rid'
      })
    });

    const outcome: ClientAuthOutcome = await client.authenticate({
      tenantCode: 'LOGISTICS_CORE',
      username: 'u',
      password: 'p'
    });

    expect(outcome.ok).toBe(true);
    if (outcome.ok) {
      expect(outcome.value.graph.context.tenant.code).toBe('LOGISTICS_CORE');
    }
  });

  it('returns AUTH_205 when server emits an unsupported MAJOR', async () => {
    const graph = AuthGraphBuilder.forTenant('LOGISTICS_CORE').withSchemaVersion('2.0.0').build();

    const client = new UmsAuthClient({
      baseUrl: 'https://ums.example.com',
      fetchImpl: makeFetch(200, {
        token: 'T', tokenType: 'Bearer', expiresIn: 60,
        issuedAt: new Date().toISOString(), format: 'JSON', graph, requestId: 'rid'
      })
    });

    const outcome = await client.authenticate({ tenantCode: 'LOGISTICS_CORE', username: 'u', password: 'p' });
    expect(outcome.ok).toBe(false);
    if (!outcome.ok) expect(outcome.error.code).toBe('AUTH_205');
  });

  it('maps HTTP 401 to AUTH_006 (InvalidCredentials)', async () => {
    const client = new UmsAuthClient({
      baseUrl: 'https://ums.example.com',
      fetchImpl: makeFetch(401, 'unauthorized')
    });

    const outcome = await client.authenticate({ tenantCode: 'LOGISTICS_CORE', username: 'u', password: 'bad' });
    expect(outcome.ok).toBe(false);
    if (!outcome.ok) expect(outcome.error.code).toBe('AUTH_006');
  });

  it('maps HTTP 404 to AUTH_002 (TenantNotFound)', async () => {
    const client = new UmsAuthClient({
      baseUrl: 'https://ums.example.com',
      fetchImpl: makeFetch(404, 'not found')
    });

    const outcome = await client.authenticate({ tenantCode: 'ZZZ', username: 'u', password: 'p' });
    expect(outcome.ok).toBe(false);
    if (!outcome.ok) expect(outcome.error.code).toBe('AUTH_002');
  });
});
