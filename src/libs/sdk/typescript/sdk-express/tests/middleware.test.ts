import { describe, it, expect } from 'vitest';
import { AsyncLocalAuthGraphAccessor, MemoryAuthGraphAccessor } from '@ums/sdk-authorization';
import { AuthGraphBuilder } from '@ums/sdk-testing';
import type { AuthorizationGraph } from '@ums/sdk-contracts';
import { umsAuthGraph } from '../src/index.js';

function fakeJwt(graph: AuthorizationGraph): string {
  const header = base64Url(Buffer.from('{"alg":"none","typ":"JWT"}'));
  const payload = base64Url(Buffer.from(JSON.stringify({ graph })));
  return `${header}.${payload}.`;
}

function base64Url(buffer: Buffer): string {
  return buffer.toString('base64').replace(/=+$/, '').replace(/\+/g, '-').replace(/\//g, '_');
}

function makeRes() {
  const body: { status?: number; payload?: unknown } = {};
  return {
    status(code: number) {
      body.status = code;
      return {
        json(p: unknown) {
          body.payload = p;
        }
      };
    },
    body
  };
}

describe('@ums/sdk-express middleware', () => {
  it('binds graph to MemoryAuthGraphAccessor and calls next', async () => {
    const graph = AuthGraphBuilder.forTenant('LOGISTICS_CORE').withScope('PURCHASE_ORDER.VIEW').build();
    const accessor = new MemoryAuthGraphAccessor();
    const mw = umsAuthGraph({ accessor });

    await new Promise<void>((resolve) => {
      mw({ headers: { authorization: `Bearer ${fakeJwt(graph)}` } }, makeRes() as never, () => resolve());
    });

    const bound = accessor.current();
    expect(bound).not.toBeNull();
    expect(bound!.context.tenant.code).toBe('LOGISTICS_CORE');
  });

  it('binds graph inside AsyncLocalAuthGraphAccessor scope', async () => {
    const graph = AuthGraphBuilder.forTenant('LOGISTICS_CORE').build();
    const accessor = new AsyncLocalAuthGraphAccessor();
    const mw = umsAuthGraph({ accessor });

    let observed: AuthorizationGraph | null = null;
    await new Promise<void>((resolve) => {
      mw({ headers: { authorization: `Bearer ${fakeJwt(graph)}` } }, makeRes() as never, () => {
        observed = accessor.current();
        resolve();
      });
    });
    expect(observed).not.toBeNull();
    expect(observed!.context.tenant.code).toBe('LOGISTICS_CORE');
  });

  it('rejects with 401 AUTH_205 on unsupported MAJOR', async () => {
    const graph = AuthGraphBuilder.forTenant('LOGISTICS_CORE').withSchemaVersion('2.0.0').build();
    const accessor = new MemoryAuthGraphAccessor();
    const mw = umsAuthGraph({ accessor });
    const res = makeRes();

    mw({ headers: { authorization: `Bearer ${fakeJwt(graph)}` } }, res as never, () => {
      throw new Error('next() should not be called on rejection');
    });

    expect(res.body.status).toBe(401);
    expect((res.body.payload as { code: string }).code).toBe('AUTH_205');
  });

  it('rejects expired graph when rejectExpired is true', async () => {
    const graph = AuthGraphBuilder.forTenant('LOGISTICS_CORE').buildExpired();
    const accessor = new MemoryAuthGraphAccessor();
    const mw = umsAuthGraph({ accessor, rejectExpired: true });
    const res = makeRes();

    mw({ headers: { authorization: `Bearer ${fakeJwt(graph)}` } }, res as never, () => {
      throw new Error('next() should not be called on expiry rejection');
    });

    expect(res.body.status).toBe(401);
    expect((res.body.payload as { code: string }).code).toBe('AUTH_201');
  });

  it('passes through when no bearer is provided', async () => {
    const accessor = new MemoryAuthGraphAccessor();
    const mw = umsAuthGraph({ accessor });
    let called = false;
    await new Promise<void>((resolve) => {
      mw({ headers: {} }, makeRes() as never, () => { called = true; resolve(); });
    });
    expect(called).toBe(true);
    expect(accessor.current()).toBeNull();
  });
});
