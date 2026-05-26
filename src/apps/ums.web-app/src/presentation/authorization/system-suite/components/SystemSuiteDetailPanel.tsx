import React, { useState } from 'react';
import { Box, Shield, Key, ChevronDown, ChevronRight, Folder, FolderOpen, Layers, Plus, EyeOff, ShieldCheck, Trash2, KeyRound } from 'lucide-react';
import { SystemSuite } from '@domain/authorization/models/system-suite.model';
import { SystemSuiteProfileCard } from './SystemSuiteProfileCard';
import { DetailPanelShell, DetailTab } from '@shared/components/DetailPanelShell';
import { useI18n } from '@app/i18n/use-i18n';
import {
  useAddModule,
  useRemoveModule,
  useActivateModule,
  useDeactivateModule,
  useRegisterAction,
  useRemoveAction
} from '@app/authorization/hooks/use-system-suite';
import { M3TextField } from '@shared/components/M3TextField';
import { InlineAddForm } from '@shared/components/InlineAddForm';
import { IconButton } from '@shared/components/Tooltip';

type SystemSuiteTab = 'overview' | 'modules' | 'actions';

interface SystemSuiteDetailPanelProps {
  activeSystemSuite: SystemSuite | undefined;
  isLoading: boolean;
  onSystemSuiteUpdate: (systemSuiteId: string, patch: Partial<SystemSuite>) => void;
  onEditingChange: (isEditing: boolean) => void;
}

