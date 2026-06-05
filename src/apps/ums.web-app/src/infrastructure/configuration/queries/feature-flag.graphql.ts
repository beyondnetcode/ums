/**
 * feature-flag.graphql.ts — GraphQL query definitions for FeatureFlag
 *
 * Read operations (paginated list, single flag) use GraphQL.
 * The scoped list by SystemSuite uses REST (no dedicated GraphQL resolver).
 */
import { graphqlClient, GraphQlValidationError } from '@infra/http/graphqlClient';

const GET_FEATURE_FLAGS = `
  query FeatureFlags(
    $page: Int!
    $pageSize: Int!
    $search: String
    $criteria: String
    $status: String
    $sortBy: String
    $sortOrder: String
    $flagType: String
  ) {
    featureFlags(
      page: $page
      pageSize: $pageSize
      search: $search
      criteria: $criteria
      status: $status
      sortBy: $sortBy
      sortOrder: $sortOrder
      flagType: $flagType
    ) {
      items {
        featureFlagId
        systemSuiteId
        systemSuiteCode
        systemSuiteName
        flagCode
        flagType
        flagTargets
        status
        rolloutPercentage
        criteria {
          criteriaId
          criteriaType
          operator
          value
          createdAtUtc
        }
      }
      page
      pageSize
      totalItems
      totalPages
    }
  }
`;

const GET_FEATURE_FLAG_BY_ID = `
  query FeatureFlagById($id: UUID!) {
    featureFlagById(id: $id) {
      featureFlagId
      systemSuiteId
      systemSuiteCode
      systemSuiteName
      flagCode
      flagType
      flagTargets
      status
      rolloutPercentage
      criteria {
        criteriaId
        criteriaType
        operator
        value
        createdAtUtc
      }
    }
  }
`;

// ── DTOs for GraphQL responses ─────────────────────────────────────────────────

export interface GraphqlFeatureFlagCriteriaDto {
  criteriaId:   string;
  criteriaType: string;
  operator:     string;
  value:        string;
  createdAtUtc: string;
}

export interface GraphqlFeatureFlagDto {
  featureFlagId:      string;
  systemSuiteId:      string;
  systemSuiteCode:    string;
  systemSuiteName:    string;
  flagCode:           string;
  flagType:           string;
  flagTargets:        string;
  status:             string;
  rolloutPercentage:  number | null;
  criteria:           GraphqlFeatureFlagCriteriaDto[];
}

export interface GraphqlFeatureFlagPage {
  items:      GraphqlFeatureFlagDto[];
  page:       number;
  pageSize:   number;
  totalItems: number;
  totalPages: number;
}

export interface GetFeatureFlagsResponse {
  featureFlags: GraphqlFeatureFlagPage;
}

export interface GetFeatureFlagByIdResponse {
  featureFlagById: GraphqlFeatureFlagDto | null;
}

function validatePageParams(params: { page: number; pageSize: number }): void {
  if (!Number.isInteger(params.page) || params.page < 1)
    throw new GraphQlValidationError('Invalid page parameter', [`page must be a positive integer, got: ${params.page}`]);
  if (!Number.isInteger(params.pageSize) || params.pageSize < 1)
    throw new GraphQlValidationError('Invalid pageSize parameter', [`pageSize must be a positive integer, got: ${params.pageSize}`]);
}

export const graphqlFeatureFlagQueries = {
  getFeatureFlags: async (params: {
    page: number;
    pageSize: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    flagType?: string;
  }): Promise<GetFeatureFlagsResponse> => {
    validatePageParams(params);
    const variables: Record<string, unknown> = {
      page:     params.page,
      pageSize: params.pageSize,
    };
    if (params.search    !== undefined) variables.search    = params.search;
    if (params.criteria  !== undefined) variables.criteria  = params.criteria;
    if (params.status    !== undefined) variables.status    = params.status;
    if (params.sortBy    !== undefined) variables.sortBy    = params.sortBy;
    if (params.sortOrder !== undefined) variables.sortOrder = params.sortOrder;
    if (params.flagType  !== undefined) variables.flagType  = params.flagType;
    return graphqlClient.request<GetFeatureFlagsResponse>(GET_FEATURE_FLAGS, variables);
  },

  getFeatureFlagById: async (id: string): Promise<GetFeatureFlagByIdResponse> => {
    if (!id || id.trim() === '')
      throw new GraphQlValidationError('Invalid id parameter', ['id must be a non-empty string']);
    return graphqlClient.request<GetFeatureFlagByIdResponse>(GET_FEATURE_FLAG_BY_ID, { id });
  },
};
