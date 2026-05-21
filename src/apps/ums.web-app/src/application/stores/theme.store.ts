/**
 * theme.store.ts — Application store for theme preferences
 *
 * M-1: Separated from auth.store.ts to respect SRP.
 * C-2: No DOM manipulation here — presentation layer handles document.body.
 */
import { create } from 'zustand';

interface ThemeState {
  isDarkMode: boolean;
  toggleDarkMode: () => void;
}

export const useThemeStore = create<ThemeState>((set) => ({
  isDarkMode: true,
  toggleDarkMode: () => set((state) => ({ isDarkMode: !state.isDarkMode })),
}));