export const SystemSuiteDetailPanel: React.FC<SystemSuiteDetailPanelProps> = ({
  activeSystemSuite,
  isLoading,
  onSystemSuiteUpdate,
  onEditingChange,
}) => {
  const t = useI18n();
  const [activeTab, setActiveTab] = useState<SystemSuiteTab>('overview');
  const [expandedNodes, setExpandedNodes] = useState<Record<string, boolean>>({});

  const toggleNode = (nodeId: string) => {
    setExpandedNodes((prev) => ({
      ...prev,
      [nodeId]: !prev[nodeId],
    }));
  };

  const isExpanded = (nodeId: string) => expandedNodes[nodeId] !== false; // expanded by default

  const tabs: DetailTab<SystemSuiteTab>[] = [
    { key: 'overview', label: t.overview, icon: <Box className="w-4 h-4" /> },
    { key: 'modules', label: t.modules, icon: <Shield className="w-4 h-4" /> },
    { key: 'actions', label: t.actions, icon: <Key className="w-4 h-4" /> },
  ];

  // Forms and Mutations
  const suiteId = activeSystemSuite?.systemSuiteId ?? '';

  const addModuleMutation = useAddModule(suiteId);
  const removeModuleMutation = useRemoveModule(suiteId);
  const activateModuleMutation = useActivateModule(suiteId);
  const deactivateModuleMutation = useDeactivateModule(suiteId);
  const registerActionMutation = useRegisterAction(suiteId);
  const removeActionMutation = useRemoveAction(suiteId);

  const [isAddingModule, setIsAddingModule] = useState(false);
  const [modCode, setModCode] = useState('');
  const [modName, setModName] = useState('');
  const [modDesc, setModDesc] = useState('');
  const [modSort, setModSort] = useState('1');
  const [modError, setModError] = useState('');

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
        code: modCode.toUpperCase().replace(/\s+/g, '_'),
        name: modName.trim(),
        description: modDesc.trim(),
        sortOrder: parseInt(modSort) || 1,
      });
      setModCode(''); setModName(''); setModDesc(''); setModSort('1');
      setIsAddingModule(false);
    } catch {
      // Handled by hook
    }
  };

  const handleRegisterAction = async (e: React.FormEvent) => {
    e.preventDefault();
    setActError('');
    if (!actCode.trim()) { setActError('Código requerido'); return; }
    if (!actName.trim()) { setActError('Nombre requerido'); return; }

    try {
      await registerActionMutation.mutateAsync({
        code: actCode.toUpperCase().replace(/\s+/g, '_'),
        name: actName.trim(),
      });
      setActCode(''); setActName('');
      setIsAddingAction(false);
    } catch {
      // Handled by hook
    }
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
        {activeTab === 'modules' && (
          <div className="space-y-4">
            <InlineAddForm
              isOpen={isAddingModule}
              onToggle={(open) => { setIsAddingModule(open); if (!open) setModError(''); }}
              onSubmit={handleAddModule}
              addLabel="Agregar Módulo"
              title="Nuevo Módulo Estructural"
              cancelLabel={t.cancelEdit}
              submitLabel="Guardar Módulo"
              isLoading={addModuleMutation.isPending}
              error={modError || undefined}
            >
              <M3TextField
                label="Código del Módulo"
                required
                value={modCode}
                onChange={(e) => setModCode(e.target.value)}
                placeholder="e.g. SEC"
              />
              <M3TextField
                label="Nombre del Módulo"
                required
                value={modName}
                onChange={(e) => setModName(e.target.value)}
                placeholder="e.g. Seguridad"
              />
              <M3TextField
                label="Descripción"
                value={modDesc}
                onChange={(e) => setModDesc(e.target.value)}
                placeholder="e.g. Módulo de administración de seguridad"
              />
              <M3TextField
                label="Orden"
                type="number"
                value={modSort}
                onChange={(e) => setModSort(e.target.value)}
                placeholder="1"
              />
            </InlineAddForm>

            <div className="space-y-3">
              {!activeSystemSuite.modules || activeSystemSuite.modules.length === 0 ? (
                <div className="flex flex-col items-center justify-center p-8 text-center border border-dashed border-m3-outline/25 rounded-xl bg-m3-surface-container/10 animate-fadeIn">
                  <Shield className="w-8 h-8 text-m3-secondary/50 mb-2" />
                  <p className="text-sm font-medium text-m3-on-surface">{t.noModulesConfigured}</p>
                </div>
              ) : (
                activeSystemSuite.modules.map((module) => {
                  const moduleId = `module-${module.id}`;
                  const moduleExpanded = isExpanded(moduleId);

                  return (
                    <div key={module.id} className="border border-m3-outline/20 rounded-lg overflow-hidden bg-m3-surface-container/5 transition-all hover:border-m3-outline/40 animate-fadeIn">
                      {/* Module Header */}
                      <div
                        className="flex items-center justify-between p-3.5 bg-m3-surface-container/10 select-none border-b border-m3-outline/10 hover:bg-m3-surface-container/20 transition-colors group/module"
                      >
                        <div
                          onClick={() => toggleNode(moduleId)}
                          className="flex items-center gap-2 cursor-pointer flex-1"
                        >
                          {moduleExpanded ? <ChevronDown className="w-4 h-4 text-m3-secondary" /> : <ChevronRight className="w-4 h-4 text-m3-secondary" />}
                          <Layers className="w-4 h-4 text-m3-primary" />
                          <div className="flex items-center gap-2">
                            <span className="font-semibold text-sm text-m3-on-surface">{module.name}</span>
                            <span className="text-[10px] font-mono px-1.5 py-0.5 rounded bg-m3-surface-variant text-m3-on-surface-variant border border-m3-outline/10">{module.code}</span>
                          </div>
                        </div>
                        <div className="flex items-center gap-3">
                          <span className={`text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded-full ${module.status === 'Active' ? 'bg-emerald-500/10 text-emerald-500 border border-emerald-500/20' : 'bg-amber-500/10 text-amber-500 border border-amber-500/20'}`}>
                            {module.status}
                          </span>
                          <span className="text-xs text-m3-secondary">Ord: {module.sortOrder}</span>

                          {/* Module Actions on Hover */}
                          <div className="flex items-center gap-0.5 opacity-0 group-hover/module:opacity-100 transition-opacity">
                            {module.status === 'Active' ? (
                              <IconButton
                                tooltip="Desactivar"
                                onClick={(e) => { e.stopPropagation(); deactivateModuleMutation.mutate(module.id); }}
                                disabled={deactivateModuleMutation.isPending}
                              >
                                <EyeOff className="w-3.5 h-3.5" />
                              </IconButton>
                            ) : (
                              <IconButton
                                tooltip="Activar"
                                onClick={(e) => { e.stopPropagation(); activateModuleMutation.mutate(module.id); }}
                                disabled={activateModuleMutation.isPending}
                              >
                                <ShieldCheck className="w-3.5 h-3.5" />
                              </IconButton>
                            )}
                            <IconButton
                              tooltip="Eliminar"
                              onClick={(e) => { e.stopPropagation(); removeModuleMutation.mutate(module.id); }}
                              disabled={removeModuleMutation.isPending}
                              className="hover:text-m3-error hover:bg-m3-error/10"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                            </IconButton>
                          </div>
                        </div>
                      </div>

                      {/* Module Description & Menus */}
                      {moduleExpanded && (
                        <div className="p-4 space-y-4">
                          {module.description && (
                            <p className="text-xs text-m3-secondary italic pl-1 border-l-2 border-m3-outline/20">{module.description}</p>
                          )}

                          {/* Menus List */}
                          {!module.menus || module.menus.length === 0 ? (
                            <p className="text-xs text-m3-secondary/70 italic pl-4">No hay menús configurados.</p>
                          ) : (
                            <div className="relative pl-2 border-l border-m3-outline/15 space-y-3 ml-2">
                              {module.menus.map((menu) => {
                                const menuId = `menu-${menu.id}`;
                                const menuExpanded = isExpanded(menuId);

                                return (
                                  <div key={menu.id} className="space-y-1.5 animate-fadeIn">
                                    {/* Menu Node */}
                                    <div
                                      onClick={() => toggleNode(menuId)}
                                      className="flex items-center gap-2 cursor-pointer py-1.5 px-2 rounded hover:bg-m3-surface-variant/20 transition-colors select-none"
                                    >
                                      {menuExpanded ? <ChevronDown className="w-3.5 h-3.5 text-m3-secondary" /> : <ChevronRight className="w-3.5 h-3.5 text-m3-secondary" />}
                                      {menuExpanded ? <FolderOpen className="w-4 h-4 text-amber-500" /> : <Folder className="w-4 h-4 text-amber-500" />}
                                      <div className="flex items-baseline gap-1.5">
                                        <span className="text-xs font-semibold text-m3-on-surface">{menu.label}</span>
                                        <span className="text-[9px] font-mono text-m3-secondary">({menu.code})</span>
                                      </div>
                                    </div>

                                    {/* SubMenus List */}
                                    {menuExpanded && (
                                      <div className="pl-6 space-y-2.5 border-l border-m3-outline/10 ml-4 pt-0.5">
                                        {!menu.subMenus || menu.subMenus.length === 0 ? (
                                          <p className="text-[10px] text-m3-secondary/50 italic">No submenus configured</p>
                                        ) : (
                                          menu.subMenus.map((subMenu) => {
                                            const subMenuId = `submenu-${subMenu.id}`;
                                            const subMenuExpanded = isExpanded(subMenuId);

                                            return (
                                              <div key={subMenu.id} className="space-y-1.5 animate-fadeIn">
                                                {/* SubMenu Node */}
                                                <div
                                                  onClick={() => toggleNode(subMenuId)}
                                                  className="flex items-center gap-2 cursor-pointer py-1 px-1.5 rounded hover:bg-m3-surface-variant/15 transition-colors select-none"
                                                >
                                                  {subMenuExpanded ? <ChevronDown className="w-3 h-3 text-m3-secondary/70" /> : <ChevronRight className="w-3 h-3 text-m3-secondary/70" />}
                                                  <FolderOpen className="w-3.5 h-3.5 text-amber-400/80" />
                                                  <div className="flex items-baseline gap-1.5">
                                                    <span className="text-[11px] font-semibold text-m3-on-surface/90">{subMenu.label}</span>
                                                    <span className="text-[8px] font-mono text-m3-secondary/70">({subMenu.code})</span>
                                                  </div>
                                                </div>

                                                {/* Options List */}
                                                {subMenuExpanded && (
                                                  <div className="pl-5 space-y-1.5 border-l border-m3-outline/10 ml-3 pt-0.5">
                                                    {!subMenu.options || subMenu.options.length === 0 ? (
                                                      <p className="text-[9px] text-m3-secondary/40 italic">No options configured</p>
                                                    ) : (
                                                      <div className="grid grid-cols-1 gap-1.5">
                                                        {subMenu.options.map((opt) => (
                                                          <div key={opt.id} className="flex items-start justify-between p-2 rounded-lg bg-m3-surface-container/20 border border-m3-outline/10 hover:border-m3-outline/25 transition-all hover:translate-x-[1px] duration-150 animate-fadeIn">
                                                            <div className="flex-1 min-w-0 pr-2">
                                                              <div className="flex items-center gap-1.5 flex-wrap">
                                                                <span className="text-[10px] font-bold text-m3-on-surface">{opt.label}</span>
                                                                <span className="text-[8px] font-mono text-m3-secondary">({opt.code})</span>
                                                              </div>
                                                              <p className="text-[9px] text-m3-secondary mt-0.5 truncate max-w-full" title={opt.description}>
                                                                {opt.description}
                                                              </p>
                                                            </div>
                                                            <div className="flex items-center gap-1.5 flex-shrink-0">
                                                              <span className="text-[7px] font-bold uppercase tracking-wider px-1 py-0.5 rounded bg-m3-primary/10 text-m3-primary border border-m3-primary/20 flex items-center gap-0.5">
                                                                <KeyRound className="w-2 h-2" />
                                                                {opt.actionCode}
                                                              </span>
                                                            </div>
                                                          </div>
                                                        ))}
                                                      </div>
                                                    )}
                                                  </div>
                                                )}
                                              </div>
                                            );
                                          })
                                        )}
                                      </div>
                                    )}
                                  </div>
                                );
                              })}
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  );
                })
              )}
            </div>
          </div>
        )}
        {activeTab === 'actions' && (
          <div className="space-y-4">
            <InlineAddForm
              isOpen={isAddingAction}
              onToggle={(open) => { setIsAddingAction(open); if (!open) setActError(''); }}
              onSubmit={handleRegisterAction}
              addLabel="Registrar Acción"
              title="Nueva Acción del Sistema"
              cancelLabel={t.cancelEdit}
              submitLabel="Guardar Acción"
              isLoading={registerActionMutation.isPending}
              error={actError || undefined}
            >
              <M3TextField
                label="Código de Acción"
                required
                value={actCode}
                onChange={(e) => setActCode(e.target.value)}
                placeholder="e.g. INVENTORY_DELETE"
              />
              <M3TextField
                label="Nombre de la Acción"
                required
                value={actName}
                onChange={(e) => setActName(e.target.value)}
                placeholder="e.g. Eliminar Inventario"
              />
            </InlineAddForm>

            {!activeSystemSuite.actions || activeSystemSuite.actions.length === 0 ? (
              <div className="flex flex-col items-center justify-center p-8 text-center border border-dashed border-m3-outline/25 rounded-xl bg-m3-surface-container/10 animate-fadeIn">
                <Key className="w-8 h-8 text-m3-secondary/50 mb-2" />
                <p className="text-sm font-medium text-m3-on-surface">{t.noActionsConfigured}</p>
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 animate-fadeIn">
                {activeSystemSuite.actions.map((action) => (
                  <div
                    key={action.id}
                    className="group/action flex items-center justify-between p-3 rounded-lg border border-m3-outline/15 bg-m3-surface-container/5 hover:bg-m3-surface-container/10 hover:border-m3-outline/30 hover:-translate-y-[1px] transition-all duration-200 shadow-sm"
                  >
                    <div className="flex items-center gap-3 flex-1 min-w-0">
                      <div className="p-2 rounded bg-m3-primary/10 text-m3-primary">
                        <Key className="w-3.5 h-3.5" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-xs font-semibold text-m3-on-surface truncate" title={action.name}>
                          {action.name}
                        </p>
                        <p className="text-[10px] font-mono text-m3-secondary truncate">
                          {action.code}
                        </p>
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
            )}
          </div>
        )}
      </div>
    </DetailPanelShell>
  );
};
