/**
 * devTools.store.ts
 *
 * Development-only store for convenience overrides (user impersonation,
 * language switching, etc.). This store exists ONLY in development mode.
 * It is never imported by production feature code; only by development
 * surfaces and by the application composition root when configuring
 * X-User-Id / X-Language dev headers.
 *
 * In production builds this module is still bundled but all state is inert
 * since the API will use real JWT authentication.
 */
import { create } from 'zustand';

interface DevToolsState {
  /** Dev user GUID forwarded as X-User-Id header (mirrors DevAuthMiddleware) */
  devUserId: string;
  /** UI language used while real i18n is not yet driven by the backend */
  devLanguage: 'en' | 'es';
  setDevUserId: (id: string) => void;
  setDevLanguage: (lang: 'en' | 'es') => void;
}

export const useDevToolsStore = create<DevToolsState>(() => ({
  devUserId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', // standard sample dev GUID
  devLanguage: 'es',
  setDevUserId: (id) => useDevToolsStore.setState({ devUserId: id }),
  setDevLanguage: (lang) => useDevToolsStore.setState({ devLanguage: lang }),
}));
