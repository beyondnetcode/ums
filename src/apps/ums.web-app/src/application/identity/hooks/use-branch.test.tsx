import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import {
  useGetBranches,
  useAddBranch,
  useRemoveBranch,
  useDeactivateBranch,
  useReactivateBranch,
} from './use-branch';
import tenantService from '@infra/identity/services/tenant.service';

vi.mock('@infra/identity/services/tenant.service', () => ({
  tenantService: {
    getBranches: vi.fn(),
    addBranch: vi.fn(),
    removeBranch: vi.fn(),
    deactivateBranch: vi.fn(),
    reactivateBranch: vi.fn(),
  },
  default: {
    getBranches: vi.fn(),
    addBranch: vi.fn(),
    removeBranch: vi.fn(),
    deactivateBranch: vi.fn(),
    reactivateBranch: vi.fn(),
  },
}));

vi.mock('@app/i18n/use-i18n', () => ({
  useI18n: () => ({
    notifBranchAdded: 'Branch Added',
    notifBranchAddedMsg: (code: string) => `Branch ${code} added`,
    notifBranchAddFailed: 'Add Failed',
    notifBranchAddFailedMsg: 'Could not add branch',
    notifBranchRemoved: 'Branch Removed',
    notifBranchRemovedMsg: 'Branch removed successfully',
    notifBranchRemoveFailed: 'Remove Failed',
    notifBranchRemoveFailedMsg: 'Could not remove branch',
    notifBranchDeactivated: 'Branch Deactivated',
    notifBranchDeactivatedMsg: 'Branch deactivated',
    notifBranchDeactivateFailed: 'Deactivate Failed',
    notifBranchDeactivateFailedMsg: 'Could not deactivate branch',
    notifBranchReactivated: 'Branch Reactivated',
    notifBranchReactivatedMsg: 'Branch reactivated',
    notifBranchReactivateFailed: 'Reactivate Failed',
    notifBranchReactivateFailedMsg: 'Could not reactivate branch',
  }),
}));

vi.mock('@app/hooks/use-notified-mutation', () => ({
  useNotifiedMutation: (config: any) => {
    const mutationFn = config.mutationFn;
    return {
      mutate: vi.fn((vars: any) => mutationFn(vars)),
      mutateAsync: vi.fn(async (vars: any) => mutationFn(vars)),
      isPending: false,
      isSuccess: true,
      isError: false,
      data: { branchId: 'new-branch' },
    };
  },
}));

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

describe('use-branch hooks', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('useGetBranches returns branches for tenant', async () => {
    const mockBranches = [
      { branchId: 'b1', code: 'B1', name: 'Branch 1', isActive: true },
    ];
    vi.mocked(tenantService.getBranches).mockResolvedValue(mockBranches);

    const wrapper = createWrapper();
    const { result } = renderHook(() => useGetBranches('t1'), { wrapper });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data?.[0].name).toBe('Branch 1');
    expect(tenantService.getBranches).toHaveBeenCalledWith('t1');
  });

  it('useGetBranches returns empty array on 404', async () => {
    const error = new Error('Not Found');
    (error as any).response = { status: 404 };
    vi.mocked(tenantService.getBranches).mockRejectedValue(error);

    const wrapper = createWrapper();
    const { result } = renderHook(() => useGetBranches('t1'), { wrapper });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toEqual([]);
  });

  it('useGetBranches is disabled when tenantId is null', () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useGetBranches(null), { wrapper });

    expect(result.current.isFetching).toBe(false);
    expect(result.current.data).toBeUndefined();
  });

  it('useAddBranch returns mutation object', () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useAddBranch('t1'), { wrapper });

    expect(result.current.mutate).toBeDefined();
    expect(typeof result.current.mutate).toBe('function');
  });

  it('useRemoveBranch returns mutation object', () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useRemoveBranch('t1'), { wrapper });

    expect(result.current.mutate).toBeDefined();
    expect(typeof result.current.mutate).toBe('function');
  });

  it('useDeactivateBranch returns mutation object', () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useDeactivateBranch('t1'), { wrapper });

    expect(result.current.mutate).toBeDefined();
    expect(typeof result.current.mutate).toBe('function');
  });

  it('useReactivateBranch returns mutation object', () => {
    const wrapper = createWrapper();
    const { result } = renderHook(() => useReactivateBranch('t1'), { wrapper });

    expect(result.current.mutate).toBeDefined();
    expect(typeof result.current.mutate).toBe('function');
  });
});
