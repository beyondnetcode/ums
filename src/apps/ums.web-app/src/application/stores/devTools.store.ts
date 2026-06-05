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
 *
 * Note: Language is now managed by i18n.store.ts. This store syncs its
 * devLanguage to i18nStore for backward compatibility during migration.
 */
import { create } from 'zustand';
import { useI18nStore } from './i18n.store';

interface DevToolsState {
  devUserId: string;
  devLanguage: 'en' | 'es';
  setDevUserId: (id: string) => void;
  setDevLanguage: (lang: 'en' | 'es') => void;
}

export const useDevToolsStore = create<DevToolsState>(set => ({
  devUserId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  devLanguage: 'es',
  setDevUserId: id => set({ devUserId: id }),
  setDevLanguage: lang => {
    set({ devLanguage: lang });
    useI18nStore.getState().setLanguage(lang);
  },
}));
