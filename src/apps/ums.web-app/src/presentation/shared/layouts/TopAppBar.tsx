import React from 'react';
import { Database, Menu, Sun, Moon, Bell, Globe, LogOut } from 'lucide-react';
import { useAuthStore } from '@app/stores/auth.store';
import { useThemeStore } from '@app/stores/theme.store';
import { useDevToolsStore } from '@app/stores/devTools.store';
import { useI18nStore } from '@app/stores/i18n.store';
import { useNotificationStore } from '@app/stores/notification.store';
import { useI18n } from '@app/i18n/use-i18n';
import { NotificationCenter } from '../components/NotificationCenter';
import { Tooltip } from '../components/Tooltip';

export const TopAppBar: React.FC<{ onToggleNav: () => void }> = ({ onToggleNav }) => {
  const { user, logout } = useAuthStore();
  const { isDarkMode, toggleDarkMode } = useThemeStore();
  const { devUserId } = useDevToolsStore();
  const { language, setLanguage } = useI18nStore();
  const { notifications, setIsOpen, isOpen } = useNotificationStore();
  const t = useI18n();

  const unreadCount = notifications.filter((n) => !n.read).length;

  const handleLanguageToggle = () => {
    setLanguage(language === 'en' ? 'es' : 'en');
  };

  return (
    <>
      <header className="sticky top-0 z-40 bg-m3-surface/85 backdrop-blur-md border-b border-m3-outline/25 h-16 px-6 flex items-center justify-between select-none">
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
          <div className="hidden md:flex items-center gap-2 px-3 py-1.5 bg-m3-primary-container/20 rounded-xl border border-m3-primary-container/30 text-[11px] font-medium text-m3-primary">
            <span>{t.devUser} {devUserId.substring(0, 8)}...</span>
          </div>

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
              <div className="w-8 h-8 rounded-full bg-m3-primary/15 border border-m3-primary/30 flex items-center justify-center font-bold text-xs text-m3-primary">
                {user.username.substring(0, 2).toUpperCase()}
              </div>
              <Tooltip content={t.logoutBtn} placement="bottom">
                <button
                  onClick={logout}
                  aria-label="Log out"
                  className="p-2 rounded-full hover:bg-m3-error/15 text-m3-secondary hover:text-m3-error transition-all"
                >
                  <LogOut className="w-4 h-4" />
                </button>
              </Tooltip>
            </div>
          )}
        </div>
      </header>
      <NotificationCenter />
    </>
  );
};
