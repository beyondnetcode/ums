import React from 'react';
import { useI18n } from '@app/i18n/use-i18n';
import { useUserAccountDashboard } from '@app/identity/hooks/use-user-account-dashboard';
import { UserAccountForm } from '../components/UserAccountForm';
import { UserAccountDetailPanel } from '../components/UserAccountDetailPanel';
import { UserAccountListPanel } from '../components/UserAccountListPanel';
import { PageShell } from '@shared/layouts/PageShell';
import { MasterDetailLayout } from '@shared/layouts/MasterDetailLayout';
import { M3Dialog } from '@shared/components/M3Dialog';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { M3TextField } from '@shared/components/M3TextField';
import { M3Button } from '@shared/components/M3Button';
import { SortOption, FilterOption, QueryCriteriaOption } from '@shared/components/M3DataView';
import { Lock } from 'lucide-react';

export default function UserAccountDashboardScreen(): React.JSX.Element {
  const t = useI18n();
  const dashboard = useUserAccountDashboard();

  const criteriaOptions: QueryCriteriaOption[] = [
    { label: t.byEmail, value: 'email' },
    { label: t.byUserId, value: 'id' },
  ];
  const filterOptions: FilterOption[] = [
    { label: t.allStatuses, value: 'all' },
    { label: t.active, value: 'Active' },
    { label: t.pending, value: 'Pending' },
    { label: t.blocked, value: 'Blocked' },
  ];
  const sortOptions: SortOption[] = [
    { label: t.sortByEmail, value: 'email' },
    { label: t.sortByStatus, value: 'status' },
    { label: t.sortByCategory, value: 'category' },
  ];

  return (
    <PageShell>
      <MasterDetailLayout
        splitterLabel="Resize user account detail panel"
        overlay={
          <>
            <M3FormDialog
              open={dashboard.showBlockDialog}
              onClose={() => dashboard.setShowBlockDialog(false)}
              title={t.blockUserTitle}
              icon={<Lock className="w-5 h-5 text-m3-error" />}
              footer={
                <>
                  <M3Button variant="text" onClick={() => dashboard.setShowBlockDialog(false)} type="button">
                    {t.cancelBtn}
                  </M3Button>
                  <M3Button
                    variant="filled"
                    onClick={dashboard.confirmBlock}
                    disabled={!dashboard.blockReason.trim()}
                    className="bg-m3-error hover:bg-m3-error/90 border-0"
                  >
                    {t.blockBtn}
                  </M3Button>
                </>
              }
            >
              <div className="space-y-3 pt-2">
                <p className="text-xs text-m3-secondary">
                  {t.blockUserMessage}
                </p>
                <M3TextField
                  label={t.blockReasonLabel || 'Reason for blocking'}
                  required
                  value={dashboard.blockReason}
                  onChange={(e) => dashboard.setBlockReason(e.target.value)}
                  placeholder="e.g. Security policy violation"
                  helperText={t.blockReasonHelper || 'Please provide a justification for blocking this account'}
                />
              </div>
            </M3FormDialog>

            <M3Dialog
              open={dashboard.showRestoreDialog}
              title={t.restoreUserTitle}
              message={t.restoreUserMessage}
              onScrimClick={() => dashboard.setShowRestoreDialog(false)}
              actions={[
                { label: t.cancelBtn, variant: 'outlined', onClick: () => dashboard.setShowRestoreDialog(false) },
                { label: t.restoreBtn, variant: 'filled', onClick: dashboard.confirmRestore },
              ]}
            />
            <UserAccountForm
              isOpen={dashboard.isCreateOpen}
              onClose={() => dashboard.setIsCreateOpen(false)}
              onSuccess={dashboard.handleCreateSuccess}
              tenantId={dashboard.selectedTenantId}
            />
          </>
        }
        master={
          <UserAccountListPanel
            accounts={dashboard.knownAccounts}
            selectedId={dashboard.selectedId}
            isLoading={dashboard.isLoadingList}
            error={dashboard.listError}
            viewMode={dashboard.viewMode}
            onViewModeChange={dashboard.setViewMode}
            searchCriteria={dashboard.searchCriteria}
            onSearchCriteriaChange={dashboard.setSearchCriteria}
            searchValue={dashboard.searchValue}
            onSearchValueChange={dashboard.setSearchValue}
            onSearchSubmit={dashboard.handleQuerySubmit}
            onRegisterNew={() => dashboard.setIsCreateOpen(true)}
            sortBy={dashboard.sortBy}
            onSortByChange={dashboard.setSortBy}
            sortOrder={dashboard.sortOrder}
            onSortOrderToggle={() => dashboard.setSortOrder((o) => o === 'asc' ? 'desc' : 'asc')}
            activeFilter={dashboard.activeFilter}
            onFilterChange={(val) => { dashboard.setActiveFilter(val); dashboard.setPage(1); }}
            page={dashboard.page}
            pageSize={dashboard.pageSize}
            totalItems={dashboard.totalItems}
            totalPages={dashboard.totalPages}
            startIndex={dashboard.startIndex}
            appliedTerm={dashboard.appliedQuery.term}
            onPageChange={dashboard.setPage}
            onResetQuery={dashboard.handleResetQuery}
            onSelectAccount={dashboard.handleSelectAccount}
            criteriaOptions={criteriaOptions}
            filterOptions={filterOptions}
            sortOptions={sortOptions}
            tenants={dashboard.tenants}
            selectedTenantId={dashboard.selectedTenantId}
            onTenantChange={dashboard.setSelectedTenantId}
          />
        }
        detail={
          <UserAccountDetailPanel
            activeAccount={dashboard.activeAccount}
            isLoading={dashboard.isLoadingList}
            onAccountActivate={dashboard.handleActivate}
            onAccountBlock={dashboard.handleBlockRequest}
            onAccountRestore={dashboard.handleRestoreRequest}
            onAccountUpdate={dashboard.patchAccount}
          />
        }
      />
    </PageShell>
  );
}

