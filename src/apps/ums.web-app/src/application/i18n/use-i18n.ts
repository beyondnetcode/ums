import { useAuthStore } from '../stores/auth.store';
import translations from './translations';

export const useI18n = () => {
  const lang = useAuthStore((state) => state.devLanguage);
  return translations[lang];
};
