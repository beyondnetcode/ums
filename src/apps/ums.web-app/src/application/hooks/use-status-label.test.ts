import { describe, expect, it, beforeEach } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useStatusLabel } from './use-status-label';
import { useI18nStore } from '@app/stores/i18n.store';

describe('useStatusLabel', () => {
  beforeEach(() => {
    useI18nStore.getState().setLanguage('en');
  });

  it('returns translated label for Active status', () => {
    const { result } = renderHook(() => useStatusLabel());
    const getStatusLabel = result.current;

    expect(getStatusLabel('Active')).toBe('Active');
  });

  it('returns translated label for Suspended status', () => {
    const { result } = renderHook(() => useStatusLabel());
    const getStatusLabel = result.current;

    expect(getStatusLabel('Suspended')).toBe('Suspended');
  });

  it('returns translated label for Pending status', () => {
    const { result } = renderHook(() => useStatusLabel());
    const getStatusLabel = result.current;

    expect(getStatusLabel('Pending')).toBe('Pending');
  });

  it('returns pending for unknown status', () => {
    const { result } = renderHook(() => useStatusLabel());
    const getStatusLabel = result.current;

    expect(getStatusLabel('Unknown')).toBe('Pending');
  });

  it('returns spanish labels when language is es', () => {
    useI18nStore.getState().setLanguage('es');

    const { result } = renderHook(() => useStatusLabel());
    const getStatusLabel = result.current;

    expect(getStatusLabel('Active')).toBe('Activo');
    expect(getStatusLabel('Suspended')).toBe('Suspendido');
    expect(getStatusLabel('Pending')).toBe('Pendiente');
  });
});
