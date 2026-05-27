import React from 'react';
import type { SystemSuite } from '@domain/authorization/models/system-suite.model';
import type { PermissionTemplateItem } from '@domain/authorization/models/permission-template.model';
import { useSetTemplateItemEffect, useRemoveTemplateItem, useAddTemplateItem } from '@app/authorization/hooks/use-permission-template';
import { CheckCircle2, XCircle, MinusCircle, Info } from 'lucide-react';

interface SystemActionsPanelProps {
  suite: SystemSuite | undefined | null;
  items: PermissionTemplateItem[];
  templateId: string;
  isDraft: boolean;
}

export const SystemActionsPanel: React.FC<SystemActionsPanelProps> = ({ suite, items, templateId, isDraft }) => {
  const setEffect = useSetTemplateItemEffect(templateId);
  const removeItem = useRemoveTemplateItem(templateId);
  const addItem = useAddTemplateItem(templateId);

  if (!suite || suite.actions.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full text-m3-secondary/50 p-6 text-center">
        <Info className="w-8 h-8 mb-2 opacity-20" />
        <p className="text-xs">No hay acciones de sistema globales en esta suite.</p>
      </div>
    );
  }

  // System actions are usually assigned directly to the SystemSuite targetId.
  // We'll look for items where targetId === suite.systemSuiteId OR targetType === 'SystemSuite'
  const systemItems = items.filter(i => i.targetType === 'SystemSuite' || i.targetId === suite.systemSuiteId);

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
      if (existingItem) {
        await removeItem.mutateAsync(existingItem.itemId);
      }
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
        isDenied: effect === 'Deny'
      });
    }
  };

  return (
    <div className="p-4 space-y-4">
      <div className="mb-4">
        <h3 className="text-sm font-bold text-m3-on-surface">Acciones del Sistema</h3>
        <p className="text-xs text-m3-secondary">Acciones globales que no están atadas a un módulo específico.</p>
      </div>

      <div className="grid gap-3">
        {suite.actions.map(action => {
          const state = getActionState(action.id);
          const StateIcon = state === 'Allow' ? CheckCircle2 : state === 'Deny' ? XCircle : MinusCircle;
          
          return (
            <div key={action.id} className="p-3 rounded-lg border border-m3-outline/20 bg-m3-surface-container/30 flex items-center justify-between">
              <div>
                <p className="text-xs font-bold text-m3-on-surface">{action.name}</p>
                <p className="text-[10px] text-m3-secondary font-mono mt-0.5">{action.code}</p>
              </div>

              <div className="flex gap-1 bg-m3-surface-variant/30 p-1 rounded-md border border-m3-outline/10">
                <button
                  onClick={() => handleApplyEffect(action.id, 'Allow')}
                  disabled={!isDraft || addItem.isPending || setEffect.isPending}
                  title="Permitir"
                  className={`p-1.5 rounded transition-colors disabled:opacity-50 ${state === 'Allow' ? 'bg-emerald-500/15 text-emerald-600' : 'text-m3-secondary hover:text-emerald-500 hover:bg-emerald-500/10'}`}
                >
                  <CheckCircle2 className="w-4 h-4" />
                </button>
                <button
                  onClick={() => handleApplyEffect(action.id, 'Deny')}
                  disabled={!isDraft || addItem.isPending || setEffect.isPending}
                  title="Denegar"
                  className={`p-1.5 rounded transition-colors disabled:opacity-50 ${state === 'Deny' ? 'bg-rose-500/15 text-rose-600' : 'text-m3-secondary hover:text-rose-500 hover:bg-rose-500/10'}`}
                >
                  <XCircle className="w-4 h-4" />
                </button>
                <button
                  onClick={() => handleApplyEffect(action.id, 'Neutral')}
                  disabled={!isDraft || addItem.isPending || removeItem.isPending}
                  title="Neutral"
                  className={`p-1.5 rounded transition-colors disabled:opacity-50 ${state === 'Neutral' ? 'bg-m3-secondary/20 text-m3-on-surface' : 'text-m3-secondary hover:bg-m3-secondary/20 hover:text-m3-on-surface'}`}
                >
                  <MinusCircle className="w-4 h-4" />
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};
