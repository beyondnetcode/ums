import React, { useState } from 'react';
import {
  useActivateDelegation,
  useRevokeDelegation,
} from '@app/identity/hooks/use-delegation';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';
import { useNotificationStore } from '@app/stores/notification.store';
import { Delegation } from '@domain/identity/models/delegation.model';
import {
  Shield,
  Sliders,
  ShieldAlert,
  CheckCircle2,
} from 'lucide-react';
import { M3Card } from '@shared/components/M3Card';
import { M3Button } from '@shared/components/M3Button';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';

// ─── Props ───────────────────────────────────────────────────────────────────

interface DelegationProfileCardProps {
  delegation: Delegation;
}

// ─── Component ───────────────────────────────────────────────────────────────

const UNIMAR_USERS: Record<string, string> = {
  '5f4e3d01-1b0a-9f8e-7d6c-543210987654': 'Gerente',
  '5f4e3d02-1b0a-9f8e-7d6c-543210987654': 'Analista',
  '5f4e3d05-1b0a-9f8e-7d6c-543210987654': 'Socio',
};

const formatUserId = (id: string) => {
  return UNIMAR_USERS[id] || `Usuario (${id.substring(0, 8)})`;
};

export const DelegationProfileCard: React.FC<DelegationProfileCardProps> = ({
  delegation,
}) => {
  const t = useI18n();
  const statusLabel = useStatusLabel();
  const addNotification = useNotificationStore((s) => s.addNotification);

  const activateMutation = useActivateDelegation(delegation.delegationId);
  const revokeMutation = useRevokeDelegation(delegation.delegationId);
  const isPendingMutation = activateMutation.isPending || revokeMutation.isPending;

  const handleToggleStatus = (newStatus: 'Active' | 'Revoked') => {
    addNotification({
      title: newStatus === 'Active' ? t.notifActivated : t.notifSuspended,
      message: t.notifStatusSetTo(newStatus),
      type: newStatus === 'Active' ? 'success' : 'warning',
    });
    if (newStatus === 'Active') {
      activateMutation.mutate(undefined, { onError: () => {} });
    } else {
      revokeMutation.mutate('User requested revocation', { onError: () => {} });
    }
  };

  return (
    <M3Card
      variant="elevated"
      className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm"
    >
      <div className="flex justify-between items-start gap-4 pb-3.5 border-b border-m3-outline/15 mb-4">
        <div className="flex gap-3 flex-1 min-w-0">
          <div className="p-2 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10 self-start flex-shrink-0">
            <Shield className="w-5 h-5" />
          </div>
          <div className="min-w-0">
            <h3 className="text-sm font-semibold text-m3-on-surface flex items-center gap-1.5 flex-wrap">
              Delegación de Acceso
              <CodeBadge code={delegation.scopeType} />
            </h3>
            <p className="text-xs text-m3-secondary mt-0.5">
              De: {formatUserId(delegation.delegatingAdminId)} → Para: {formatUserId(delegation.delegatedAdminId)}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2 flex-shrink-0">
          <StatusBadge status={delegation.status} label={statusLabel(delegation.status)} />
        </div>
      </div>

      <div className="flex items-center justify-between text-xs">
        <div className="flex items-center gap-1.5 text-m3-secondary font-medium">
          <Sliders className="w-3.5 h-3.5" />
          <span>{t.stateControls}</span>
        </div>
        {delegation.status === 'Active' ? (
          <M3Button
            variant="outlined"
            onClick={() => handleToggleStatus('Revoked')}
            loading={isPendingMutation}
            className="text-rose-500 border-rose-500/30 hover:bg-rose-500/10"
          >
            <ShieldAlert className="w-3.5 h-3.5 mr-1.5" /> Revoke
          </M3Button>
        ) : delegation.status === 'Pending' ? (
          <M3Button
            variant="filled"
            onClick={() => handleToggleStatus('Active')}
            loading={isPendingMutation}
            className="bg-emerald-600 hover:bg-emerald-500"
          >
            <CheckCircle2 className="w-3.5 h-3.5 mr-1.5" /> {t.activateBtn}
          </M3Button>
        ) : null}
      </div>
    </M3Card>
  );
};
