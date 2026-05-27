/**
 * system-suite.graphql.ts — GraphQL query definitions for the Authorization / SystemSuite bounded context
 *
 * All read operations use these GraphQL queries.
 * Commands/transactions use REST endpoints via system-suite.service.ts.
 */
import { graphqlClient, GraphQlValidationError } from '@infra/http/graphqlClient';

const GET_SYSTEM_SUITES = `
  query SystemSuites(
    $page: Int!
    $pageSize: Int!
    $search: String
    $criteria: String
    $status: String
    $sortBy: String
    $sortOrder: String
    $tenantId: UUID
  ) {
    getSystemSuites: systemSuites(
      page: $page
      pageSize: $pageSize
      search: $search
      criteria: $criteria
      status: $status
      sortBy: $sortBy
      sortOrder: $sortOrder
      tenantId: $tenantId
    ) {
      items {
        systemSuiteId
        tenantId
        code
        name
        description
        status
        modules {
          id
          code
          name
          description
          status
          sortOrder
          menus {
            id
            code
            label
            description
            sortOrder
            subMenus {
              id
              code
              label
              description
              sortOrder
              options {
                id
                code
                label
                description
                actionCode
                sortOrder
              }
            }
          }
        }
        actions {
          id
          code
          name
        }
        domainResources {
          id
          type
          code
          name
          description
          moduleId
        }
      }
      page
      pageSize
      totalItems
      totalPages
    }
  }
`;

const GET_SYSTEM_SUITE_BY_ID = `
  query SystemSuite($systemSuiteId: UUID!) {
    getSystemSuiteById: systemSuiteById(systemSuiteId: $systemSuiteId) {
      systemSuiteId
      tenantId
      code
      name
      description
      status
      modules {
        id
        code
        name
        description
        status
        sortOrder
        menus {
          id
          code
          label
          description
          sortOrder
          subMenus {
            id
            code
            label
            description
            sortOrder
            options {
              id
              code
              label
              description
              actionCode
              sortOrder
            }
          }
        }
      }
      actions {
        id
        code
        name
      }
      domainResources {
        id
        type
        code
        name
        description
        moduleId
      }
    }
  }
`;

export interface GraphqlSystemSuiteActionDto {
  id: string;
  code: string;
  name: string;
}

export interface GraphqlDomainResourceDto {
  id: string;
  type: 'Aggregate' | 'Entity';
  code: string;
  name: string;
  description: string;
  moduleId: string | null;
}

export interface GraphqlSystemSuiteOptionDto {
  id: string;
  code: string;
  label: string;
  description: string;
  actionCode: string;
  sortOrder: number;
}

export interface GraphqlSystemSuiteSubMenuDto {
  id: string;
  code: string;
  label: string;
  description: string;
  sortOrder: number;
  options: GraphqlSystemSuiteOptionDto[];
}

export interface GraphqlSystemSuiteMenuDto {
  id: string;
  code: string;
  label: string;
  description: string;
  sortOrder: number;
  subMenus: GraphqlSystemSuiteSubMenuDto[];
}

export interface GraphqlSystemSuiteModuleDto {
  id: string;
  code: string;
  name: string;
  description: string;
  status: string;
  sortOrder: number;
  menus: GraphqlSystemSuiteMenuDto[];
}

export interface GraphqlSystemSuiteDto {
  systemSuiteId: string;
  tenantId: string;
  code: string;
  name: string;
  description: string;
  status: string;
  modules: GraphqlSystemSuiteModuleDto[];
  actions: GraphqlSystemSuiteActionDto[];
  domainResources: GraphqlDomainResourceDto[];
}

export interface GraphqlSystemSuitePage {
  items: GraphqlSystemSuiteDto[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface GetSystemSuitesResponse {
  getSystemSuites: GraphqlSystemSuitePage;
}

export interface GetSystemSuiteByIdResponse {
  getSystemSuiteById: GraphqlSystemSuiteDto | null;
}

function validateGetSystemSuitesParams(params: {
  page: number;
  pageSize: number;
}): void {
  if (!Number.isInteger(params.page) || params.page < 1) {
    throw new GraphQlValidationError('Invalid page parameter', [`page must be a positive integer, got: ${params.page}`]);
  }
  if (!Number.isInteger(params.pageSize) || params.pageSize < 1) {
    throw new GraphQlValidationError('Invalid pageSize parameter', [`pageSize must be a positive integer, got: ${params.pageSize}`]);
  }
}

export const graphqlSystemSuiteQueries = {
  getSystemSuites: async (params: {
    page: number;
    pageSize: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    tenantId?: string;
  }): Promise<GetSystemSuitesResponse> => {
    validateGetSystemSuitesParams(params);

    const variables: Record<string, unknown> = {
      page: params.page,
      pageSize: params.pageSize,
    };
    if (params.search !== undefined)     variables.search = params.search;
    if (params.criteria !== undefined)   variables.criteria = params.criteria;
    if (params.status !== undefined)     variables.status = params.status;
    if (params.sortBy !== undefined)     variables.sortBy = params.sortBy;
    if (params.sortOrder !== undefined)  variables.sortOrder = params.sortOrder;
    if (params.tenantId !== undefined)   variables.tenantId = params.tenantId;

    return graphqlClient.request<GetSystemSuitesResponse>(GET_SYSTEM_SUITES, variables);
  },

  getSystemSuiteById: async (systemSuiteId: string): Promise<GetSystemSuiteByIdResponse> => {
    if (!systemSuiteId || systemSuiteId.trim() === '') {
      throw new GraphQlValidationError('Invalid systemSuiteId parameter', ['systemSuiteId must be a non-empty string']);
    }
    return graphqlClient.request<GetSystemSuiteByIdResponse>(GET_SYSTEM_SUITE_BY_ID, { systemSuiteId });
  },
};
