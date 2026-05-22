import React, { useCallback } from 'react';
import { Mail, ArrowRight } from 'lucide-react';
import { UserAccount } from '@domain/identity/models/user-account.model';
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

interface UserAccountListPanelProps {
  accounts: UserAccount[];
  selectedId: string;
  isLoading: boolean;
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
  onSelectAccount: (accountId: string) => void;
  criteriaOptions: QueryCriteriaOption[];
  filterOptions: FilterOption[];
  sortOptions: SortOption[];
}

export const UserAccountListPanel: React.FC<UserAccountListPanelProps> = ({
  accounts,
  selectedId,
  isLoading,
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
  onSelectAccount,
  criteriaOptions,
  filterOptions,
  sortOptions,
}) => {
  const t = useI18n();
  const getStatusLabel = useStatusLabel();

  const renderAccountRow = useCallback((account: UserAccount) => {
    const isSelected = account.userAccountId === selectedId;
    return (
      <EntityRow
        key={account.userAccountId}
        selected={isSelected}
        onClick={() => onSelectAccount(account.userAccountId)}
        leading={
          <div className="flex items-center gap-3">
            <div className={`p-2 rounded-lg ${isSelected ? 'bg-m3-primary/15' : 'bg-m3-surface-container/50'}`}>
              <Mail className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`} />
            </div>
            <div>
              <span className="text-sm font-semibold text-m3-on-surface">{account.email}</span>
              <span className="ml-2 text-[10px] text-m3-secondary/70 font-mono">
                {account.userAccountId.substring(0, 8)}...
              </span>
            </div>
          </div>
        }
        trailing={
          <div className="flex items-center gap-2">
            <CodeBadge label={account.category} />
            <StatusBadge status={account.status} label={getStatusLabel(account.status)} />
            <ArrowRight className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-outline'}`} />
          </div>
        }
      />
    );
  }, [selectedId, onSelectAccount, getStatusLabel]);

  const renderAccountCard = useCallback((account: UserAccount) => {
    const isSelected = account.userAccountId === selectedId;
    return (
      <EntityCard
        key={account.userAccountId}
        selected={isSelected}
        onClick={() => onSelectAccount(account.userAccountId)}
        icon={<Mail className="w-5 h-5" />}
        title={account.email}
        subtitle={account.userAccountId.substring(0, 8)}
        badges={
          <>
            <CodeBadge label={account.category} />
            <StatusBadge status={account.status} label={getStatusLabel(account.status)} />
          </>
        }
      />
    );
  }, [selectedId, onSelectAccount, getStatusLabel]);

  const pagination = totalItems > 0 ? {
    page,
    pageSize,
    totalItems,
    totalPages,
    onPageChange,
  } : undefined;

  return (
    <M3DataView
      title={t.userAccounts}
      subtitle={t.userAccountMaintenanceSubtitle}
      searchCriteria={criteriaOptions}
      activeCriteria={searchCriteria}
      onCriteriaChange={onSearchCriteriaChange}
      searchValue={searchValue}
      onSearchValueChange={onSearchValueChange}
      onSearchSubmit={onSearchSubmit}
      onRegisterNew={onRegisterNew}
      registerLabel={t.registerNew}
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
      isEmpty={!isLoading && accounts.length === 0}
      emptyLabel={appliedTerm ? t.noResultsFound : t.noAccountsFound}
      emptyTitle={appliedTerm ? t.noResultsTitle : t.noAccountsTitle}
      loadingLabel={t.loadingAccounts}
      criteriaLabel={t.criteria}
      searchTermLabel={t.searchTerm}
      searchButtonLabel={t.searchBtn}
      renderList={() => (
        <div className="divide-y divide-m3-outline/10">
          {accounts.map(renderAccountRow)}
        </div>
      )}
      renderThumbnail={() => (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {accounts.map(renderAccountCard)}
        </div>
      )}
      pagination={pagination}
      telemetryInfo={
        <div className="flex items-center gap-1.5">
          <span className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse flex-shrink-0" />
          <span className="text-xs text-m3-secondary/70 truncate">
            {totalItems} {t.records} · {pageSize} {t.perPage}
          </span>
        </div>
      }
    />
  );
};
