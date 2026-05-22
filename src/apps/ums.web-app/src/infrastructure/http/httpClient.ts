/**
 * httpClient.ts — Infrastructure HTTP abstraction
 *
 * Single configured Axios instance for the entire app.
 * Auth/dev headers are injected via the shared request-context provider.
 */
import axios, { type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';
import { BASE_URL, getRequestContext, DEV_TENANT_ID } from './request-context';
import { getCsrfToken, CSRF_HEADER_NAME } from './csrf';

function createHttpClient(): AxiosInstance {
  const client = axios.create({
    baseURL: BASE_URL,
    headers: { 'Content-Type': 'application/json' },
  });

  client.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    const { userId, language } = getRequestContext();
    if (userId)   config.headers.set('X-User-Id',  userId);
    if (language) config.headers.set('X-Language', language);
    config.headers.set('X-Tenant-Id', DEV_TENANT_ID);

    const csrfToken = getCsrfToken();
    if (csrfToken && config.method && !['get', 'head', 'options'].includes(config.method)) {
      config.headers.set(CSRF_HEADER_NAME, csrfToken);
    }

    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    (error) => {
      const message: string =
        error.response?.data?.detail ??
        error.response?.data?.message ??
        error.message ??
        'An unexpected error occurred';
      const status: number = error.response?.status ?? 0;
      return Promise.reject(Object.assign(error, { normalised: { message, status } }));
    },
  );

  return client;
}

export const httpClient = createHttpClient();
