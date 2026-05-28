/**
 * FeatureFlagDashboardScreen
 *
 * Master-detail layout for the FeatureFlag aggregate:
 *  • Left panel  — paginated list with filter/sort controls
 *  • Right panel — detail view (overview + criteria) for the selected flag
 *  • Modal       — create new feature flag form
 */
import React from 'react';
import { useFeatureFlagDashboard } from '@app/configuration/hooks/use-feature-flag-dashboard';
import { FeatureFlagListPanel } from '../components/FeatureFlagListPanel';
import { FeatureFlagDetailPanel } from '../components/FeatureFlagDetailPanel';
import { FeatureFlagForm } from '../components/FeatureFlagForm';
import { PageShell } from '@shared/layouts/PageShell';
import { MasterDetailLayout } from '@shared/layouts/MasterDetailLayout';

export default function FeatureFlagDashboardScreen(): React.JSX.Element {
  const d = useFeatureFlagDashboard();

  const startIndex = d.totalItems > 0 ? (d.page - 1) * 20 + 1 : 0;

  return (
    <PageShell>
      <FeatureFlagForm
        isOpen={d.isCreateOpen}
        onClose={() => d.setIsCreateOpen(false)}
        onSuccess={(featureFlagId) => {
          d.handleCreateSuccess();
          d.setSelectedId(featureFlagId);
        }}
      />

      <MasterDetailLayout
        splitterLabel="Resize feature flag detail panel"
        master={
          <FeatureFlagListPanel
            flags={d.knownFlags}
            selectedId={d.selectedId}
            isLoading={d.isLoadingList}
            error={d.listError}
            viewMode={d.viewMode}
            onViewModeChange={d.setViewMode}
            searchValue={d.searchValue}
            onSearchValueChange={d.setSearchValue}
            onSearchSubmit={d.handleQuerySubmit}
            onRegisterNew={() => d.setIsCreateOpen(true)}
            activeFilter={d.activeFilter}
            onFilterChange={(val) => { d.setActiveFilter(val); d.setPage(1); }}
            sortBy={d.sortBy}
            onSortByChange={d.setSortBy}
            sortOrder={d.sortOrder}
            onSortOrderToggle={() => d.setSortOrder((o) => (o === 'asc' ? 'desc' : 'asc'))}
            page={d.page}
            pageSize={20}
            totalItems={d.totalItems}
            totalPages={d.totalPages}
            startIndex={startIndex}
            appliedTerm={d.appliedSearch}
            onPageChange={d.setPage}
            onResetQuery={d.handleResetQuery}
            onSelectFlag={d.handleSelect}
          />
        }
        detail={
          <FeatureFlagDetailPanel
            flag={d.activeFlag}
            isLoading={d.isLoadingDetail}
          />
        }
      />
    </PageShell>
  );
}
