import React, { useState } from 'react';
import { useAuthStore } from '../../../application/stores/auth.store';
import { useNotificationStore } from '../../../application/stores/notification.store';
import {
  Database,
  Building2,
  User,
  LogOut,
  Sun,
  Moon,
  Bell,
  Globe,
  Globe2,
  ChevronRight,
  ChevronDown,
  ShieldCheck,
  Cpu,
  Menu
} from 'lucide-react';
import { NotificationCenter } from '../components/NotificationCenter';

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

  return (
    <div className="min-h-screen flex flex-col transition-colors duration-500 bg-m3-surface text-m3-on-surface">
      {/* Background ambient glowing shapes for premium wow-factor */}
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
                UMS ENTERPRISE
              </h1>
              <p className="text-[10px] text-m3-secondary/70 font-semibold uppercase tracking-widest">
                Modular Monolith
              </p>
            </div>
          </div>
        </div>

        {/* Action Controls */}
        <div className="flex items-center gap-3">
          {/* Active Developer Headers indicator */}
          <div className="hidden md:flex items-center gap-2 px-3 py-1.5 bg-m3-primary-container/20 rounded-xl border border-m3-primary-container/30 text-[10px] font-bold text-m3-primary uppercase tracking-wide">
            <span>Dev User: {devUserId.substring(0, 8)}...</span>
          </div>

          {/* X-Language Toggle */}
          <button
            onClick={handleLanguageToggle}
            className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all flex items-center gap-1.5 border border-m3-outline/30"
            title="Toggle System Language Culture"
          >
            <Globe className="w-4 h-4 text-indigo-400" />
            <span className="text-[10px] font-extrabold tracking-wider uppercase">{devLanguage}</span>
          </button>

          {/* Dark / Light Toggle */}
          <button
            onClick={toggleDarkMode}
            className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all border border-m3-outline/30"
            title="Toggle Light/Dark Theme"
          >
            {isDarkMode ? <Sun className="w-4.5 h-4.5 text-amber-400" /> : <Moon className="w-4.5 h-4.5 text-indigo-500" />}
          </button>

          {/* Bell Notification badge */}
          <button
            onClick={() => setIsOpen(!isOpen)}
            className="p-2.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all relative border border-m3-outline/30"
            title="Open System Audits Drawer"
          >
            <Bell className="w-4.5 h-4.5" />
            {unreadCount > 0 && (
              <span className="absolute top-1 right-1 bg-m3-primary text-m3-on-primary font-extrabold text-[8px] px-1.5 py-0.5 rounded-full scale-90 animate-pulse">
                {unreadCount}
              </span>
            )}
          </button>

          {user && (
            <div className="h-8 w-px bg-m3-outline/30" />
          )}

          {user && (
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 rounded-full bg-m3-primary/15 border border-m3-primary/30 flex items-center justify-center font-bold text-xs text-m3-primary">
                {user.username.substring(0, 2).toUpperCase()}
              </div>
              <button
                onClick={logout}
                className="p-2 rounded-full hover:bg-m3-error/15 text-m3-secondary hover:text-m3-error transition-all"
                title="Logout"
              >
                <LogOut className="w-4 h-4" />
              </button>
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
            {/* Top Collapsible Modules Links */}
            <nav className="space-y-4 px-3 select-none">
              {(() => {
                const modules = [
                  {
                    key: 'identity',
                    name: 'Identity Context',
                    icon: <ShieldCheck className="w-5 h-5 text-m3-primary" />,
                    members: [
                      { id: 'tenants', name: 'Tenant', icon: <Building2 className="w-4.5 h-4.5" /> },
                    ],
                  },
                  {
                    key: 'system',
                    name: 'System Diagnostics',
                    icon: <Cpu className="w-5 h-5 text-indigo-400" />,
                    members: [
                      { id: 'profile', name: 'Profile Statistics', icon: <User className="w-4.5 h-4.5" /> },
                      { id: 'login', name: 'Developer Session', icon: <LogOut className="w-4.5 h-4.5" /> },
                    ],
                  },
                ];

                if (navCollapsed) {
                  // Collapsed Rail Mode: Render all member icons directly
                  return (
                    <div className="space-y-2.5 flex flex-col items-center">
                      {modules.flatMap((m) => m.members).map((tab) => {
                        const isActive = activeTab === tab.id;
                        return (
                          <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id as any)}
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

                // Expanded Drawer Mode: Render collapsible modules and indented members
                return modules.map((mod) => {
                  const isExpanded = expandedModules[mod.key];
                  return (
                    <div key={mod.key} className="space-y-1.5">
                      {/* Module Collapsible Trigger */}
                      <button
                        onClick={() => toggleModule(mod.key)}
                        className="w-full flex items-center justify-between px-3 py-2.5 rounded-xl hover:bg-m3-primary/5 transition-all text-left"
                      >
                        <div className="flex items-center gap-2.5">
                          {mod.icon}
                          <span className="text-[10px] font-black uppercase tracking-wider text-m3-on-surface/80">
                            {mod.name}
                          </span>
                        </div>
                        {isExpanded ? (
                          <ChevronDown className="w-4 h-4 text-m3-secondary transition-transform duration-200" />
                        ) : (
                          <ChevronRight className="w-4 h-4 text-m3-secondary transition-transform duration-200" />
                        )}
                      </button>

                      {/* Indented Module Members */}
                      {isExpanded && (
                        <div className="pl-2.5 ml-2.5 border-l border-m3-outline/25 space-y-1 animate-slideDown">
                          {mod.members.map((tab) => {
                            const isActive = activeTab === tab.id;
                            return (
                              <button
                                key={tab.id}
                                onClick={() => setActiveTab(tab.id as any)}
                                className={`w-full flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all duration-150 text-left font-semibold text-xs tracking-wider uppercase ${
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

            {/* Bottom Credits info */}
            {!navCollapsed && (
              <div className="px-6 py-4 border-t border-m3-outline/10 text-[9px] text-m3-secondary/50 font-bold uppercase tracking-widest leading-relaxed">
                <p>UMS Identity Portal</p>
                <p className="mt-1 font-normal text-indigo-400">Clean Architecture v1.2</p>
              </div>
            )}
          </div>
        </aside>

        {/* Content Panel */}
        <main className="flex-1 overflow-y-auto p-8 relative">
          <div className="max-w-6xl mx-auto space-y-6">
            {children}
          </div>
        </main>
      </div>

      {/* Slide-over Notifications */}
      <NotificationCenter />
    </div>
  );
};
