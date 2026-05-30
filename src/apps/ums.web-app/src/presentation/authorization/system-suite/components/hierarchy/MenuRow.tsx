import React, { useState } from 'react';
import { ChevronDown, ChevronRight, Folder, FolderOpen, Pencil, Trash2 } from 'lucide-react';
import { useInlineEdit } from '@app/hooks/use-inline-edit';
import { useAddSubMenu, useRemoveSubMenu, useUpdateMenu } from '@app/authorization/hooks/use-system-suite';
import { M3TextField } from '@shared/components/M3TextField';
import { InlineAddForm } from '@shared/components/InlineAddForm';
import { IconButton } from '@shared/components/Tooltip';
import { CodeBadge } from '@shared/components/CodeBadge';
import { ErrorDisplay } from '@shared/components/data-display/ErrorDisplay';
import { formatSystemCode } from '@app/utils/security';
import { SubMenuRow } from './SubMenuRow';
import { AddSubState, emptySub } from './types';

type MenuType = {
  id: string;
  code: string;
  label: string;
  description?: string;
  sortOrder?: number;
  subMenus?: Array<{
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
  }>;
};

interface MenuRowProps {
  suiteId: string;
  moduleId: string;
  menu: MenuType;
  isExpanded: boolean;
  isSubExpanded: (id: string) => boolean;
  onToggle: () => void;
  onToggleSub: (id: string) => void;
  onRemoveMenu: (id: string) => void;
  isRemovingMenu: boolean;
}

interface MenuDraft {
  label: string;
  description: string;
  sortOrder: number;
}

