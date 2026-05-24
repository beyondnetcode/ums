import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import {
  useGetAllUserAccounts,
  useCreateUserAccount,
  useActivateUserAccount,
  useBlockUserAccount,
  useRestoreUserAccount,
} from './use-user-account';
import userAccountService from '@infra/identity/services/user-account.service';

vi.mock('@infra/identity/services/user-account.service', () => ({
  userAccountService: {
    getAllUserAccounts: vi.fn(),
    createUserAccount: vi.fn(),
    activateUserAccount: vi.fn(),
    blockUserAccount: vi.fn(),
    restoreUserAccount: vi.fn(),
  },
  default: {
    getAllUserAccounts: vi.fn(),
    createUserAccount: vi.fn(),
    activateUserAccount: vi.fn(),
    blockUserAccount: vi.fn(),
    restoreUserAccount: vi.fn(),
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

describe('UserAccount hooks', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('useGetAllUserAccounts returns paged list', async () => {
    const mockPage = {
      items: [
        {
          userAccountId: '12345678-1234-1234-1234-1234567890ab',
          tenantId: '87654321-4321-4321-4321-0987654321fe',
          email: 'admin@ransa.pe',
          category: 'Internal',
          status: 'Active',
        },
      ],
      page: 1,
      pageSize: 20,
      totalItems: 1,
      totalPages: 1,
    };

    vi.mocked(userAccountService.getAllUserAccounts).mockResolvedValue(mockPage);

    const wrapper = createWrapper();
    const { result } = renderHook(
      () =>
        useGetAllUserAccounts({
          page: 1,
          pageSize: 20,
          tenantId: '87654321-4321-4321-4321-0987654321fe',
        }),
      { wrapper }
    );

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data?.items[0].email).toBe('admin@ransa.pe');
    expect(userAccountService.getAllUserAccounts).toHaveBeenCalledWith({
      page: 1,
      pageSize: 20,
      tenantId: '87654321-4321-4321-4321-0987654321fe',
    });
  });

  it('useCreateUserAccount calls service successfully', async () => {
    const payload = {
      tenantId: '87654321-4321-4321-4321-0987654321fe',
      email: 'new@ransa.pe',
      category: 'Internal' as const,
    };

    vi.mocked(userAccountService.createUserAccount).mockResolvedValue({
      userAccountId: 'abc-def-guid',
    });

    const wrapper = createWrapper();
    const { result } = renderHook(() => useCreateUserAccount(), { wrapper });

    await act(async () => {
      result.current.mutate(payload);
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userAccountService.createUserAccount).toHaveBeenCalledWith(payload);
  });

  it('useActivateUserAccount activates an account', async () => {
    vi.mocked(userAccountService.activateUserAccount).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useActivateUserAccount('user-id'), { wrapper });

    await act(async () => {
      result.current.mutate();
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userAccountService.activateUserAccount).toHaveBeenCalledWith('user-id');
  });

  it('useBlockUserAccount blocks an account with reason', async () => {
    vi.mocked(userAccountService.blockUserAccount).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useBlockUserAccount('user-id'), { wrapper });

    await act(async () => {
      result.current.mutate('Security violation');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userAccountService.blockUserAccount).toHaveBeenCalledWith('user-id', 'Security violation');
  });

  it('useRestoreUserAccount restores a blocked account', async () => {
    vi.mocked(userAccountService.restoreUserAccount).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRestoreUserAccount('user-id'), { wrapper });

    await act(async () => {
      result.current.mutate();
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userAccountService.restoreUserAccount).toHaveBeenCalledWith('user-id');
  });
});
