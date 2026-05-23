import React, { useState } from 'react';
import { useI18n } from '@app/i18n/use-i18n';
import {
  useGetDelegationsByDelegatedAdmin,
  useGetDelegationsByDelegatingAdmin,
} from '@app/identity/hooks/use-delegation';
import { DelegationListPanel } from '../components/DelegationListPanel';
import { DelegationForm } from '../components/DelegationForm';
import { PageShell } from '@shared/layouts/PageShell';
import { M3Dialog } from '@shared/components/M3Dialog';

// In a real app, these come from an auth context.
const CURRENT_USER_ID = 'f3e2d1c0-b9a8-7f6e-5d4c-321098765432';
const CURRENT_TENANT_ID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';

export default function DelegationDashboardScreen(): React.JSX.Element {
  const t = useI18n();

  const [selectedId, setSelectedId] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [showRevokeDialog, setShowRevokeDialog] = useState(false);

  const receivedQuery = useGetDelegationsByDelegatedAdmin(CURRENT_USER_ID, CURRENT_TENANT_ID);
  const grantedQuery = useGetDelegationsByDelegatingAdmin(CURRENT_USER_ID, CURRENT_TENANT_ID);

  const handleCreateSuccess = () => {
    setIsCreateOpen(false);
  };

  return (
    <PageShell>
      <div className="flex flex-col h-full gap-4 p-4">
        <div className="flex items-center justify-between">
          <h1 className="text-lg font-semibold text-m3-on-surface">Delegation Management</h1>
          <button
            onClick={() => setIsCreateOpen(true)}
            className="px-4 py-1.5 text-sm rounded-full bg-m3-primary text-m3-on-primary hover:bg-m3-primary/90 transition-colors"
            type="button"
          >
            New Delegation
          </button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 flex-1 min-h-0">
          <div className="border border-m3-outline/20 rounded-2xl overflow-hidden bg-m3-surface">
            <DelegationListPanel
              delegations={receivedQuery.data ?? []}
              selectedId={selectedId}
              isLoading={receivedQuery.isLoading}
              error={receivedQuery.error ?? null}
              title="Delegations Received"
              emptyLabel="No delegations have been granted to you."
              onSelectDelegation={setSelectedId}
            />
          </div>

          <div className="border border-m3-outline/20 rounded-2xl overflow-hidden bg-m3-surface">
            <DelegationListPanel
              delegations={grantedQuery.data ?? []}
              selectedId={selectedId}
              isLoading={grantedQuery.isLoading}
              error={grantedQuery.error ?? null}
              title="Delegations Granted"
              emptyLabel="You have not granted any delegations."
              onSelectDelegation={setSelectedId}
              onRegisterNew={() => setIsCreateOpen(true)}
            />
          </div>
        </div>
      </div>

      <M3Dialog
        open={showRevokeDialog}
        title="Revoke Delegation"
        message="Are you sure you want to revoke this delegation? This action cannot be undone."
        onScrimClick={() => setShowRevokeDialog(false)}
        actions={[
          { label: t.cancelBtn, variant: 'outlined', onClick: () => setShowRevokeDialog(false) },
          {
            label: 'Revoke',
            variant: 'filled',
            className: 'bg-m3-error hover:bg-m3-error/90 border-0',
            onClick: () => setShowRevokeDialog(false),
          },
        ]}
      />

      <DelegationForm
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={handleCreateSuccess}
        defaultTenantId={CURRENT_TENANT_ID}
        defaultDelegatingAdminId={CURRENT_USER_ID}
      />
    </PageShell>
  );
}
