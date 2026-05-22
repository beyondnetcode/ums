import { sharedTranslations } from './namespaces/shared.translations';
import { identityTranslations } from './namespaces/identity.translations';

export type Language = 'es' | 'en';

const translations = {
  es: {
    ...sharedTranslations.es,
    ...identityTranslations.es,
  },
  en: {
    ...sharedTranslations.en,
    ...identityTranslations.en,
  },
} as const;

export type TranslationKeys = keyof typeof translations.es;
export type Translations = typeof translations.es;

export default translations;
