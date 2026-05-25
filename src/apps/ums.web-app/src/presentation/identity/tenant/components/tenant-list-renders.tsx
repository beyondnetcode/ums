import React from 'react';
import { Building2, ArrowRight, Check, ChevronRight } from 'lucide-react';
import { Tenant } from '@domain/identity/models/tenant.model';
import { M3Card } from '@shared/components/M3Card';
import { StatusBadge } from '@shared/components/StatusBadge';
import { HierarchicalRow, HierarchicalExpandButton } from '@shared/components/HierarchicalList';
import type { TreeNode } from '@app/hooks/use-tree-nodes';

export const renderTenantParentRow = (
  node: TreeNode<Tenant>,
  isSelected: boolean,
  isExpanded: boolean,
  onToggle: () => void,
  onSelectTenant: (id: string) => void,
  statusLabel: (status: string) => string,
  t: Record<string, string>
) => {
  const id = node.item.tenantId;
  const tenant = node.item;
  const hasChildren = node.children.length > 0;

  return (
    <HierarchicalRow
      key={id}
      hasChildren={hasChildren}
      isExpanded={isExpanded}
      isChild={false}
      onToggleExpand={onToggle}
      onClick={() => onSelectTenant(id)}
      isSelected={isSelected}
    >
      <td className="py-3.5 px-5">
        <div className="flex items-center gap-3">
          <HierarchicalExpandButton
            hasChildren={hasChildren}
            isExpanded={isExpanded}
            onClick={(e) => { e.stopPropagation(); onToggle(); }}
          />
          <div className={`p-2 rounded-lg border transition-colors ${
            isSelected
              ? 'bg-m3-primary text-white border-m3-primary'
              : 'bg-m3-surface-container/60 border-m3-outline/20 text-m3-secondary group-hover:text-m3-primary group-hover:border-m3-primary/30'
          }`}>
            <Building2 className="w-4 h-4" />
          </div>
          <div>
            <p className="font-medium text-m3-on-surface">{tenant.name}</p>
            <p className="text-xs text-m3-secondary/60 truncate max-w-[170px] md:max-w-xs">{tenant.companyReference || tenant.type}</p>
          </div>
        </div>
      </td>
      <td className="py-3.5 px-4 font-mono text-xs font-medium text-m3-on-surface">{tenant.code}</td>
      <td className="py-3.5 px-4 text-xs">{tenant.type}</td>
      <td className="py-3.5 px-4">
        <StatusBadge status={tenant.status} label={statusLabel(tenant.status)} />
      </td>
      <td className="py-3.5 px-5 text-right">
        <div className="flex items-center justify-end gap-1.5">
          {isSelected && (
            <span className="h-5 w-5 bg-m3-primary text-m3-on-primary rounded-full flex items-center justify-center">
              <Check className="w-3 h-3" />
            </span>
          )}
          <span className="text-xs font-medium text-m3-primary opacity-0 group-hover:opacity-100 group-hover:translate-x-0.5 transition-all flex items-center gap-1">
            {t.manage} <ArrowRight className="w-3.5 h-3.5" />
          </span>
        </div>
      </td>
    </HierarchicalRow>
  );
};

export const renderTenantChildRow = (
  child: Tenant,
  isChildSelected: boolean,
  onSelectTenant: (id: string) => void,
  statusLabel: (status: string) => string,
  t: Record<string, string>
) => {
  const hasChildren = false;
  return (
    <HierarchicalRow
      key={child.tenantId}
      hasChildren={hasChildren}
      isExpanded={false}
      isChild={true}
      onToggleExpand={() => {}}
      onClick={() => onSelectTenant(child.tenantId)}
      isSelected={isChildSelected}
    >
      <td className="py-3.5 pl-10">
        <div className="flex items-center gap-3">
          <span className="w-4" />
          <div className="p-2 rounded-lg border bg-m3-surface-container/40 border-m3-outline/15 text-m3-secondary/70">
            <Building2 className="w-4 h-4" />
          </div>
          <div>
            <p className="text-xs font-medium text-m3-on-surface">{child.name}</p>
            <p className="text-[10px] text-m3-secondary/60 truncate max-w-[170px] md:max-w-xs">{child.companyReference || child.type}</p>
          </div>
        </div>
      </td>
      <td className="py-3.5 px-4 font-mono text-[10px] font-medium text-m3-on-surface opacity-70">{child.code}</td>
      <td className="py-3.5 px-4 text-[10px] opacity-70">{child.type}</td>
      <td className="py-3.5 px-4">
        <StatusBadge status={child.status} label={statusLabel(child.status)} />
      </td>
      <td className="py-3.5 px-5 text-right">
        <div className="flex items-center justify-end gap-1.5">
          {isChildSelected && (
            <span className="h-5 w-5 bg-m3-primary text-m3-on-primary rounded-full flex items-center justify-center">
              <Check className="w-3 h-3" />
            </span>
          )}
          <span className="text-xs font-medium text-m3-primary opacity-0 group-hover:opacity-100 group-hover:translate-x-0.5 transition-all flex items-center gap-1">
            {t.manage} <ArrowRight className="w-3.5 h-3.5" />
          </span>
        </div>
      </td>
    </HierarchicalRow>
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
        isSelected ? 'border-m3-primary bg-m3-primary-container/15' : 'border-m3-outline/25 hover:border-m3-primary/30'
      }`}
    >
      <div className="flex justify-between items-start gap-4">
        <div className="flex gap-3 flex-1">
          <div className={`p-2.5 rounded-lg border ${isSelected ? 'bg-m3-primary text-white border-m3-primary' : 'bg-m3-primary/10 text-m3-primary border-m3-primary/10'}`}>
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
              onClick={(e) => { e.stopPropagation(); onToggle(); }}
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
          <p className="font-medium text-m3-on-surface truncate mt-0.5">{tenant.companyReference || '-'}</p>
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
      isChildSelected ? 'border-m3-primary bg-m3-primary-container/10' : 'border-m3-outline/15 hover:border-m3-primary/20'
    }`}
  >
    <div className="flex justify-between items-start gap-3">
      <div className="flex gap-2.5">
        <div className={`p-2 rounded-lg border ${isChildSelected ? 'bg-m3-primary/80 text-white border-m3-primary/80' : 'bg-m3-surface-container/40 border-m3-outline/15 text-m3-secondary/70'}`}>
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
        <p className="font-medium text-m3-on-surface truncate mt-0.5">{child.companyReference || '-'}</p>
      </div>
    </div>
  </M3Card>
);
