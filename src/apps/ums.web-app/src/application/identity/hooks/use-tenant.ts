import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import tenantService from '../../../infrastructure/identity/services/tenant.service';
import { useNotificationStore } from '../../stores/notification.store';
import { useI18n } from '../../i18n/use-i18n';
import { CreateTenantPayload, Tenant } from '../../../domain/identity/models/tenant.model';

export const useGetAllTenants = () => {
  return useQuery<Tenant[]>({
    queryKey: ['tenants'],
    queryFn: () => tenantService.getAllTenants(),
    staleTime: 30_000,
  });
};

export const useGetTenant = (tenantId: string | null) => {
  return useQuery<Tenant | null>({
    queryKey: ['tenants', tenantId],
    queryFn: async () => {
      if (!tenantId) throw new Error('Tenant ID required');
      try {
        return await tenantService.getTenantById(tenantId);
      } catch (err: any) {
        if (err?.response?.status === 404) {
          return null;
        }
        throw err;
      }
    },
    enabled: !!tenantId,
    retry: (failureCount, error: any) => {
      if (error?.response?.status === 404) return false;
      return failureCount < 1;
    },
  });
};

export const useCreateTenant = () => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);
  const t = useI18n();

  return useMutation({
    mutationFn: (payload: CreateTenantPayload) => tenantService.createTenant(payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['tenants'] });
      addNotification({
        title: t.notifTenantCreated,
        message: t.notifTenantCreatedMsg(data.name, data.code),
        type: 'success',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: t.notifTenantCreateFailed,
        message: error.response?.data?.detail || error.message || t.notifTenantCreateFailed,
        type: 'error',
      });
    },
  });
};

export const useActivateTenant = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);
  const t = useI18n();

  return useMutation({
    mutationFn: () => tenantService.activateTenant(tenantId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId] });
      addNotification({
        title: t.notifActivated,
        message: t.notifActivatedMsg,
        type: 'success',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: t.notifActivateFailed,
        message: error.response?.data?.detail || error.message || t.notifActivateFailedMsg,
        type: 'error',
      });
    },
  });
};

export const useSuspendTenant = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);
  const t = useI18n();

  return useMutation({
    mutationFn: () => tenantService.suspendTenant(tenantId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId] });
      addNotification({
        title: t.notifSuspended,
        message: t.notifSuspendedMsg,
        type: 'warning',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: t.notifSuspendFailed,
        message: error.response?.data?.detail || error.message || t.notifSuspendFailedMsg,
        type: 'error',
      });
    },
  });
};
