import axios from 'axios';
import { getApiHeaders } from '../config/api-header.provider';
import { useAuthStore } from '../../../application/stores/auth.store';
import { Tenant, CreateTenantPayload, CreateTenantResponse } from '../../../domain/identity/models/tenant.model';
import { Branch, AddBranchPayload, AddBranchResponse } from '../../../domain/identity/models/branch.model';
import {
  validateTenants,
  validateTenant,
  validateCreateTenantResponse,
  validateBranches,
  validateAddBranchResponse,
} from '../../../domain/identity/models/tenant.schema';

const API_BASE_URL = '/api/v1';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const headers = getApiHeaders();
  Object.assign(config.headers, headers);
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    if (status === 401) {
      useAuthStore.getState().logout();
    }
    return Promise.reject(error);
  },
);

export const tenantService = {
  getAllTenants: async (): Promise<Tenant[]> => {
    const response = await apiClient.get<Tenant[]>('/tenants');
    return validateTenants(response.data);
  },

  getTenantById: async (tenantId: string): Promise<Tenant> => {
    const response = await apiClient.get<Tenant>(`/tenants/${tenantId}`);
    return validateTenant(response.data);
  },

  createTenant: async (payload: CreateTenantPayload): Promise<CreateTenantResponse> => {
    const response = await apiClient.post<CreateTenantResponse>('/tenants', payload);
    return validateCreateTenantResponse(response.data);
  },

  activateTenant: async (tenantId: string): Promise<void> => {
    await apiClient.post(`/tenants/${tenantId}/activate`);
  },

  suspendTenant: async (tenantId: string): Promise<void> => {
    await apiClient.post(`/tenants/${tenantId}/suspend`);
  },

  getBranches: async (tenantId: string): Promise<Branch[]> => {
    const response = await apiClient.get<Branch[]>(`/tenants/${tenantId}/branches`);
    return validateBranches(response.data);
  },

  addBranch: async (tenantId: string, payload: AddBranchPayload): Promise<AddBranchResponse> => {
    const response = await apiClient.post<AddBranchResponse>(`/tenants/${tenantId}/branches`, payload);
    return validateAddBranchResponse(response.data);
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
