import { create } from 'zustand';

export interface AppNotification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: string;
  read: boolean;
}

interface NotificationState {
  notifications: AppNotification[];
  isOpen: boolean;
  addNotification: (notification: Omit<AppNotification, 'id' | 'timestamp' | 'read'>) => void;
  markAsRead: (id: string) => void;
  markAllAsRead: () => void;
  clearAll: () => void;
  setIsOpen: (isOpen: boolean) => void;
}

export const useNotificationStore = create<NotificationState>((set) => ({
  notifications: [
    {
      id: 'init',
      title: 'System Initialized',
      message: 'UMS Client running in Clean Architecture mode with local emulators.',
      type: 'info',
      timestamp: new Date().toISOString(),
      read: false,
    }
  ],
  isOpen: false,
  addNotification: (n) => set((state) => {
    const newN: AppNotification = {
      ...n,
      id: Math.random().toString(36).substring(2, 9),
      timestamp: new Date().toISOString(),
      read: false,
    };
    return {
      notifications: [newN, ...state.notifications].slice(0, 50), // Cap at 50 logs
    };
  }),
  markAsRead: (id) => set((state) => ({
    notifications: state.notifications.map((n) => n.id === id ? { ...n, read: true } : n),
  })),
  markAllAsRead: () => set((state) => ({
    notifications: state.notifications.map((n) => ({ ...n, read: true })),
  })),
  clearAll: () => set({ notifications: [] }),
  setIsOpen: (isOpen) => set({ isOpen }),
}));
