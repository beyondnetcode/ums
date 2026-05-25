import React, { useCallback } from 'react';
import { Box, ArrowRight } from 'lucide-react';
import { SystemSuite } from '@domain/authorization/models/system-suite.model';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';
import {
  M3DataView,
  SortOption,
  FilterOption,
  QueryCriteriaOption,
} from '@shared/components/M3DataView';
import { EntityRow } from '@shared/components/EntityRow';
import { EntityCard } from '@shared/components/EntityCard';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';
import { ApiErrorBanner } from '@shared/components/ApiErrorBanner';

interface SystemSuiteListPanelProps {
  systemSuites: SystemSuite[];
  selectedId: string;
  isLoading: boolean;
  error: Error | null;
  viewMode: 'list' | 'thumbnail';
  onViewModeChange: (mode: 'list' | 'thumbnail') => void;
  searchCriteria: string;
  onSearchCriteriaChange: (criteria: string) => void;
  searchValue: string;
  onSearchValueChange: (value: string) => void;
  onSearchSubmit: (event: React.FormEvent) => void;
  onRegisterNew: () => void;
  sortBy: string;
  onSortByChange: (value: string) => void;
  sortOrder: 'asc' | 'desc';
  onSortOrderToggle: () => void;
  activeFilter: string;
  onFilterChange: (value: string) => void;
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  startIndex: number;
  appliedTerm: string;
  onPageChange: (page: number) => void;
  onResetQuery: () => void;
  onSelectSystemSuite: (systemSuiteId: string) => void;
  criteriaOptions: QueryCriteriaOption[];
  filterOptions: FilterOption[];
  sortOptions: SortOption[];
}

export const SystemSuiteListPanel: React.FC<SystemSuiteListPanelProps> = ({
  systemSuites,
  selectedId,
  isLoading,
  error,
  viewMode,
  onViewModeChange,
  searchCriteria,
  onSearchCriteriaChange,
  searchValue,
  onSearchValueChange,
  onSearchSubmit,
  onRegisterNew,
  sortBy,
  onSortByChange,
  sortOrder,
  onSortOrderToggle,
  activeFilter,
  onFilterChange,
  page,
  pageSize,
  totalItems,
  totalPages,
  startIndex,
  appliedTerm,
  onPageChange,
  onResetQuery,
  onSelectSystemSuite,
  criteriaOptions,
  filterOptions,
  sortOptions,
}) => {
  const t = useI18n();
  const getStatusLabel = useStatusLabel();

  const renderSystemSuiteRow = useCallback((systemSuite: SystemSuite) => {
    const isSelected = systemSuite.systemSuiteId === selectedId;
    return (
      <div key={systemSuite.systemSuiteId} onClick={() => onSelectSystemSuite(systemSuite.systemSuiteId)}>
        <EntityRow
          id={systemSuite.systemSuiteId}
          isActive={systemSuite.status === 'Active'}
          className={`${isSelected ? 'border-m3-primary bg-m3-primary/5' : ''} cursor-pointer`}
          actions={
            <div className="flex items-center gap-2">
              <CodeBadge code={systemSuite.code} />
              <StatusBadge status={systemSuite.status} label={getStatusLabel(systemSuite.status)} />
              <ArrowRight className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-outline'}`} />
            </div>
          }
        >
          <div className="flex items-center gap-3">
            <div className={`p-2 rounded-lg ${isSelected ? 'bg-m3-primary/15' : 'bg-m3-surface-container/50'}`}>
              <Box className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`} />
            </div>
            <div>
              <span className="text-sm font-semibold text-m3-on-surface">{systemSuite.name}</span>
              <span className="ml-2 text-[10px] text-m3-secondary/70 font-mono">
                {systemSuite.systemSuiteId.substring(0, 8)}...
              </span>
            </div>
          </div>
        </EntityRow>
      </div>
    );
  }, [selectedId, onSelectSystemSuite, getStatusLabel]);

  const renderSystemSuiteCard = useCallback((systemSuite: SystemSuite) => {
    const isSelected = systemSuite.systemSuiteId === selectedId;
    return (
      <EntityCard
        key={systemSuite.systemSuiteId}
        selected={isSelected}
        onClick={() => onSelectSystemSuite(systemSuite.systemSuiteId)}
        icon={<Box className="w-5 h-5" />}
        title={systemSuite.name}
        subtitle={systemSuite.code}
        badges={
          <>
            <CodeBadge code={systemSuite.code} />
            <StatusBadge status={systemSuite.status} label={getStatusLabel(systemSuite.status)} />
          </>
        }
      />
    );
  }, [selectedId, onSelectSystemSuite, getStatusLabel]);

  const pagination = totalItems > 0 ? {
    page,
    pageSize,
    totalItems,
    totalPages,
    onPageChange,
  } : undefined;

  const footerTelemetry = (
    <div className="flex items-center gap-3">
      <div className="flex items-center gap-1.5">
        <span className="h-2 w-2 rounded-full bg-m3-primary animate-pulse" />
        <span className="text-xs font-medium text-m3-secondary/80">
          {t.showing ?? 'Showing'} {totalItems === 0 ? 0 : startIndex + 1}-{Math.min(startIndex + pageSize, totalItems)} {t.of ?? 'of'} {totalItems} {t.systemSuites ?? 'System Suites'}
        </span>
      </div>
      {appliedTerm.trim() && (
        <button onClick={onResetQuery} className="text-xs font-medium text-rose-500 hover:underline flex items-center gap-1">
          <span className="w-3 h-3">{t.clearFilter ?? 'Clear'}</span>
        </button>
      )}
    </div>
  );

  return (
    <M3DataView
      title={t.systemSuiteMaintenance}
      subtitle={t.systemSuiteMaintenanceSubtitle}
      searchPlaceholder={t.searchPlaceholder}
      searchCriteria={criteriaOptions}
      activeCriteria={searchCriteria}
      onCriteriaChange={onSearchCriteriaChange}
      searchValue={searchValue}
      onSearchValueChange={onSearchValueChange}
      onSearchSubmit={onSearchSubmit}
      onRegisterNew={onRegisterNew}
      registerLabel={t.newBtn}
      viewMode={viewMode}
      onViewModeChange={onViewModeChange}
      sortOptions={sortOptions}
      sortBy={sortBy}
      onSortByChange={onSortByChange}
      sortOrder={sortOrder}
      onSortOrderToggle={onSortOrderToggle}
      filterOptions={filterOptions}
      activeFilter={activeFilter}
      onFilterChange={onFilterChange}
      isLoading={isLoading}
      isEmpty={!isLoading && systemSuites.length === 0}
      emptyLabel={appliedTerm ? (t.noResultsFound ?? 'No results found') : (t.noSystemSuitesFound ?? 'No system suites found')}
      emptyTitle={appliedTerm ? (t.noResultsTitle ?? 'No results') : (t.noSystemSuitesTitle ?? 'No system suites')}
      loadingLabel={t.loadingProfile}
      criteriaLabel={t.criteria}
      searchTermLabel={t.searchTerm}
      searchButtonLabel={t.searchBtn}
      renderList={() => (
        <>
          {error && <ApiErrorBanner error={error} />}
          <div className="divide-y divide-m3-outline/10">
            {systemSuites.map(renderSystemSuiteRow)}
          </div>
        </>
      )}
      renderThumbnail={() => (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {systemSuites.map(renderSystemSuiteCard)}
        </div>
      )}
      pagination={pagination}
      telemetryInfo={footerTelemetry}
    />
  );
};
