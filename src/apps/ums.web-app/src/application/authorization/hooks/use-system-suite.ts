import { useQuery } from '@tanstack/react-query';
import systemSuiteService from '@infra/authorization/services/system-suite.service';
import { useNotifiedMutation } from '@app/hooks/use-notified-mutation';
import { useI18n } from '@app/i18n/use-i18n';
import { CreateSystemSuitePayload, SystemSuite, SystemSuitePage } from '@domain/authorization/models/system-suite.model';
import { getHttpStatus, isNonRecoverable, isNetworkError, getRetryOptions } from '@app/utils/error-utils';
import { CONTEXT_QUERY_CONFIG } from '@app/shared/config/query.config';

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

// ─── Queries ────────────────────────────────────────────────────────────────

export const useGetAllSystemSuites = (params: SystemSuiteQueryParams | null) => {
  return useQuery<SystemSuitePage>({
    queryKey: ['system-suites', params?.page, params?.pageSize, params?.search, params?.criteria, params?.status, params?.sortBy, params?.sortOrder, params?.tenantId],
    queryFn: () => systemSuiteService.getAll(params!),
    enabled: !!params,
    ...CONTEXT_QUERY_CONFIG.SYSTEM_SUITE,
    ...getRetryOptions({ maxRetries: 1, networkErrorMaxRetries: 2 }),
  });
};

