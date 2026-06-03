import React, { useState } from 'react';
import { ChevronDown, ChevronRight, Layers, EyeOff, ShieldCheck, Trash2 } from 'lucide-react';
import { useAddMenu, useRemoveMenu, useActivateModule, useDeactivateModule } from '@app/authorization/hooks/use-system-suite';
import { InlineAddForm } from '@shared/components/InlineAddForm';
import { IconButton } from '@shared/components/Tooltip';
import { CodeBadge } from '@shared/components/CodeBadge';
import { StatusBadge } from '@shared/components/StatusBadge';
import { M3TextField } from '@shared/components/M3TextField';
import { formatSystemCode } from '@app/utils/security';
import { MenuRow } from './MenuRow';
import { AddMenuState, emptyMenu } from './types';

type ModuleType = {
  id: string;
  code: string;
  name: string;
  description?: string;
  status: string;
  sortOrder?: number;
  menus?: Array<{
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
  }>;
};

interface ModuleCardProps {
  suiteId: string;
  module: ModuleType;
  isExpanded: boolean;
  isNodeExpanded: (id: string) => boolean;
  onToggle: () => void;
  onToggleNode: (id: string) => void;
  onDeactivate: () => void;
  onActivate: () => void;
  onRemove: () => void;
  isDeactivating: boolean;
  isActivating: boolean;
  isRemoving: boolean;
}

export const ModuleCard: React.FC<ModuleCardProps> = ({
  suiteId, module, isExpanded, isNodeExpanded, onToggle, onToggleNode,
  onDeactivate, onActivate, onRemove, isDeactivating, isActivating, isRemoving,
}) => {
  const [isAddingMenu, setIsAddingMenu] = useState(false);
  const [menu, setMenu] = useState<AddMenuState>(emptyMenu());

  const addMenuMutation = useAddMenu(suiteId, module.id);
  const removeMenuMutation = useRemoveMenu(suiteId, module.id);

  const handleAddMenu = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!menu.code.trim()) { setMenu(s => ({ ...s, error: 'Código requerido' })); return; }
    if (!menu.label.trim()) { setMenu(s => ({ ...s, error: 'Etiqueta requerida' })); return; }
    try {
      await addMenuMutation.mutateAsync({
        code: formatSystemCode(menu.code),
        label: menu.label.trim(),
        description: menu.desc.trim(),
        sortOrder: parseInt(menu.sort) || 1,
      });
      setMenu(emptyMenu());
      setIsAddingMenu(false);
    } catch { /* handled by hook */ }
  };

  return (
    <div className="border border-m3-outline/20 rounded-lg overflow-hidden bg-m3-surface-container/5 transition-all hover:border-m3-outline/40 animate-fadeIn">
      <div className="flex items-center justify-between p-3.5 bg-m3-surface-container/10 select-none border-b border-m3-outline/10 hover:bg-m3-surface-container/20 transition-colors group/module">
        <div onClick={onToggle} className="flex items-center gap-2 cursor-pointer flex-1">
          {isExpanded ? <ChevronDown className="w-4 h-4 text-m3-secondary" /> : <ChevronRight className="w-4 h-4 text-m3-secondary" />}
          <Layers className="w-4 h-4 text-m3-primary" />
          <div className="flex items-center gap-2">
            <span className="font-semibold text-sm text-m3-on-surface">{module.name}</span>
            <CodeBadge code={module.code} size="xs" />
          </div>
        </div>
        <div className="flex items-center gap-3">
          <StatusBadge status={module.status} label={module.status} />
          <span className="text-xs text-m3-secondary">Ord: {module.sortOrder}</span>
          <div className="flex items-center gap-0.5 opacity-0 group-hover/module:opacity-100 transition-opacity">
            {module.status === 'Active' ? (
              <IconButton tooltip="Desactivar" onClick={(e) => { e.stopPropagation(); onDeactivate(); }} disabled={isDeactivating}>
                <EyeOff className="w-3.5 h-3.5" />
              </IconButton>
            ) : (
              <IconButton tooltip="Activar" onClick={(e) => { e.stopPropagation(); onActivate(); }} disabled={isActivating}>
                <ShieldCheck className="w-3.5 h-3.5" />
              </IconButton>
            )}
            <IconButton
              tooltip="Eliminar"
              onClick={(e) => { e.stopPropagation(); onRemove(); }}
              disabled={isRemoving}
              className="hover:text-m3-error hover:bg-m3-error/10"
            >
              <Trash2 className="w-3.5 h-3.5" />
            </IconButton>
          </div>
        </div>
      </div>

      <div className={`p-4 space-y-4 ${isExpanded ? 'block' : 'hidden'}`}>
        {module.description && (
          <p className="text-xs text-m3-secondary italic pl-1 border-l-2 border-m3-outline/20">{module.description}</p>
        )}

        <InlineAddForm
          isOpen={isAddingMenu}
          onToggle={(open) => { setIsAddingMenu(open); if (!open) setMenu(emptyMenu()); }}
          onSubmit={handleAddMenu}
          addLabel="+"
          title="Nuevo Menú"
          cancelLabel="Cancelar"
          submitLabel="Guardar Menú"
          isLoading={addMenuMutation.isPending}
          error={menu.error || undefined}
        >
          <M3TextField label="Código" required value={menu.code} onChange={(e) => setMenu(s => ({ ...s, code: e.target.value }))} placeholder="e.g. ADMIN" />
          <M3TextField label="Etiqueta" required value={menu.label} onChange={(e) => setMenu(s => ({ ...s, label: e.target.value }))} placeholder="e.g. Administración" />
          <M3TextField label="Descripción" value={menu.desc} onChange={(e) => setMenu(s => ({ ...s, desc: e.target.value }))} placeholder="Opcional" />
          <M3TextField label="Orden" type="number" value={menu.sort} onChange={(e) => setMenu(s => ({ ...s, sort: e.target.value }))} placeholder="1" />
        </InlineAddForm>

        {!module.menus || module.menus.length === 0 ? (
          <p className="text-xs text-m3-secondary/40 italic text-center py-4">No hay menús configurados.</p>
        ) : (
          <div className="space-y-3">
            {module.menus.map((menuItem) => (
              <MenuRow
                key={menuItem.id}
                suiteId={suiteId}
                moduleId={module.id}
                menu={menuItem}
                isExpanded={isNodeExpanded(menuItem.id)}
                isSubExpanded={isNodeExpanded}
                onToggle={() => onToggleNode(menuItem.id)}
                onToggleSub={onToggleNode}
                onRemoveMenu={(id) => removeMenuMutation.mutate(id)}
                isRemovingMenu={removeMenuMutation.isPending}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};