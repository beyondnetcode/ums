import React, { useState } from 'react';
import { ChevronDown, ChevronRight, FolderOpen, Pencil, Trash2 } from 'lucide-react';
import { useInlineEdit } from '@app/hooks/use-inline-edit';
import { useAddOption, useRemoveOption, useUpdateSubMenu } from '@app/authorization/hooks/use-system-suite';
import { M3TextField } from '@shared/components/M3TextField';
import { InlineAddForm } from '@shared/components/InlineAddForm';
import { IconButton } from '@shared/components/Tooltip';
import { CodeBadge } from '@shared/components/CodeBadge';
import { ErrorDisplay } from '@shared/components/data-display/ErrorDisplay';
import { formatSystemCode } from '@app/utils/security';
import { OptionRow } from './OptionRow';
import { AddOptState, emptyOpt } from './types';

type SubMenuType = {
  id: string;
  code: string;
  label: string;
  description?: string;
  sortOrder?: number;
  options?: Array<{
    id: string;
    code: string;
    label: string;
    description?: string;
    actionCode: string;
    sortOrder?: number;
  }>;
};

interface SubMenuRowProps {
  suiteId: string;
  moduleId: string;
  menuId: string;
  subMenu: SubMenuType;
  isExpanded: boolean;
  onToggle: () => void;
  onRemoveSubMenu: (id: string) => void;
  isRemovingSubMenu: boolean;
}

interface SubMenuDraft {
  label: string;
  description: string;
  sortOrder: number;
}

