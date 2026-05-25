import React, { useCallback } from 'react';
import { Mail, Layers } from 'lucide-react';
import { UserAccount } from '@domain/identity/models/user-account.model';
import { Tenant } from '@domain/identity/models/tenant.model';
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
import { M3Card } from '@shared/components/M3Card';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';
import { TenantSelector } from '@presentation/identity/tenant/components/TenantSelector';


interface UserAccountListPanelProps {
  accounts: UserAccount[];
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
  onSelectAccount: (accountId: string) => void;
  criteriaOptions: QueryCriteriaOption[];
  filterOptions: FilterOption[];
  sortOptions: SortOption[];
  tenants: Tenant[];
  selectedTenantId: string;
  onTenantChange: (tenantId: string) => void;
}



export const UserAccountListPanel: React.FC<UserAccountListPanelProps> = ({
  accounts,
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
  appliedTerm,
  onPageChange,
  onSelectAccount,
  criteriaOptions,
  filterOptions,
  sortOptions,
  tenants,
  selectedTenantId,
  onTenantChange,
}) => {
  const t = useI18n();
  const getStatusLabel = useStatusLabel();

  const renderAccountRow = useCallback((account: UserAccount) => {
    const isSelected = account.userAccountId === selectedId;
    return (
      <div key={account.userAccountId} onClick={() => onSelectAccount(account.userAccountId)}>
        <EntityRow
          id={account.userAccountId}
          isActive={account.status === 'Active'}
          className={`${isSelected ? 'border-m3-primary bg-m3-primary/5' : ''} cursor-pointer`}
          actions={
            <div className="flex items-center gap-2">
              <CodeBadge code={account.category} />
              <StatusBadge status={account.status} label={getStatusLabel(account.status)} />
              <ArrowRight className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-outline'}`} />
            </div>
          }
        >
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
        </EntityRow>
      </div>
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
            <CodeBadge code={account.category} />
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
    <div className="flex flex-col h-full gap-4">
      <M3Card
        variant="elevated"
        className="flex-shrink-0 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm p-4"
      >
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            <div className="p-1.5 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10 flex-shrink-0">
              <Layers className="w-4 h-4" />
            </div>
            <div>
              <span className="text-xs font-semibold text-m3-secondary uppercase tracking-wider block">
                {t.activeTenant}
              </span>
              <span className="text-[10px] text-m3-secondary/70">
                {t.filterAccountsByTenant}
              </span>
            </div>
          </div>
          <div className="w-full sm:w-72">
            <TenantSelector
              tenants={tenants}
              selectedTenantId={selectedTenantId}
              onTenantChange={onTenantChange}
              label={t.activeTenant}
              className="mb-0"
            />
          </div>
        </div>
      </M3Card>

      <div className="flex-1 min-h-0">
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
            <>
              {error && <ErrorBanner error={error} />}
              <div className="divide-y divide-m3-outline/10">
                {accounts.map(renderAccountRow)}
              </div>
            </>
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
      </div>
    </div>
  );
};
