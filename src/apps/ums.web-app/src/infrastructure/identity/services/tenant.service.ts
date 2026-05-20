import axios from 'axios';
import { useAuthStore } from '../../../application/stores/auth.store';
import { Tenant, CreateTenantPayload, CreateTenantResponse } from '../../../domain/identity/models/tenant.model';
import { Branch, AddBranchPayload, AddBranchResponse } from '../../../domain/identity/models/branch.model';

const API_BASE_URL = '/api/v1';

// Create a configured axios client
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Dynamic dev headers injection
apiClient.interceptors.request.use((config) => {
  const { devUserId, devLanguage } = useAuthStore.getState();
  
  if (devUserId) {
    config.headers['X-User-Id'] = devUserId;
  }
  if (devLanguage) {
    config.headers['X-Language'] = devLanguage;
  }
  
  return config;
});

export const tenantService = {
  // Tenant CRUD & status
  getAllTenants: async (): Promise<Tenant[]> => {
    const response = await apiClient.get<Tenant[]>('/tenants');
    return response.data;
  },

  getTenantById: async (tenantId: string): Promise<Tenant> => {
    const response = await apiClient.get<Tenant>(`/tenants/${tenantId}`);
    return response.data;
  },

  createTenant: async (payload: CreateTenantPayload): Promise<CreateTenantResponse> => {
    const response = await apiClient.post<CreateTenantResponse>('/tenants', payload);
    return response.data;
  },

  activateTenant: async (tenantId: string): Promise<void> => {
    await apiClient.post(`/tenants/${tenantId}/activate`);
  },

  suspendTenant: async (tenantId: string): Promise<void> => {
    await apiClient.post(`/tenants/${tenantId}/suspend`);
  },

  // Branch operations
  getBranches: async (tenantId: string): Promise<Branch[]> => {
    const response = await apiClient.get<Branch[]>(`/tenants/${tenantId}/branches`);
    return response.data;
  },

  addBranch: async (tenantId: string, payload: AddBranchPayload): Promise<AddBranchResponse> => {
    const response = await apiClient.post<AddBranchResponse>(`/tenants/${tenantId}/branches`, payload);
    return response.data;
  },

  removeBranch: async (tenantId: string, branchId: string): Promise<void> => {
    await apiClient.delete(`/tenants/${tenantId}/branches/${branchId}`);
  },

  deactivateBranch: async (tenantId: string, branchId: string): Promise<void> => {
    await apiClient.post(`/tenants/${tenantId}/branches/${branchId}/deactivate`);
  },

  reactivateBranch: async (tenantId: string, branchId: string): Promise<void> => {
    await apiClient.post(`/tenants/${tenantId}/branches/${branchId}/reactivate`);
  },
};
export default tenantService;
