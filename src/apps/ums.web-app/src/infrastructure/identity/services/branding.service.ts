/**
 * branding.service.ts
 *
 * Infrastructure service for Tenant Branding bounded context.
 * Queries use GraphQL via graphqlClient.
 * Commands/transactions use REST via httpClient.
 * All responses are validated at runtime with Zod before returning.
 */
import { httpClient } from '@infra/http/httpClient';
import { graphqlBrandingQueries } from '@infra/identity/queries/branding.graphql';
import {
  BrandingSchema,
  type Branding,
  type SetBrandingPayload,
  type UpdateBrandingPayload,
} from '@domain/identity/schemas/branding.schema';

export const brandingService = {
  // ── Queries (GraphQL) ─────────────────────────────────────────────────────

  getBranding: async (tenantId: string): Promise<Branding | null> => {
    const response = await graphqlBrandingQueries.getBranding(tenantId);
    if (!response.tenantBranding) return null;
    return BrandingSchema.parse(response.tenantBranding);
  },

  // ── Commands (REST) ───────────────────────────────────────────────────────

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
