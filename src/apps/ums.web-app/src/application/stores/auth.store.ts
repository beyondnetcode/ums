import { create } from 'zustand';

interface AuthState {
  user: { id: string; username: string; email: string; role: string } | null;
  isAuthenticated: boolean;
  isDarkMode: boolean;
  devUserId: string;
  devLanguage: 'en' | 'es';
  login: (user: { id: string; username: string; email: string; role: string }) => void;
  logout: () => void;
  toggleDarkMode: () => void;
  setDevUserId: (id: string) => void;
  setDevLanguage: (lang: 'en' | 'es') => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  isDarkMode: true, // Default dark mode for premium look
  devUserId: '',
  devLanguage: 'es',
  login: (user) => set({ user, isAuthenticated: true }),
  logout: () => set({ user: null, isAuthenticated: false }),
  toggleDarkMode: () => set((state) => {
    const nextMode = !state.isDarkMode;
    if (nextMode) {
      document.body.classList.add('dark');
    } else {
      document.body.classList.remove('dark');
    }
    return { isDarkMode: nextMode };
  }),
  setDevUserId: (id) => set({ devUserId: id }),
  setDevLanguage: (lang) => set({ devLanguage: lang }),
}));

