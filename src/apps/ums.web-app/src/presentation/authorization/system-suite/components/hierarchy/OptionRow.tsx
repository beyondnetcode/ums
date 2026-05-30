import React, { useState } from 'react';
import { Pencil, Trash2, KeyRound } from 'lucide-react';
import { useInlineEdit } from '@app/hooks/use-inline-edit';
import { useUpdateOption, useRemoveOption } from '@app/authorization/hooks/use-system-suite';
import { M3TextField } from '@shared/components/M3TextField';
import { IconButton } from '@shared/components/Tooltip';
import { CodeBadge } from '@shared/components/CodeBadge';
import { ErrorDisplay } from '@shared/components/data-display/ErrorDisplay';
import { formatSystemCode } from '@app/utils/security';

type OptionType = {
  id: string;
  code: string;
  label: string;
  description?: string;
  actionCode: string;
  sortOrder?: number;
};

interface OptionRowProps {
  suiteId: string;
  moduleId: string;
  menuId: string;
  subMenuId: string;
  option: OptionType;
}

interface OptionDraft {
  label: string;
  description: string;
  actionCode: string;
  sortOrder: number;
}

export const OptionRow: React.FC<OptionRowProps> = ({
  suiteId, moduleId, menuId, subMenuId, option,
}) => {
  const [editError, setEditError] = useState('');
  const updateOptionMutation = useUpdateOption(suiteId, moduleId, menuId, subMenuId, option.id);
  const removeOptionMutation = useRemoveOption(suiteId, moduleId, menuId, subMenuId, option.id);

  const edit = useInlineEdit<OptionDraft>(['label', 'description', 'actionCode', 'sortOrder']);

  const handleStartEdit = () => {
    edit.openEdit(option.id, {
      label: option.label,
      description: option.description ?? '',
      actionCode: option.actionCode,
      sortOrder: option.sortOrder ?? 1,
    });
    setEditError('');
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    const label = edit.draft.label?.trim() ?? '';
    const actionCode = edit.draft.actionCode?.trim() ?? '';
    if (!label) { setEditError('Etiqueta requerida'); return; }
    if (!actionCode) { setEditError('Código de acción requerido'); return; }
    try {
      await updateOptionMutation.mutateAsync({
        label,
        description: edit.draft.description?.trim() ?? '',
        actionCode: formatSystemCode(actionCode),
        sortOrder: Number(edit.draft.sortOrder) || 1,
      });
      edit.cancelEdit();
      setEditError('');
    } catch { /* handled by hook */ }
  };

  if (edit.isEditing(option.id)) {
    return (
      <form onSubmit={handleUpdate} className="p-2.5 rounded-lg border border-m3-primary/30 bg-m3-surface-container/20 space-y-2 animate-fadeIn">
        <div className="grid grid-cols-2 gap-1.5">
          <M3TextField label="Etiqueta" required value={edit.draft.label ?? ''} onChange={(e) => edit.setField('label', e.target.value)} />
          <M3TextField label="Código de Acción" required value={edit.draft.actionCode ?? ''} onChange={(e) => edit.setField('actionCode', e.target.value)} />
        </div>
        <M3TextField label="Descripción" value={edit.draft.description ?? ''} onChange={(e) => edit.setField('description', e.target.value)} />
        <M3TextField label="Orden" type="number" value={String(edit.draft.sortOrder ?? 1)} onChange={(e) => edit.setField('sortOrder', parseInt(e.target.value) || 1)} />
        <ErrorDisplay error={editError} variant="text" />
        <div className="flex gap-1.5 justify-end">
          <button type="button" onClick={edit.cancelEdit} className="text-[11px] px-2.5 py-1 rounded-md text-m3-secondary hover:bg-m3-surface-variant/20 transition-colors">Cancelar</button>
          <button type="submit" disabled={updateOptionMutation.isPending} className="text-[11px] px-2.5 py-1 rounded-md bg-m3-primary text-m3-on-primary hover:bg-m3-primary/80 disabled:opacity-50 transition-colors">
            {updateOptionMutation.isPending ? 'Guardando…' : 'Guardar'}
          </button>
        </div>
      </form>
    );
  }

  return (
    <div className="group/opt flex items-start justify-between p-2 rounded-lg bg-m3-surface-container/20 border border-m3-outline/10 hover:border-m3-outline/25 transition-all hover:translate-x-[1px] duration-150 animate-fadeIn">
      <div className="flex-1 min-w-0 pr-2">
        <div className="flex items-center gap-1.5 flex-wrap">
          <span className="text-[10px] font-bold text-m3-on-surface">{option.label}</span>
          <CodeBadge code={option.code} size="xs" />
        </div>
        {option.description && (
          <p className="text-[9px] text-m3-secondary mt-0.5 truncate max-w-full" title={option.description}>
            {option.description}
          </p>
        )}
      </div>
      <div className="flex items-center gap-1.5 flex-shrink-0">
        <span className="text-[7px] font-bold uppercase tracking-wider px-1 py-0.5 rounded bg-m3-primary/10 text-m3-primary border border-m3-primary/20 flex items-center gap-0.5">
          <KeyRound className="w-2 h-2" />
          {option.actionCode}
        </span>
        <IconButton
          tooltip="Editar Opción"
          onClick={handleStartEdit}
          className="opacity-0 group-hover/opt:opacity-100 transition-opacity"
        >
          <Pencil className="w-3 h-3" />
        </IconButton>
        <IconButton
          tooltip="Eliminar Opción"
          onClick={() => removeOptionMutation.mutate(option.id)}
          disabled={removeOptionMutation.isPending}
          className="opacity-0 group-hover/opt:opacity-100 transition-opacity hover:text-m3-error hover:bg-m3-error/10"
        >
          <Trash2 className="w-3 h-3" />
        </IconButton>
      </div>
    </div>
  );
};