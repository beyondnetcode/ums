/**
 * user-account.service.ts
 *
 * Infrastructure service for the Identity / UserAccount bounded context.
 * Queries use GraphQL via graphqlClient.
 * Commands/transactions use REST via httpClient.
 * All responses are validated at runtime with Zod before returning.
 */
import { httpClient } from '@infra/http/httpClient';
import { graphqlQueries } from '@infra/identity/queries/user-account.graphql';
import { logger } from '@app/utils/logger';
import {
  UserAccountPageSchema,
  UserAccountSchema,
  CreateUserAccountResponseSchema,
  type UserAccount,
  type UserAccountPage,
  type CreateUserAccountPayload,
  type CreateUserAccountResponse,
} from '@domain/identity/schemas/user-account.schema';

export const userAccountService = {
  // ── Queries (GraphQL) ─────────────────────────────────────────────────────

  getAllUserAccounts: async (params?: {
    page?: number;
    pageSize?: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    tenantId?: string;
  }): Promise<UserAccountPage> => {
    const response = await graphqlQueries.getUserAccounts({
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 20,
      search: params?.search,
      criteria: params?.criteria,
      status: params?.status,
      sortBy: params?.sortBy,
      sortOrder: params?.sortOrder,
      tenantId: params?.tenantId,
    });

    const pageResult = UserAccountPageSchema.safeParse(response.getUserAccounts);
    if (!pageResult.success) {
      logger.error('Invalid GraphQL response shape for userAccounts query', pageResult.error);
      throw new Error('Invalid GraphQL response shape for userAccounts query');
    }
    return pageResult.data;
  },

  getUserAccountById: async (userAccountId: string): Promise<UserAccount> => {
    const response = await graphqlQueries.getUserAccountById(userAccountId);
    if (!response.getUserAccountById) throw new Error('User account not found');
    return UserAccountSchema.parse(response.getUserAccountById);
  },

  // ── Commands (REST) ───────────────────────────────────────────────────────

  createUserAccount: async (payload: CreateUserAccountPayload): Promise<CreateUserAccountResponse> => {
    const { data } = await httpClient.post('/user-accounts', payload);
    return CreateUserAccountResponseSchema.parse(data);
  },

  activateUserAccount: async (userAccountId: string): Promise<void> => {
    await httpClient.post(`/user-accounts/${userAccountId}/activate`);
  },

  blockUserAccount: async (userAccountId: string, reason: string): Promise<void> => {
    await httpClient.post(`/user-accounts/${userAccountId}/block`, null, { params: { reason } });
  },

  restoreUserAccount: async (userAccountId: string): Promise<void> => {
    await httpClient.post(`/user-accounts/${userAccountId}/restore`);
  },
};

export default userAccountService;
