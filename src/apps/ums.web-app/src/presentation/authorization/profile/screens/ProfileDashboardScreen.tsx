import React, { useState } from 'react';
import { useGetAllProfiles, useGetProfile } from '@app/authorization/hooks/use-profile';
import { useQueryState } from '@app/shared/hooks/use-query-state';
import { usePaginationState } from '@app/shared/hooks/use-pagination-state';
import { ProfileListPanel } from '../components/ProfileListPanel';
import { ProfileDetailPanel } from '../components/ProfileDetailPanel';
import { ProfileForm } from '../components/ProfileForm';
import { M3AuthorizationGraphPanel } from '../components/M3AuthorizationGraphPanel';
import { PageShell } from '@shared/layouts/PageShell';
import { MasterDetailLayout } from '@shared/layouts/MasterDetailLayout';

export default function ProfileDashboardScreen(): React.JSX.Element {
  const [selectedId, setSelectedId] = useState('');
  const [viewMode, setViewMode] = useState<'list' | 'thumbnail'>('list');

  const queryState = useQueryState({
    criteria: 'userId',
    filter: 'all',
    sortBy: 'userId',
  });

  const paginationState = usePaginationState({
    initialPageSize: 20,
  });

  // Dialog Open States
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isGraphOpen, setIsGraphOpen] = useState(false);
  const [graphProfileId, setGraphProfileId] = useState('');

  // Fetch paginated master list
  const { data: pageData, isLoading: loadingList, error: listError } = useGetAllProfiles({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
    search: queryState.appliedQuery.term || undefined,
    criteria: 'userId',
    status: queryState.activeFilter,
    sortBy: queryState.sortBy,
    sortOrder: queryState.sortOrder,
  });

  // Fetch detail for selected profile
  const { data: activeProfile, isLoading: loadingDetail } = useGetProfile(selectedId || null);



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
            queryState={queryState}
            paginationState={{
              ...paginationState,
              totalItems: pageData?.totalItems ?? 0,
              totalPages: pageData?.totalPages ?? 0,
            }}
            onRegisterNew={() => setIsCreateOpen(true)}
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
