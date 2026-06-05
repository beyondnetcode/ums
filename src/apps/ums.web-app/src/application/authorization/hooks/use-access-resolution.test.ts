import { renderHook } from '@testing-library/react';
import { useAccessResolution } from './use-access-resolution';
import { useAuthStore } from '@app/stores/auth.store';
import { AccessEffect } from '@domain/authorization/schemas/authorization-graph.schema';
import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('@app/stores/auth.store');

describe('useAccessResolution', () => {
  const mockGraph = {
    menuAccess: [
      {
        code: 'sys',
        status: 'Active',
        menus: [
          {
            code: 'profiles',
            subMenus: [
              {
                code: 'profiles-list',
                options: [
                  { code: 'view', actionCode: 'read', effect: AccessEffect.Allow },
                  { code: 'create', actionCode: 'create', effect: AccessEffect.Deny },
                  { code: 'delete', actionCode: 'delete', effect: AccessEffect.NotGranted },
                ],
              },
            ],
          },
        ],
      },
    ],
    featureFlags: [
      { flagCode: 'new-ui', isEnabled: true },
      { flagCode: 'beta-feature', isEnabled: false },
    ],
  };

  beforeEach(() => {
    vi.mocked(useAuthStore).mockImplementation((selector: any) =>
      selector({
        user: { authorizationGraph: mockGraph },
      })
    );
  });

  it('should return true for hasModuleAccess if module exists and is Active', () => {
    const { result } = renderHook(() => useAccessResolution());
    expect(result.current.hasModuleAccess('sys')).toBe(true);
  });

  it('should return false for hasModuleAccess if module does not exist', () => {
    const { result } = renderHook(() => useAccessResolution());
    expect(result.current.hasModuleAccess('nonexistent')).toBe(false);
  });

  it('should return true for hasMenuAccess if menu exists', () => {
    const { result } = renderHook(() => useAccessResolution());
    expect(result.current.hasMenuAccess('sys', 'profiles')).toBe(true);
  });

  it('should correctly evaluate option access based on precedence (Allow)', () => {
    const { result } = renderHook(() => useAccessResolution());
    expect(result.current.hasOptionAccess('profiles', 'view')).toBe(true);
    expect(result.current.hasOptionAccess('profiles-list', 'view')).toBe(true);
  });

  it('should correctly evaluate option access based on precedence (Deny)', () => {
    const { result } = renderHook(() => useAccessResolution());
    expect(result.current.hasOptionAccess('profiles', 'create')).toBe(false);
  });

  it('should correctly evaluate option access based on precedence (NotGranted)', () => {
    const { result } = renderHook(() => useAccessResolution());
    expect(result.current.hasOptionAccess('profiles', 'delete')).toBe(false);
  });

  it('should correctly evaluate feature flags', () => {
    const { result } = renderHook(() => useAccessResolution());
    expect(result.current.hasFeatureFlag('new-ui')).toBe(true);
    expect(result.current.hasFeatureFlag('beta-feature')).toBe(false);
  });
});
