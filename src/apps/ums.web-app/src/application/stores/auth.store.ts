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

// DEV BYPASS — auto-authenticated with dev-user (mirrors DevAuthMiddleware on the API).
// Remove this before implementing real auth.
const DEV_USER = import.meta.env.DEV
  ? { id: 'dev-user', username: 'Developer', email: 'dev@ums.local', role: 'admin' }
  : null;

export const useAuthStore = create<AuthState>((set) => ({
  user: DEV_USER,
  isAuthenticated: import.meta.env.DEV,
  isDarkMode: true, // Default dark mode for premium look
  devUserId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', // Standard sample dev Guid
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

