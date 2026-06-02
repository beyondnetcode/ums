import React, { useState } from 'react';
import { Database, Menu, Sun, Moon, Bell, Globe, User, LogOut } from 'lucide-react';
import { useAuthStore } from '@app/stores/auth.store';
import { useThemeStore } from '@app/stores/theme.store';
import { useDevToolsStore } from '@app/stores/devTools.store';
import { useI18nStore } from '@app/stores/i18n.store';
import { useNotificationStore } from '@app/stores/notification.store';
import { useI18n } from '@app/i18n/use-i18n';
import { NotificationCenter } from '../components/NotificationCenter';
import { ToastQueue } from '../components/ToastQueue';
import { Tooltip } from '../components/Tooltip';
import { ConnectedUserDrawer } from '../components/ConnectedUserDrawer';

export const TopAppBar: React.FC<{ onToggleNav: () => void }> = ({ onToggleNav }) => {
  const { user, logout } = useAuthStore();
  const { isDarkMode, toggleDarkMode } = useThemeStore();
  useDevToolsStore();
  const { language, setLanguage } = useI18nStore();
  const { notifications, setIsOpen, isOpen } = useNotificationStore();
  const t = useI18n();

  const [isUserDrawerOpen, setIsUserDrawerOpen] = useState(false);

  const unreadCount = notifications.filter((n) => !n.read).length;

  const handleLanguageToggle = () => {
    setLanguage(language === 'en' ? 'es' : 'en');
  };

  const handleLogout = () => {
    logout();
  };

  const getUserInitials = () => {
    if (!user?.username) return '??';
    return user.username.substring(0, 2).toUpperCase();
  };

  const getTooltipMessage = () => {
    if (!user) return 'Not logged in';
    return `${user.username}\n${user.email}\nTenant: ${user.tenantCode || 'N/A'}`;
  };

  return (
    <>
      <header
        role="banner"
        data-testid="top-app-bar"
        className="sticky top-0 z-40 bg-m3-surface/85 backdrop-blur-md border-b border-m3-outline/25 h-16 px-6 flex items-center justify-between select-none"
      >
        <div className="flex items-center gap-3">
          <button
            onClick={onToggleNav}
            aria-label="Toggle navigation"
            className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary transition-colors lg:block hidden"
          >
            <Menu className="w-5 h-5" />
          </button>

          <div className="flex items-center gap-2">
            <div className="p-2 bg-gradient-to-tr from-m3-primary to-indigo-600 text-white rounded-xl shadow-md">
              <Database className="w-5 h-5" />
            </div>
            <div>
              <h1 className="text-sm font-extrabold tracking-wider bg-gradient-to-r from-m3-primary to-indigo-500 bg-clip-text text-transparent">
                {t.appName}
              </h1>
              <p className="text-[10px] text-m3-secondary/70 font-semibold uppercase tracking-widest">
                {t.appSubtitle}
              </p>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-3">
          {user && (
            <div className="flex items-center gap-2">
              <div className="hidden md:flex items-center gap-2 px-3 py-1.5 bg-m3-primary-container/20 rounded-xl border border-m3-primary-container/30 text-[11px] font-medium text-m3-primary">
                <span>{t.devUser} {user.username}</span>
              </div>
              <div className="hidden md:flex items-center gap-2 px-3 py-1.5 bg-indigo-50 dark:bg-indigo-950/30 rounded-xl border border-indigo-200 dark:border-indigo-800 text-[11px] font-semibold text-indigo-600 dark:text-indigo-400">
                <span>{user.isInternalAdmin ? 'Admin Local' : (user.tenantName || 'Tenant N/A')}</span>
              </div>
            </div>
          )}

          {user && (
            <Tooltip content={t.logoutBtn} placement="bottom">
              <button
                onClick={handleLogout}
                aria-label={t.logoutBtn}
                className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all border border-m3-outline/30"
              >
                <LogOut className="w-4 h-4" />
              </button>
            </Tooltip>
          )}

          <Tooltip content={t.toggleLanguage} placement="bottom">
            <button
              onClick={handleLanguageToggle}
              aria-label={language === 'en' ? 'Switch to Spanish' : 'Switch to English'}
              className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all flex items-center gap-1.5 border border-m3-outline/30"
            >
              <Globe className="w-4 h-4 text-indigo-400" />
              <span className="text-[11px] font-medium">{language.toUpperCase()}</span>
            </button>
          </Tooltip>

          <Tooltip content={t.toggleTheme} placement="bottom">
            <button
              onClick={toggleDarkMode}
              aria-label={isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'}
              className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all border border-m3-outline/30"
            >
              {isDarkMode ? <Sun className="w-4 h-4 text-amber-400" /> : <Moon className="w-4 h-4 text-indigo-500" />}
            </button>
          </Tooltip>

          <Tooltip content={t.openNotifications} placement="bottom">
            <button
              onClick={() => setIsOpen(!isOpen)}
              aria-label={`Notifications${unreadCount > 0 ? `, ${unreadCount} unread` : ''}`}
              className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all relative border border-m3-outline/30"
            >
              <Bell className="w-4 h-4" />
              {unreadCount > 0 && (
                <span className="absolute top-1 right-1 bg-m3-primary text-m3-on-primary font-extrabold text-[8px] px-1.5 py-0.5 rounded-full scale-90 animate-pulse">
                  {unreadCount}
                </span>
              )}
            </button>
          </Tooltip>

          {user && (
            <div className="h-8 w-px bg-m3-outline/30" />
          )}

          {user && (
            <div className="flex items-center gap-2">
              <Tooltip
                content={
                  <div className="text-center">
                    <p className="font-bold">{user.username}</p>
                    <p className="text-xs opacity-75">{user.email}</p>
                    <p className="text-xs opacity-75 mt-1">Tenant: {user.tenantCode || 'N/A'}</p>
                    <p className="text-[10px] mt-2 opacity-60">Click to view session details</p>
                  </div>
                }
                placement="bottom"
              >
                <button
                  onClick={() => setIsUserDrawerOpen(true)}
                  aria-label="View connected user details"
                  className="w-9 h-9 rounded-full bg-m3-primary/15 border-2 border-m3-primary/30 flex items-center justify-center font-bold text-xs text-m3-primary hover:bg-m3-primary/25 hover:border-m3-primary/50 transition-all cursor-pointer"
                >
                  {getUserInitials()}
                </button>
              </Tooltip>
            </div>
          )}
        </div>
      </header>

      <NotificationCenter />
      <ToastQueue />

      <ConnectedUserDrawer
        isOpen={isUserDrawerOpen}
        onClose={() => setIsUserDrawerOpen(false)}
        onLogout={handleLogout}
      />
    </>
  );
};
