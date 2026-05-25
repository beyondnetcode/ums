import { useQuery } from '@tanstack/react-query';
import systemSuiteService from '@infra/authorization/services/system-suite.service';
import { useNotifiedMutation } from '@app/hooks/use-notified-mutation';
import { useI18n } from '@app/i18n/use-i18n';
import { CreateSystemSuitePayload, SystemSuite, SystemSuitePage } from '@domain/authorization/models/system-suite.model';
import { getHttpStatus } from '@app/errors/http-error';
import { GraphQlUnavailableError, GraphQlValidationError } from '@infra/http/graphqlClient';

// ─── Query params ───────────────────────────────────────────────────────────

export interface SystemSuiteQueryParams {
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

export const useGetAllSystemSuites = (params: SystemSuiteQueryParams) => {
  return useQuery<SystemSuitePage>({
    queryKey: ['system-suites', params.page, params.pageSize, params.search, params.criteria, params.status, params.sortBy, params.sortOrder, params.tenantId],
    queryFn: () => systemSuiteService.getAllSystemSuites(params),
    staleTime: 30_000,
    retry: (failureCount, error: unknown) => {
      if (isNonRecoverableError(error)) return false;
      if (isNetworkError(error)) return failureCount < 2;
      return failureCount < 1;
    },
    retryDelay: (attempt) => Math.min(1000 * 2 ** attempt, 5000),
  });
};

export const useGetSystemSuite = (systemSuiteId: string | null) => {
  return useQuery<SystemSuite | null>({
    queryKey: ['system-suites', systemSuiteId],
    queryFn: async () => {
      if (!systemSuiteId) throw new Error('SystemSuite ID required');
      try {
        return await systemSuiteService.getSystemSuiteById(systemSuiteId);
      } catch (err: unknown) {
        if (getHttpStatus(err) === 404) return null;
        throw err;
      }
    },
    enabled: !!systemSuiteId,
    retry: (failureCount, error: unknown) => {
      if (isNonRecoverableError(error)) return false;
      if (isNetworkError(error)) return failureCount < 2;
      if (getHttpStatus(error) === 404) return false;
      return failureCount < 1;
    },
  });
};

// ─── Mutations ──────────────────────────────────────────────────────────────

export const useCreateSystemSuite = () => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (payload: CreateSystemSuitePayload) => systemSuiteService.createSystemSuite(payload),
    invalidateKeys: [['system-suites']],
    successNotif: (data) => ({
      title: t.notifSystemSuiteCreated,
      message: t.notifSystemSuiteCreatedMsg(data.systemSuiteId),
    }),
    errorNotif: () => ({
      title: t.notifSystemSuiteCreateFailed,
      message: t.notifSystemSuiteCreateFailed,
    }),
  });
};

export const useSetSystemSuiteStatus = (systemSuiteId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (status: string) => systemSuiteService.setSystemSuiteStatus(systemSuiteId, status),
    invalidateKeys: [['system-suites', systemSuiteId]],
    successNotif: () => ({
      title: t.notifStatusChanged,
      message: t.notifStatusChangedMsg,
    }),
    errorNotif: () => ({
      title: t.notifStatusChangeFailed,
      message: t.notifStatusChangeFailedMsg,
    }),
  });
};
