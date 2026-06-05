/**
 * i18n.store.ts — Independent internationalization store.
 *
 * Manages the active language state separately from devTools.
 * In development, devTools can sync with this store, but production
 * code should only depend on this store for language selection.
 */
import { create } from 'zustand';

export type SupportedLanguage = 'en' | 'es';

interface I18nState {
  /** Active UI language */
  language: SupportedLanguage;
  /** Set the active language and update document.documentElement.lang */
  setLanguage: (lang: SupportedLanguage) => void;
}

export const useI18nStore = create<I18nState>(set => ({
  language: 'es',
  setLanguage: lang => {
    document.documentElement.lang = lang;
    set({ language: lang });
  },
}));
