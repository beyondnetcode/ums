import { httpClient } from '@infra/http/httpClient';
import { logger } from '@app/utils/logger';
import {
  ParameterDefinitionSchema,
  ParameterDefinitionPageSchema,
  type ParameterDefinition,
  type ParameterDefinitionPage,
  type ParameterDefinitionFilter,
  type CreateParameterDefinitionPayload,
  type UpdateParameterDefinitionPayload,
} from '@domain/configuration/schemas/parameter-catalog/parameter-definition.schema';

export const parameterCatalogService = {
  getAll: async (filter?: ParameterDefinitionFilter): Promise<ParameterDefinitionPage> => {
    const params = new URLSearchParams();
    if (filter?.search) params.set('search', filter.search);
    if (filter?.scopeId) params.set('scopeId', String(filter.scopeId));
    if (filter?.isActive !== undefined) params.set('isActive', String(filter.isActive));

    try {
      const { data } = await httpClient.get<{ items: ParameterDefinition[]; totalItems: number }>(
        `/parameter-definitions?${params.toString()}`
      );
      const result = ParameterDefinitionPageSchema.safeParse(data);
      if (!result.success) {
        logger.error('Invalid response for parameter definitions', result.error);
        throw new Error('Invalid response for parameter definitions');
      }
      return result.data;
    } catch (error) {
      logger.error('Failed to fetch parameter definitions', error);
      throw error;
    }
  },

  getById: async (id: string): Promise<ParameterDefinition> => {
    const { data } = await httpClient.get<ParameterDefinition>(`/parameter-definitions/${id}`);
    return ParameterDefinitionSchema.parse(data);
  },

  createParameterDefinition: async (
    payload: CreateParameterDefinitionPayload
  ): Promise<ParameterDefinition> => {
    try {
      const { data } = await httpClient.post<ParameterDefinition>(
        '/parameter-definitions',
        payload
      );
      return ParameterDefinitionSchema.parse(data);
    } catch (error) {
      logger.error('Failed to create parameter definition', error);
      throw error;
    }
  },

  updateParameterDefinition: async (
    id: string,
    payload: UpdateParameterDefinitionPayload
  ): Promise<ParameterDefinition> => {
    try {
      const { data } = await httpClient.patch<ParameterDefinition>(
        `/parameter-definitions/${id}`,
        payload
      );
      return ParameterDefinitionSchema.parse(data);
    } catch (error) {
      logger.error('Failed to update parameter definition', error);
      throw error;
    }
  },

  deleteParameterDefinition: async (id: string): Promise<void> => {
    try {
      await httpClient.delete(`/parameter-definitions/${id}`);
    } catch (error) {
      logger.error('Failed to delete parameter definition', error);
      throw error;
    }
  },

  clearCache: () => {},
};

export default parameterCatalogService;
