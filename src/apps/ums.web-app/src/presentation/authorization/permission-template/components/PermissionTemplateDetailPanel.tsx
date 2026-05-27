/**
 * PermissionTemplateDetailPanel
 *
 * Full CRUD for the Permission Template aggregate:
 *   Create  → via DashboardScreen (PermissionTemplateForm)
 *   Read    → overview tab (metadata) + items tab (effect tree)
 *   Update  → lifecycle transitions (Publish / Deprecate) + item mutations
 *   Delete  → trash button (Draft only) with inline confirmation
 */
import React, { useState, useCallback } from 'react';
import {
  ShieldCheck, SendHorizonal, Ban, Trash2,
  List, Info, AlertTriangle, Blocks
} from 'lucide-react';
import type {
  PermissionTemplateDetail, ExclusiveArcTarget,
} from '@domain/authorization/models/permission-template.model';
import { useGetSystemSuite } from '@app/authorization/hooks/use-system-suite';
import { DetailPanelShell, type DetailTab } from '@shared/components/DetailPanelShell';
import { StatusBadge } from '@shared/components/StatusBadge';
import { M3Button } from '@shared/components/M3Button';
import { KeyValueRow } from '@shared/components/KeyValueRow';
import {
  usePublishPermissionTemplate,
  useDeprecatePermissionTemplate,
  useDeletePermissionTemplate,
} from '@app/authorization/hooks/use-permission-template';

import { PermissionTree } from './tree/PermissionTree';
import { NodeDetailPanel } from './tree/NodeDetailPanel';
import { SystemActionsPanel } from './tree/SystemActionsPanel';
import type { UITreeNodeData } from './tree/TreeNode';

// ─── Types ────────────────────────────────────────────────────────────────────

type TemplateTab = 'overview' | 'tree' | 'actions';

