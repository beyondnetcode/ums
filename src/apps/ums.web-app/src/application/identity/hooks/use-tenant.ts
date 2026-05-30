import { useQuery } from '@tanstack/react-query';
import tenantService from '@infra/identity/services/tenant.service';
import { useNotifiedMutation } from '@app/hooks/use-notified-mutation';
import { useI18n } from '@app/i18n/use-i18n';
import { CreateTenantPayload, Tenant, TenantPage } from '@domain/identity/models/tenant.model';
import { getHttpStatus, isNonRecoverable, isNetworkError, getRetryOptions } from '@app/utils/error-utils';
import { CONTEXT_QUERY_CONFIG } from '@app/shared/config/query.config';

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

// ─── Queries ────────────────────────────────────────────────────────────────

export const useGetAllTenants = (params: TenantQueryParams | null) => {
  return useQuery<TenantPage>({
    queryKey: ['tenants', params?.page, params?.pageSize, params?.search, params?.criteria, params?.status, params?.sortBy, params?.sortOrder],
    queryFn: () => tenantService.getAll(params!),
    enabled: !!params,
    ...CONTEXT_QUERY_CONFIG.TENANT,
    ...getRetryOptions({ maxRetries: 1, networkErrorMaxRetries: 2 }),
  });
};

export const useGetTenant = (tenantId: string | null) => {
  return useQuery<Tenant | null>({
    queryKey: ['tenants', tenantId],
    queryFn: async () => {
      if (!tenantId) throw new Error('Tenant ID required');
      try {
        return await tenantService.getById(tenantId);
      } catch (err: unknown) {
        if (getHttpStatus(err) === 404) return null;
        throw err;
      }
    },
    enabled: !!tenantId,
    ...CONTEXT_QUERY_CONFIG.TENANT,
    ...getRetryOptions({ maxRetries: 1, networkErrorMaxRetries: 2 }),
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
