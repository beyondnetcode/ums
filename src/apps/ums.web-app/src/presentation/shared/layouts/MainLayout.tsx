import React, { useState, useCallback } from 'react';
import { useAuthStore } from '../../../application/stores/auth.store';
import { useNotificationStore } from '../../../application/stores/notification.store';
import { useI18n } from '../../../application/i18n/use-i18n';
import { useIdleTimeout } from '../../../application/hooks/use-idle-timeout';
import {
  Database,
  Building2,
  User,
  LogOut,
  Sun,
  Moon,
  Bell,
  Globe,
  ChevronRight,
  ChevronDown,
  ShieldCheck,
  Cpu,
  Menu
} from 'lucide-react';
import { NotificationCenter } from '../components/NotificationCenter';
import { Tooltip } from '../components/Tooltip';

interface MainLayoutProps {
  children: React.ReactNode;
  activeTab: 'tenants' | 'profile' | 'login';
  setActiveTab: (tab: 'tenants' | 'profile' | 'login') => void;
}

export const MainLayout: React.FC<MainLayoutProps> = ({
  children,
  activeTab,
  setActiveTab,
}) => {
  const { isDarkMode, toggleDarkMode, user, devUserId, devLanguage, logout, setDevLanguage } = useAuthStore();
  const { notifications, setIsOpen, isOpen } = useNotificationStore();
  const t = useI18n();
  const [navCollapsed, setNavCollapsed] = useState(false);
  const [expandedModules, setExpandedModules] = useState<{ [key: string]: boolean }>({
    identity: true,
    system: true,
  });

  const toggleModule = (moduleKey: string) => {
    setExpandedModules((prev) => ({
      ...prev,
      [moduleKey]: !prev[moduleKey],
    }));
  };

  const unreadCount = notifications.filter((n) => !n.read).length;

  const handleLanguageToggle = () => {
    setDevLanguage(devLanguage === 'en' ? 'es' : 'en');
  };

  const handleIdleLogout = useCallback(() => {
    logout();
    addNotification({
      title: t.sessionExpired,
      message: t.sessionExpiredMsg,
      type: 'warning',
    });
  }, [logout, addNotification, t]);

  useIdleTimeout({
    timeoutMs: 15 * 60 * 1000,
    onIdle: handleIdleLogout,
    enabled: !!user,
  });

  return (
    <div className="min-h-screen flex flex-col transition-colors duration-500 bg-m3-surface text-m3-on-surface">
      <div className="m3-glow top-0 left-1/4 w-[450px] h-[450px] bg-m3-primary/10 dark:bg-m3-primary/5" />
      <div className="m3-glow bottom-10 right-10 w-[350px] h-[350px] bg-indigo-500/10 dark:bg-indigo-500/5" />

      {/* Top App Bar */}
      <header className="sticky top-0 z-40 bg-m3-surface/85 backdrop-blur-md border-b border-m3-outline/25 h-16 px-6 flex items-center justify-between select-none">
        <div className="flex items-center gap-3">
          <button
            onClick={() => setNavCollapsed(!navCollapsed)}
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

        {/* Action Controls */}
        <div className="flex items-center gap-3">
          <div className="hidden md:flex items-center gap-2 px-3 py-1.5 bg-m3-primary-container/20 rounded-xl border border-m3-primary-container/30 text-[11px] font-medium text-m3-primary">
            <span>{t.devUser} {devUserId.substring(0, 8)}...</span>
          </div>

          {/* Language Toggle */}
          <Tooltip content={t.toggleLanguage} placement="bottom">
            <button
              onClick={handleLanguageToggle}
              className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all flex items-center gap-1.5 border border-m3-outline/30"
            >
              <Globe className="w-4 h-4 text-indigo-400" />
              <span className="text-[11px] font-medium">{devLanguage.toUpperCase()}</span>
            </button>
          </Tooltip>

          {/* Dark / Light Toggle */}
          <Tooltip content={t.toggleTheme} placement="bottom">
            <button
              onClick={toggleDarkMode}
              className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all border border-m3-outline/30"
            >
              {isDarkMode ? <Sun className="w-4.5 h-4.5 text-amber-400" /> : <Moon className="w-4.5 h-4.5 text-indigo-500" />}
            </button>
          </Tooltip>

          {/* Bell Notification badge */}
          <Tooltip content={t.openNotifications} placement="bottom">
            <button
              onClick={() => setIsOpen(!isOpen)}
              className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all relative border border-m3-outline/30"
            >
              <Bell className="w-4.5 h-4.5" />
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
                  className="p-2 rounded-full hover:bg-m3-error/15 text-m3-secondary hover:text-m3-error transition-all"
                >
                  <LogOut className="w-4 h-4" />
                </button>
              </Tooltip>
            </div>
          )}
        </div>
      </header>

      {/* Main Body Grid */}
      <div className="flex-1 flex overflow-hidden">
        {/* Navigation Rail */}
        <aside
          className={`bg-m3-surface border-r border-m3-outline/25 select-none transition-all duration-300 ${
            navCollapsed ? 'w-20' : 'w-64'
          } lg:block hidden`}
        >
          <div className="flex flex-col h-full py-6 justify-between">
            <nav className="space-y-4 px-3 select-none">
              {(() => {
                const modules = [
                  {
                    key: 'identity',
                    name: t.identityContext,
                    icon: <ShieldCheck className="w-5 h-5 text-m3-primary" />,
                    members: [
                      { id: 'tenants', name: t.tenant, icon: <Building2 className="w-4.5 h-4.5" /> },
                    ],
                  },
                  {
                    key: 'system',
                    name: t.systemDiagnostics,
                    icon: <Cpu className="w-5 h-5 text-indigo-400" />,
                    members: [
                      { id: 'profile', name: t.profileStats, icon: <User className="w-4.5 h-4.5" /> },
                      { id: 'login', name: t.developerSession, icon: <LogOut className="w-4.5 h-4.5" /> },
                    ],
                  },
                ];

                if (navCollapsed) {
                  return (
                    <div className="space-y-2.5 flex flex-col items-center">
                      {modules.flatMap((m) => m.members).map((tab) => {
                        const isActive = activeTab === tab.id;
                        return (
                          <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id as 'tenants' | 'profile' | 'login')}
                            className={`p-3.5 rounded-2xl transition-all duration-200 text-center ${
                              isActive
                                ? 'bg-m3-primary-container text-m3-on-primary-container font-extrabold elevation-1'
                                : 'text-m3-secondary hover:bg-m3-primary/10 hover:text-m3-primary'
                            }`}
                            title={tab.name}
                          >
                            <span className={isActive ? 'text-m3-primary scale-110' : ''}>{tab.icon}</span>
                          </button>
                        );
                      })}
                    </div>
                  );
                }

                return modules.map((mod) => {
                  const isExpanded = expandedModules[mod.key];
                  return (
                    <div key={mod.key} className="space-y-1.5">
                      <button
                        onClick={() => toggleModule(mod.key)}
                        className="w-full flex items-center justify-between px-3 py-2.5 rounded-xl hover:bg-m3-primary/5 transition-all text-left"
                      >
                        <div className="flex items-center gap-2.5">
                          {mod.icon}
                          <span className="text-xs font-semibold text-m3-on-surface/80">
                            {mod.name}
                          </span>
                        </div>
                        {isExpanded ? (
                          <ChevronDown className="w-4 h-4 text-m3-secondary transition-transform duration-200" />
                        ) : (
                          <ChevronRight className="w-4 h-4 text-m3-secondary transition-transform duration-200" />
                        )}
                      </button>

                      {isExpanded && (
                        <div className="pl-2.5 ml-2.5 border-l border-m3-outline/25 space-y-1 animate-slideDown">
                          {mod.members.map((tab) => {
                            const isActive = activeTab === tab.id;
                            return (
                              <button
                                key={tab.id}
                                onClick={() => setActiveTab(tab.id as 'tenants' | 'profile' | 'login')}
                                className={`w-full flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all duration-150 text-left font-medium text-sm ${
                                  isActive
                                    ? 'bg-m3-primary-container text-m3-on-primary-container font-extrabold elevation-1'
                                    : 'text-m3-secondary hover:bg-m3-primary/10 hover:text-m3-primary'
                                }`}
                              >
                                <span className={isActive ? 'text-m3-primary' : 'text-m3-secondary'}>
                                  {tab.icon}
                                </span>
                                <span>{tab.name}</span>
                              </button>
                            );
                          })}
                        </div>
                      )}
                    </div>
                  );
                });
              })()}
            </nav>

            {!navCollapsed && (
              <div className="px-6 py-4 border-t border-m3-outline/10 text-[10px] text-m3-secondary/50 font-normal leading-relaxed">
                <p>{t.portalFooter}</p>
                <p className="mt-1 font-normal text-indigo-400">{t.archVersion}</p>
              </div>
            )}
          </div>
        </aside>

        {/* Content Panel */}
        <main className="flex-1 overflow-y-auto flex flex-col relative">
          <div className="flex-1 flex flex-col p-5 min-h-0">
            {children}
          </div>
        </main>
      </div>

      <NotificationCenter />
    </div>
  );
};
