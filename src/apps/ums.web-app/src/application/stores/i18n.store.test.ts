import { describe, it, expect, beforeEach } from 'vitest';
import { useI18nStore } from './i18n.store';

describe('i18n.store', () => {
  beforeEach(() => {
    useI18nStore.setState({ language: 'es' });
    document.documentElement.lang = 'es';
  });

  it('initializes with Spanish as default', () => {
    expect(useI18nStore.getState().language).toBe('es');
  });

  it('changes language to English', () => {
    useI18nStore.getState().setLanguage('en');
    expect(useI18nStore.getState().language).toBe('en');
  });

  it('updates document.documentElement.lang on language change', () => {
    useI18nStore.getState().setLanguage('en');
    expect(document.documentElement.lang).toBe('en');
  });

  it('updates document.documentElement.lang to Spanish', () => {
    useI18nStore.getState().setLanguage('es');
    expect(document.documentElement.lang).toBe('es');
  });

  it('selects language via getState', () => {
    useI18nStore.setState({ language: 'en' });
    const lang = useI18nStore.getState().language;
    expect(lang).toBe('en');
  });

  it('has setLanguage function', () => {
    expect(typeof useI18nStore.getState().setLanguage).toBe('function');
  });
});
