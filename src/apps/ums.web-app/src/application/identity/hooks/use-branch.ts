import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import tenantService from '../../../infrastructure/identity/services/tenant.service';
import { useNotificationStore } from '../../stores/notification.store';
import { useI18n } from '../../i18n/use-i18n';
import { AddBranchPayload, Branch } from '../../../domain/identity/models/branch.model';

export const useGetBranches = (tenantId: string | null) => {
  return useQuery<Branch[]>({
    queryKey: ['tenants', tenantId, 'branches'],
    queryFn: async () => {
      if (!tenantId) throw new Error('Tenant ID required');
      try {
        return await tenantService.getBranches(tenantId);
      } catch (err: any) {
        if (err?.response?.status === 404) {
          return [];
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

export const useAddBranch = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);
  const t = useI18n();

  return useMutation({
    mutationFn: (payload: AddBranchPayload) => tenantService.addBranch(tenantId, payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId, 'branches'] });
      addNotification({
        title: t.notifBranchAdded,
        message: t.notifBranchAddedMsg(data.code),
        type: 'success',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: t.notifBranchAdded,
        message: error.response?.data?.detail || error.message || t.notifBranchAdded,
        type: 'error',
      });
    },
  });
};

export const useRemoveBranch = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);
  const t = useI18n();

  return useMutation({
    mutationFn: (branchId: string) => tenantService.removeBranch(tenantId, branchId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId, 'branches'] });
      addNotification({
        title: t.notifBranchRemoved,
        message: t.notifBranchRemovedMsg,
        type: 'warning',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: t.notifBranchRemoved,
        message: error.response?.data?.detail || error.message || t.notifBranchRemovedMsg,
        type: 'error',
      });
    },
  });
};

export const useDeactivateBranch = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);
  const t = useI18n();

  return useMutation({
    mutationFn: (branchId: string) => tenantService.deactivateBranch(tenantId, branchId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId, 'branches'] });
      addNotification({
        title: t.notifBranchDeactivated,
        message: t.notifBranchDeactivatedMsg,
        type: 'info',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: t.notifBranchDeactivated,
        message: error.response?.data?.detail || error.message || t.notifBranchDeactivatedMsg,
        type: 'error',
      });
    },
  });
};

export const useReactivateBranch = (tenantId: string) => {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((state) => state.addNotification);
  const t = useI18n();

  return useMutation({
    mutationFn: (branchId: string) => tenantService.reactivateBranch(tenantId, branchId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants', tenantId, 'branches'] });
      addNotification({
        title: t.notifBranchReactivated,
        message: t.notifBranchReactivatedMsg,
        type: 'success',
      });
    },
    onError: (error: any) => {
      addNotification({
        title: t.notifBranchReactivated,
        message: error.response?.data?.detail || error.message || t.notifBranchReactivatedMsg,
        type: 'error',
      });
    },
  });
};
