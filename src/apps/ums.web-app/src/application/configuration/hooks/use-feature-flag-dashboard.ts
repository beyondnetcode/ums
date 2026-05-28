/**
 * useFeatureFlagDashboard — orchestrates state for the feature flag dashboard.
 */
import { useState, useCallback } from 'react';
import {
  useGetAllFeatureFlags,
  useGetFeatureFlagById,
} from '@app/configuration/hooks/use-feature-flag';
import type { FeatureFlag } from '@domain/configuration/models/feature-flag.model';

const PAGE_SIZE = 20;

export function useFeatureFlagDashboard() {
  const [selectedId, setSelectedId]         = useState('');
  const [isCreateOpen, setIsCreateOpen]     = useState(false);
  const [viewMode, setViewMode]             = useState<'list' | 'thumbnail'>('list');
  const [searchValue, setSearchValue]       = useState('');
  const [appliedSearch, setAppliedSearch]   = useState('');
  const [activeFilter, setActiveFilter]     = useState('all');
  const [sortBy, setSortBy]                 = useState('flagCode');
  const [sortOrder, setSortOrder]           = useState<'asc' | 'desc'>('asc');
  const [page, setPage]                     = useState(1);

  const { data: pageData, isLoading: isLoadingList, error: listError } =
    useGetAllFeatureFlags({
      page,
      pageSize: PAGE_SIZE,
      search: appliedSearch || undefined,
      status: activeFilter !== 'all' ? activeFilter : undefined,
      sortBy,
      sortOrder,
    });

  const { data: activeFlag, isLoading: isLoadingDetail } =
    useGetFeatureFlagById(selectedId || null);

  const knownFlags: FeatureFlag[] = pageData?.items ?? [];
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

    knownFlags,
    isLoadingList,
    listError: listError as Error | null,
    activeFlag: activeFlag ?? undefined,
    isLoadingDetail,
    totalItems,
    totalPages,

    handleSelect,
    handleQuerySubmit,
    handleResetQuery,
    handleCreateSuccess,
  };
}
