import React, { useState, useMemo } from 'react';
import {
  Box, Shield, Key, ChevronDown, ChevronRight, Users,
  Folder, FolderOpen, Layers, EyeOff, ShieldCheck, Trash2, KeyRound, Plus, Pencil,
  ChevronsUpDown, ChevronsDownUp,
} from 'lucide-react';
import { SystemSuite } from '@domain/authorization/models/system-suite.model';
import { SystemSuiteProfileCard } from './SystemSuiteProfileCard';
import { DetailPanelShell, DetailTab } from '@shared/components/DetailPanelShell';
import { useI18n } from '@app/i18n/use-i18n';
import { useInlineEdit } from '@app/hooks/use-inline-edit';
import {
  useAddModule,
  useRemoveModule,
  useActivateModule,
  useDeactivateModule,
  useAddMenu,
  useRemoveMenu,
  useAddSubMenu,
  useUpdateSubMenu,
  useRemoveSubMenu,
  useAddOption,
  useUpdateOption,
  useRemoveOption,
  useRegisterAction,
  useRemoveAction,
} from '@app/authorization/hooks/use-system-suite';
import { M3TextField } from '@shared/components/M3TextField';
import { InlineAddForm } from '@shared/components/InlineAddForm';
import { IconButton } from '@shared/components/Tooltip';
import { CodeBadge } from '@shared/components/CodeBadge';
import { formatSystemCode } from '@app/utils/security';
import { SystemSuiteRolesPanel } from './SystemSuiteRolesPanel';
import { SystemSuiteDomainResourcesPanel } from './SystemSuiteDomainResourcesPanel';
import { Database } from 'lucide-react';
import { ChildEntityToolbar } from '@shared/components/ChildEntityToolbar';

type SystemSuiteTab = 'overview' | 'modules' | 'domain-resources' | 'actions' | 'roles';

interface SystemSuiteDetailPanelProps {
  activeSystemSuite: SystemSuite | undefined;
  isLoading: boolean;
  onSystemSuiteUpdate: (systemSuiteId: string, patch: Partial<SystemSuite>) => void;
  onEditingChange: (isEditing: boolean) => void;
}

// ── Inline add form state helper ─────────────────────────────────────────────

interface AddMenuState  { code: string; label: string; desc: string; sort: string; error: string }
interface AddSubState   { code: string; label: string; desc: string; sort: string; error: string }
interface AddOptState   { code: string; label: string; desc: string; actionCode: string; sort: string; error: string }

const emptyMenu  = (): AddMenuState  => ({ code: '', label: '', desc: '', sort: '1', error: '' });
const emptySub   = (): AddSubState   => ({ code: '', label: '', desc: '', sort: '1', error: '' });
const emptyOpt   = (): AddOptState   => ({ code: '', label: '', desc: '', actionCode: '', sort: '1', error: '' });

interface CompactAddButtonProps {
  label: string;
  onClick: () => void;
  showLabel?: boolean;
}

const CompactAddButton: React.FC<CompactAddButtonProps> = ({ label, onClick, showLabel = false }) => (
  <button
    type="button"
    aria-label={label}
    title={label}
    onClick={onClick}
    className={`inline-flex h-6 items-center justify-center gap-1 rounded-md text-m3-secondary/70 transition-colors hover:bg-m3-primary/10 hover:text-m3-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-m3-primary ${
      showLabel ? 'px-1.5 text-[10px] font-medium' : 'w-6'
    }`}
  >
    <Plus className="h-3 w-3" />
    {showLabel && label}
  </button>
);

// ── Option row (with inline edit) ────────────────────────────────────────────

type OptionType = NonNullable<SystemSuite['modules']>[number]['menus'][number]['subMenus'][number]['options'][number];

interface OptionRowProps {
  suiteId: string;
  moduleId: string;
  menuId: string;
  subMenuId: string;
  option: OptionType;
  onRemove: () => void;
  isRemoving: boolean;
}

interface OptionDraft {
  label: string;
  description: string;
  actionCode: string;
  sortOrder: number;
}

