/**
 * navigation.config.ts — Static navigation structure for the sidebar rail.
 *
 * Keeps MainLayout focused on rendering/behaviour by moving the
 * route → label → icon mapping into a declarative data structure.
 * Translations are resolved at render time via the `nameKey` field.
 */
import React from 'react';

export type NavItemId =
  | 'tenants'
  | 'users'
  | 'delegations'
  | 'systemSuites'
  | 'permissionTemplates'
  | 'featureFlags'
  | 'appConfigurations'
  | 'parameterCatalog'
  | 'profiles'
  | 'profile'
  | 'login';

export type UserRole = 'admin' | 'moderator' | 'user' | 'super_admin';

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
  permissionTemplates: '/permission-templates',
  featureFlags: '/feature-flags',
  appConfigurations: '/app-configurations',
  parameterCatalog: '/parameter-catalog',
  profiles: '/profiles',
  profile: '/profile',
  login: '/login',
};

export const pathToTab = (pathname: string): NavItemId => {
  if (pathname.startsWith('/tenants')) return 'tenants';
  if (pathname.startsWith('/users')) return 'users';
  if (pathname.startsWith('/delegations')) return 'delegations';
  if (pathname.startsWith('/system-suites')) return 'systemSuites';
  if (pathname.startsWith('/permission-templates')) return 'permissionTemplates';
  if (pathname.startsWith('/feature-flags')) return 'featureFlags';
  if (pathname.startsWith('/app-configurations')) return 'appConfigurations';
  if (pathname.startsWith('/parameter-catalog')) return 'parameterCatalog';
  if (pathname.startsWith('/profiles')) return 'profiles';
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
  Flag: React.ComponentType<{ className?: string }>;
  User: React.ComponentType<{ className?: string }>;
  LogOut: React.ComponentType<{ className?: string }>;
  Settings: React.ComponentType<{ className?: string }>;
  primaryColorClass: string;
  indigoColorClass: string;
  t: Record<string, string>;
}

export const NAV_MODULES = (deps: NavModulesFactoryDeps): NavModule[] => [
  {
    key: 'idm',
    nameKey: 'identityContext',
    icon: <deps.ShieldCheck className={`w-5 h-5 ${deps.primaryColorClass}`} />,
    members: [
      { id: 'tenants', nameKey: 'tenant', icon: <deps.Building2 className="w-4 h-4" /> },
      { id: 'users', nameKey: 'userAccounts', icon: <deps.Users className="w-4 h-4" /> },
    ],
  },
  {
    key: 'auth',
    nameKey: 'authorizationContext',
    icon: <deps.ShieldCheck className={`w-5 h-5 ${deps.primaryColorClass}`} />,
    members: [
      { id: 'systemSuites', nameKey: 'systemSuites', icon: <deps.Cpu className="w-4 h-4" /> },
      {
        id: 'permissionTemplates',
        nameKey: 'permissionTemplates',
        icon: <deps.ShieldCheck className="w-4 h-4" />,
      },
      { id: 'profiles', nameKey: 'profilesHeader', icon: <deps.ShieldCheck className="w-4 h-4" /> },
      { id: 'featureFlags', nameKey: 'featureFlags', icon: <deps.Flag className="w-4 h-4" /> },
    ],
  },
  {
    key: 'sys',
    nameKey: 'systemDiagnostics',
    icon: <deps.Cpu className={`w-5 h-5 ${deps.indigoColorClass}`} />,
    members: [
      {
        id: 'appConfigurations',
        nameKey: 'systemParameters',
        icon: <deps.Settings className="w-4 h-4" />,
      },
      {
        id: 'parameterCatalog',
        nameKey: 'parameterCatalog',
        icon: <deps.Settings className="w-4 h-4" />,
      },
      { id: 'profile', nameKey: 'profileStats', icon: <deps.User className="w-4 h-4" /> },
    ],
  },
];