interface Props {
  template:    PermissionTemplateDetail | undefined;
  isLoading:   boolean;
  onDeleted?:  () => void;   // called after a successful delete
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

const STATUS_LABEL: Record<string, string> = {
  Draft:      'Borrador',
  Published:  'Publicada',
  Deprecated: 'Descontinuada',
};

const STATUS_COLOR_MAP = {
  Published:  { bg: 'bg-emerald-500/10', border: 'border-emerald-500/25', text: 'text-emerald-500' },
  Deprecated: { bg: 'bg-rose-500/10',    border: 'border-rose-500/25',    text: 'text-rose-500' },
  Draft:      { bg: 'bg-amber-500/10',   border: 'border-amber-500/25',   text: 'text-amber-500' },
};

// ─── Main component ───────────────────────────────────────────────────────────

export const PermissionTemplateDetailPanel: React.FC<Props> = ({ template, isLoading, onDeleted }) => {
  const [activeTab, setActiveTab] = useState<TemplateTab>('overview');
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [selectedNode, setSelectedNode] = useState<UITreeNodeData | null>(null);

  const publish    = usePublishPermissionTemplate(template?.templateId ?? '');
  const deprecate  = useDeprecatePermissionTemplate(template?.templateId ?? '');
  const deleteTpl  = useDeletePermissionTemplate();

  const { data: suite, isLoading: loadingSuite } = useGetSystemSuite(template?.systemSuiteId ?? null);

  const isDraft     = template?.status === 'Draft';
  const isPublished = template?.status === 'Published';

  const tabs: DetailTab<TemplateTab>[] = [
    { key: 'overview', label: 'General',  icon: <Info className="w-3.5 h-3.5" /> },
    { key: 'tree',     label: 'Árbol de Permisos', icon: <List className="w-3.5 h-3.5" /> },
    { key: 'actions',  label: 'Acciones de Sistema', icon: <Blocks className="w-3.5 h-3.5" /> },
  ];

  const handleDelete = useCallback(async () => {
    if (!template) return;
    await deleteTpl.mutateAsync(template.templateId);
    setShowDeleteConfirm(false);
    onDeleted?.();
  }, [template, deleteTpl, onDeleted]);

  // ── Header ─────────────────────────────────────────────────────────────────

  const header = template ? (
    <div className="p-4 rounded-xl border border-m3-outline/20 bg-m3-surface-container/30 space-y-3">
      {/* Title + status */}
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center gap-2.5 min-w-0">
          <div className="p-2 rounded-lg bg-m3-primary/10 shrink-0">
            <ShieldCheck className="w-5 h-5 text-m3-primary" />
          </div>
          <div className="min-w-0">
            <p className="text-sm font-bold text-m3-on-surface leading-tight">
              Plantilla <span className="text-m3-primary">v{template.version}</span>
            </p>
            <p className="text-[11px] text-m3-secondary mt-0.5">
              {template.items.length} {template.items.length === 1 ? 'ítem' : 'ítems'}
            </p>
          </div>
        </div>
        <StatusBadge
          status={template.status}
          label={STATUS_LABEL[template.status] ?? template.status}
          colorMap={STATUS_COLOR_MAP}
        />
      </div>

      {/* Context chips */}
      <div className="flex flex-wrap gap-2">
        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-m3-surface-container/60 border border-m3-outline/20 text-[10px] text-m3-secondary">
          <span className="font-semibold text-[9px] uppercase tracking-wide">Rol</span>
          <span>{template.roleName}</span>
        </span>
        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-m3-surface-container/60 border border-m3-outline/20 text-[10px] text-m3-secondary">
          <span className="font-semibold text-[9px] uppercase tracking-wide">Suite</span>
          <span>{template.systemSuiteName}</span>
        </span>
      </div>

      {/* Lifecycle actions + delete */}
      <div className="flex items-center gap-2 flex-wrap pt-0.5">
        {isDraft && (
          <M3Button
            variant="tonal"
            icon={<SendHorizonal className="w-3.5 h-3.5" />}
            disabled={publish.isPending}
            onClick={() => publish.mutate()}
            className="text-xs h-8 px-3"
          >
            {publish.isPending ? 'Publicando…' : 'Publicar'}
          </M3Button>
        )}
        {isPublished && (
          <M3Button
            variant="outlined"
            icon={<Ban className="w-3.5 h-3.5" />}
            disabled={deprecate.isPending}
            onClick={() => deprecate.mutate()}
            className="text-xs h-8 px-3 border-rose-500/40 text-rose-500 hover:bg-rose-500/10"
          >
            {deprecate.isPending ? 'Descontinuando…' : 'Descontinuar'}
          </M3Button>
        )}

        {/* Spacer */}
        <span className="flex-1" />

        {/* Delete — Draft only */}
        {isDraft && !showDeleteConfirm && (
          <button
            type="button"
            onClick={() => setShowDeleteConfirm(true)}
            className="inline-flex items-center gap-1 text-[11px] text-m3-secondary/50 hover:text-rose-500 transition-colors px-2 py-1 rounded hover:bg-rose-500/10"
            title="Eliminar plantilla (solo Borradores)"
          >
            <Trash2 className="w-3.5 h-3.5" />
            Eliminar
          </button>
        )}

        {/* Delete confirmation inline */}
        {isDraft && showDeleteConfirm && (
          <div className="flex items-center gap-2 px-2 py-1 rounded-lg bg-rose-500/10 border border-rose-500/25 text-[11px]">
            <AlertTriangle className="w-3.5 h-3.5 text-rose-500 shrink-0" />
            <span className="text-rose-600 dark:text-rose-400 font-medium">¿Eliminar esta plantilla?</span>
            <button
              type="button"
              onClick={() => setShowDeleteConfirm(false)}
              className="text-m3-secondary hover:text-m3-on-surface px-1.5 py-0.5 rounded hover:bg-m3-surface-variant/20 transition-colors"
            >
              No
            </button>
            <button
              type="button"
              onClick={handleDelete}
              disabled={deleteTpl.isPending}
              className="text-rose-600 font-semibold px-1.5 py-0.5 rounded bg-rose-500/15 hover:bg-rose-500/25 transition-colors disabled:opacity-50"
            >
              {deleteTpl.isPending ? 'Eliminando…' : 'Sí, eliminar'}
            </button>
          </div>
        )}
      </div>
    </div>
  ) : undefined;

  // ── Tab content ─────────────────────────────────────────────────────────────

  const overviewContent = template ? (
    <dl className="space-y-0.5">
      <KeyValueRow label="Estado"   value={STATUS_LABEL[template.status] ?? template.status} />
      <KeyValueRow label="Versión"  value={`v${template.version}`} />
      <KeyValueRow label="Ítems"    value={String(template.items.length)} />
      <KeyValueRow label="Rol"      value={template.roleName} />
      <KeyValueRow label="Suite"    value={template.systemSuiteName} bordered={false} />
    </dl>
  ) : null;

  const treeContent = template ? (
    <div className="flex h-[500px] -mx-4 -mb-4 overflow-hidden rounded-b-xl border-t border-m3-outline/20">
      <div className="w-[45%] h-full">
        {loadingSuite ? (
          <div className="flex items-center justify-center h-full text-m3-secondary/50 text-xs">Cargando estructura...</div>
        ) : (
          <PermissionTree
            suite={suite}
            items={template.items}
            selectedNodeId={selectedNode?.id ?? null}
            onSelectNode={setSelectedNode}
          />
        )}
      </div>
      <div className="w-[55%] h-full bg-m3-surface-container/10 overflow-y-auto">
        <NodeDetailPanel 
          node={selectedNode} 
          suite={suite}
          templateId={template.templateId}
          isDraft={isDraft}
        />
      </div>
    </div>
  ) : null;

  const actionsContent = template ? (
    <div className="min-h-[300px]">
      <SystemActionsPanel
        suite={suite}
        items={template.items}
        templateId={template.templateId}
        isDraft={isDraft}
      />
    </div>
  ) : null;

  return (
    <DetailPanelShell<TemplateTab>
      isLoading={isLoading}
      isEmpty={!template}
      loadingLabel="Cargando plantilla…"
      emptyLabel="Seleccione una plantilla en el panel izquierdo para ver sus detalles."
      header={header}
      tabs={tabs}
      activeTab={activeTab}
      onTabChange={(tab) => { setActiveTab(tab); setShowDeleteConfirm(false); }}
      entityKey={template?.templateId}
    >
      {activeTab === 'overview' && overviewContent}
      {activeTab === 'tree' && treeContent}
      {activeTab === 'actions' && actionsContent}
    </DetailPanelShell>
  );
};
