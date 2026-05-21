import { useDevToolsStore } from '../stores/devTools.store';
import translations from './translations';

export const useI18n = () => {
  const lang = useDevToolsStore((state) => state.devLanguage);
  return translations[lang];
};
