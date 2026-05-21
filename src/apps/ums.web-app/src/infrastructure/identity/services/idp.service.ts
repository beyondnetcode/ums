/**
 * idp.service.ts
 *
 * Infrastructure service for Identity Provider bounded context.
 * Queries and commands use REST API endpoints.
 */
import { httpClient } from '@infra/http/httpClient';
import {
  IdentityProviderListSchema,
  type IdentityProvider,
  type RegisterIdentityProviderPayload,
} from '@domain/identity/schemas/identity-provider.schema';

export const idpService = {
  getByIdentityProviders: async (tenantId: string): Promise<IdentityProvider[]> => {
    const response = await httpClient.get<IdentityProvider[]>(
      `/tenants/${tenantId}/identity-providers`,
    );
    return IdentityProviderListSchema.parse(response);
  },

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
