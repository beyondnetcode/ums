import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import tenantService from '../../../infrastructure/identity/services/tenant.service';
import { useNotificationStore } from '../../stores/notification.store';
import { CreateTenantPayload, Tenant } from '../../../domain/identity/models/tenant.model';

export const useGetTenant = (tenantId: string | null) => {
  return useQuery<Tenant | null>({
    queryKey: ['tenants', tenantId],
    queryFn: async () => {
      if (!tenantId) throw new Error('Tenant ID required');
      try {
        return await tenantService.getTenantById(tenantId);
      } catch (err: any) {
        // Gracefully handle 404 — tenant not yet seeded in backend (local prototype mode)
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

  return useMutation({
    mutationFn: (payload: CreateTenantPayload) => tenantService.createTenant(payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['tenants'] });
      addNotification({
        title: 'Tenant Created',
        message: `Successfully registered tenant '${data.name}' with code: ${data.code}.`,
        type: 'success',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: 'Tenant Creation Failed',
        message: error.response?.data?.detail || error.message || 'Error occurred during tenant registration.',
        type: 'error',
      });
    },
  });
};

export const useActivateTenant = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);

  return useMutation({
    mutationFn: () => tenantService.activateTenant(tenantId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId] });
      addNotification({
        title: 'Tenant Activated',
        message: 'The selected tenant state was successfully restored to Active.',
        type: 'success',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: 'Activation Failed',
        message: error.response?.data?.detail || error.message || 'Could not restore active tenant status.',
        type: 'error',
      });
    },
  });
};

export const useSuspendTenant = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);

  return useMutation({
    mutationFn: () => tenantService.suspendTenant(tenantId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId] });
      addNotification({
        title: 'Tenant Suspended',
        message: 'The selected tenant has been suspended. API access is restricted.',
        type: 'warning',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: 'Suspension Failed',
        message: error.response?.data?.detail || error.message || 'Could not suspend tenant status.',
        type: 'error',
      });
    },
  });
};
