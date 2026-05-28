import { describe, it, expect } from 'vitest';
import { NAV_ROUTES, pathToTab } from './navigation.config';

describe('navigation.config', () => {
  it('exports NAV_ROUTES with correct paths', () => {
    expect(NAV_ROUTES.tenants).toBe('/tenants');
    expect(NAV_ROUTES.users).toBe('/users');
    expect(NAV_ROUTES.delegations).toBe('/delegations');
    expect(NAV_ROUTES.systemSuites).toBe('/system-suites');
    expect(NAV_ROUTES.permissionTemplates).toBe('/permission-templates');
    expect(NAV_ROUTES.featureFlags).toBe('/feature-flags');
    expect(NAV_ROUTES.profiles).toBe('/profiles');
    expect(NAV_ROUTES.profile).toBe('/profile');
    expect(NAV_ROUTES.login).toBe('/login');
  });

  it('pathToTab returns correct tab for tenants path', () => {
    expect(pathToTab('/tenants')).toBe('tenants');
    expect(pathToTab('/tenants/123')).toBe('tenants');
  });

  it('pathToTab returns correct tab for users path', () => {
    expect(pathToTab('/users')).toBe('users');
    expect(pathToTab('/users/456/edit')).toBe('users');
  });

  it('pathToTab returns correct tab for delegations path', () => {
    expect(pathToTab('/delegations')).toBe('delegations');
  });

  it('pathToTab returns correct tab for system-suites path', () => {
    expect(pathToTab('/system-suites')).toBe('systemSuites');
    expect(pathToTab('/system-suites/abc')).toBe('systemSuites');
  });

  it('pathToTab returns correct tab for permission-templates path', () => {
    expect(pathToTab('/permission-templates')).toBe('permissionTemplates');
  });

  it('pathToTab returns correct tab for feature-flags path', () => {
    expect(pathToTab('/feature-flags')).toBe('featureFlags');
  });

  it('pathToTab returns correct tab for profiles path', () => {
    expect(pathToTab('/profiles')).toBe('profiles');
  });

  it('pathToTab returns correct tab for profile path', () => {
    expect(pathToTab('/profile')).toBe('profile');
  });

  it('pathToTab returns correct tab for login path', () => {
    expect(pathToTab('/login')).toBe('login');
  });

  it('pathToTab returns default tenants for unknown path', () => {
    expect(pathToTab('/unknown')).toBe('tenants');
    expect(pathToTab('/')).toBe('tenants');
  });
});
