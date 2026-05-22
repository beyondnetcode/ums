import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useNotifiedMutation } from './use-notified-mutation';

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

describe('useNotifiedMutation', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('calls mutationFn with provided variables', async () => {
    const mutationFn = vi.fn().mockResolvedValue({ id: '1' });
    const wrapper = createWrapper();

    const { result } = renderHook(
      () =>
        useNotifiedMutation({
          mutationFn,
          successNotif: () => ({ title: 'OK', message: 'Done' }),
          errorNotif: () => ({ title: 'Error', message: 'Failed' }),
        }),
      { wrapper }
    );

    await act(async () => {
      result.current.mutate({ name: 'test' });
    });

    await waitFor(() => {
      expect(mutationFn).toHaveBeenCalled();
    });

    const callArgs = mutationFn.mock.calls[0][0];
    expect(callArgs).toEqual({ name: 'test' });
  });

  it('shows success notification on success', async () => {
    const mutationFn = vi.fn().mockResolvedValue({ id: '1', name: 'created' });
    const wrapper = createWrapper();

    const { result } = renderHook(
      () =>
        useNotifiedMutation({
          mutationFn,
          successNotif: (data) => ({ title: 'Created', message: data.name }),
          errorNotif: () => ({ title: 'Error', message: 'Failed' }),
        }),
      { wrapper }
    );

    await act(async () => {
      result.current.mutate({});
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
  });

  it('shows error notification on failure', async () => {
    const mutationFn = vi.fn().mockRejectedValue(new Error('API error'));
    const wrapper = createWrapper();

    const { result } = renderHook(
      () =>
        useNotifiedMutation({
          mutationFn,
          successNotif: () => ({ title: 'OK', message: 'Done' }),
          errorNotif: (error) => ({
            title: 'Failed',
            message: error instanceof Error ? error.message : 'Unknown',
          }),
        }),
      { wrapper }
    );

    await act(async () => {
      result.current.mutate({});
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });

  it('returns mutation state properties', () => {
    const mutationFn = vi.fn().mockResolvedValue({});
    const wrapper = createWrapper();

    const { result } = renderHook(
      () =>
        useNotifiedMutation({
          mutationFn,
          successNotif: () => ({ title: 'OK', message: 'Done' }),
          errorNotif: () => ({ title: 'Error', message: 'Failed' }),
        }),
      { wrapper }
    );

    expect(result.current).toHaveProperty('mutate');
    expect(result.current).toHaveProperty('mutateAsync');
    expect(result.current).toHaveProperty('isPending');
    expect(result.current).toHaveProperty('isSuccess');
    expect(result.current).toHaveProperty('isError');
  });
});