export const useGetSystemSuite = (systemSuiteId: string | null) => {
  return useQuery<SystemSuite | null>({
    queryKey: ['system-suites', systemSuiteId],
    queryFn: async () => {
      if (!systemSuiteId) throw new Error('SystemSuite ID required');
      try {
        return await systemSuiteService.getById(systemSuiteId);
      } catch (err: unknown) {
        if (getHttpStatus(err) === 404) return null;
        throw err;
      }
    },
    enabled: !!systemSuiteId,
    ...CONTEXT_QUERY_CONFIG.SYSTEM_SUITE,
    ...getRetryOptions({ maxRetries: 1, networkErrorMaxRetries: 2 }),
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

// ─── Menu Mutations ───────────────────────────────────────────────────────────

export const useAddMenu = (systemSuiteId: string, moduleId: string) => {
  return useNotifiedMutation({
    mutationFn: (payload: { code: string; label: string; description?: string; sortOrder: number }) =>
      systemSuiteService.addMenu(systemSuiteId, moduleId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Menú Registrado', message: 'El menú fue agregado correctamente.' }),
    errorNotif: () => ({ title: 'Error al Registrar Menú', message: 'No se pudo agregar el menú.' }),
  });
};

export const useUpdateMenu = (systemSuiteId: string, moduleId: string, menuId: string) => {
  return useNotifiedMutation({
    mutationFn: (payload: { label: string; description?: string; sortOrder: number }) =>
      systemSuiteService.updateMenu(systemSuiteId, moduleId, menuId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Menú Actualizado', message: 'El menú fue actualizado correctamente.' }),
    errorNotif: () => ({ title: 'Error al Actualizar Menú', message: 'No se pudo actualizar el menú.' }),
  });
};

export const useRemoveMenu = (systemSuiteId: string, moduleId: string) => {
  return useNotifiedMutation({
    mutationFn: (menuId: string) => systemSuiteService.removeMenu(systemSuiteId, moduleId, menuId),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Menú Eliminado', message: 'El menú fue eliminado.', type: 'warning' as const }),
    errorNotif: () => ({ title: 'Error al Eliminar Menú', message: 'No se pudo eliminar el menú.' }),
  });
};

// ─── SubMenu Mutations ────────────────────────────────────────────────────────

export const useAddSubMenu = (systemSuiteId: string, moduleId: string, menuId: string) => {
  return useNotifiedMutation({
    mutationFn: (payload: { code: string; label: string; description?: string; sortOrder: number }) =>
      systemSuiteService.addSubMenu(systemSuiteId, moduleId, menuId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Submenú Registrado', message: 'El submenú fue agregado correctamente.' }),
    errorNotif: () => ({ title: 'Error al Registrar Submenú', message: 'No se pudo agregar el submenú.' }),
  });
};

export const useUpdateSubMenu = (systemSuiteId: string, moduleId: string, menuId: string, subMenuId: string) => {
  return useNotifiedMutation({
    mutationFn: (payload: { label: string; description?: string; sortOrder: number }) =>
      systemSuiteService.updateSubMenu(systemSuiteId, moduleId, menuId, subMenuId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Submenú Actualizado', message: 'El submenú fue actualizado correctamente.' }),
    errorNotif: () => ({ title: 'Error al Actualizar Submenú', message: 'No se pudo actualizar el submenú.' }),
  });
};

export const useRemoveSubMenu = (systemSuiteId: string, moduleId: string, menuId: string) => {
  return useNotifiedMutation({
    mutationFn: (subMenuId: string) => systemSuiteService.removeSubMenu(systemSuiteId, moduleId, menuId, subMenuId),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Submenú Eliminado', message: 'El submenú fue eliminado.', type: 'warning' as const }),
    errorNotif: () => ({ title: 'Error al Eliminar Submenú', message: 'No se pudo eliminar el submenú.' }),
  });
};

// ─── Option Mutations ─────────────────────────────────────────────────────────

export const useAddOption = (systemSuiteId: string, moduleId: string, menuId: string, subMenuId: string) => {
  return useNotifiedMutation({
    mutationFn: (payload: { code: string; label: string; description?: string; actionCode: string; sortOrder: number }) =>
      systemSuiteService.addOption(systemSuiteId, moduleId, menuId, subMenuId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Opción Registrada', message: 'La opción fue agregada correctamente.' }),
    errorNotif: () => ({ title: 'Error al Registrar Opción', message: 'No se pudo agregar la opción.' }),
  });
};

export const useUpdateOption = (systemSuiteId: string, moduleId: string, menuId: string, subMenuId: string, optionId: string) => {
  return useNotifiedMutation({
    mutationFn: (payload: { label: string; description?: string; actionCode: string; sortOrder: number }) =>
      systemSuiteService.updateOption(systemSuiteId, moduleId, menuId, subMenuId, optionId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Opción Actualizada', message: 'La opción fue actualizada correctamente.' }),
    errorNotif: () => ({ title: 'Error al Actualizar Opción', message: 'No se pudo actualizar la opción.' }),
  });
};

export const useRemoveOption = (systemSuiteId: string, moduleId: string, menuId: string, subMenuId: string) => {
  return useNotifiedMutation({
    mutationFn: (optionId: string) => systemSuiteService.removeOption(systemSuiteId, moduleId, menuId, subMenuId, optionId),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({ title: 'Opción Eliminada', message: 'La opción fue eliminada.', type: 'warning' as const }),
    errorNotif: () => ({ title: 'Error al Eliminar Opción', message: 'No se pudo eliminar la opción.' }),
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

// ─── Domain Resources Mutations ──────────────────────────────────────────────

export const useAddDomainResource = (systemSuiteId: string) => {
  return useNotifiedMutation({
    mutationFn: (payload: { moduleId?: string | null; type: 'Aggregate' | 'Entity'; code: string; name: string; description: string; }) =>
      systemSuiteService.addDomainResource(systemSuiteId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: 'Recurso de Dominio Registrado',
      message: 'El recurso de dominio ha sido registrado correctamente.',
    }),
    errorNotif: () => ({
      title: 'Error al Registrar Recurso de Dominio',
      message: 'No se pudo registrar el recurso de dominio.',
    }),
  });
};

export const useUpdateDomainResource = (systemSuiteId: string) => {
  return useNotifiedMutation({
    mutationFn: (payload: { domainResourceId: string; name: string; description: string; }) =>
      systemSuiteService.updateDomainResource(systemSuiteId, payload.domainResourceId, payload),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: 'Recurso de Dominio Actualizado',
      message: 'El recurso de dominio fue actualizado correctamente.',
    }),
    errorNotif: () => ({
      title: 'Error al Actualizar Recurso de Dominio',
      message: 'No se pudo actualizar el recurso de dominio.',
    }),
  });
};

export const useRemoveDomainResource = (systemSuiteId: string) => {
  return useNotifiedMutation({
    mutationFn: (domainResourceId: string) => systemSuiteService.removeDomainResource(systemSuiteId, domainResourceId),
    invalidateKeys: [['system-suites', systemSuiteId], ['system-suites']],
    successNotif: () => ({
      title: 'Recurso de Dominio Removido',
      message: 'El recurso de dominio fue eliminado correctamente.',
      type: 'warning' as const,
    }),
    errorNotif: () => ({
      title: 'Error al Remover Recurso de Dominio',
      message: 'No se pudo remover el recurso de dominio.',
    }),
  });
};
