/**
 * auth.store.ts
 *
 * Production auth state only. Does NOT contain any dev-specific overrides;
 * those live in devTools.store.ts.
 *
 * DEV BYPASS: auto-authenticated with a dev user that mirrors the
 * DevAuthMiddleware identity on the .NET API. Remove / gate behind
 * import.meta.env.DEV before implementing real JWT auth.
 *
 * C-2: DOM manipulation removed — presentation layer handles document.body.
 * M-1: Theme state moved to theme.store.ts.
 */
import { create } from 'zustand';

interface AuthUser {
  id: string;
  username: string;
  email: string;
  role: string;
}

interface AuthState {
  user: AuthUser | null;
  isAuthenticated: boolean;
  login:  (user: AuthUser) => void;
  logout: () => void;
}

const DEV_USER: AuthUser | null = import.meta.env.DEV
  ? { id: 'dev-user', username: 'Developer', email: 'dev@ums.local', role: 'admin' }
  : null;

export const useAuthStore = create<AuthState>((set) => ({
  user: DEV_USER,
  isAuthenticated: !!DEV_USER,

  login: (user) => set({ user, isAuthenticated: true }),

  logout: () => set({ user: null, isAuthenticated: false }),
}));