export const SubMenuRow: React.FC<SubMenuRowProps> = ({
  suiteId, moduleId, menuId, subMenu, isExpanded, onToggle,
  onRemoveSubMenu, isRemovingSubMenu,
}) => {
  const [isAddingOpt, setIsAddingOpt] = useState(false);
  const [opt, setOpt] = useState<AddOptState>(emptyOpt());
  const [editError, setEditError] = useState('');

  const addOptionMutation = useAddOption(suiteId, moduleId, menuId, subMenu.id);
  const removeOptionMutation = useRemoveOption(suiteId, moduleId, menuId, subMenu.id);
  const updateSubMenuMutation = useUpdateSubMenu(suiteId, moduleId, menuId, subMenu.id);

  const edit = useInlineEdit<SubMenuDraft>(['label', 'description', 'sortOrder']);

  const handleAddOption = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!opt.code.trim()) { setOpt(s => ({ ...s, error: 'Código requerido' })); return; }
    if (!opt.label.trim()) { setOpt(s => ({ ...s, error: 'Etiqueta requerida' })); return; }
    if (!opt.actionCode.trim()) { setOpt(s => ({ ...s, error: 'Código de acción requerido' })); return; }
    try {
      await addOptionMutation.mutateAsync({
        code: formatSystemCode(opt.code),
        label: opt.label.trim(),
        description: opt.desc.trim(),
        actionCode: formatSystemCode(opt.actionCode),
        sortOrder: parseInt(opt.sort) || 1,
      });
      setOpt(emptyOpt());
      setIsAddingOpt(false);
    } catch { /* handled by hook */ }
  };

  const handleStartEditSub = () => {
    edit.openEdit(subMenu.id, {
      label: subMenu.label,
      description: subMenu.description ?? '',
      sortOrder: subMenu.sortOrder ?? 1,
    });
    setEditError('');
  };

  const handleUpdateSubMenu = async (e: React.FormEvent) => {
    e.preventDefault();
    const label = edit.draft.label?.trim() ?? '';
    if (!label) { setEditError('Etiqueta requerida'); return; }
    try {
      await updateSubMenuMutation.mutateAsync({
        label,
        description: edit.draft.description?.trim() ?? '',
        sortOrder: Number(edit.draft.sortOrder) || 1,
      });
      edit.cancelEdit();
      setEditError('');
    } catch { /* handled by hook */ }
  };

  const handleCancelEditSub = () => {
    edit.cancelEdit();
    setEditError('');
  };

  return (
    <div className="space-y-1.5 animate-fadeIn">
      {edit.isEditing(subMenu.id) ? (
        <form onSubmit={handleUpdateSubMenu} className="p-2.5 rounded-lg border border-m3-primary/30 bg-m3-surface-container/20 space-y-2 animate-fadeIn">
          <div className="grid grid-cols-2 gap-1.5">
            <M3TextField label="Etiqueta" required value={edit.draft.label ?? ''} onChange={(e) => edit.setField('label', e.target.value)} />
            <M3TextField label="Orden" type="number" value={String(edit.draft.sortOrder ?? 1)} onChange={(e) => edit.setField('sortOrder', parseInt(e.target.value) || 1)} />
          </div>
          <M3TextField label="Descripción" value={edit.draft.description ?? ''} onChange={(e) => edit.setField('description', e.target.value)} />
          <ErrorDisplay error={editError} variant="text" />
          <div className="flex gap-1.5 justify-end">
            <button type="button" onClick={handleCancelEditSub} className="text-[11px] px-2.5 py-1 rounded-md text-m3-secondary hover:bg-m3-surface-variant/20 transition-colors">Cancelar</button>
            <button type="submit" disabled={updateSubMenuMutation.isPending} className="text-[11px] px-2.5 py-1 rounded-md bg-m3-primary text-m3-on-primary hover:bg-m3-primary/80 disabled:opacity-50 transition-colors">
              {updateSubMenuMutation.isPending ? 'Guardando…' : 'Guardar'}
            </button>
          </div>
        </form>
      ) : (
        <div className="flex items-center gap-1 group/sm">
          <div
            onClick={onToggle}
            className="flex items-center gap-2 cursor-pointer py-1 px-1.5 rounded hover:bg-m3-surface-variant/15 transition-colors select-none flex-1"
          >
            {isExpanded ? <ChevronDown className="w-3 h-3 text-m3-secondary/70" /> : <ChevronRight className="w-3 h-3 text-m3-secondary/70" />}
            <FolderOpen className="w-3.5 h-3.5 text-amber-400/80" />
            <div className="flex items-center gap-1.5">
              <span className="text-[11px] font-semibold text-m3-on-surface/90">{subMenu.label}</span>
              <CodeBadge code={subMenu.code} size="xs" />
            </div>
          </div>
          <div className="flex items-center gap-0.5 opacity-0 group-hover/sm:opacity-100 transition-opacity pr-1">
            {isExpanded && (
              <button
                type="button"
                aria-label="Agregar Opción"
                title="Agregar Opción"
                onClick={() => setIsAddingOpt(true)}
                className="inline-flex h-6 items-center justify-center gap-1 rounded-md text-m3-secondary/70 transition-colors hover:bg-m3-primary/10 hover:text-m3-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-m3-primary w-6"
              >
                <span className="h-3 w-3">+</span>
              </button>
            )}
            <IconButton tooltip="Editar Submenú" onClick={handleStartEditSub} className="hover:text-m3-primary hover:bg-m3-primary/10">
              <Pencil className="w-3 h-3" />
            </IconButton>
            <IconButton
              tooltip="Eliminar Submenú"
              onClick={() => onRemoveSubMenu(subMenu.id)}
              disabled={isRemovingSubMenu}
              className="hover:text-m3-error hover:bg-m3-error/10"
            >
              <Trash2 className="w-3 h-3" />
            </IconButton>
          </div>
        </div>
      )}

      <div className={`pl-4 space-y-2 border-l border-m3-primary/10 ml-3 pt-0.5 overflow-x-auto no-scrollbar pb-1 ${isExpanded && !edit.isEditing(subMenu.id) ? 'block' : 'hidden'}`}>
        {isAddingOpt && (
          <InlineAddForm
            isOpen
            onToggle={(open) => { setIsAddingOpt(open); if (!open) setOpt(emptyOpt()); }}
            onSubmit={handleAddOption}
            addLabel="Opción"
            title="Nueva Opción"
            cancelLabel="Cancelar"
            submitLabel="Guardar Opción"
            isLoading={addOptionMutation.isPending}
            error={opt.error || undefined}
          >
            <M3TextField label="Código" required value={opt.code} onChange={(e) => setOpt(s => ({ ...s, code: e.target.value }))} placeholder="e.g. VIEW" />
            <M3TextField label="Etiqueta" required value={opt.label} onChange={(e) => setOpt(s => ({ ...s, label: e.target.value }))} placeholder="e.g. Ver Usuarios" />
            <M3TextField label="Descripción" value={opt.desc} onChange={(e) => setOpt(s => ({ ...s, desc: e.target.value }))} placeholder="Opcional" />
            <M3TextField label="Código de Acción" required value={opt.actionCode} onChange={(e) => setOpt(s => ({ ...s, actionCode: e.target.value }))} placeholder="e.g. USER_VIEW" />
            <M3TextField label="Orden" type="number" value={opt.sort} onChange={(e) => setOpt(s => ({ ...s, sort: e.target.value }))} placeholder="1" />
          </InlineAddForm>
        )}

        {!subMenu.options || subMenu.options.length === 0 ? (
          <p className="text-[9px] text-m3-secondary/40 italic">No hay opciones configuradas.</p>
        ) : (
          <div className="grid grid-cols-1 gap-1.5">
            {subMenu.options.map((option) => (
              <OptionRow
                key={option.id}
                suiteId={suiteId}
                moduleId={moduleId}
                menuId={menuId}
                subMenuId={subMenu.id}
                option={option}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};