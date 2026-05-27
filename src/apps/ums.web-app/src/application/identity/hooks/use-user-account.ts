/**
 * use-user-account.ts — TanStack Query hooks for UserAccount bounded context
 *
 * Queries use useQuery directly (GraphQL reads).
 * Mutations use useNotifiedMutation factory (REST writes).
 */
import { useQuery } from '@tanstack/react-query';
import userAccountService from '@infra/identity/services/user-account.service';
import { useNotifiedMutation } from '@app/hooks/use-notified-mutation';
import { useI18n } from '@app/i18n/use-i18n';
import {
  CreateUserAccountPayload,
  UserAccount,
  UserAccountPage,
} from '@domain/identity/models/user-account.model';
import { getHttpStatus } from '@app/errors/http-error';
import { GraphQlValidationError, GraphQlUnavailableError } from '@infra/http/graphqlClient';

export interface UserAccountQueryParams {
  page: number;
  pageSize: number;
  search?: string;
  criteria?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  tenantId?: string;
}

function isNonRecoverableError(error: unknown): boolean {
  if (error instanceof GraphQlValidationError) return true;
  const status = getHttpStatus(error);
  if (status === 400 || status === 401 || status === 403 || status === 404 || status === 422) return true;
  return false;
}

function isNetworkError(error: unknown): boolean {
  return error instanceof GraphQlUnavailableError;
}

// ─── Queries ────────────────────────────────────────────────────────────────

export const useGetAllUserAccounts = (params: UserAccountQueryParams) => {
  return useQuery<UserAccountPage>({
    queryKey: ['user-accounts', params.page, params.pageSize, params.search, params.criteria, params.status, params.sortBy, params.sortOrder, params.tenantId],
    queryFn: () => userAccountService.getAllUserAccounts(params),
    staleTime: 30_000,
    retry: (failureCount, error: unknown) => {
      if (isNonRecoverableError(error)) return false;
      if (isNetworkError(error)) return failureCount < 2;
      return failureCount < 1;
    },
    retryDelay: (attempt) => Math.min(1000 * 2 ** attempt, 5000),
  });
};

export const useGetUserAccount = (userAccountId: string | null) => {
  return useQuery<UserAccount | null>({
    queryKey: ['user-accounts', userAccountId],
    queryFn: async () => {
      if (!userAccountId) throw new Error('User account ID required');
      try {
        return await userAccountService.getUserAccountById(userAccountId);
      } catch (err: unknown) {
        if (getHttpStatus(err) === 404) return null;
        throw err;
      }
    },
    enabled: !!userAccountId,
    retry: (failureCount, error: unknown) => {
      if (isNonRecoverableError(error)) return false;
      if (isNetworkError(error)) return failureCount < 2;
      if (getHttpStatus(error) === 404) return false;
      return failureCount < 1;
    },
  });
};

// ─── Mutations ──────────────────────────────────────────────────────────────

export const useCreateUserAccount = () => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (payload: CreateUserAccountPayload) => userAccountService.createUserAccount(payload),
    invalidateKeys: [['user-accounts']],
    successNotif: () => ({
      title: t.notifUserCreated,
      message: t.notifUserCreatedMsg,
    }),
    errorNotif: () => ({
      title: t.notifUserCreateFailed,
      message: t.notifUserCreateFailedMsg,
    }),
  });
};

export const useActivateUserAccount = (userAccountId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: () => userAccountService.activateUserAccount(userAccountId),
    invalidateKeys: [['user-accounts', userAccountId]],
    successNotif: () => ({
      title: t.notifActivated,
      message: t.notifUserActivatedMsg,
    }),
    errorNotif: () => ({
      title: t.notifActivateFailed,
      message: t.notifUserActivateFailedMsg,
    }),
  });
};

export const useBlockUserAccount = (userAccountId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (reason: string) => userAccountService.blockUserAccount(userAccountId, reason),
    invalidateKeys: [['user-accounts', userAccountId]],
    successNotif: () => ({
      title: t.notifBlocked,
      message: t.notifUserBlockedMsg,
      type: 'warning',
    }),
    errorNotif: () => ({
      title: t.notifBlockFailed,
      message: t.notifUserBlockFailedMsg,
    }),
  });
};

export const useRestoreUserAccount = (userAccountId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: () => userAccountService.restoreUserAccount(userAccountId),
    invalidateKeys: [['user-accounts', userAccountId]],
    successNotif: () => ({
      title: t.notifRestored,
      message: t.notifUserRestoredMsg,
    }),
    errorNotif: () => ({
      title: t.notifRestoreFailed,
      message: t.notifUserRestoreFailedMsg,
    }),
  });
};

export const useSetUserAccountPassword = (userAccountId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (password: string) => userAccountService.setUserAccountPassword({
      userAccountId,
      password,
    }),
    invalidateKeys: [['user-accounts'], ['user-accounts', userAccountId]],
    successNotif: () => ({
      title: t.notifPasswordUpdated,
      message: t.notifPasswordUpdatedMsg,
    }),
    errorNotif: () => ({
      title: t.notifPasswordUpdateFailed,
      message: t.notifPasswordUpdateFailedMsg,
    }),
  });
};
