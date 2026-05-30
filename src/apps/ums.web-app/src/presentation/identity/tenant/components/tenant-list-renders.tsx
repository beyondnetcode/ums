import React from 'react';
import { Building2, ArrowRight, ChevronRight } from 'lucide-react';
import { Tenant } from '@domain/identity/models/tenant.model';
import { M3Card } from '@shared/components/M3Card';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';
import { EntityRow } from '@shared/components/EntityRow';
import type { TreeNode } from '@app/hooks/use-tree-nodes';

export const renderTenantParentRow = (
  node: TreeNode<Tenant>,
  isSelected: boolean,
  isExpanded: boolean,
  onToggle: () => void,
  onSelectTenant: (id: string) => void,
  statusLabel: (status: string) => string
) => {
  const id = node.item.tenantId;
  const tenant = node.item;
  const hasChildren = node.children.length > 0;

  return (
    <EntityRow
      key={id}
      id={id}
      isActive={tenant.status === 'Active'}
      selected={isSelected}
      onClick={() => onSelectTenant(id)}
      leading={
        <div className="flex items-center gap-2">
          {hasChildren ? (
            <button
              onClick={e => {
                e.stopPropagation();
                onToggle();
              }}
              className={`p-0.5 rounded transition-transform duration-200 ${isExpanded ? 'rotate-90' : ''}`}
            >
              <ChevronRight className="w-3.5 h-3.5 text-m3-secondary" />
            </button>
          ) : (
            <span className="w-4" />
          )}
          <div
            className={`p-2 rounded-lg transition-colors ${isSelected ? 'bg-m3-primary/15' : 'bg-m3-surface-container/50'}`}
          >
            <Building2
              className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`}
            />
          </div>
        </div>
      }
      trailingColumns={[
        { content: <CodeBadge code={tenant.code} />, width: 'w-20' },
        { content: <CodeBadge code={tenant.type} />, width: 'w-20' },
        {
          content: <StatusBadge status={tenant.status} label={statusLabel(tenant.status)} />,
          width: 'w-20',
        },
        {
          content: (
            <ArrowRight
              className={`w-4 h-4 transition-transform ${isSelected ? 'text-m3-primary translate-x-0.5' : 'text-m3-outline/30'}`}
            />
          ),
          width: 'w-5',
        },
      ]}
    >
      <span className="text-sm font-semibold text-m3-on-surface line-clamp-1">{tenant.name}</span>
      {tenant.companyReference && (
        <span className="text-[10px] text-m3-secondary/60 line-clamp-1 mt-0.5">
          {tenant.companyReference}
        </span>
      )}
    </EntityRow>
  );
};

export const renderTenantChildRow = (
  child: Tenant,
  isChildSelected: boolean,
  onSelectTenant: (id: string) => void,
  statusLabel: (status: string) => string
) => {
  return (
    <EntityRow
      key={child.tenantId}
      id={child.tenantId}
      isActive={child.status === 'Active'}
      selected={isChildSelected}
      onClick={() => onSelectTenant(child.tenantId)}
      className="ml-6 border-l-2 border-m3-outline/15 rounded-l-none"
      leading={
        <div className="flex items-center gap-2">
          <span className="w-4" />
          <div className="p-2 rounded-lg bg-m3-surface-container/40 border border-m3-outline/15 text-m3-secondary/70">
            <Building2 className="w-4 h-4" />
          </div>
        </div>
      }
      trailingColumns={[
        { content: <CodeBadge code={child.code} />, width: 'w-20' },
        { content: <CodeBadge code={child.type} />, width: 'w-20' },
        {
          content: <StatusBadge status={child.status} label={statusLabel(child.status)} />,
          width: 'w-20',
        },
        {
          content: (
            <ArrowRight
              className={`w-4 h-4 transition-transform ${isChildSelected ? 'text-m3-primary translate-x-0.5' : 'text-m3-outline/30'}`}
            />
          ),
          width: 'w-5',
        },
      ]}
    >
      <span className="text-sm font-medium text-m3-on-surface line-clamp-1">{child.name}</span>
      {child.companyReference && (
        <span className="text-xs text-m3-secondary/60 line-clamp-1 mt-0.5">
          {child.companyReference}
        </span>
      )}
    </EntityRow>
  );
};

export const renderTenantParentCard = (
  node: TreeNode<Tenant>,
  isSelected: boolean,
  _isExpanded: boolean,
  onToggle: () => void,
  onSelectTenant: (id: string) => void,
  statusLabel: (status: string) => string,
  t: Record<string, string>
) => {
  const id = node.item.tenantId;
  const tenant = node.item;
  const children = node.children;
  const hasChildren = children.length > 0;

  return (
    <M3Card
      key={id}
      onClick={() => onSelectTenant(id)}
      variant={isSelected ? 'elevated' : 'filled'}
      className={`p-5 cursor-pointer border transition-all duration-150 hover:-translate-y-0.5 hover:shadow-md ${
        isSelected
          ? 'border-m3-primary bg-m3-primary-container/15'
          : 'border-m3-outline/25 hover:border-m3-primary/30'
      }`}
    >
      <div className="flex justify-between items-start gap-4">
        <div className="flex gap-3 flex-1">
          <div
            className={`p-2.5 rounded-lg border ${isSelected ? 'bg-m3-primary text-white border-m3-primary' : 'bg-m3-primary/10 text-m3-primary border-m3-primary/10'}`}
          >
            <Building2 className="w-5 h-5" />
          </div>
          <div className="flex-1 min-w-0">
            <h4 className="text-sm font-medium text-m3-on-surface line-clamp-1">{tenant.name}</h4>
            <p className="font-mono text-xs text-m3-secondary/70 mt-0.5">{tenant.code}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <StatusBadge status={tenant.status} label={statusLabel(tenant.status)} />
          {hasChildren && (
            <button
              onClick={e => {
                e.stopPropagation();
                onToggle();
              }}
              className="p-1 rounded transition-transform duration-200"
            >
              <ChevronRight className="w-4 h-4 text-m3-secondary" />
            </button>
          )}
        </div>
      </div>
      <div className="mt-4 pt-3 border-t border-m3-outline/10 grid grid-cols-2 gap-2 text-xs">
        <div>
          <p className="text-m3-secondary font-medium">{t.colCategory}</p>
          <p className="font-medium text-m3-on-surface mt-0.5">{tenant.type}</p>
        </div>
        <div>
          <p className="text-m3-secondary font-medium">{t.ref}</p>
          <p className="font-medium text-m3-on-surface truncate mt-0.5">
            {tenant.companyReference || '-'}
          </p>
        </div>
      </div>
      {hasChildren && (
        <div className="mt-2 text-[10px] text-m3-secondary/60">
          {children.length} {children.length === 1 ? t.tenant : t.tenants}
        </div>
      )}
    </M3Card>
  );
};

export const renderTenantChildCard = (
  child: Tenant,
  isChildSelected: boolean,
  onSelectTenant: (id: string) => void,
  statusLabel: (status: string) => string,
  t: Record<string, string>
) => (
  <M3Card
    key={child.tenantId}
    onClick={() => onSelectTenant(child.tenantId)}
    variant={isChildSelected ? 'elevated' : 'outlined'}
    className={`p-4 cursor-pointer border transition-all duration-150 ${
      isChildSelected
        ? 'border-m3-primary bg-m3-primary-container/10'
        : 'border-m3-outline/15 hover:border-m3-primary/20'
    }`}
  >
    <div className="flex justify-between items-start gap-3">
      <div className="flex gap-2.5">
        <div
          className={`p-2 rounded-lg border ${isChildSelected ? 'bg-m3-primary/80 text-white border-m3-primary/80' : 'bg-m3-surface-container/40 border-m3-outline/15 text-m3-secondary/70'}`}
        >
          <Building2 className="w-4 h-4" />
        </div>
        <div>
          <h4 className="text-xs font-medium text-m3-on-surface">{child.name}</h4>
          <p className="font-mono text-[10px] text-m3-secondary/60 mt-0.5">{child.code}</p>
        </div>
      </div>
      <StatusBadge status={child.status} label={statusLabel(child.status)} />
    </div>
    <div className="mt-3 pt-2 border-t border-m3-outline/10 grid grid-cols-2 gap-2 text-[10px]">
      <div>
        <p className="text-m3-secondary font-medium">{t.colCategory}</p>
        <p className="font-medium text-m3-on-surface mt-0.5">{child.type}</p>
      </div>
      <div>
        <p className="text-m3-secondary font-medium">{t.ref}</p>
        <p className="font-medium text-m3-on-surface truncate mt-0.5">
          {child.companyReference || '-'}
        </p>
      </div>
    </div>
  </M3Card>
);
