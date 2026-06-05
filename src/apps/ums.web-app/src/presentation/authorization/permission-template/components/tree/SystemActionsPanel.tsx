import React, { useState } from 'react';
import type { SystemSuite } from '@domain/authorization/models/system-suite.model';
import type { PermissionTemplateItem } from '@domain/authorization/models/permission-template.model';
import {
  useSetTemplateItemEffect,
  useRemoveTemplateItem,
  useAddTemplateItem,
} from '@app/authorization/hooks/use-permission-template';
import { CheckCircle2, XCircle, MinusCircle, Info, Key } from 'lucide-react';
import { PermissionSectionToolbar } from '@shared/components/PermissionSectionToolbar';
import { CodeBadge } from '@shared/components/CodeBadge';
import { EntityRow } from '@shared/components/EntityRow';
import { EntityCard } from '@shared/components/EntityCard';

interface SystemActionsPanelProps {
  suite: SystemSuite | undefined | null;
  items: PermissionTemplateItem[];
  templateId: string;
  isDraft: boolean;
}

export const SystemActionsPanel: React.FC<SystemActionsPanelProps> = ({
  suite,
  items,
  templateId,
  isDraft,
}) => {
  const setEffect = useSetTemplateItemEffect(templateId);
  const removeItem = useRemoveTemplateItem(templateId);
  const addItem = useAddTemplateItem(templateId);

  const [viewMode, setViewMode] = useState<'list' | 'thumbnail' | 'tree'>('list');
  const [sortBy, setSortBy] = useState('name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');

  if (!suite || suite.actions.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full text-m3-secondary/50 p-6 text-center">
        <Info className="w-8 h-8 mb-2 opacity-20" />
        <p className="text-xs">No hay acciones de sistema globales en esta suite.</p>
      </div>
    );
  }

  const systemItems = items.filter(
    i => i.targetType === 'SystemSuite' || i.targetId === suite.systemSuiteId
  );

  const getActionState = (actionId: string) => {
    const item = systemItems.find(i => i.actionId === actionId);
    if (!item) return 'Neutral';
    if (item.isAllowed) return 'Allow';
    if (item.isDenied) return 'Deny';
    return 'Neutral';
  };

  const handleApplyEffect = async (actionId: string, effect: 'Allow' | 'Deny' | 'Neutral') => {
    if (!isDraft) return;
    const existingItem = systemItems.find(i => i.actionId === actionId);
    if (effect === 'Neutral') {
      if (existingItem) await removeItem.mutateAsync(existingItem.itemId);
      return;
    }
    if (existingItem) {
      await setEffect.mutateAsync({ itemId: existingItem.itemId, effect });
    } else {
      await addItem.mutateAsync({
        targetType: 'SystemSuite',
        targetId: suite.systemSuiteId,
        actionId: actionId,
        isAllowed: effect === 'Allow',
        isDenied: effect === 'Deny',
      });
    }
  };

  const sortedActions = [...suite.actions];
  if (sortBy === 'name') {
    sortedActions.sort((a, b) => {
      const cmp = a.name.localeCompare(b.name);
      return sortOrder === 'asc' ? cmp : -cmp;
    });
  } else if (sortBy === 'code') {
    sortedActions.sort((a, b) => {
      const cmp = a.code.localeCompare(b.code);
      return sortOrder === 'asc' ? cmp : -cmp;
    });
  }

  const renderRow = (action: NonNullable<SystemSuite['actions']>[number]) => {
    const state = getActionState(action.id);
    const StateIcon = state === 'Allow' ? CheckCircle2 : state === 'Deny' ? XCircle : MinusCircle;
    const stateColor =
      state === 'Allow'
        ? 'text-emerald-500'
        : state === 'Deny'
          ? 'text-rose-500'
          : 'text-m3-secondary/30';

    return (
      <EntityRow
        key={action.id}
        id={action.id}
        leading={
          <div className="p-1.5 rounded bg-m3-primary/10 text-m3-primary">
            <Key className="w-3.5 h-3.5" />
          </div>
        }
        trailingColumns={[
          { content: <CodeBadge code={action.code} size="xs" />, width: 'w-28' },
          {
            content: (
              <div className="flex gap-0.5 bg-m3-surface-variant/30 p-0.5 rounded-md border border-m3-outline/10">
                <button
                  onClick={() => handleApplyEffect(action.id, 'Allow')}
                  disabled={!isDraft || addItem.isPending || setEffect.isPending}
                  title="Permitir"
                  className={`p-1 rounded transition-colors disabled:opacity-50 ${state === 'Allow' ? 'bg-emerald-500/15 text-emerald-600' : 'text-m3-secondary hover:text-emerald-500 hover:bg-emerald-500/10'}`}
                >
                  <CheckCircle2 className="w-3 h-3" />
                </button>
                <button
                  onClick={() => handleApplyEffect(action.id, 'Deny')}
                  disabled={!isDraft || addItem.isPending || setEffect.isPending}
                  title="Denegar"
                  className={`p-1 rounded transition-colors disabled:opacity-50 ${state === 'Deny' ? 'bg-rose-500/15 text-rose-600' : 'text-m3-secondary hover:text-rose-500 hover:bg-rose-500/10'}`}
                >
                  <XCircle className="w-3 h-3" />
                </button>
                <button
                  onClick={() => handleApplyEffect(action.id, 'Neutral')}
                  disabled={!isDraft || addItem.isPending || removeItem.isPending}
                  title="Neutral"
                  className={`p-1 rounded transition-colors disabled:opacity-50 ${state === 'Neutral' ? 'bg-m3-secondary/20 text-m3-on-surface' : 'text-m3-secondary hover:bg-m3-secondary/20 hover:text-m3-on-surface'}`}
                >
                  <MinusCircle className="w-3 h-3" />
                </button>
              </div>
            ),
            width: 'w-28',
          },
        ]}
      >
        <div>
          <span className="text-xs font-semibold text-m3-on-surface">{action.name}</span>
          <div className={`flex items-center gap-1 text-[10px] font-medium ${stateColor}`}>
            <StateIcon className="w-3 h-3" />
            {state === 'Allow' ? 'Permitido' : state === 'Deny' ? 'Denegado' : 'Neutral'}
          </div>
        </div>
      </EntityRow>
    );
  };

  const renderCard = (action: NonNullable<SystemSuite['actions']>[number]) => {
    const state = getActionState(action.id);

    return (
      <EntityCard
        key={action.id}
        icon={<Key className="w-4 h-4" />}
        title={action.name}
        subtitle={<CodeBadge code={action.code} size="xs" />}
        badges={
          <div className="flex gap-0.5 bg-m3-surface-variant/30 p-0.5 rounded-md border border-m3-outline/10">
            <button
              onClick={() => handleApplyEffect(action.id, 'Allow')}
              disabled={!isDraft || addItem.isPending || setEffect.isPending}
              title="Permitir"
              className={`p-1 rounded transition-colors disabled:opacity-50 ${state === 'Allow' ? 'bg-emerald-500/15 text-emerald-600' : 'text-m3-secondary hover:text-emerald-500 hover:bg-emerald-500/10'}`}
            >
              <CheckCircle2 className="w-3.5 h-3.5" />
            </button>
            <button
              onClick={() => handleApplyEffect(action.id, 'Deny')}
              disabled={!isDraft || addItem.isPending || setEffect.isPending}
              title="Denegar"
              className={`p-1 rounded transition-colors disabled:opacity-50 ${state === 'Deny' ? 'bg-rose-500/15 text-rose-600' : 'text-m3-secondary hover:text-rose-500 hover:bg-rose-500/10'}`}
            >
              <XCircle className="w-3.5 h-3.5" />
            </button>
            <button
              onClick={() => handleApplyEffect(action.id, 'Neutral')}
              disabled={!isDraft || addItem.isPending || removeItem.isPending}
              title="Neutral"
              className={`p-1 rounded transition-colors disabled:opacity-50 ${state === 'Neutral' ? 'bg-m3-secondary/20 text-m3-on-surface' : 'text-m3-secondary hover:bg-m3-secondary/20 hover:text-m3-on-surface'}`}
            >
              <MinusCircle className="w-3.5 h-3.5" />
            </button>
          </div>
        }
      />
    );
  };

  return (
    <div className="flex flex-col h-full">
      <PermissionSectionToolbar
        viewMode={viewMode}
        onViewModeChange={setViewMode}
        sortOptions={[
          { label: 'Nombre', value: 'name' },
          { label: 'Código', value: 'code' },
        ]}
        sortBy={sortBy}
        onSortByChange={setSortBy}
        sortOrder={sortOrder}
        onSortOrderToggle={() => setSortOrder(o => (o === 'asc' ? 'desc' : 'asc'))}
        itemCount={suite.actions.length}
        itemLabel="acción"
      />

      <div className="flex-1 overflow-y-auto px-1 pb-2">
        {!isDraft && (
          <div className="flex items-center gap-2 text-[10px] text-m3-secondary/70 bg-m3-surface-variant/30 p-2 rounded-lg mb-3">
            <Info className="w-3 h-3" />
            Solo puedes modificar reglas en plantillas en estado Borrador (Draft).
          </div>
        )}

        {viewMode === 'list' ? (
          <div className="flex flex-col gap-0.5">{sortedActions.map(renderRow)}</div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
            {sortedActions.map(renderCard)}
          </div>
        )}
      </div>
    </div>
  );
};
