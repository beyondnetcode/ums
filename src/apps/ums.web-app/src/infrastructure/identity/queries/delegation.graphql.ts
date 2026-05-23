/**
 * delegation.graphql.ts — GraphQL query definitions for the UserManagementDelegation bounded context
 *
 * All read operations use these GraphQL queries.
 * Commands/transactions use REST endpoints via delegation.service.ts.
 */
import { graphqlClient, GraphQlValidationError } from '@infra/http/graphqlClient';

const GET_DELEGATION_BY_ID =
  'query DelegationById($delegationId: UUID!) { getDelegationById: delegationById(delegationId: $delegationId) { delegationId tenantId delegatingAdminId delegatedAdminId scopeType scopeId allowedActions validFrom validUntil maxDurationDays requiresApproval approvalRequestId status revokedAt revokedBy revocationReason } }';

const GET_DELEGATIONS_BY_DELEGATED_ADMIN =
  'query DelegationsByDelegatedAdmin($delegatedAdminId: UUID!, $tenantId: UUID!) { getDelegationsByDelegatedAdmin: delegationsByDelegatedAdmin(delegatedAdminId: $delegatedAdminId, tenantId: $tenantId) { delegationId tenantId delegatingAdminId delegatedAdminId scopeType scopeId allowedActions validFrom validUntil maxDurationDays requiresApproval approvalRequestId status revokedAt revokedBy revocationReason } }';

const GET_DELEGATIONS_BY_DELEGATING_ADMIN =
  'query DelegationsByDelegatingAdmin($delegatingAdminId: UUID!, $tenantId: UUID!) { getDelegationsByDelegatingAdmin: delegationsByDelegatingAdmin(delegatingAdminId: $delegatingAdminId, tenantId: $tenantId) { delegationId tenantId delegatingAdminId delegatedAdminId scopeType scopeId allowedActions validFrom validUntil maxDurationDays requiresApproval approvalRequestId status revokedAt revokedBy revocationReason } }';

export interface GraphqlDelegationDto {
  delegationId: string;
  tenantId: string;
  delegatingAdminId: string;
  delegatedAdminId: string;
  scopeType: string;
  scopeId: string | null;
  allowedActions: string[];
  validFrom: string;
  validUntil: string;
  maxDurationDays: number | null;
  requiresApproval: boolean;
  approvalRequestId: string | null;
  status: string;
  revokedAt: string | null;
  revokedBy: string | null;
  revocationReason: string | null;
}

export interface GetDelegationByIdResponse {
  getDelegationById: GraphqlDelegationDto | null;
}

export interface GetDelegationsByDelegatedAdminResponse {
  getDelegationsByDelegatedAdmin: GraphqlDelegationDto[];
}

export interface GetDelegationsByDelegatingAdminResponse {
  getDelegationsByDelegatingAdmin: GraphqlDelegationDto[];
}

export const graphqlDelegationQueries = {
  getDelegationById: async (delegationId: string): Promise<GetDelegationByIdResponse> => {
    if (!delegationId || delegationId.trim() === '') {
      throw new GraphQlValidationError('Invalid delegationId parameter', ['delegationId must be a non-empty string']);
    }
    return graphqlClient.request<GetDelegationByIdResponse>(GET_DELEGATION_BY_ID, { delegationId });
  },

  getDelegationsByDelegatedAdmin: async (
    delegatedAdminId: string,
    tenantId: string,
  ): Promise<GetDelegationsByDelegatedAdminResponse> => {
    if (!delegatedAdminId || !tenantId) {
      throw new GraphQlValidationError('Invalid parameters', ['delegatedAdminId and tenantId are required']);
    }
    return graphqlClient.request<GetDelegationsByDelegatedAdminResponse>(
      GET_DELEGATIONS_BY_DELEGATED_ADMIN,
      { delegatedAdminId, tenantId },
    );
  },

  getDelegationsByDelegatingAdmin: async (
    delegatingAdminId: string,
    tenantId: string,
  ): Promise<GetDelegationsByDelegatingAdminResponse> => {
    if (!delegatingAdminId || !tenantId) {
      throw new GraphQlValidationError('Invalid parameters', ['delegatingAdminId and tenantId are required']);
    }
    return graphqlClient.request<GetDelegationsByDelegatingAdminResponse>(
      GET_DELEGATIONS_BY_DELEGATING_ADMIN,
      { delegatingAdminId, tenantId },
    );
  },
};