const OptionRow: React.FC<OptionRowProps> = ({ suiteId, moduleId, menuId, subMenuId, option, onRemove, isRemoving }) => {
  const [editError, setEditError] = useState('');
  const updateOptionMutation = useUpdateOption(suiteId, moduleId, menuId, subMenuId, option.id);

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
        {editError && <p className="text-[10px] text-m3-error">{editError}</p>}
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
          onClick={onRemove}
          disabled={isRemoving}
          className="opacity-0 group-hover/opt:opacity-100 transition-opacity hover:text-m3-error hover:bg-m3-error/10"
        >
          <Trash2 className="w-3 h-3" />
        </IconButton>
      </div>
    </div>
  );
};

// ── SubMenu row (options + add option form) ───────────────────────────────────

interface SubMenuRowProps {
  suiteId: string;
  moduleId: string;
  menuId: string;
  subMenu: NonNullable<SystemSuite['modules']>[number]['menus'][number]['subMenus'][number];
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

const SubMenuRow: React.FC<SubMenuRowProps> = ({
  suiteId, moduleId, menuId, subMenu, isExpanded, onToggle,
  onRemoveSubMenu, isRemovingSubMenu,
}) => {
  const [isAddingOpt, setIsAddingOpt]   = useState(false);
  const [opt, setOpt]                   = useState<AddOptState>(emptyOpt);
  const [editError, setEditError]       = useState('');

  const addOptionMutation    = useAddOption(suiteId, moduleId, menuId, subMenu.id);
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
      {/* SubMenu header */}
      {edit.isEditing(subMenu.id) ? (
        <form onSubmit={handleUpdateSubMenu} className="p-2.5 rounded-lg border border-m3-primary/30 bg-m3-surface-container/20 space-y-2 animate-fadeIn">
          <div className="grid grid-cols-2 gap-1.5">
            <M3TextField label="Etiqueta" required value={edit.draft.label ?? ''} onChange={(e) => edit.setField('label', e.target.value)} />
            <M3TextField label="Orden" type="number" value={String(edit.draft.sortOrder ?? 1)} onChange={(e) => edit.setField('sortOrder', parseInt(e.target.value) || 1)} />
          </div>
          <M3TextField label="Descripción" value={edit.draft.description ?? ''} onChange={(e) => edit.setField('description', e.target.value)} />
          {editError && <p className="text-[10px] text-m3-error">{editError}</p>}
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
              <CompactAddButton label="Agregar Opción" onClick={() => setIsAddingOpt(true)} />
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

      {/* Options + add option form */}
      {isExpanded && !edit.isEditing(subMenu.id) && (
        <div className="pl-5 space-y-1.5 border-l border-m3-outline/10 ml-3 pt-0.5">
          {/* Add option inline form */}
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
                  onRemove={() => removeOptionMutation.mutate(option.id)}
                  isRemoving={removeOptionMutation.isPending}
                />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

// ── Menu row (submenus + add submenu form) ────────────────────────────────────

interface MenuRowProps {
  suiteId: string;
  moduleId: string;
  menu: NonNullable<SystemSuite['modules']>[number]['menus'][number];
  isExpanded: boolean;
  isSubExpanded: (id: string) => boolean;
  onToggle: () => void;
  onToggleSub: (id: string) => void;
  onRemoveMenu: (id: string) => void;
  isRemovingMenu: boolean;
}

const MenuRow: React.FC<MenuRowProps> = ({
  suiteId, moduleId, menu, isExpanded, isSubExpanded,
  onToggle, onToggleSub, onRemoveMenu, isRemovingMenu,
}) => {
  const [isAddingSub, setIsAddingSub] = useState(false);
  const [sub, setSub] = useState<AddSubState>(emptySub);

  const addSubMenuMutation    = useAddSubMenu(suiteId, moduleId, menu.id);
  const removeSubMenuMutation = useRemoveSubMenu(suiteId, moduleId, menu.id);

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

  return (
    <div className="space-y-1.5 animate-fadeIn">
      {/* Menu header */}
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
            <CompactAddButton label="Agregar Submenú" onClick={() => setIsAddingSub(true)} />
          )}
          <IconButton
            tooltip="Eliminar Menú"
            onClick={() => onRemoveMenu(menu.id)}
            disabled={isRemovingMenu}
            className="hover:text-m3-error hover:bg-m3-error/10"
          >
            <Trash2 className="w-3 h-3" />
          </IconButton>
        </div>
      </div>

      {/* SubMenus */}
      {isExpanded && (
        <div className="pl-6 space-y-2.5 border-l border-m3-outline/10 ml-4 pt-0.5">
          {/* Add submenu inline form */}
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
              <M3TextField label="Etiqueta" required value={sub.label} onChange={(e) => setSub(s => ({ ...s, label: e.target.value }))} placeholder="e.g. Usuarios" />
              <M3TextField label="Descripción" value={sub.desc} onChange={(e) => setSub(s => ({ ...s, desc: e.target.value }))} placeholder="Opcional" />
              <M3TextField label="Orden" type="number" value={sub.sort} onChange={(e) => setSub(s => ({ ...s, sort: e.target.value }))} placeholder="1" />
            </InlineAddForm>
          )}

          {!menu.subMenus || menu.subMenus.length === 0 ? (
            <p className="text-[10px] text-m3-secondary/50 italic">No hay submenús configurados.</p>
          ) : (
            menu.subMenus.map((subMenu) => (
              <SubMenuRow
                key={subMenu.id}
                suiteId={suiteId}
                moduleId={moduleId}
                menuId={menu.id}
                subMenu={subMenu}
                isExpanded={isSubExpanded(`submenu-${subMenu.id}`)}
                onToggle={() => onToggleSub(`submenu-${subMenu.id}`)}
                onRemoveSubMenu={(id) => removeSubMenuMutation.mutate(id)}
                isRemovingSubMenu={removeSubMenuMutation.isPending}
              />
            ))
          )}
        </div>
      )}
    </div>
  );
};

// ── Main panel ────────────────────────────────────────────────────────────────

export const SystemSuiteDetailPanel: React.FC<SystemSuiteDetailPanelProps> = ({
  activeSystemSuite,
  isLoading,
  onSystemSuiteUpdate,
  onEditingChange,
}) => {
  const t = useI18n();
  const [activeTab, setActiveTab] = useState<SystemSuiteTab>('overview');
  const [expandedNodes, setExpandedNodes] = useState<Record<string, boolean>>({});

  // ── Child entity view controls ─────────────────────────────────────────────
  const [modulesViewMode, setModulesViewMode] = useState<'list' | 'thumbnail'>('list');
  const [modulesFilter, setModulesFilter] = useState('all');
  const [modulesSortBy, setModulesSortBy] = useState('name');
  const [modulesSortOrder, setModulesSortOrder] = useState<'asc' | 'desc'>('asc');

  const [actionsViewMode, setActionsViewMode] = useState<'list' | 'thumbnail'>('thumbnail');
  const [actionsFilter, setActionsFilter] = useState('all');
  const [actionsSortBy, setActionsSortBy] = useState('name');
  const [actionsSortOrder, setActionsSortOrder] = useState<'asc' | 'desc'>('asc');

  const toggleNode = (nodeId: string) => {
    setExpandedNodes((prev) => ({ ...prev, [nodeId]: !prev[nodeId] }));
  };

  const isExpanded = (nodeId: string) => expandedNodes[nodeId] !== false;

  // ── Collapse / expand all ──────────────────────────────────────────────────
  const allModuleNodeIds = useMemo(() => {
    if (!activeSystemSuite?.modules) return [];
    const ids: string[] = [];
    for (const mod of activeSystemSuite.modules) {
      ids.push(`module-${mod.id}`);
      for (const menuItem of mod.menus ?? []) {
        ids.push(`menu-${menuItem.id}`);
        for (const sub of menuItem.subMenus ?? []) {
          ids.push(`submenu-${sub.id}`);
        }
      }
    }
    return ids;
  }, [activeSystemSuite?.modules]);

  const anyCollapsed = allModuleNodeIds.some((id) => expandedNodes[id] === false);

  const handleToggleAll = () => {
    if (anyCollapsed) {
      // Expand all: reset to default (all nodes expanded by default)
      setExpandedNodes({});
    } else {
      // Collapse all: set every known node to false
      const collapsed: Record<string, boolean> = {};
      allModuleNodeIds.forEach((id) => { collapsed[id] = false; });
      setExpandedNodes(collapsed);
    }
  };

  const tabs: DetailTab<SystemSuiteTab>[] = [
    { key: 'overview', label: t.overview, icon: <Box className="w-4 h-4" /> },
    { key: 'modules',  label: t.modules,  icon: <Shield className="w-4 h-4" /> },
    { key: 'domain-resources', label: 'Recursos de Dominio', icon: <Database className="w-4 h-4" /> },
    { key: 'actions',  label: t.actions,  icon: <Key className="w-4 h-4" /> },
    { key: 'roles', label: t.roles, icon: <Users className="w-4 h-4" /> },
  ];

  const suiteId = activeSystemSuite?.systemSuiteId ?? '';

  // Module mutations
  const addModuleMutation        = useAddModule(suiteId);
  const removeModuleMutation     = useRemoveModule(suiteId);
  const activateModuleMutation   = useActivateModule(suiteId);
  const deactivateModuleMutation = useDeactivateModule(suiteId);
  const registerActionMutation   = useRegisterAction(suiteId);
  const removeActionMutation     = useRemoveAction(suiteId);

  // Module add form state
  const [isAddingModule, setIsAddingModule] = useState(false);
  const [modCode, setModCode] = useState('');
  const [modName, setModName] = useState('');
  const [modDesc, setModDesc] = useState('');
  const [modSort, setModSort] = useState('1');
  const [modError, setModError] = useState('');

  // Action add form state
  const [isAddingAction, setIsAddingAction] = useState(false);
  const [actCode, setActCode] = useState('');
  const [actName, setActName] = useState('');
  const [actError, setActError] = useState('');

  const handleAddModule = async (e: React.FormEvent) => {
    e.preventDefault();
    setModError('');
    if (!modCode.trim()) { setModError('Código requerido'); return; }
    if (!modName.trim()) { setModError('Nombre requerido'); return; }
    try {
      await addModuleMutation.mutateAsync({
        code: formatSystemCode(modCode),
        name: modName.trim(),
        description: modDesc.trim(),
        sortOrder: parseInt(modSort) || 1,
      });
      setModCode(''); setModName(''); setModDesc(''); setModSort('1');
      setIsAddingModule(false);
    } catch { /* handled by hook */ }
  };

  const handleRegisterAction = async (e: React.FormEvent) => {
    e.preventDefault();
    setActError('');
    if (!actCode.trim()) { setActError('Código requerido'); return; }
    if (!actName.trim()) { setActError('Nombre requerido'); return; }
    try {
      await registerActionMutation.mutateAsync({
        code: formatSystemCode(actCode),
        name: actName.trim(),
      });
      setActCode(''); setActName('');
      setIsAddingAction(false);
    } catch { /* handled by hook */ }
  };

  if (isLoading || !activeSystemSuite) {
    return (
      <DetailPanelShell
        isLoading={isLoading}
        isEmpty={!activeSystemSuite}
        tabs={[]}
        activeTab={activeTab}
        onTabChange={setActiveTab}
        header={
          <div className="flex items-center justify-center h-32">
            <p className="text-sm text-m3-secondary">{t.selectSystemSuiteToView || 'Select system suite'}</p>
          </div>
        }
      >
        <div />
      </DetailPanelShell>
    );
  }

  return (
    <DetailPanelShell
      isLoading={false}
      isEmpty={false}
      tabs={tabs}
      activeTab={activeTab}
      onTabChange={setActiveTab}
      entityKey={activeSystemSuite.systemSuiteId}
      header={
        <SystemSuiteProfileCard
          systemSuite={activeSystemSuite}
          onSystemSuiteUpdate={onSystemSuiteUpdate}
          onEditingChange={onEditingChange}
        />
      }
    >
      <div className="space-y-4">

        {/* ── Overview ── */}
        {activeTab === 'overview' && (
          <div>
            <h3 className="text-sm font-medium text-m3-on-surface mb-2">{t.systemSuiteDetails}</h3>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.code}</dt>
                <dd className="text-m3-on-surface font-mono text-xs">{activeSystemSuite.code}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.name}</dt>
                <dd className="text-m3-on-surface">{activeSystemSuite.name}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.description}</dt>
                <dd className="text-m3-on-surface">{activeSystemSuite.description || '-'}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.status}</dt>
                <dd className="text-m3-on-surface">{activeSystemSuite.status}</dd>
              </div>
            </dl>
          </div>
        )}

        {/* ── Modules ── */}
        {activeTab === 'modules' && (
          <div className="space-y-4">
            <ChildEntityToolbar
              viewMode={modulesViewMode}
              onViewModeChange={setModulesViewMode}
              filterOptions={[
                { label: 'Todos', value: 'all' },
                { label: 'Activos', value: 'Active' },
                { label: 'Inactivos', value: 'Inactive' },
              ]}
              activeFilter={modulesFilter}
              onFilterChange={setModulesFilter}
              sortOptions={[
                { label: 'Nombre', value: 'name' },
                { label: 'Código', value: 'code' },
                { label: 'Orden', value: 'sortOrder' },
              ]}
              sortBy={modulesSortBy}
              onSortByChange={setModulesSortBy}
              sortOrder={modulesSortOrder}
              onSortOrderToggle={() => setModulesSortOrder((o) => (o === 'asc' ? 'desc' : 'asc'))}
              itemCount={activeSystemSuite.modules?.length ?? 0}
              itemLabel="Módulo"
            />

            {/* Collapse / Expand all toggle */}
            {allModuleNodeIds.length > 0 && (
              <div className="flex justify-end">
                <button
                  type="button"
                  onClick={handleToggleAll}
                  className="flex items-center gap-1.5 text-[11px] font-medium text-m3-secondary hover:text-m3-on-surface transition-colors px-2.5 py-1 rounded-md hover:bg-m3-surface-variant/25 border border-transparent hover:border-m3-outline/20 select-none"
                  title={anyCollapsed ? 'Expandir todos los nodos' : 'Colapsar todos los nodos'}
                >
                  {anyCollapsed ? (
                    <><ChevronsUpDown className="w-3.5 h-3.5" /> Expandir Todo</>
                  ) : (
                    <><ChevronsDownUp className="w-3.5 h-3.5" /> Colapsar Todo</>
                  )}
                </button>
              </div>
            )}

            <InlineAddForm
              isOpen={isAddingModule}
              onToggle={(open) => { setIsAddingModule(open); if (!open) setModError(''); }}
              onSubmit={handleAddModule}
              addLabel="Agregar Módulo"
              title="Nuevo Módulo Estructural"
              cancelLabel={t.cancelEdit}
              submitLabel="Guardar Módulo"
              isLoading={addModuleMutation.isPending}
              triggerEmphasis="quiet"
              error={modError || undefined}
            >
              <M3TextField label="Código del Módulo" required value={modCode} onChange={(e) => setModCode(e.target.value)} placeholder="e.g. SEC" />
              <M3TextField label="Nombre del Módulo" required value={modName} onChange={(e) => setModName(e.target.value)} placeholder="e.g. Seguridad" />
              <M3TextField label="Descripción" value={modDesc} onChange={(e) => setModDesc(e.target.value)} placeholder="e.g. Módulo de administración de seguridad" />
              <M3TextField label="Orden" type="number" value={modSort} onChange={(e) => setModSort(e.target.value)} placeholder="1" />
            </InlineAddForm>

            {(() => {
              let filteredModules = activeSystemSuite.modules ?? [];
              if (modulesFilter !== 'all') {
                filteredModules = filteredModules.filter((m) => m.status === modulesFilter);
              }
              filteredModules = [...filteredModules].sort((a, b) => {
                let cmp = 0;
                if (modulesSortBy === 'name') cmp = a.name.localeCompare(b.name);
                else if (modulesSortBy === 'code') cmp = a.code.localeCompare(b.code);
                else if (modulesSortBy === 'sortOrder') cmp = (a.sortOrder ?? 0) - (b.sortOrder ?? 0);
                return modulesSortOrder === 'asc' ? cmp : -cmp;
              });

              if (!activeSystemSuite.modules || activeSystemSuite.modules.length === 0) {
                return (
                  <div className="flex flex-col items-center justify-center p-8 text-center border border-dashed border-m3-outline/25 rounded-xl bg-m3-surface-container/10 animate-fadeIn">
                    <Shield className="w-8 h-8 text-m3-secondary/50 mb-2" />
                    <p className="text-sm font-medium text-m3-on-surface">{t.noModulesConfigured}</p>
                  </div>
                );
              }

              if (filteredModules.length === 0) {
                return (
                  <div className="flex flex-col items-center justify-center p-6 text-center border border-dashed border-m3-outline/25 rounded-xl bg-m3-surface-container/10 animate-fadeIn">
                    <p className="text-sm font-medium text-m3-on-surface">No hay módulos que coincidan con el filtro</p>
                  </div>
                );
              }

              if (modulesViewMode === 'thumbnail') {
                return (
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 animate-fadeIn">
                    {filteredModules.map((module) => {
                      const moduleNodeId = `module-${module.id}`;
                      const moduleExpanded = isExpanded(moduleNodeId);
                      return (
                        <ModuleCard
                          key={module.id}
                          suiteId={suiteId}
                          module={module}
                          isExpanded={moduleExpanded}
                          isNodeExpanded={isExpanded}
                          onToggle={() => toggleNode(moduleNodeId)}
                          onToggleNode={toggleNode}
                          onDeactivate={() => deactivateModuleMutation.mutate(module.id)}
                          onActivate={() => activateModuleMutation.mutate(module.id)}
                          onRemove={() => removeModuleMutation.mutate(module.id)}
                          isDeactivating={deactivateModuleMutation.isPending}
                          isActivating={activateModuleMutation.isPending}
                          isRemoving={removeModuleMutation.isPending}
                        />
                      );
                    })}
                  </div>
                );
              }

              return (
                <div className="space-y-3 animate-fadeIn">
                  {filteredModules.map((module) => {
                    const moduleNodeId = `module-${module.id}`;
                    const moduleExpanded = isExpanded(moduleNodeId);
                    return (
                      <ModuleCard
                        key={module.id}
                        suiteId={suiteId}
                        module={module}
                        isExpanded={moduleExpanded}
                        isNodeExpanded={isExpanded}
                        onToggle={() => toggleNode(moduleNodeId)}
                        onToggleNode={toggleNode}
                        onDeactivate={() => deactivateModuleMutation.mutate(module.id)}
                        onActivate={() => activateModuleMutation.mutate(module.id)}
                        onRemove={() => removeModuleMutation.mutate(module.id)}
                        isDeactivating={deactivateModuleMutation.isPending}
                        isActivating={activateModuleMutation.isPending}
                        isRemoving={removeModuleMutation.isPending}
                      />
                    );
                  })}
                </div>
              );
            })()}
          </div>
        )}

        {/* ── Domain Resources ── */}
        {activeTab === 'domain-resources' && (
          <SystemSuiteDomainResourcesPanel systemSuite={activeSystemSuite} />
        )}

        {/* ── Actions ── */}
        {activeTab === 'actions' && (
          <div className="space-y-4">
            <ChildEntityToolbar
              viewMode={actionsViewMode}
              onViewModeChange={setActionsViewMode}
              filterOptions={[
                { label: 'Todas', value: 'all' },
              ]}
              activeFilter={actionsFilter}
              onFilterChange={setActionsFilter}
              sortOptions={[
                { label: 'Nombre', value: 'name' },
                { label: 'Código', value: 'code' },
              ]}
              sortBy={actionsSortBy}
              onSortByChange={setActionsSortBy}
              sortOrder={actionsSortOrder}
              onSortOrderToggle={() => setActionsSortOrder((o) => (o === 'asc' ? 'desc' : 'asc'))}
              itemCount={activeSystemSuite.actions?.length ?? 0}
              itemLabel="Acción"
            />

            <InlineAddForm
              isOpen={isAddingAction}
              onToggle={(open) => { setIsAddingAction(open); if (!open) setActError(''); }}
              onSubmit={handleRegisterAction}
              addLabel="Registrar Acción"
              title="Nueva Acción del Sistema"
              cancelLabel={t.cancelEdit}
              submitLabel="Guardar Acción"
              isLoading={registerActionMutation.isPending}
              triggerEmphasis="quiet"
              error={actError || undefined}
            >
              <M3TextField label="Código de Acción" required value={actCode} onChange={(e) => setActCode(e.target.value)} placeholder="e.g. INVENTORY_DELETE" />
              <M3TextField label="Nombre de la Acción" required value={actName} onChange={(e) => setActName(e.target.value)} placeholder="e.g. Eliminar Inventario" />
            </InlineAddForm>

            {(() => {
              let filteredActions = activeSystemSuite.actions ?? [];
              if (actionsSortBy === 'name') {
                filteredActions = [...filteredActions].sort((a, b) => {
                  const cmp = a.name.localeCompare(b.name);
                  return actionsSortOrder === 'asc' ? cmp : -cmp;
                });
              } else if (actionsSortBy === 'code') {
                filteredActions = [...filteredActions].sort((a, b) => {
                  const cmp = a.code.localeCompare(b.code);
                  return actionsSortOrder === 'asc' ? cmp : -cmp;
                });
              }

              if (!activeSystemSuite.actions || activeSystemSuite.actions.length === 0) {
                return (
                  <div className="flex flex-col items-center justify-center p-8 text-center border border-dashed border-m3-outline/25 rounded-xl bg-m3-surface-container/10 animate-fadeIn">
                    <Key className="w-8 h-8 text-m3-secondary/50 mb-2" />
                    <p className="text-sm font-medium text-m3-on-surface">{t.noActionsConfigured}</p>
                  </div>
                );
              }

              if (actionsViewMode === 'list') {
                return (
                  <div className="flex flex-col gap-1 animate-fadeIn">
                    {filteredActions.map((action) => (
                      <div
                        key={action.id}
                        className="group/action flex items-center justify-between p-2.5 rounded-lg border border-m3-outline/10 bg-m3-surface-container/5 hover:bg-m3-surface-container/10 hover:border-m3-outline/25 transition-all duration-150"
                      >
                        <div className="flex items-center gap-3 flex-1 min-w-0">
                          <div className="p-1.5 rounded bg-m3-primary/10 text-m3-primary">
                            <Key className="w-3.5 h-3.5" />
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="text-xs font-semibold text-m3-on-surface truncate" title={action.name}>{action.name}</p>
                            <p className="text-[10px] font-mono text-m3-secondary truncate">{action.code}</p>
                          </div>
                        </div>
                        <IconButton
                          tooltip="Remover Acción"
                          onClick={() => removeActionMutation.mutate(action.code)}
                          disabled={removeActionMutation.isPending}
                          className="opacity-0 group-hover/action:opacity-100 transition-opacity hover:text-m3-error hover:bg-m3-error/10"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </IconButton>
                      </div>
                    ))}
                  </div>
                );
              }

              return (
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 animate-fadeIn">
                  {filteredActions.map((action) => (
                    <div
                      key={action.id}
                      className="group/action flex items-center justify-between p-3 rounded-lg border border-m3-outline/15 bg-m3-surface-container/5 hover:bg-m3-surface-container/10 hover:border-m3-outline/30 hover:-translate-y-[1px] transition-all duration-200 shadow-sm"
                    >
                      <div className="flex items-center gap-3 flex-1 min-w-0">
                        <div className="p-2 rounded bg-m3-primary/10 text-m3-primary">
                          <Key className="w-3.5 h-3.5" />
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-xs font-semibold text-m3-on-surface truncate" title={action.name}>{action.name}</p>
                          <p className="text-[10px] font-mono text-m3-secondary truncate">{action.code}</p>
                        </div>
                      </div>
                      <IconButton
                        tooltip="Remover Acción"
                        onClick={() => removeActionMutation.mutate(action.code)}
                        disabled={removeActionMutation.isPending}
                        className="opacity-0 group-hover/action:opacity-100 transition-opacity hover:text-m3-error hover:bg-m3-error/10"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </IconButton>
                    </div>
                  ))}
                </div>
              );
            })()}
          </div>
        )}

        {activeTab === 'roles' && (
          <SystemSuiteRolesPanel systemSuiteId={suiteId} />
        )}
      </div>
    </DetailPanelShell>
  );
};

