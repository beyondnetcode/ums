import { describe, it, expect, beforeEach } from 'vitest';
import { configureRequestContext, getRequestContext, BASE_URL, DEFAULT_TENANT_ID } from './request-context';

describe('request-context', () => {
  beforeEach(() => {
    configureRequestContext(() => ({}));
  });

  it('exports BASE_URL', () => {
    expect(BASE_URL).toBeDefined();
    expect(typeof BASE_URL).toBe('string');
  });

  it('exports DEFAULT_TENANT_ID', () => {
    expect(DEFAULT_TENANT_ID).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
  });

  it('returns empty context by default', () => {
    const context = getRequestContext();
    expect(context).toEqual({});
  });

  it('returns configured context with userId', () => {
    configureRequestContext(() => ({ userId: 'user-123' }));
    const context = getRequestContext();
    expect(context.userId).toBe('user-123');
  });

  it('returns configured context with language', () => {
    configureRequestContext(() => ({ language: 'en' }));
    const context = getRequestContext();
    expect(context.language).toBe('en');
  });

  it('returns configured context with both userId and language', () => {
    configureRequestContext(() => ({ userId: 'user-456', language: 'es' }));
    const context = getRequestContext();
    expect(context.userId).toBe('user-456');
    expect(context.language).toBe('es');
  });

  it('returns configured context with tenantId', () => {
    configureRequestContext(() => ({ tenantId: 'tenant-abc' }));
    const context = getRequestContext();
    expect(context.tenantId).toBe('tenant-abc');
  });

  it('allows reconfiguring the provider', () => {
    configureRequestContext(() => ({ userId: 'first' }));
    expect(getRequestContext().userId).toBe('first');

    configureRequestContext(() => ({ userId: 'second' }));
    expect(getRequestContext().userId).toBe('second');
  });
});
