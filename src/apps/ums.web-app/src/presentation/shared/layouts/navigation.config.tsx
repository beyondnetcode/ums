/**
 * navigation.config.ts — Static navigation structure for the sidebar rail.
 *
 * Keeps MainLayout focused on rendering/behaviour by moving the
 * route → label → icon mapping into a declarative data structure.
 * Translations are resolved at render time via the `nameKey` field.
 */
import React from 'react';

export type NavItemId = 'tenants' | 'users' | 'delegations' | 'systemSuites' | 'profile' | 'login';

export interface NavItem {
  id: NavItemId;
  nameKey: string;
  icon: React.ReactNode;
}

export interface NavModule {
  key: string;
  nameKey: string;
  icon: React.ReactNode;
  members: NavItem[];
}

export const NAV_ROUTES: Record<NavItemId, string> = {
  tenants: '/tenants',
  users: '/users',
  delegations: '/delegations',
  systemSuites: '/system-suites',
  profile: '/profile',
  login: '/login',
};

export const pathToTab = (pathname: string): NavItemId => {
  if (pathname.startsWith('/tenants')) return 'tenants';
  if (pathname.startsWith('/users')) return 'users';
  if (pathname.startsWith('/delegations')) return 'delegations';
  if (pathname.startsWith('/system-suites')) return 'systemSuites';
  if (pathname.startsWith('/profile')) return 'profile';
  if (pathname.startsWith('/login')) return 'login';
  return 'tenants';
};

interface NavModulesFactoryDeps {
  ShieldCheck: React.ComponentType<{ className?: string }>;
  Building2: React.ComponentType<{ className?: string }>;
  Users: React.ComponentType<{ className?: string }>;
  GitMerge: React.ComponentType<{ className?: string }>;
  Cpu: React.ComponentType<{ className?: string }>;
  User: React.ComponentType<{ className?: string }>;
  LogOut: React.ComponentType<{ className?: string }>;
  primaryColorClass: string;
  indigoColorClass: string;
  t: Record<string, string>;
}

export const NAV_MODULES = (deps: NavModulesFactoryDeps): NavModule[] => [
  {
    key: 'identity',
    nameKey: 'identityContext',
    icon: <deps.ShieldCheck className={`w-5 h-5 ${deps.primaryColorClass}`} />,
    members: [
      { id: 'tenants',     nameKey: 'tenant',              icon: <deps.Building2 className="w-4 h-4" /> },
      { id: 'users',       nameKey: 'userAccounts',        icon: <deps.Users className="w-4 h-4" /> },
      // TODO: Delegations - will be implemented later
      // { id: 'delegations', nameKey: 'delegationManagement', icon: <deps.GitMerge className="w-4 h-4" /> },
    ],
  },
  {
    key: 'authorization',
    nameKey: 'authorizationContext',
    icon: <deps.ShieldCheck className={`w-5 h-5 ${deps.primaryColorClass}`} />,
    members: [
      { id: 'systemSuites', nameKey: 'systemSuites', icon: <deps.Cpu className="w-4 h-4" /> },
    ],
  },
  {
    key: 'system',
    nameKey: 'systemDiagnostics',
    icon: <deps.Cpu className={`w-5 h-5 ${deps.indigoColorClass}`} />,
    members: [
      { id: 'profile', nameKey: 'profileStats', icon: <deps.User className="w-4 h-4" /> },
      { id: 'login', nameKey: 'developerSession', icon: <deps.LogOut className="w-4 h-4" /> },
    ],
  },
];
