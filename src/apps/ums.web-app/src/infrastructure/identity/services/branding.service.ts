/**
 * branding.service.ts
 *
 * Infrastructure service for Tenant Branding bounded context.
 * Queries and commands use REST API endpoints.
 */
import { httpClient } from '@infra/http/httpClient';
import {
  BrandingSchema,
  type Branding,
  type SetBrandingPayload,
  type UpdateBrandingPayload,
} from '@domain/identity/schemas/branding.schema';

export const brandingService = {
  getBranding: async (tenantId: string): Promise<Branding | null> => {
    try {
      const response = await httpClient.get<Branding>(
        `/tenants/${tenantId}/branding`,
      );
      return BrandingSchema.parse(response);
    } catch {
      return null;
    }
  },

  setBranding: async (tenantId: string, payload: SetBrandingPayload): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/branding`, payload);
  },

  updateBranding: async (tenantId: string, payload: UpdateBrandingPayload): Promise<void> => {
    await httpClient.put(`/tenants/${tenantId}/branding`, payload);
  },

  removeBranding: async (tenantId: string): Promise<void> => {
    await httpClient.delete(`/tenants/${tenantId}/branding`);
  },

  verifyDns: async (tenantId: string): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/branding/dns/verify`);
  },

  failDns: async (tenantId: string): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/branding/dns/fail`);
  },
};
