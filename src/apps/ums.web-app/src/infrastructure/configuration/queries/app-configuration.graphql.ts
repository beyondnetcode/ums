import { graphqlClient } from '@infra/http/graphqlClient';
import type { AppConfiguration, AppConfigurationPage } from '@domain/configuration/schemas/app-configuration.schema';

export interface GetAppConfigurationsResponse {
  appConfigurations: AppConfigurationPage;
}

export interface GetAppConfigurationByIdResponse {
  appConfigurationById: AppConfiguration | null;
}

export const graphqlAppConfigurationQueries = {
  getAppConfigurations: async (params: {
    page: number;
    pageSize: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    scope?: string;
    tenantId?: string;
    systemSuiteId?: string;
    moduleId?: string;
  }): Promise<GetAppConfigurationsResponse> => {
    const query = `
      query GetAppConfigurations(
        $page: Int!,
        $pageSize: Int!,
        $search: String,
        $criteria: String,
        $status: String,
        $sortBy: String,
        $sortOrder: String,
        $scope: String,
        $tenantId: UUID,
        $systemSuiteId: UUID,
        $moduleId: UUID
      ) {
        appConfigurations(
          page: $page,
          pageSize: $pageSize,
          search: $search,
          criteria: $criteria,
          status: $status,
          sortBy: $sortBy,
          sortOrder: $sortOrder,
          scope: $scope,
          tenantId: $tenantId,
          systemSuiteId: $systemSuiteId,
          moduleId: $moduleId
        ) {
          items {
            appConfigurationId
            tenantId
            systemSuiteId
            moduleId
            code
            value
            description
            scope
            isInheritable
            isEncrypted
            version
            status
          }
          page
          pageSize
          totalItems
          totalPages
        }
      }
    `;

    return graphqlClient.request<GetAppConfigurationsResponse>(query, params);
  },

  getAppConfigurationById: async (appConfigurationId: string): Promise<GetAppConfigurationByIdResponse> => {
    const query = `
      query GetAppConfigurationById($appConfigurationId: UUID!) {
        appConfigurationById(appConfigurationId: $appConfigurationId) {
          appConfigurationId
          tenantId
          systemSuiteId
          moduleId
          code
          value
          description
          scope
          isInheritable
          isEncrypted
          version
          status
        }
      }
    `;

    return graphqlClient.request<GetAppConfigurationByIdResponse>(query, { appConfigurationId });
  },
};