import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { tenantService } from '../../../infrastructure/identity/services/tenant.service';
import { useNotificationStore } from '../../stores/notification.store';
import { useI18n } from '../../i18n/use-i18n';
import { AddBranchPayload, Branch } from '../../../domain/identity/models/branch.model';

const isNotFound = (error: unknown): boolean =>
  error instanceof AxiosError && error.response?.status === 404;

const getErrorMessage = (error: unknown, fallback: string): string => {
  if (error instanceof AxiosError) {
    return (error.response?.data as { detail?: string })?.detail || error.message || fallback;
  }
  return fallback;
};

export const useGetBranches = (tenantId: string | null) => {
  return useQuery<Branch[]>({
    queryKey: ['tenants', tenantId, 'branches'],
    queryFn: async () => {
      if (!tenantId) throw new Error('Tenant ID required');
      try {
        return await tenantService.getBranches(tenantId);
      } catch (err) {
        if (isNotFound(err)) {
          return [];
        }
        throw err;
      }
    },
    enabled: !!tenantId,
    retry: (failureCount, error) => {
      if (isNotFound(error)) return false;
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
    onError: (error) => {
      addNotification({
        title: t.notifBranchAdded,
        message: getErrorMessage(error, t.notifBranchAdded),
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
    onError: (error) => {
      addNotification({
        title: t.notifBranchRemoved,
        message: getErrorMessage(error, t.notifBranchRemovedMsg),
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
    onError: (error) => {
      addNotification({
        title: t.notifBranchDeactivated,
        message: getErrorMessage(error, t.notifBranchDeactivatedMsg),
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
    onError: (error) => {
      addNotification({
        title: t.notifBranchReactivated,
        message: getErrorMessage(error, t.notifBranchReactivatedMsg),
        type: 'error',
      });
    },
  });
};
