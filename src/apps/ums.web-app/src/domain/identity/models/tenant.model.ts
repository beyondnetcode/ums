export type TenantStatus = 'Active' | 'Suspended' | 'Pending';

export interface Tenant {
  tenantId: string;
  code: string;
  name: string;
  type: string;
  status: TenantStatus;
  parentTenantId: string | null;
  companyReference: string | null;
}

export interface CreateTenantPayload {
  code: string;
  name: string;
  type: string;
  companyReference?: string;
}

export interface CreateTenantResponse {
  tenantId: string;
  code: string;
  name: string;
}
