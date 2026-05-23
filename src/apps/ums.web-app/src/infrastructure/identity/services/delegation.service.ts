/**
 * delegation.service.ts
 *
 * Infrastructure service for the Identity / UserManagementDelegation bounded context.
 * Queries use GraphQL via graphqlDelegationQueries.
 * Commands/transactions use REST via httpClient.
 * All responses are validated at runtime with Zod before returning.
 */
import { httpClient } from '@infra/http/httpClient';
import { graphqlDelegationQueries } from '@infra/identity/queries/delegation.graphql';
import { logger } from '@app/utils/logger';
import {
  DelegationSchema,
  DelegationListSchema,
  CreateDelegationResponseSchema,
  type Delegation,
  type CreateDelegationPayload,
  type CreateDelegationResponse,
} from '@domain/identity/schemas/delegation.schema';

export const delegationService = {
  // ── Queries (GraphQL) ─────────────────────────────────────────────────────

  getDelegationById: async (delegationId: string): Promise<Delegation> => {
    const response = await graphqlDelegationQueries.getDelegationById(delegationId);
    if (!response.getDelegationById) throw new Error('Delegation not found');
    return DelegationSchema.parse(response.getDelegationById);
  },

  getDelegationsByDelegatedAdmin: async (delegatedAdminId: string, tenantId: string): Promise<Delegation[]> => {
    const response = await graphqlDelegationQueries.getDelegationsByDelegatedAdmin(delegatedAdminId, tenantId);
    const result = DelegationListSchema.safeParse(response.getDelegationsByDelegatedAdmin);
    if (!result.success) {
      logger.error('Invalid GraphQL response shape for delegationsByDelegatedAdmin query', result.error);
      throw new Error('Invalid GraphQL response shape for delegationsByDelegatedAdmin query');
    }
    return result.data;
  },

  getDelegationsByDelegatingAdmin: async (delegatingAdminId: string, tenantId: string): Promise<Delegation[]> => {
    const response = await graphqlDelegationQueries.getDelegationsByDelegatingAdmin(delegatingAdminId, tenantId);
    const result = DelegationListSchema.safeParse(response.getDelegationsByDelegatingAdmin);
    if (!result.success) {
      logger.error('Invalid GraphQL response shape for delegationsByDelegatingAdmin query', result.error);
      throw new Error('Invalid GraphQL response shape for delegationsByDelegatingAdmin query');
    }
    return result.data;
  },

  // ── Commands (REST) ───────────────────────────────────────────────────────

  createDelegation: async (payload: CreateDelegationPayload): Promise<CreateDelegationResponse> => {
    const { data } = await httpClient.post('/delegations', payload);
    return CreateDelegationResponseSchema.parse(data);
  },

  activateDelegation: async (delegationId: string): Promise<void> => {
    await httpClient.post(`/delegations/${delegationId}/activate`);
  },

  revokeDelegation: async (delegationId: string, reason: string): Promise<void> => {
    await httpClient.post(`/delegations/${delegationId}/revoke`, null, { params: { reason } });
  },

  expireDelegation: async (delegationId: string): Promise<void> => {
    await httpClient.post(`/delegations/${delegationId}/expire`);
  },
};

export default delegationService;
