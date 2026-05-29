import { useQuery } from '@tanstack/react-query';
import tenantService from '@infra/identity/services/tenant.service';
import { useNotifiedMutation } from '@app/hooks/use-notified-mutation';
import { useI18n } from '@app/i18n/use-i18n';
import { CreateTenantPayload, Tenant, TenantPage } from '@domain/identity/models/tenant.model';
import { getHttpStatus } from '@app/errors/http-error';
import { GraphQlUnavailableError, GraphQlValidationError } from '@infra/http/graphqlClient';

// ─── Query params ───────────────────────────────────────────────────────────

export interface TenantQueryParams {
  page: number;
  pageSize: number;
  search?: string;
  criteria?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
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

export const useGetAllTenants = (params: TenantQueryParams | null) => {
  return useQuery<TenantPage>({
    queryKey: ['tenants', params?.page, params?.pageSize, params?.search, params?.criteria, params?.status, params?.sortBy, params?.sortOrder],
    queryFn: () => tenantService.getAllTenants(params!),
    enabled: !!params,
    staleTime: 30_000,
    retry: (failureCount, error: unknown) => {
      if (isNonRecoverableError(error)) return false;
      if (isNetworkError(error)) return failureCount < 2;
      return failureCount < 1;
    },
    retryDelay: (attempt) => Math.min(1000 * 2 ** attempt, 5000),
  });
};

export const useGetTenant = (tenantId: string | null) => {
  return useQuery<Tenant | null>({
    queryKey: ['tenants', tenantId],
    queryFn: async () => {
      if (!tenantId) throw new Error('Tenant ID required');
      try {
        return await tenantService.getTenantById(tenantId);
      } catch (err: unknown) {
        if (getHttpStatus(err) === 404) return null;
        throw err;
      }
    },
    enabled: !!tenantId,
    retry: (failureCount, error: unknown) => {
      if (isNonRecoverableError(error)) return false;
      if (isNetworkError(error)) return failureCount < 2;
      if (getHttpStatus(error) === 404) return false;
      return failureCount < 1;
    },
  });
};

// ─── Mutations ──────────────────────────────────────────────────────────────

export const useCreateTenant = () => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (payload: CreateTenantPayload) => tenantService.createTenant(payload),
    invalidateKeys: [['tenants']],
    successNotif: (data) => ({
      title: t.notifTenantCreated,
      message: t.notifTenantCreatedMsg(data.name, data.code),
    }),
    errorNotif: () => ({
      title: t.notifTenantCreateFailed,
      message: t.notifTenantCreateFailed,
    }),
  });
};

export const useActivateTenant = (tenantId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: () => tenantService.activateTenant(tenantId),
    invalidateKeys: [['tenants', tenantId]],
    successNotif: () => ({
      title: t.notifActivated,
      message: t.notifActivatedMsg,
    }),
    errorNotif: () => ({
      title: t.notifActivateFailed,
      message: t.notifActivateFailedMsg,
    }),
  });
};

export const useSuspendTenant = (tenantId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: () => tenantService.suspendTenant(tenantId),
    invalidateKeys: [['tenants', tenantId]],
    successNotif: () => ({
      title: t.notifSuspended,
      message: t.notifSuspendedMsg,
      type: 'warning' as const,
    }),
    errorNotif: () => ({
      title: t.notifSuspendFailed,
      message: t.notifSuspendFailedMsg,
    }),
  });
};
