import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import tenantService from '../../../infrastructure/identity/services/tenant.service';
import { useNotificationStore } from '../../stores/notification.store';
import { AddBranchPayload, Branch } from '../../../domain/identity/models/branch.model';

export const useGetBranches = (tenantId: string | null) => {
  return useQuery<Branch[]>({
    queryKey: ['tenants', tenantId, 'branches'],
    queryFn: async () => {
      if (!tenantId) throw new Error('Tenant ID required');
      try {
        return await tenantService.getBranches(tenantId);
      } catch (err: any) {
        // Gracefully handle 404 — tenant not yet in backend (local prototype mode)
        const status = err?.response?.status;
        if (status === 404) {
          return [];
        }
        throw err;
      }
    },
    enabled: !!tenantId,
    retry: (failureCount, error: any) => {
      // Do not retry on 404 — the resource simply doesn't exist on the backend yet
      if (error?.response?.status === 404) return false;
      return failureCount < 1;
    },
  });
};

export const useAddBranch = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);

  return useMutation({
    mutationFn: (payload: AddBranchPayload) => tenantService.addBranch(tenantId, payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId, 'branches'] });
      addNotification({
        title: 'Branch Added',
        message: `Successfully added branch '${payloadToName(data.code)}' [Code: ${data.code}] to tenant.`,
        type: 'success',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: 'Add Branch Failed',
        message: error.response?.data?.detail || error.message || 'Error occurred while adding branch.',
        type: 'error',
      });
    },
  });
};

export const useRemoveBranch = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);

  return useMutation({
    mutationFn: (branchId: string) => tenantService.removeBranch(tenantId, branchId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId, 'branches'] });
      addNotification({
        title: 'Branch Removed',
        message: 'The branch was successfully deleted from the tenant configuration.',
        type: 'warning',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: 'Removal Failed',
        message: error.response?.data?.detail || error.message || 'Could not delete branch.',
        type: 'error',
      });
    },
  });
};

export const useDeactivateBranch = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);

  return useMutation({
    mutationFn: (branchId: string) => tenantService.deactivateBranch(tenantId, branchId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId, 'branches'] });
      addNotification({
        title: 'Branch Deactivated',
        message: 'Selected branch status has been toggled to inactive.',
        type: 'info',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: 'Deactivation Failed',
        message: error.response?.data?.detail || error.message || 'Could not deactivate branch.',
        type: 'error',
      });
    },
  });
};

export const useReactivateBranch = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);

  return useMutation({
    mutationFn: (branchId: string) => tenantService.reactivateBranch(tenantId, branchId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId, 'branches'] });
      addNotification({
        title: 'Branch Reactivated',
        message: 'Selected branch status was successfully restored to active.',
        type: 'success',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: 'Reactivation Failed',
        message: error.response?.data?.detail || error.message || 'Could not reactivate branch.',
        type: 'error',
      });
    },
  });
};

// Simple helper to fallback to readable name
const payloadToName = (code: string) => {
  return code.toLowerCase().replace(/_/g, ' ');
};
