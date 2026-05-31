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
  SetUserAccountPasswordPayloadSchema,
  SetUserAccountPasswordResponseSchema,
  type SetUserAccountPasswordPayload,
  type SetUserAccountPasswordResponse,
} from '@domain/identity/schemas/user-account.schema';

export interface UserAccountQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  criteria?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: string;
  tenantId?: string;
}

export const userAccountService = {
  // ── Queries (REST) ────────────────────────────────────────────────────────
  // TODO: Changed from GraphQL to REST due to planned GraphQL migration/issues. Revisit GraphQL implementation later.

  getAll: async (params?: UserAccountQueryParams): Promise<UserAccountPage> => {
    const { data } = await httpClient.get('/user-accounts', {
      params: {
        page: params?.page ?? 1,
        pageSize: params?.pageSize ?? 20,
        search: params?.search,
        criteria: params?.criteria,
        status: params?.status,
        sortBy: params?.sortBy,
        sortOrder: params?.sortOrder,
        tenantId: params?.tenantId,
      }
    });

    const pageResult = UserAccountPageSchema.safeParse(data);
    if (!pageResult.success) {
      logger.error('Invalid REST response shape for userAccounts query', pageResult.error);
      throw new Error('Invalid REST response shape for userAccounts query');
    }
    return pageResult.data;
  },

  getById: async (userAccountId: string): Promise<UserAccount> => {
    const { data } = await httpClient.get(`/user-accounts/${userAccountId}`);
    return UserAccountSchema.parse(data);
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

  setUserAccountPassword: async (payload: SetUserAccountPasswordPayload): Promise<SetUserAccountPasswordResponse> => {
    const validPayload = SetUserAccountPasswordPayloadSchema.parse(payload);
    const { data } = await httpClient.post(`/user-accounts/${validPayload.userAccountId}/passwords`, validPayload);
    return SetUserAccountPasswordResponseSchema.parse(data);
  },
};

export default userAccountService;
