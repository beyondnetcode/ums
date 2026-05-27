import { useQuery } from '@tanstack/react-query';
import tenantService from '@infra/identity/services/tenant.service';
import { useNotifiedMutation } from '@app/hooks/use-notified-mutation';
import { useI18n } from '@app/i18n/use-i18n';
import { AddBranchPayload, Branch } from '@domain/identity/models/branch.model';
import { getHttpStatus } from '@app/errors/http-error';

// ─── Query ──────────────────────────────────────────────────────────────────

export const useGetBranches = (tenantId: string | null) => {
  return useQuery<Branch[]>({
    queryKey: ['tenants', tenantId, 'branches'],
    queryFn: async () => {
      if (!tenantId) throw new Error('Tenant ID required');
      try {
        return await tenantService.getBranches(tenantId);
      } catch (err: unknown) {
        if (getHttpStatus(err) === 404) return [];
        throw err;
      }
    },
    enabled: !!tenantId,
    retry: (failureCount, error: unknown) => {
      if (getHttpStatus(error) === 404) return false;
      return failureCount < 1;
    },
  });
};

// ─── Mutations ──────────────────────────────────────────────────────────────

export const useAddBranch = (tenantId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (payload: AddBranchPayload) => tenantService.addBranch(tenantId, payload),
    invalidateKeys: [['tenants', tenantId, 'branches']],
    successNotif: (data) => ({
      title: t.notifBranchAdded,
      message: t.notifBranchAddedMsg(data.code),
    }),
    errorNotif: () => ({
      title: t.notifBranchAddFailed ?? 'Error al Registrar Sucursal',
      message: t.notifBranchAddFailedMsg ?? 'No se pudo registrar la sucursal.',
    }),
  });
};

export const useRemoveBranch = (tenantId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (branchId: string) => tenantService.removeBranch(tenantId, branchId),
    invalidateKeys: [['tenants', tenantId, 'branches']],
    successNotif: () => ({
      title: t.notifBranchRemoved,
      message: t.notifBranchRemovedMsg,
      type: 'warning' as const,
    }),
    errorNotif: () => ({
      title: t.notifBranchRemoveFailed ?? 'Error al Eliminar Sucursal',
      message: t.notifBranchRemoveFailedMsg ?? 'No se pudo eliminar la sucursal.',
    }),
  });
};

export const useDeactivateBranch = (tenantId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (branchId: string) => tenantService.deactivateBranch(tenantId, branchId),
    invalidateKeys: [['tenants', tenantId, 'branches']],
    successNotif: () => ({
      title: t.notifBranchDeactivated,
      message: t.notifBranchDeactivatedMsg,
      type: 'info' as const,
    }),
    errorNotif: () => ({
      title: t.notifBranchDeactivateFailed ?? 'Error al Desactivar Sucursal',
      message: t.notifBranchDeactivateFailedMsg ?? 'No se pudo desactivar la sucursal.',
    }),
  });
};

export const useReactivateBranch = (tenantId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (branchId: string) => tenantService.reactivateBranch(tenantId, branchId),
    invalidateKeys: [['tenants', tenantId, 'branches']],
    successNotif: () => ({
      title: t.notifBranchReactivated,
      message: t.notifBranchReactivatedMsg,
    }),
    errorNotif: () => ({
      title: t.notifBranchReactivateFailed ?? 'Error al Reactivar Sucursal',
      message: t.notifBranchReactivateFailedMsg ?? 'No se pudo reactivar la sucursal.',
    }),
  });
};