export const MenuRow: React.FC<MenuRowProps> = ({
  suiteId, moduleId, menu, isExpanded, isSubExpanded,
  onToggle, onToggleSub, onRemoveMenu, isRemovingMenu,
}) => {
  const [isAddingSub, setIsAddingSub] = useState(false);
  const [sub, setSub] = useState<AddSubState>(emptySub());
  const [editError, setEditError] = useState('');

  const addSubMenuMutation = useAddSubMenu(suiteId, moduleId, menu.id);
  const removeSubMenuMutation = useRemoveSubMenu(suiteId, moduleId, menu.id);
  const updateMenuMutation = useUpdateMenu(suiteId, moduleId, menu.id);

  const edit = useInlineEdit<MenuDraft>(['label', 'description', 'sortOrder']);

  const handleAddSubMenu = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!sub.code.trim()) { setSub(s => ({ ...s, error: 'Código requerido' })); return; }
    if (!sub.label.trim()) { setSub(s => ({ ...s, error: 'Etiqueta requerida' })); return; }
    try {
      await addSubMenuMutation.mutateAsync({
        code: formatSystemCode(sub.code),
        label: sub.label.trim(),
        description: sub.desc.trim(),
        sortOrder: parseInt(sub.sort) || 1,
      });
      setSub(emptySub());
      setIsAddingSub(false);
    } catch { /* handled by hook */ }
  };

  const handleStartEditMenu = () => {
    edit.openEdit(menu.id, {
      label: menu.label,
      description: menu.description ?? '',
      sortOrder: menu.sortOrder ?? 1,
    });
    setEditError('');
  };

  const handleUpdateMenu = async (e: React.FormEvent) => {
    e.preventDefault();
    const label = edit.draft.label?.trim() ?? '';
    if (!label) { setEditError('Etiqueta requerida'); return; }
    try {
      await updateMenuMutation.mutateAsync({
        label,
        description: edit.draft.description?.trim() ?? '',
        sortOrder: Number(edit.draft.sortOrder) || 1,
      });
      edit.cancelEdit();
      setEditError('');
    } catch { /* handled by hook */ }
  };

  const handleCancelEditMenu = () => {
    edit.cancelEdit();
    setEditError('');
  };

  return (
    <div className="space-y-1.5 animate-fadeIn">
      {edit.isEditing(menu.id) ? (
        <form onSubmit={handleUpdateMenu} className="p-2.5 rounded-lg border border-m3-primary/30 bg-m3-surface-container/20 space-y-2 animate-fadeIn">
          <div className="grid grid-cols-2 gap-1.5">
            <M3TextField label="Etiqueta" required value={edit.draft.label ?? ''} onChange={(e) => edit.setField('label', e.target.value)} />
            <M3TextField label="Orden" type="number" value={String(edit.draft.sortOrder ?? 1)} onChange={(e) => edit.setField('sortOrder', parseInt(e.target.value) || 1)} />
          </div>
          <M3TextField label="Descripción" value={edit.draft.description ?? ''} onChange={(e) => edit.setField('description', e.target.value)} />
          <ErrorDisplay error={editError} variant="text" />
          <div className="flex gap-1.5 justify-end">
            <button type="button" onClick={handleCancelEditMenu} className="text-[11px] px-2.5 py-1 rounded-md text-m3-secondary hover:bg-m3-surface-variant/20 transition-colors">Cancelar</button>
            <button type="submit" disabled={updateMenuMutation.isPending} className="text-[11px] px-2.5 py-1 rounded-md bg-m3-primary text-m3-on-primary hover:bg-m3-primary/80 disabled:opacity-50 transition-colors">
              {updateMenuMutation.isPending ? 'Guardando…' : 'Guardar'}
            </button>
          </div>
        </form>
      ) : (
        <div className="flex items-center gap-1 group/mn">
          <div
            onClick={onToggle}
            className="flex items-center gap-2 cursor-pointer py-1.5 px-2 rounded hover:bg-m3-surface-variant/20 transition-colors select-none flex-1"
          >
            {isExpanded ? <ChevronDown className="w-3.5 h-3.5 text-m3-secondary" /> : <ChevronRight className="w-3.5 h-3.5 text-m3-secondary" />}
            {isExpanded ? <FolderOpen className="w-4 h-4 text-amber-500" /> : <Folder className="w-4 h-4 text-amber-500" />}
            <div className="flex items-center gap-1.5">
              <span className="text-xs font-semibold text-m3-on-surface">{menu.label}</span>
              <CodeBadge code={menu.code} size="xs" />
            </div>
          </div>
          <div className="flex items-center gap-0.5 opacity-0 group-hover/mn:opacity-100 transition-opacity pr-1">
            {isExpanded && (
              <button
                type="button"
                aria-label="Agregar Submenú"
                title="Agregar Submenú"
                onClick={() => setIsAddingSub(true)}
                className="inline-flex h-6 items-center justify-center gap-1 rounded-md text-m3-secondary/70 transition-colors hover:bg-m3-primary/10 hover:text-m3-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-m3-primary w-6"
              >
                <span className="h-3 w-3">+</span>
              </button>
            )}
            <IconButton tooltip="Editar Menú" onClick={handleStartEditMenu} className="hover:text-m3-primary hover:bg-m3-primary/10">
              <Pencil className="w-3 h-3" />
            </IconButton>
            <IconButton
              tooltip="Eliminar Menú"
              onClick={() => onRemoveMenu(menu.id)}
              disabled={isRemovingMenu}
              className="hover:text-m3-error hover:bg-m3-error/10"
            >
              <Trash2 className="w-3.5 h-3.5" />
            </IconButton>
          </div>
        </div>
      )}

      <div className={`pl-5 space-y-3 border-l border-m3-primary/10 ml-3.5 pt-0.5 overflow-x-auto no-scrollbar pb-1 ${isExpanded && !edit.isEditing(menu.id) ? 'block' : 'hidden'}`}>
        {isAddingSub && (
          <InlineAddForm
            isOpen
            onToggle={(open) => { setIsAddingSub(open); if (!open) setSub(emptySub()); }}
            onSubmit={handleAddSubMenu}
            addLabel="Submenú"
            title="Nuevo Submenú"
            cancelLabel="Cancelar"
            submitLabel="Guardar Submenú"
            isLoading={addSubMenuMutation.isPending}
            error={sub.error || undefined}
          >
            <M3TextField label="Código" required value={sub.code} onChange={(e) => setSub(s => ({ ...s, code: e.target.value }))} placeholder="e.g. USERS" />
            <M3TextField label="Etiqueta" required value={sub.label} onChange={(e) => setSub(s => ({ ...s, label: e.target.value }))} placeholder="e.g. Gestión de Usuarios" />
            <M3TextField label="Descripción" value={sub.desc} onChange={(e) => setSub(s => ({ ...s, desc: e.target.value }))} placeholder="Opcional" />
            <M3TextField label="Orden" type="number" value={sub.sort} onChange={(e) => setSub(s => ({ ...s, sort: e.target.value }))} placeholder="1" />
          </InlineAddForm>
        )}

        {!menu.subMenus || menu.subMenus.length === 0 ? (
          <p className="text-[9px] text-m3-secondary/40 italic">No hay submenús configurados.</p>
        ) : (
          <div className="space-y-2">
            {menu.subMenus.map((subMenu) => (
              <SubMenuRow
                key={subMenu.id}
                suiteId={suiteId}
                moduleId={moduleId}
                menuId={menu.id}
                subMenu={subMenu}
                isExpanded={isSubExpanded(subMenu.id)}
                onToggle={() => onToggleSub(subMenu.id)}
                onRemoveSubMenu={(id) => removeSubMenuMutation.mutate(id)}
                isRemovingSubMenu={removeSubMenuMutation.isPending}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};