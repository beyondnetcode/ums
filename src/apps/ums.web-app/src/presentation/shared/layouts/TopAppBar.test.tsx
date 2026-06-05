import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { TopAppBar } from './TopAppBar';
import * as authStoreModule from '@app/stores/auth.store';
import * as themeStoreModule from '@app/stores/theme.store';
import * as devToolsStoreModule from '@app/stores/devTools.store';
import * as i18nStoreModule from '@app/stores/i18n.store';
import * as notificationStoreModule from '@app/stores/notification.store';
import * as useI18nModule from '@app/i18n/use-i18n';

vi.mock('@app/stores/auth.store');
vi.mock('@app/stores/theme.store');
vi.mock('@app/stores/devTools.store');
vi.mock('@app/stores/i18n.store');
vi.mock('@app/stores/notification.store');
vi.mock('@app/i18n/use-i18n');
vi.mock('../components/NotificationCenter', () => ({
  NotificationCenter: () => <div data-testid="notification-center" />,
}));
vi.mock('../components/ToastQueue', () => ({
  ToastQueue: () => <div data-testid="toast-queue" />,
}));
vi.mock('../components/Tooltip', () => ({ Tooltip: ({ children }: any) => <>{children}</> }));

describe('TopAppBar', () => {
  const mockOnToggleNav = vi.fn();

  beforeEach(() => {
    vi.restoreAllMocks();

    vi.mocked(authStoreModule.useAuthStore).mockReturnValue({
      user: { id: 'u-1', username: 'testuser' },
      logout: vi.fn(),
    } as any);

    vi.mocked(themeStoreModule.useThemeStore).mockReturnValue({
      isDarkMode: false,
      toggleDarkMode: vi.fn(),
    } as any);

    vi.mocked(devToolsStoreModule.useDevToolsStore).mockReturnValue({} as any);

    vi.mocked(i18nStoreModule.useI18nStore).mockReturnValue({
      language: 'en',
      setLanguage: vi.fn(),
    } as any);

    vi.mocked(notificationStoreModule.useNotificationStore).mockImplementation((selector: any) => {
      const state = {
        notifications: [],
        setIsOpen: vi.fn(),
        isOpen: false,
      };
      return selector ? selector(state) : state;
    });

    vi.mocked(useI18nModule.useI18n).mockReturnValue({
      appName: 'UMS',
      appSubtitle: 'User Management',
      devUser: 'Dev:',
      toggleLanguage: 'Toggle Language',
      toggleTheme: 'Toggle Theme',
      openNotifications: 'Notifications',
      logoutBtn: 'Logout',
    } as any);
  });

  it('renders header element', () => {
    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.getByRole('banner')).toBeInTheDocument();
  });

  it('renders app name', () => {
    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.getByText('UMS')).toBeInTheDocument();
  });

  it('renders app subtitle', () => {
    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.getByText('User Management')).toBeInTheDocument();
  });

  it('renders NotificationCenter', () => {
    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.getByTestId('notification-center')).toBeInTheDocument();
  });

  it('renders ToastQueue', () => {
    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.getByTestId('toast-queue')).toBeInTheDocument();
  });

  it('calls onToggleNav when menu button is clicked', () => {
    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    const menuButton = screen.getByLabelText('Toggle navigation');
    fireEvent.click(menuButton);
    expect(mockOnToggleNav).toHaveBeenCalled();
  });

  it('shows user info when user exists', () => {
    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.getByText(/Dev:/)).toBeInTheDocument();
  });

  it('shows user initials', () => {
    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.getByText('TE')).toBeInTheDocument();
  });

  it('calls logout when logout button is clicked', () => {
    const logout = vi.fn();
    vi.mocked(authStoreModule.useAuthStore).mockReturnValue({
      user: { id: 'u-1', username: 'testuser' },
      logout,
    } as any);

    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    const logoutButton = screen.getByLabelText('Logout');
    fireEvent.click(logoutButton);
    expect(logout).toHaveBeenCalled();
  });

  it('calls toggleDarkMode when theme button is clicked', () => {
    const toggleDarkMode = vi.fn();
    vi.mocked(themeStoreModule.useThemeStore).mockReturnValue({
      isDarkMode: false,
      toggleDarkMode,
    } as any);

    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    const themeButton = screen.getByLabelText('Switch to dark mode');
    fireEvent.click(themeButton);
    expect(toggleDarkMode).toHaveBeenCalled();
  });

  it('calls setLanguage when language button is clicked', () => {
    const setLanguage = vi.fn();
    vi.mocked(i18nStoreModule.useI18nStore).mockReturnValue({
      language: 'en',
      setLanguage,
    } as any);

    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    const langButton = screen.getByLabelText('Switch to Spanish');
    fireEvent.click(langButton);
    expect(setLanguage).toHaveBeenCalledWith('es');
  });

  it('shows unread notification count when there are unread notifications', () => {
    vi.mocked(notificationStoreModule.useNotificationStore).mockImplementation((selector: any) => {
      const state = {
        notifications: [
          { id: '1', read: false, title: 'Test', message: 'Test', type: 'info' as const },
        ],
        setIsOpen: vi.fn(),
        isOpen: false,
      };
      return selector ? selector(state) : state;
    });

    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.getByText('1')).toBeInTheDocument();
  });

  it('calls setIsOpen when notification button is clicked', () => {
    const setIsOpen = vi.fn();
    vi.mocked(notificationStoreModule.useNotificationStore).mockImplementation((selector: any) => {
      const state = {
        notifications: [],
        setIsOpen,
        isOpen: false,
      };
      return selector ? selector(state) : state;
    });

    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    const notifButton = screen.getByLabelText('Notifications');
    fireEvent.click(notifButton);
    expect(setIsOpen).toHaveBeenCalledWith(true);
  });

  it('does not show user info when no user', () => {
    vi.mocked(authStoreModule.useAuthStore).mockReturnValue({
      user: null,
      logout: vi.fn(),
    } as any);

    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.queryByText(/Dev:/)).not.toBeInTheDocument();
  });

  it('does not show logout button when no user', () => {
    vi.mocked(authStoreModule.useAuthStore).mockReturnValue({
      user: null,
      logout: vi.fn(),
    } as any);

    render(<TopAppBar onToggleNav={mockOnToggleNav} />);
    expect(screen.queryByLabelText('Log out')).not.toBeInTheDocument();
  });
});
