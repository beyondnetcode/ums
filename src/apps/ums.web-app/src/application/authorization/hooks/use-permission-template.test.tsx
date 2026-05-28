import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import {
  useGetAllPermissionTemplates,
  useGetPermissionTemplate,
  useCreatePermissionTemplate,
  usePublishPermissionTemplate,
  useDeprecatePermissionTemplate,
  useAddTemplateItem,
  useRemoveTemplateItem,
} from './use-permission-template';
import permissionTemplateService from '@infra/authorization/services/permission-template.service';

vi.mock('@infra/authorization/services/permission-template.service', () => ({
  permissionTemplateService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    publish: vi.fn(),
    deprecate: vi.fn(),
    addItem: vi.fn(),
    removeItem: vi.fn(),
  },
  default: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    publish: vi.fn(),
    deprecate: vi.fn(),
    addItem: vi.fn(),
    removeItem: vi.fn(),
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

describe('use-permission-template hooks', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('useGetAllPermissionTemplates returns paged list', async () => {
    const mockPage = {
      items: [
        { templateId: 't1', code: 'T1', name: 'Template 1', status: 'Active', createdAt: '2024-01-01' },
      ],
      page: 1,
      pageSize: 20,
      totalItems: 1,
      totalPages: 1,
    };

    vi.mocked(permissionTemplateService.getAll).mockResolvedValue(mockPage);

    const wrapper = createWrapper();
    const { result } = renderHook(
      () => useGetAllPermissionTemplates({ page: 1, pageSize: 20 }),
      { wrapper }
    );

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data?.items[0].name).toBe('Template 1');
    expect(permissionTemplateService.getAll).toHaveBeenCalledWith({ page: 1, pageSize: 20 });
  });

  it('useGetPermissionTemplate returns template by id', async () => {
    const mockTemplate = {
      templateId: 't1',
      code: 'T1',
      name: 'Template 1',
      status: 'Active',
      createdAt: '2024-01-01',
      items: [],
    };

    vi.mocked(permissionTemplateService.getById).mockResolvedValue(mockTemplate);

    const wrapper = createWrapper();
    const { result } = renderHook(() => useGetPermissionTemplate('t1'), { wrapper });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data?.name).toBe('Template 1');
    expect(permissionTemplateService.getById).toHaveBeenCalledWith('t1');
  });

  it('useGetPermissionTemplate returns null on 404', async () => {
    const error = new Error('Not Found');
    (error as any).response = { status: 404 };
    vi.mocked(permissionTemplateService.getById).mockRejectedValue(error);

    const wrapper = createWrapper();
    const { result } = renderHook(() => useGetPermissionTemplate('nonexistent'), { wrapper });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toBeNull();
  });

  it('useCreatePermissionTemplate calls service successfully', async () => {
    vi.mocked(permissionTemplateService.create).mockResolvedValue({ templateId: 'new-template-id' });

    const wrapper = createWrapper();
    const { result } = renderHook(() => useCreatePermissionTemplate(), { wrapper });

    await act(async () => {
      result.current.mutate({ code: 'NEW', name: 'New Template' });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(permissionTemplateService.create).toHaveBeenCalledWith({ code: 'NEW', name: 'New Template' });
  });

  it('usePublishPermissionTemplate calls service successfully', async () => {
    vi.mocked(permissionTemplateService.publish).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => usePublishPermissionTemplate('t1'), { wrapper });

    await act(async () => {
      result.current.mutate();
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(permissionTemplateService.publish).toHaveBeenCalledWith('t1');
  });

  it('useDeprecatePermissionTemplate calls service successfully', async () => {
    vi.mocked(permissionTemplateService.deprecate).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useDeprecatePermissionTemplate('t1'), { wrapper });

    await act(async () => {
      result.current.mutate();
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(permissionTemplateService.deprecate).toHaveBeenCalledWith('t1');
  });

  it('useAddTemplateItem calls service successfully', async () => {
    vi.mocked(permissionTemplateService.addItem).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useAddTemplateItem('t1'), { wrapper });

    await act(async () => {
      result.current.mutate({ code: 'ITEM1', effect: 'Allow', sortOrder: 1 });
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(permissionTemplateService.addItem).toHaveBeenCalledWith('t1', expect.any(Object));
  });

  it('useRemoveTemplateItem calls service successfully', async () => {
    vi.mocked(permissionTemplateService.removeItem).mockResolvedValue();

    const wrapper = createWrapper();
    const { result } = renderHook(() => useRemoveTemplateItem('t1'), { wrapper });

    await act(async () => {
      result.current.mutate('item1');
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(permissionTemplateService.removeItem).toHaveBeenCalledWith('t1', 'item1');
  });
});
