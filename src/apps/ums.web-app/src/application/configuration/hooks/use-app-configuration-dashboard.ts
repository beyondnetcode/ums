/**
 * useAppConfigurationDashboard — orchestrates state for the app configuration dashboard.
 */
import { useState, useCallback } from 'react';
import {
  useGetAllAppConfigurations,
  useGetAppConfigurationById,
} from '@app/configuration/hooks/use-app-configuration';
import type { AppConfiguration } from '@domain/configuration/schemas/app-configuration.schema';
import { useQueryState } from '@app/shared/hooks/use-query-state';
import { usePaginationState } from '@app/shared/hooks/use-pagination-state';

export function useAppConfigurationDashboard() {
  const [selectedId, setSelectedId]         = useState('');
  const [isCreateOpen, setIsCreateOpen]     = useState(false);
  const [viewMode, setViewMode]             = useState<'list' | 'thumbnail'>('list');
  
  const queryState = useQueryState({
    criteria: 'code',
    filter: 'all',
    sortBy: 'code',
  });

  const paginationState = usePaginationState({
    initialPageSize: 10,
  });

  const shouldFetch = queryState.appliedQuery.filterApplied;

  const { data: pageData, isLoading: isLoadingList, error: listError } =
    useGetAllAppConfigurations(
      shouldFetch
        ? {
            page: paginationState.page,
            pageSize: paginationState.pageSize,
            search: queryState.appliedQuery.term || undefined,
            status: queryState.activeFilter !== 'all' ? queryState.activeFilter : undefined,
            sortBy: queryState.sortBy,
            sortOrder: queryState.sortOrder,
            scope: queryState.activeFilter !== 'all' ? queryState.activeFilter : undefined,
          }
        : { page: 1, pageSize: 10 },
    );

  const { data: activeConfig, isLoading: isLoadingDetail } =
    useGetAppConfigurationById(selectedId || null);

  const knownConfigs: AppConfiguration[] = pageData?.items ?? [];
  const totalItems  = pageData?.totalItems ?? 0;
  const totalPages  = pageData?.totalPages ?? 0;

  const handleSelect = useCallback((id: string) => {
    setSelectedId(id);
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
    queryState,
    paginationState,

    knownConfigs,
    isLoadingList,
    listError: listError as Error | null,
    activeConfig: activeConfig ?? undefined,
    isLoadingDetail,
    totalItems,
    totalPages,

    handleSelect,
    handleCreateSuccess,
    requiresFilter: !shouldFetch,
  };
}