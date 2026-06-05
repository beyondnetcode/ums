import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import {
  useGetAllSystemSuites,
  useGetSystemSuite,
  useCreateSystemSuite,
  useSetSystemSuiteStatus,
  useAddModule,
  useRemoveModule,
  useActivateModule,
  useDeactivateModule,
  useAddMenu,
  useRemoveMenu,
  useAddSubMenu,
  useRemoveSubMenu,
  useAddOption,
  useRemoveOption,
  useRegisterAction,
  useRemoveAction,
  useAddDomainResource,
  useRemoveDomainResource,
} from './use-system-suite';
import systemSuiteService from '@infra/authorization/services/system-suite.service';

vi.mock('@infra/authorization/services/system-suite.service', () => ({
  systemSuiteService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    createSystemSuite: vi.fn(),
    setSystemSuiteStatus: vi.fn(),
    addModule: vi.fn(),
    removeModule: vi.fn(),
    activateModule: vi.fn(),
    deactivateModule: vi.fn(),
    addMenu: vi.fn(),
    removeMenu: vi.fn(),
    addSubMenu: vi.fn(),
    removeSubMenu: vi.fn(),
    addOption: vi.fn(),
    removeOption: vi.fn(),
    registerAction: vi.fn(),
    removeAction: vi.fn(),
    addDomainResource: vi.fn(),
    removeDomainResource: vi.fn(),
  },
  default: {
    getAll: vi.fn(),
    getById: vi.fn(),
    createSystemSuite: vi.fn(),
    setSystemSuiteStatus: vi.fn(),
    addModule: vi.fn(),
    removeModule: vi.fn(),
    activateModule: vi.fn(),
    deactivateModule: vi.fn(),
    addMenu: vi.fn(),
    removeMenu: vi.fn(),
    addSubMenu: vi.fn(),
    removeSubMenu: vi.fn(),
    addOption: vi.fn(),
    removeOption: vi.fn(),
    registerAction: vi.fn(),
    removeAction: vi.fn(),
    addDomainResource: vi.fn(),
    removeDomainResource: vi.fn(),
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

describe('use-system-suite hooks', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('useGetAllSystemSuites returns paged list', async () => {
    const mockPage = {
      items: [
        {
          systemSuiteId: 's1',
          code: 'S1',
          name: 'Suite 1',
          status: 'Active',
          createdAt: '2024-01-01',
        },
      ],
      page: 1,
      pageSize: 20,
      totalItems: 1,
      totalPages: 1,
    };

    vi.mocked(systemSuiteService.getAll).mockResolvedValue(mockPage);

    const wrapper = createWrapper();
    const { result } = renderHook(() => useGetAllSystemSuites({ page: 1, pageSize: 20 }), {
      wrapper,
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data?.items[0].name).toBe('Suite 1');
    expect(systemSuiteService.getAll).toHaveBeenCalledWith({ page: 1, pageSize: 20 });
  });

  it('useGetSystemSuite returns suite by id', async () => {
    const mockSuite = {
      systemSuiteId: 's1',
      code: 'S1',
      name: 'Suite 1',
      status: 'Active',
      createdAt: '2024-01-01',
    };

    vi.mocked(systemSuiteService.getById).mockResolvedValue(mockSuite);

    const wrapper = createWrapper();
    const { result } = renderHook(() => useGetSystemSuite('s1'), { wrapper });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data?.name).toBe('Suite 1');
    expect(systemSuiteService.getById).toHaveBeenCalledWith('s1');
  });

  it('useGetSystemSuite returns null on 404', async () => {
    const error = new Error('Not Found');
    (error as any).response = { status: 404 };
    vi.mocked(systemSuiteService.getById).mockRejectedValue(error);

    const wrapper = createWrapper();
    const { result } = renderHook(() => useGetSystemSuite('nonexistent'), { wrapper });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toBeNull();
  });

  it('useCreateSystemSuite calls service successfully', async () => {
    vi.mocked(systemSuiteService.createSystemSuite).mockResolvedValue({
      systemSuiteId: 'new-suite-id',
    });

    const wrapper = createWrapper();
    const { result } = renderHook(() => useCreateSystemSuite(), { wrapper });

    await act(async () => {
      result.current.mutate({ code: 'NEW', name: 'New Suite' });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.createSystemSuite).toHaveBeenCalledWith({
      code: 'NEW',
      name: 'New Suite',
    });
  });

  it('useSetSystemSuiteStatus calls service successfully', async () => {
    vi.mocked(systemSuiteService.setSystemSuiteStatus).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useSetSystemSuiteStatus('s1'), { wrapper });

    await act(async () => {
      result.current.mutate('Active');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.setSystemSuiteStatus).toHaveBeenCalledWith('s1', 'Active');
  });

  it('useAddModule calls service successfully', async () => {
    vi.mocked(systemSuiteService.addModule).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useAddModule('s1'), { wrapper });

    await act(async () => {
      result.current.mutate({ code: 'MOD1', name: 'Module 1', sortOrder: 1 });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.addModule).toHaveBeenCalledWith('s1', expect.any(Object));
  });

  it('useRemoveModule calls service successfully', async () => {
    vi.mocked(systemSuiteService.removeModule).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRemoveModule('s1'), { wrapper });

    await act(async () => {
      result.current.mutate('mod1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.removeModule).toHaveBeenCalledWith('s1', 'mod1');
  });

  it('useActivateModule calls service successfully', async () => {
    vi.mocked(systemSuiteService.activateModule).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useActivateModule('s1'), { wrapper });

    await act(async () => {
      result.current.mutate('mod1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.activateModule).toHaveBeenCalledWith('s1', 'mod1');
  });

  it('useDeactivateModule calls service successfully', async () => {
    vi.mocked(systemSuiteService.deactivateModule).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useDeactivateModule('s1'), { wrapper });

    await act(async () => {
      result.current.mutate('mod1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.deactivateModule).toHaveBeenCalledWith('s1', 'mod1');
  });

  it('useAddMenu calls service successfully', async () => {
    vi.mocked(systemSuiteService.addMenu).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useAddMenu('s1', 'mod1'), { wrapper });

    await act(async () => {
      result.current.mutate({ code: 'MENU1', label: 'Menu 1', sortOrder: 1 });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.addMenu).toHaveBeenCalledWith('s1', 'mod1', expect.any(Object));
  });

  it('useRemoveMenu calls service successfully', async () => {
    vi.mocked(systemSuiteService.removeMenu).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRemoveMenu('s1', 'mod1'), { wrapper });

    await act(async () => {
      result.current.mutate('menu1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.removeMenu).toHaveBeenCalledWith('s1', 'mod1', 'menu1');
  });

  it('useAddSubMenu calls service successfully', async () => {
    vi.mocked(systemSuiteService.addSubMenu).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useAddSubMenu('s1', 'mod1', 'menu1'), { wrapper });

    await act(async () => {
      result.current.mutate({ code: 'SUB1', label: 'Sub 1', sortOrder: 1 });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.addSubMenu).toHaveBeenCalledWith(
      's1',
      'mod1',
      'menu1',
      expect.any(Object)
    );
  });

  it('useRemoveSubMenu calls service successfully', async () => {
    vi.mocked(systemSuiteService.removeSubMenu).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRemoveSubMenu('s1', 'mod1', 'menu1'), { wrapper });

    await act(async () => {
      result.current.mutate('sub1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.removeSubMenu).toHaveBeenCalledWith('s1', 'mod1', 'menu1', 'sub1');
  });

  it('useAddOption calls service successfully', async () => {
    vi.mocked(systemSuiteService.addOption).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useAddOption('s1', 'mod1', 'menu1', 'sub1'), { wrapper });

    await act(async () => {
      result.current.mutate({ code: 'OPT1', label: 'Opt 1', actionCode: 'ACT1', sortOrder: 1 });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.addOption).toHaveBeenCalledWith(
      's1',
      'mod1',
      'menu1',
      'sub1',
      expect.any(Object)
    );
  });

  it('useRemoveOption calls service successfully', async () => {
    vi.mocked(systemSuiteService.removeOption).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRemoveOption('s1', 'mod1', 'menu1', 'sub1'), {
      wrapper,
    });

    await act(async () => {
      result.current.mutate('opt1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.removeOption).toHaveBeenCalledWith(
      's1',
      'mod1',
      'menu1',
      'sub1',
      'opt1'
    );
  });

  it('useRegisterAction calls service successfully', async () => {
    vi.mocked(systemSuiteService.registerAction).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRegisterAction('s1'), { wrapper });

    await act(async () => {
      result.current.mutate({ code: 'ACT1', name: 'Action 1' });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.registerAction).toHaveBeenCalledWith('s1', {
      code: 'ACT1',
      name: 'Action 1',
    });
  });

  it('useRemoveAction calls service successfully', async () => {
    vi.mocked(systemSuiteService.removeAction).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRemoveAction('s1'), { wrapper });

    await act(async () => {
      result.current.mutate('ACT1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.removeAction).toHaveBeenCalledWith('s1', 'ACT1');
  });

  it('useAddDomainResource calls service successfully', async () => {
    vi.mocked(systemSuiteService.addDomainResource).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useAddDomainResource('s1'), { wrapper });

    await act(async () => {
      result.current.mutate({
        type: 'Aggregate',
        code: 'DR1',
        name: 'Resource 1',
        description: 'Test',
      });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.addDomainResource).toHaveBeenCalledWith('s1', expect.any(Object));
  });

  it('useRemoveDomainResource calls service successfully', async () => {
    vi.mocked(systemSuiteService.removeDomainResource).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRemoveDomainResource('s1'), { wrapper });

    await act(async () => {
      result.current.mutate('dr1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(systemSuiteService.removeDomainResource).toHaveBeenCalledWith('s1', 'dr1');
  });
});
