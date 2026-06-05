import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import {
  GraphQlError,
  GraphQlUnavailableError,
  GraphQlValidationError,
  graphqlClient,
} from './graphqlClient';

describe('GraphQlError', () => {
  it('creates an error with status', () => {
    const error = new GraphQlError('Test error', 500);
    expect(error.message).toBe('Test error');
    expect(error.status).toBe(500);
    expect(error.name).toBe('GraphQlError');
  });

  it('creates an error with response errors', () => {
    const responseErrors = [{ message: 'Field required' }];
    const error = new GraphQlError('Validation failed', 400, responseErrors);
    expect(error.responseErrors).toEqual(responseErrors);
  });

  it('creates an error with operation name', () => {
    const error = new GraphQlError('Test error', 500, undefined, 'GetTenants');
    expect(error.operationName).toBe('GetTenants');
  });

  it('creates an error with support reference ID', () => {
    const error = new GraphQlError('Test error', 500, undefined, undefined, 'ERR-123');
    expect(error.supportReferenceId).toBe('ERR-123');
  });
});

describe('GraphQlUnavailableError', () => {
  it('creates error with network message for status 0', () => {
    const error = new GraphQlUnavailableError(0);
    expect(error.message).toContain('unable to reach the API');
    expect(error.name).toBe('GraphQlUnavailableError');
  });

  it('creates error with HTTP status message', () => {
    const error = new GraphQlUnavailableError(503);
    expect(error.message).toContain('503');
  });

  it('includes support reference ID', () => {
    const error = new GraphQlUnavailableError(502, 'ERR-456');
    expect(error.supportReferenceId).toBe('ERR-456');
  });
});

describe('GraphQlValidationError', () => {
  it('creates error with details', () => {
    const error = new GraphQlValidationError('Validation failed', [
      'Field A is required',
      'Field B is invalid',
    ]);
    expect(error.message).toBe('Validation failed');
    expect(error.details).toEqual(['Field A is required', 'Field B is invalid']);
    expect(error.name).toBe('GraphQlValidationError');
  });

  it('includes support reference ID', () => {
    const error = new GraphQlValidationError('Validation failed', ['Error'], 'ERR-789');
    expect(error.supportReferenceId).toBe('ERR-789');
  });
});

describe('graphqlClient', () => {
  const originalFetch = global.fetch;

  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it('exports a request method', () => {
    expect(typeof graphqlClient.request).toBe('function');
  });

  it('sends POST request with query', async () => {
    const mockResponse = {
      ok: true,
      json: vi.fn().mockResolvedValue({ data: { tenants: [] } }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await graphqlClient.request('{ tenants { tenantId } }');

    expect(global.fetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
          'X-Tenant-Id': expect.any(String),
        }),
        body: expect.stringContaining('tenants'),
      })
    );
  });

  it('includes variables in request body', async () => {
    const mockResponse = {
      ok: true,
      json: vi.fn().mockResolvedValue({ data: { tenant: { tenantId: '123' } } }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await graphqlClient.request('query GetTenant($id: ID!) { tenant(id: $id) { tenantId } }', {
      id: '123',
    });

    const callArgs = vi.mocked(global.fetch).mock.calls[0];
    const body = JSON.parse(callArgs[1].body as string);
    expect(body.variables).toEqual({ id: '123' });
  });

  it('extracts operation name from query', async () => {
    const mockResponse = {
      ok: true,
      json: vi.fn().mockResolvedValue({ data: { tenants: [] } }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await graphqlClient.request('query GetTenants { tenants { tenantId } }');

    const callArgs = vi.mocked(global.fetch).mock.calls[0];
    const body = JSON.parse(callArgs[1].body as string);
    expect(body.operationName).toBe('GetTenants');
  });

  it('extracts operation name from mutation', async () => {
    const mockResponse = {
      ok: true,
      json: vi.fn().mockResolvedValue({ data: { createTenant: { tenantId: '123' } } }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await graphqlClient.request(
      'mutation CreateTenant($code: String!) { createTenant(code: $code) { tenantId } }',
      { code: 'TEST' }
    );

    const callArgs = vi.mocked(global.fetch).mock.calls[0];
    const body = JSON.parse(callArgs[1].body as string);
    expect(body.operationName).toBe('CreateTenant');
  });

  it('throws GraphQlUnavailableError on 502', async () => {
    const mockResponse = {
      ok: false,
      status: 502,
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await expect(graphqlClient.request('{ tenants { tenantId } }')).rejects.toThrow(
      GraphQlUnavailableError
    );
  });

  it('throws GraphQlUnavailableError on 503', async () => {
    const mockResponse = {
      ok: false,
      status: 503,
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await expect(graphqlClient.request('{ tenants { tenantId } }')).rejects.toThrow(
      GraphQlUnavailableError
    );
  });

  it('throws GraphQlValidationError on 400 with errors', async () => {
    const mockResponse = {
      ok: false,
      status: 400,
      json: vi.fn().mockResolvedValue({
        errors: [{ message: 'Field required' }],
        data: null,
      }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await expect(graphqlClient.request('{ tenants { tenantId } }')).rejects.toThrow(
      GraphQlValidationError
    );
  });

  it('throws GraphQlError on non-400 error status', async () => {
    const mockResponse = {
      ok: false,
      status: 500,
      json: vi.fn().mockResolvedValue({
        errors: [{ message: 'Internal error' }],
        data: null,
      }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await expect(graphqlClient.request('{ tenants { tenantId } }')).rejects.toThrow(GraphQlError);
  });

  it('throws GraphQlError when response has no data', async () => {
    const mockResponse = {
      ok: true,
      json: vi.fn().mockResolvedValue({ data: null }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await expect(graphqlClient.request('{ tenants { tenantId } }')).rejects.toThrow(GraphQlError);
  });

  it('throws GraphQlValidationError when response has errors in data', async () => {
    const mockResponse = {
      ok: true,
      json: vi.fn().mockResolvedValue({
        data: null,
        errors: [{ message: 'Field not found' }],
      }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    await expect(graphqlClient.request('{ tenants { tenantId } }')).rejects.toThrow(
      GraphQlValidationError
    );
  });

  it('returns data on successful response', async () => {
    const expectedData = { tenants: [{ tenantId: 'abc-123' }] };
    const mockResponse = {
      ok: true,
      json: vi.fn().mockResolvedValue({ data: expectedData }),
      headers: { get: vi.fn().mockReturnValue(null) },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    const result = await graphqlClient.request('{ tenants { tenantId } }');

    expect(result).toEqual(expectedData);
  });

  it('includes X-Error-Id header in error', async () => {
    const mockResponse = {
      ok: false,
      status: 500,
      json: vi.fn().mockResolvedValue({ errors: [{ message: 'Error' }], data: null }),
      headers: {
        get: vi.fn().mockImplementation(name => (name === 'X-Error-Id' ? 'ERR-001' : null)),
      },
    };
    vi.mocked(global.fetch).mockResolvedValue(mockResponse as unknown as Response);

    try {
      await graphqlClient.request('{ tenants { tenantId } }');
    } catch (error) {
      expect((error as GraphQlError).supportReferenceId).toBe('ERR-001');
    }
  });
});
