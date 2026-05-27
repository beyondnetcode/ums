/**
 * usePermissionTemplateDashboard — orchestrates state for the template dashboard.
 */
import { useState, useCallback } from 'react';
import { useGetAllPermissionTemplates } from './use-permission-template';
import { useGetPermissionTemplate } from './use-permission-template';
import type { PermissionTemplate } from '@domain/authorization/models/permission-template.model';

const PAGE_SIZE = 20;

export function usePermissionTemplateDashboard(tenantId?: string) {
  const [selectedId, setSelectedId]         = useState('');
  const [isCreateOpen, setIsCreateOpen]     = useState(false);
  const [viewMode, setViewMode]             = useState<'list' | 'thumbnail'>('list');
  const [searchValue, setSearchValue]       = useState('');
  const [appliedSearch, setAppliedSearch]   = useState('');
  const [activeFilter, setActiveFilter]     = useState('all');
  const [sortBy, setSortBy]                 = useState('version');
  const [sortOrder, setSortOrder]           = useState<'asc' | 'desc'>('asc');
  const [page, setPage]                     = useState(1);

  const { data: pageData, isLoading: isLoadingList, error: listError } =
    useGetAllPermissionTemplates({
      page,
      pageSize: PAGE_SIZE,
      search: appliedSearch || undefined,
      status: activeFilter !== 'all' ? activeFilter : undefined,
      sortBy,
      sortOrder,
      tenantId,
    });

  const { data: activeTemplate, isLoading: isLoadingDetail } =
    useGetPermissionTemplate(selectedId || null);

  const knownTemplates: PermissionTemplate[] = pageData?.items ?? [];
  const totalItems  = pageData?.totalItems ?? 0;
  const totalPages  = pageData?.totalPages ?? 0;

  const handleSelect = useCallback((id: string) => {
    setSelectedId(id);
  }, []);

  const handleQuerySubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    setAppliedSearch(searchValue);
    setPage(1);
  }, [searchValue]);

  const handleResetQuery = useCallback(() => {
    setSearchValue('');
    setAppliedSearch('');
    setPage(1);
  }, []);

  const handleCreateSuccess = useCallback(() => {
    setIsCreateOpen(false);
  }, []);

  return {
    selectedId,
    setSelectedId,
    isCreateOpen,
    setIsCreateOpen,
    viewMode,
    setViewMode,
    searchValue,
    setSearchValue,
    appliedSearch,
    activeFilter,
    setActiveFilter,
    sortBy,
    setSortBy,
    sortOrder,
    setSortOrder,
    page,
    setPage,

    knownTemplates,
    isLoadingList,
    listError: listError as Error | null,
    activeTemplate: activeTemplate ?? undefined,
    isLoadingDetail,
    totalItems,
    totalPages,

    handleSelect,
    handleQuerySubmit,
    handleResetQuery,
    handleCreateSuccess,
  };
}
