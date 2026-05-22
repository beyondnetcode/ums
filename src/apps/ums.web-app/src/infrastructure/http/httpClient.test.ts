import { describe, it, expect } from 'vitest';

describe('httpClient configuration', () => {
  it('exports an httpClient instance', async () => {
    const { httpClient } = await import('./httpClient');
    expect(httpClient).toBeDefined();
    expect(typeof httpClient.get).toBe('function');
    expect(typeof httpClient.post).toBe('function');
    expect(typeof httpClient.put).toBe('function');
    expect(typeof httpClient.delete).toBe('function');
  });

  it('has baseURL configured', async () => {
    const { httpClient } = await import('./httpClient');
    expect(httpClient.defaults.baseURL).toBeDefined();
  });

  it('has request interceptors configured', async () => {
    const { httpClient } = await import('./httpClient');
    expect(httpClient.interceptors.request).toBeDefined();
    expect(httpClient.interceptors.request.handlers.length).toBeGreaterThan(0);
  });

  it('has response interceptors configured', async () => {
    const { httpClient } = await import('./httpClient');
    expect(httpClient.interceptors.response).toBeDefined();
    expect(httpClient.interceptors.response.handlers.length).toBeGreaterThan(0);
  });
});
