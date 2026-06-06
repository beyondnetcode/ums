import React, { useState, useMemo } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useI18n } from '@app/i18n/use-i18n';
import { useNavigationPrefetch } from '@app/shared/hooks/use-navigation-prefetch';
import { useAccessResolution } from '@app/authorization/hooks/use-access-resolution';
import {
  Building2,
  User,
  LogOut,
  ChevronRight,
  ChevronDown,
  ShieldCheck,
  Cpu,
  Users,
  GitMerge,
  Flag,
  Settings,
} from 'lucide-react';
import { NAV_ROUTES, pathToTab, NAV_MODULES } from './navigation.config';
import type { NavModule } from './navigation.config';

interface NavRailProps {
  collapsed: boolean;
}

export const NavRail: React.FC<NavRailProps> = ({ collapsed }) => {
  const t = useI18n();
  const navigate = useNavigate();
  const location = useLocation();
  const { prefetchById } = useNavigationPrefetch();
  const [expandedModules, setExpandedModules] = useState<{ [key: string]: boolean }>({
    idm: true,
    auth: true,
    sys: true,
  });

  const activeTab = pathToTab(location.pathname);

  const modules: NavModule[] = useMemo(
    () =>
      NAV_MODULES({
        ShieldCheck,
        Building2,
        Users,
        GitMerge,
        Cpu,
        Flag,
        User,
        LogOut,
        Settings,
        primaryColorClass: 'text-m3-primary',
        indigoColorClass: 'text-indigo-400',
        t,
      }),
    [t]
  );

  const toggleModule = (moduleKey: string) => {
    setExpandedModules(prev => ({
      ...prev,
      [moduleKey]: !prev[moduleKey],
    }));
  };

  const { hasMenuAccess, hasModuleAccess, graph } = useAccessResolution();

  const filteredModules = useMemo(() => {
    // Si no hay grafo (ej. entorno legacy o modo superadmin interno sin grafo total),
    // devolvemos todo asumiendo que el router.tsx bloqueará lo necesario,
    // o aplicamos bypass si es admin.
    if (!graph) return modules;

    const getMenuCode = (id: string) => {
      if (id === 'systemSuites') return 'SYSTEM_SUITES';
      if (id === 'permissionTemplates') return 'PERMISSION_TEMPLATES';
      if (id === 'featureFlags') return 'FEATURE_FLAGS';
      if (id === 'appConfigurations') return 'APP_CONFIG';
      if (id === 'parameterCatalog') return 'PARAM_CATALOG';
      return id.toUpperCase();
    };

    return modules
      .map(mod => ({
        ...mod,
        members: mod.members.filter(tab =>
          hasMenuAccess(mod.key.toUpperCase(), getMenuCode(tab.id))
        ),
      }))
      .filter(mod => mod.members.length > 0);
  }, [modules, graph, hasMenuAccess]);

  if (collapsed) {
    return (
      <aside
        data-testid="nav-rail"
        className="bg-m3-surface border-r border-m3-outline/25 select-none transition-all duration-300 w-20 lg:block hidden"
      >
        <div className="flex flex-col h-full py-6 justify-between">
          <nav
            role="navigation"
            aria-label="Main navigation"
            className="space-y-4 px-3 select-none"
          >
            <div className="space-y-2.5 flex flex-col items-center">
              {filteredModules
                .flatMap(m => m.members)
                .map(tab => {
                  const isActive = activeTab === tab.id;
                  return (
                    <button
                      key={tab.id}
                      onClick={() => navigate(NAV_ROUTES[tab.id])}
                      onMouseOver={() => prefetchById(tab.id)}
                      className={`p-2.5 rounded-xl transition-all duration-200 text-center ${
                        isActive
                          ? 'bg-m3-primary-container text-m3-on-primary-container font-semibold'
                          : 'text-m3-secondary hover:bg-m3-primary/10 hover:text-m3-primary'
                      }`}
                      title={(t as Record<string, string>)[tab.nameKey] ?? tab.nameKey}
                    >
                      <span className={isActive ? 'text-m3-primary' : ''}>{tab.icon}</span>
                    </button>
                  );
                })}
            </div>
          </nav>
        </div>
      </aside>
    );
  }

  return (
    <aside
      data-testid="nav-rail"
      className="bg-m3-surface border-r border-m3-outline/25 select-none transition-all duration-300 w-64 lg:block hidden"
    >
      <div className="flex flex-col h-full py-6 justify-between">
        <nav role="navigation" aria-label="Main navigation" className="space-y-4 px-3 select-none">
          {filteredModules.map(mod => {
            const isExpanded = expandedModules[mod.key];
            return (
              <div key={mod.key} className="space-y-1.5">
                <button
                  onClick={() => toggleModule(mod.key)}
                  aria-expanded={isExpanded}
                  aria-controls={`nav-module-${mod.key}`}
                  className="w-full flex items-center justify-between px-2.5 py-2 rounded-lg hover:bg-m3-primary/5 transition-all text-left"
                >
                  <div className="flex items-center gap-2">
                    {mod.icon}
                    <span className="text-[11px] font-semibold uppercase tracking-wider text-m3-on-surface/70">
                      {(t as Record<string, string>)[mod.nameKey] ?? mod.nameKey}
                    </span>
                  </div>
                  {isExpanded ? (
                    <ChevronDown className="w-3.5 h-3.5 text-m3-secondary transition-transform duration-200" />
                  ) : (
                    <ChevronRight className="w-3.5 h-3.5 text-m3-secondary transition-transform duration-200" />
                  )}
                </button>

                {isExpanded && (
                  <div
                    id={`nav-module-${mod.key}`}
                    className="pl-2.5 ml-2.5 border-l border-m3-outline/25 space-y-1 animate-slideDown"
                  >
                    {mod.members.map(tab => {
                      const isActive = activeTab === tab.id;
                      return (
                        <button
                          key={tab.id}
                          onClick={() => navigate(NAV_ROUTES[tab.id])}
                          onMouseOver={() => prefetchById(tab.id)}
                          className={`w-full flex items-center gap-2.5 px-3 py-2 rounded-lg transition-all duration-150 text-left font-medium text-[12px] ${
                            isActive
                              ? 'bg-m3-primary-container text-m3-on-primary-container font-semibold'
                              : 'text-m3-secondary hover:bg-m3-primary/10 hover:text-m3-primary'
                          }`}
                        >
                          <span className={isActive ? 'text-m3-primary' : 'text-m3-secondary/70'}>
                            {tab.icon}
                          </span>
                          <span>{(t as Record<string, string>)[tab.nameKey] ?? tab.nameKey}</span>
                        </button>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </nav>

        <div className="px-6 py-4 border-t border-m3-outline/10 text-[10px] text-m3-secondary/50 font-normal leading-relaxed">
          <p>{t.portalFooter}</p>
          <p className="mt-1 font-normal text-indigo-400">{t.archVersion}</p>
        </div>
      </div>
    </aside>
  );
};
