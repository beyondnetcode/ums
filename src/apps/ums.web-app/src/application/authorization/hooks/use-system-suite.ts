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
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: t.notifStatusChanged ?? 'Estado Cambiado',
      message: t.notifStatusChangedMsg ?? 'El estado de la suite fue actualizado.',
    }),
    errorNotif: () => ({
      title: t.notifStatusChangeFailed ?? 'Error de Cambio de Estado',
      message: t.notifStatusChangeFailedMsg ?? 'No se pudo actualizar el estado.',
    }),
  });
};

// ─── Module Mutations ────────────────────────────────────────────────────────

export const useAddModule = (systemSuiteId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (payload: { code: string; name: string; description?: string; sortOrder: number }) =>
      systemSuiteService.addModule(systemSuiteId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: t.notifModuleAdded ?? 'Módulo Registrado',
      message: t.notifModuleAddedMsg ?? 'El módulo estructural ha sido registrado correctamente.',
    }),
    errorNotif: () => ({
      title: t.notifModuleAddFailed ?? 'Error al Registrar Módulo',
      message: t.notifModuleAddFailedMsg ?? 'No se pudo registrar el módulo.',
    }),
  });
};

export const useRemoveModule = (systemSuiteId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (moduleId: string) => systemSuiteService.removeModule(systemSuiteId, moduleId),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: t.notifModuleRemoved ?? 'Módulo Removido',
      message: t.notifModuleRemovedMsg ?? 'El módulo estructural fue eliminado correctamente.',
      type: 'warning' as const,
    }),
    errorNotif: () => ({
      title: t.notifModuleRemoveFailed ?? 'Error al Eliminar Módulo',
      message: t.notifModuleRemoveFailedMsg ?? 'No se pudo eliminar el módulo.',
    }),
  });
};

export const useActivateModule = (systemSuiteId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (moduleId: string) => systemSuiteService.activateModule(systemSuiteId, moduleId),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: t.notifActivated ?? 'Módulo Activado',
      message: t.notifActivatedMsg ?? 'Módulo activado con éxito.',
    }),
    errorNotif: () => ({
      title: t.notifActivateFailed ?? 'Error al Activar',
      message: t.notifActivateFailedMsg ?? 'No se pudo activar el módulo.',
    }),
  });
};

export const useDeactivateModule = (systemSuiteId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (moduleId: string) => systemSuiteService.deactivateModule(systemSuiteId, moduleId),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: t.notifSuspended ?? 'Módulo Desactivado',
      message: t.notifSuspendedMsg ?? 'Módulo desactivado con éxito.',
      type: 'info' as const,
    }),
    errorNotif: () => ({
      title: t.notifSuspendFailed ?? 'Error al Desactivar',
      message: t.notifSuspendFailedMsg ?? 'No se pudo desactivar el módulo.',
    }),
  });
};

// ─── Action Mutations ────────────────────────────────────────────────────────

export const useRegisterAction = (systemSuiteId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (payload: { code: string; name: string }) =>
      systemSuiteService.registerAction(systemSuiteId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: t.notifActionRegistered ?? 'Acción Registrada',
      message: t.notifActionRegisteredMsg ?? 'Código de acción registrado correctamente.',
    }),
    errorNotif: () => ({
      title: t.notifActionRegisterFailed ?? 'Error al Registrar Acción',
      message: t.notifActionRegisterFailedMsg ?? 'No se pudo registrar la acción.',
    }),
  });
};

export const useRemoveAction = (systemSuiteId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (code: string) => systemSuiteService.removeAction(systemSuiteId, code),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: t.notifActionRemoved ?? 'Acción Removida',
      message: t.notifActionRemovedMsg ?? 'El código de acción fue removido correctamente.',
      type: 'warning' as const,
    }),
    errorNotif: () => ({
      title: t.notifActionRemoveFailed ?? 'Error al Remover Acción',
      message: t.notifActionRemoveFailedMsg ?? 'No se pudo remover la acción.',
    }),
  });
};
