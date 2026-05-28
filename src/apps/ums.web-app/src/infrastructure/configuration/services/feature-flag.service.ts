/**
 * feature-flag.service.ts
 *
 * Infrastructure service for the Configuration / FeatureFlag bounded context.
 * Queries use GraphQL (paginated list, single flag).
 * Scoped list by SystemSuite uses REST (no GraphQL resolver).
 * Commands use REST via httpClient.
 * All responses are validated with Zod before returning.
 */
import { httpClient } from '@infra/http/httpClient';
import { graphqlFeatureFlagQueries } from '@infra/configuration/queries/feature-flag.graphql';
import { logger } from '@app/utils/logger';
import {
  FeatureFlagSchema,
  FeatureFlagPageSchema,
  FeatureFlagListSchema,
  CreateFeatureFlagResponseSchema,
  AddFeatureFlagCriteriaResponseSchema,
  type FeatureFlag,
  type FeatureFlagPage,
  type CreateFeatureFlagResponse,
  type AddFeatureFlagCriteriaResponse,
} from '@domain/configuration/schemas/feature-flag.schema';
import type {
  CreateFeatureFlagPayload,
  UpdateFeatureFlagPayload,
  AddFeatureFlagCriteriaPayload,
} from '@domain/configuration/schemas/feature-flag.commands.schema';

export const featureFlagService = {
  // ── Queries ────────────────────────────────────────────────────────────────

  getAllFeatureFlags: async (params?: {
    page?: number;
    pageSize?: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    flagType?: string;
  }): Promise<FeatureFlagPage> => {
    const response = await graphqlFeatureFlagQueries.getFeatureFlags({
      page:      params?.page ?? 1,
      pageSize:  params?.pageSize ?? 20,
      search:    params?.search,
      criteria:  params?.criteria,
      status:    params?.status,
      sortBy:    params?.sortBy,
      sortOrder: params?.sortOrder,
      flagType:  params?.flagType,
    });

    const result = FeatureFlagPageSchema.safeParse(response.getFeatureFlags);
    if (!result.success) {
      logger.error('Invalid GraphQL response shape for feature flags query', result.error);
      throw new Error('Invalid GraphQL response shape for feature flags query');
    }
    return result.data;
  },

  getFeatureFlagById: async (featureFlagId: string): Promise<FeatureFlag> => {
    const response = await graphqlFeatureFlagQueries.getFeatureFlagById(featureFlagId);
    if (!response.getFeatureFlagById) throw new Error('FeatureFlag not found');
    return FeatureFlagSchema.parse(response.getFeatureFlagById);
  },

  /** REST: returns all flags scoped to a given SystemSuite (no pagination). */
  getFeatureFlagsBySystemSuite: async (systemSuiteId: string): Promise<FeatureFlag[]> => {
    const { data } = await httpClient.get(`/system-suites/${systemSuiteId}/feature-flags`);
    const result = FeatureFlagListSchema.safeParse(data);
    if (!result.success) {
      logger.error('Invalid REST response shape for feature flags by system suite', result.error);
      throw new Error('Invalid REST response for feature flags by system suite');
    }
    return result.data;
  },

  // ── Commands ───────────────────────────────────────────────────────────────

  createFeatureFlag: async (payload: CreateFeatureFlagPayload): Promise<CreateFeatureFlagResponse> => {
    const { data } = await httpClient.post('/feature-flags', payload);
    return CreateFeatureFlagResponseSchema.parse(data);
  },

  updateFeatureFlag: async (featureFlagId: string, payload: UpdateFeatureFlagPayload): Promise<void> => {
    await httpClient.put(`/feature-flags/${featureFlagId}`, payload);
  },

  activateFlag: async (featureFlagId: string): Promise<void> => {
    await httpClient.post(`/feature-flags/${featureFlagId}/activate`);
  },

  deactivateFlag: async (featureFlagId: string): Promise<void> => {
    await httpClient.post(`/feature-flags/${featureFlagId}/deactivate`);
  },

  archiveFlag: async (featureFlagId: string): Promise<void> => {
    await httpClient.post(`/feature-flags/${featureFlagId}/archive`);
  },

  addCriteria: async (
    featureFlagId: string,
    payload: AddFeatureFlagCriteriaPayload,
  ): Promise<AddFeatureFlagCriteriaResponse> => {
    const { data } = await httpClient.post(`/feature-flags/${featureFlagId}/criteria`, payload);
    return AddFeatureFlagCriteriaResponseSchema.parse(data);
  },

  removeCriteria: async (featureFlagId: string, criteriaId: string): Promise<void> => {
    await httpClient.delete(`/feature-flags/${featureFlagId}/criteria/${criteriaId}`);
  },
};

export default featureFlagService;