// ── Module card (extracted to manage its own add-menu state) ─────────────────

type ModuleType = NonNullable<SystemSuite['modules']>[number];

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

const ModuleCard: React.FC<ModuleCardProps> = ({
  suiteId, module, isExpanded, isNodeExpanded, onToggle, onToggleNode,
  onDeactivate, onActivate, onRemove, isDeactivating, isActivating, isRemoving,
}) => {
  const [isAddingMenu, setIsAddingMenu] = useState(false);
  const [menu, setMenu] = useState<AddMenuState>(emptyMenu);

  const addMenuMutation    = useAddMenu(suiteId, module.id);
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
      {/* Module header */}
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
          <span className={`text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded-full ${module.status === 'Active' ? 'bg-emerald-500/10 text-emerald-500 border border-emerald-500/20' : 'bg-amber-500/10 text-amber-500 border border-amber-500/20'}`}>
            {module.status}
          </span>
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

      {/* Module body */}
      {isExpanded && (
        <div className="p-4 space-y-4">
          {module.description && (
            <p className="text-xs text-m3-secondary italic pl-1 border-l-2 border-m3-outline/20">{module.description}</p>
          )}

          {/* Add menu form */}
          <InlineAddForm
            isOpen={isAddingMenu}
            onToggle={(open) => { setIsAddingMenu(open); if (!open) setMenu(emptyMenu()); }}
            onSubmit={handleAddMenu}
            addLabel="Menú"
            title="Nuevo Menú"
            cancelLabel="Cancelar"
            submitLabel="Guardar Menú"
            isLoading={addMenuMutation.isPending}
            triggerEmphasis="quiet"
            error={menu.error || undefined}
          >
            <M3TextField label="Código" required value={menu.code} onChange={(e) => setMenu(s => ({ ...s, code: e.target.value }))} placeholder="e.g. ADMIN" />
            <M3TextField label="Etiqueta" required value={menu.label} onChange={(e) => setMenu(s => ({ ...s, label: e.target.value }))} placeholder="e.g. Administración" />
            <M3TextField label="Descripción" value={menu.desc} onChange={(e) => setMenu(s => ({ ...s, desc: e.target.value }))} placeholder="Opcional" />
            <M3TextField label="Orden" type="number" value={menu.sort} onChange={(e) => setMenu(s => ({ ...s, sort: e.target.value }))} placeholder="1" />
          </InlineAddForm>

          {/* Menus tree */}
          {!module.menus || module.menus.length === 0 ? (
            <p className="text-xs text-m3-secondary/70 italic pl-4">No hay menús configurados.</p>
          ) : (
            <div className="relative pl-2 border-l border-m3-outline/15 space-y-3 ml-2">
              {module.menus.map((menuItem) => (
                <MenuRow
                  key={menuItem.id}
                  suiteId={suiteId}
                  moduleId={module.id}
                  menu={menuItem}
                  isExpanded={isNodeExpanded(`menu-${menuItem.id}`)}
                  isSubExpanded={isNodeExpanded}
                  onToggle={() => onToggleNode(`menu-${menuItem.id}`)}
                  onToggleSub={onToggleNode}
                  onRemoveMenu={(id) => removeMenuMutation.mutate(id)}
                  isRemovingMenu={removeMenuMutation.isPending}
                />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};
