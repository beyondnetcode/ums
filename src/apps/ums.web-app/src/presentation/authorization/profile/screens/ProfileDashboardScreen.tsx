import React, { useState } from 'react';
import { useGetAllProfiles, useGetProfile } from '@app/authorization/hooks/use-profile';
import { ProfileListPanel } from '../components/ProfileListPanel';
import { ProfileDetailPanel } from '../components/ProfileDetailPanel';
import { ProfileForm } from '../components/ProfileForm';
import { M3AuthorizationGraphPanel } from '../components/M3AuthorizationGraphPanel';
import { PageShell } from '@shared/layouts/PageShell';
import { MasterDetailLayout } from '@shared/layouts/MasterDetailLayout';

export default function ProfileDashboardScreen(): React.JSX.Element {
  const [selectedId, setSelectedId] = useState('');
  const [searchValue, setSearchValue] = useState('');
  const [appliedSearch, setAppliedSearch] = useState('');
  const [page, setPage] = useState(1);
  const [viewMode, setViewMode] = useState<'list' | 'thumbnail'>('list');
  const [activeFilter, setActiveFilter] = useState('all');
  const [sortBy, setSortBy] = useState('userId');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  
  // Dialog Open States
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isGraphOpen, setIsGraphOpen] = useState(false);
  const [graphProfileId, setGraphProfileId] = useState('');

  // Fetch paginated master list
  const { data: pageData, isLoading: loadingList, error: listError } = useGetAllProfiles({
    page,
    pageSize: 20,
    search: appliedSearch || undefined,
    criteria: 'userId',
    status: activeFilter,
    sortBy,
    sortOrder,
  });

  // Fetch detail for selected profile
  const { data: activeProfile, isLoading: loadingDetail } = useGetProfile(selectedId || null);

  const startIndex = pageData && pageData.totalItems > 0 ? (page - 1) * 20 + 1 : 0;

  const handleQuerySubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setAppliedSearch(searchValue);
    setPage(1);
  };

  const handleResetQuery = () => {
    setSearchValue('');
    setAppliedSearch('');
    setActiveFilter('all');
    setSortBy('userId');
    setSortOrder('asc');
    setPage(1);
  };

  const handleOpenGraph = (profileId: string) => {
    setGraphProfileId(profileId);
    setIsGraphOpen(true);
  };

  return (
    <PageShell>
      {/* Create Dialog Form */}
      <ProfileForm
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={(profileId) => {
          setIsCreateOpen(false);
          setSelectedId(profileId);
        }}
      />

      {/* Multi-format Graph Exporter Dialog */}
      <M3AuthorizationGraphPanel
        isOpen={isGraphOpen}
        onClose={() => {
          setIsGraphOpen(false);
          setGraphProfileId('');
        }}
        profileId={graphProfileId}
      />

      <MasterDetailLayout
        splitterLabel="Resize profile detail panel"
        master={
          <ProfileListPanel
            profiles={pageData?.items ?? []}
            selectedId={selectedId}
            isLoading={loadingList}
            error={listError}
            viewMode={viewMode}
            onViewModeChange={setViewMode}
            searchValue={searchValue}
            onSearchValueChange={setSearchValue}
            onSearchSubmit={handleQuerySubmit}
            onRegisterNew={() => setIsCreateOpen(true)}
            activeFilter={activeFilter}
            onFilterChange={(val) => { setActiveFilter(val); setPage(1); }}
            sortBy={sortBy}
            onSortByChange={setSortBy}
            sortOrder={sortOrder}
            onSortOrderToggle={() => setSortOrder((o) => (o === 'asc' ? 'desc' : 'asc'))}
            page={page}
            pageSize={20}
            totalItems={pageData?.totalItems ?? 0}
            totalPages={pageData?.totalPages ?? 0}
            startIndex={startIndex}
            appliedTerm={appliedSearch}
            onPageChange={setPage}
            onResetQuery={handleResetQuery}
            onSelectProfile={setSelectedId}
            onOpenGraph={handleOpenGraph}
          />
        }
        detail={
          <ProfileDetailPanel
            profile={activeProfile}
            isLoading={loadingDetail}
          />
        }
      />
    </PageShell>
  );
}
