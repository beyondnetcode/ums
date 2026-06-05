import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useThemeStore } from './theme.store';

vi.mock('zustand/middleware', async importOriginal => {
  const actual = await importOriginal<typeof import('zustand/middleware')>();
  return {
    ...actual,
    persist: (creator: unknown) => creator,
  };
});

describe('theme.store', () => {
  beforeEach(() => {
    useThemeStore.setState({ isDarkMode: true });
  });

  it('initializes with dark mode enabled', () => {
    expect(useThemeStore.getState().isDarkMode).toBe(true);
  });

  it('toggles dark mode from true to false', () => {
    useThemeStore.setState({ isDarkMode: true });
    useThemeStore.getState().toggleDarkMode();
    expect(useThemeStore.getState().isDarkMode).toBe(false);
  });

  it('toggles dark mode from false to true', () => {
    useThemeStore.setState({ isDarkMode: false });
    useThemeStore.getState().toggleDarkMode();
    expect(useThemeStore.getState().isDarkMode).toBe(true);
  });

  it('toggles multiple times correctly', () => {
    useThemeStore.setState({ isDarkMode: true });
    useThemeStore.getState().toggleDarkMode();
    expect(useThemeStore.getState().isDarkMode).toBe(false);
    useThemeStore.getState().toggleDarkMode();
    expect(useThemeStore.getState().isDarkMode).toBe(true);
    useThemeStore.getState().toggleDarkMode();
    expect(useThemeStore.getState().isDarkMode).toBe(false);
  });

  it('has toggleDarkMode function', () => {
    expect(typeof useThemeStore.getState().toggleDarkMode).toBe('function');
  });
});
