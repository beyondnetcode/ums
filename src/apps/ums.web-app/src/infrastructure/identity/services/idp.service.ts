/**
 * idp.service.ts
 *
 * Infrastructure service for Identity Provider bounded context.
 * Queries use GraphQL via graphqlClient.
 * Commands/transactions use REST via httpClient.
 * All responses are validated at runtime with Zod before returning.
 */
import { httpClient } from '@infra/http/httpClient';
import { graphqlIdpQueries } from '@infra/identity/queries/idp.graphql';
import {
  IdentityProviderListSchema,
  type IdentityProvider,
  type RegisterIdentityProviderPayload,
} from '@domain/identity/schemas/identity-provider.schema';

export const idpService = {
  // ── Queries (GraphQL) ─────────────────────────────────────────────────────

  getByIdentityProviders: async (tenantId: string): Promise<IdentityProvider[]> => {
    const response = await graphqlIdpQueries.getIdentityProviders(tenantId);
    return IdentityProviderListSchema.parse(response.getTenantIdentityProviders);
  },

  // ── Commands (REST) ───────────────────────────────────────────────────────

  registerIdentityProvider: async (
    tenantId: string,
    payload: RegisterIdentityProviderPayload,
  ): Promise<{ identityProviderId: string }> => {
    const response = await httpClient.post<{ identityProviderId: string }>(
      `/tenants/${tenantId}/identity-providers`,
      payload,
    );
    return response;
  },

  activateIdentityProvider: async (tenantId: string, idpId: string): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/identity-providers/${idpId}/activate`);
  },

  deactivateIdentityProvider: async (tenantId: string, idpId: string): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/identity-providers/${idpId}/deactivate`);
  },

  removeIdentityProvider: async (tenantId: string, idpId: string): Promise<void> => {
    await httpClient.delete(`/tenants/${tenantId}/identity-providers/${idpId}`);
  },
};
