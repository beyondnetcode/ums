export interface Branch {
  branchId: string;
  code: string;
  name: string;
  isActive: boolean;
  geofencingMetadata: string | null;
}

export interface AddBranchPayload {
  code: string;
  name: string;
  geofencingMetadata?: string;
}

export interface AddBranchResponse {
  branchId: string;
  tenantId: string;
  code: string;
}
