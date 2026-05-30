import { useQuery } from '@tanstack/react-query';
import permissionTemplateService from '@infra/authorization/services/permission-template.service';
import { useNotifiedMutation } from '@app/hooks/use-notified-mutation';
import {
  type CreatePermissionTemplatePayload,
  type AddTemplateItemPayload,
  type PermissionTemplatePage,
  type PermissionTemplateDetail,
} from '@domain/authorization/models/permission-template.model';
import { getHttpStatus, getRetryOptions } from '@app/utils/error-utils';
import { CONTEXT_QUERY_CONFIG } from '@app/shared/config/query.config';

// ─── Query params ────────────────────────────────────────────────────────────

export interface PermissionTemplateQueryParams {
  page: number;
  pageSize: number;
  search?: string;
  criteria?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  tenantId?: string;
  systemSuiteId?: string;
  roleId?: string;
}

// ─── Queries ─────────────────────────────────────────────────────────────────

export const useGetAllPermissionTemplates = (params: PermissionTemplateQueryParams | null) =>
  useQuery<PermissionTemplatePage>({
    queryKey: [
      'permission-templates',
      params?.page,
      params?.pageSize,
      params?.search,
      params?.criteria,
      params?.status,
      params?.sortBy,
      params?.sortOrder,
      params?.tenantId,
    ],
    queryFn: () => permissionTemplateService.getAll(params!),
    enabled: !!params,
    ...CONTEXT_QUERY_CONFIG.PERMISSION_TEMPLATE,
    ...getRetryOptions({ maxRetries: 1 }),
  });

export const useGetPermissionTemplate = (templateId: string | null) =>
  useQuery<PermissionTemplateDetail | null>({
    queryKey: ['permission-templates', templateId],
    queryFn: async () => {
      if (!templateId) throw new Error('Template ID required');
      try {
        return await permissionTemplateService.getById(templateId);
      } catch (err) {
        if (getHttpStatus(err) === 404) return null;
        throw err;
      }
    },
    enabled: !!templateId,
    ...CONTEXT_QUERY_CONFIG.PERMISSION_TEMPLATE,
    ...getRetryOptions({ maxRetries: 1 }),
  });

// ─── Template lifecycle mutations ─────────────────────────────────────────────

export const useCreatePermissionTemplate = () =>
  useNotifiedMutation({
    mutationFn: (payload: CreatePermissionTemplatePayload) =>
      permissionTemplateService.create(payload),
    invalidateKeys: [['permission-templates']],
    successNotif: () => ({
      title: 'Plantilla Creada',
      message: 'La plantilla de permisos fue creada en estado Borrador.',
    }),
    errorNotif: () => ({
      title: 'Error al Crear Plantilla',
      message: 'No se pudo crear la plantilla de permisos.',
    }),
  });

export const usePublishPermissionTemplate = (templateId: string) =>
  useNotifiedMutation({
    mutationFn: () => permissionTemplateService.publish(templateId),
    invalidateKeys: [['permission-templates', templateId], ['permission-templates']],
    successNotif: () => ({
      title: 'Plantilla Publicada',
      message: 'La plantilla fue publicada y está disponible para asignación a perfiles.',
    }),
    errorNotif: () => ({
      title: 'Error al Publicar',
      message: 'No se pudo publicar la plantilla. Verifique que esté en estado Borrador.',
    }),
  });

export const useDeprecatePermissionTemplate = (templateId: string) =>
  useNotifiedMutation({
    mutationFn: () => permissionTemplateService.deprecate(templateId),
    invalidateKeys: [['permission-templates', templateId], ['permission-templates']],
    successNotif: () => ({
      title: 'Plantilla Descontinuada',
      message: 'La plantilla fue descontinuada. Ya no puede asignarse a nuevos perfiles.',
      type: 'warning' as const,
    }),
    errorNotif: () => ({
      title: 'Error al Descontinuar',
      message: 'No se pudo descontinuar la plantilla.',
    }),
  });

export const useDeletePermissionTemplate = () =>
  useNotifiedMutation({
    mutationFn: (templateId: string) => permissionTemplateService.delete(templateId),
    invalidateKeys: [['permission-templates']],
    successNotif: () => ({
      title: 'Plantilla Eliminada',
      message: 'La plantilla de permisos fue eliminada.',
      type: 'warning' as const,
    }),
    errorNotif: () => ({
      title: 'Error al Eliminar',
      message:
        'No se pudo eliminar la plantilla. Solo se pueden eliminar plantillas en estado Borrador.',
    }),
  });

// ─── Item mutations ───────────────────────────────────────────────────────────

export const useAddTemplateItem = (templateId: string) =>
  useNotifiedMutation({
    mutationFn: (payload: AddTemplateItemPayload) =>
      permissionTemplateService.addItem(templateId, payload),
    invalidateKeys: [['permission-templates', templateId]],
    successNotif: () => ({
      title: 'Permiso Agregado',
      message: 'El ítem de permiso fue agregado a la plantilla.',
    }),
    errorNotif: () => ({
      title: 'Error al Agregar Permiso',
      message: 'No se pudo agregar el ítem de permiso.',
    }),
  });

export const useRemoveTemplateItem = (templateId: string) =>
  useNotifiedMutation({
    mutationFn: (itemId: string) => permissionTemplateService.removeItem(templateId, itemId),
    invalidateKeys: [['permission-templates', templateId]],
    successNotif: () => ({
      title: 'Permiso Eliminado',
      message: 'El ítem fue removido de la plantilla.',
      type: 'warning' as const,
    }),
    errorNotif: () => ({
      title: 'Error al Eliminar Permiso',
      message: 'No se pudo remover el ítem de permiso.',
    }),
  });

export const useSetTemplateItemEffect = (templateId: string) =>
  useNotifiedMutation({
    mutationFn: ({ itemId, effect }: { itemId: string; effect: 'Allow' | 'Deny' | 'Neutral' }) =>
      permissionTemplateService.setItemEffect(templateId, itemId, effect),
    invalidateKeys: [['permission-templates', templateId]],
    successNotif: () => ({
      title: 'Efecto Actualizado',
      message: 'El efecto del permiso fue actualizado.',
    }),
    errorNotif: () => ({
      title: 'Error al Actualizar Efecto',
      message: 'No se pudo actualizar el efecto.',
    }),
  });

export const useActivateTemplateItem = (templateId: string) =>
  useNotifiedMutation({
    mutationFn: (itemId: string) => permissionTemplateService.activateItem(templateId, itemId),
    invalidateKeys: [['permission-templates', templateId]],
    successNotif: () => ({ title: 'Ítem Activado', message: 'El ítem de permiso fue activado.' }),
    errorNotif: () => ({ title: 'Error al Activar', message: 'No se pudo activar el ítem.' }),
  });

export const useDeactivateTemplateItem = (templateId: string) =>
  useNotifiedMutation({
    mutationFn: (itemId: string) => permissionTemplateService.deactivateItem(templateId, itemId),
    invalidateKeys: [['permission-templates', templateId]],
    successNotif: () => ({
      title: 'Ítem Desactivado',
      message: 'El ítem de permiso fue desactivado.',
      type: 'info' as const,
    }),
    errorNotif: () => ({ title: 'Error al Desactivar', message: 'No se pudo desactivar el ítem.' }),
  });
