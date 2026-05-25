import React from 'react';
import { M3Select } from '@shared/components/M3Select';
import { Tenant } from '@domain/identity/models/tenant.model';

export interface TenantSelectorProps {
  tenants: Tenant[];
  selectedTenantId: string;
  onTenantChange: (tenantId: string) => void;
  label?: string;
  className?: string;
}

export const TenantSelector: React.FC<TenantSelectorProps> = ({
  tenants,
  selectedTenantId,
  onTenantChange,
  label,
  className
}) => {
  return (
    <M3Select
      label={label || 'Select Tenant'}
      value={selectedTenantId}
      onChange={(e) => onTenantChange(e.target.value)}
      className={className}
    >
      {tenants.map((tenant) => (
        <option key={tenant.tenantId} value={tenant.tenantId}>
          {tenant.name} ({tenant.code})
        </option>
      ))}
    </M3Select>
  );
};
