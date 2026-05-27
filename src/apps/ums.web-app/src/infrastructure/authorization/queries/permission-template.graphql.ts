/**
 * permission-template.graphql.ts — GraphQL queries for PermissionTemplate (read path).
 * Commands use REST via permission-template.service.ts.
 */
import { graphqlClient } from '@infra/http/graphqlClient';

const GET_PERMISSION_TEMPLATES = `
  query PermissionTemplates(
    $page: Int!
    $pageSize: Int!
    $search: String
    $criteria: String
    $status: String
    $sortBy: String
    $sortOrder: String
    $tenantId: UUID
  ) {
    getPermissionTemplates: permissionTemplates(
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
        templateId
        tenantId
        roleId
        roleName
        systemSuiteId
        systemSuiteName
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

const GET_PERMISSION_TEMPLATE_BY_ID = `
  query PermissionTemplateById($templateId: UUID!) {
    getPermissionTemplateById: permissionTemplateById(templateId: $templateId) {
      templateId
      tenantId
      roleId
      roleName
      systemSuiteId
      systemSuiteName
      version
      status
      items {
        itemId
        targetType
        targetId
        targetName
        actionId
        actionName
        isAllowed
        isDenied
        isActive
      }
    }
  }
`;

export const graphqlPermissionTemplateQueries = {
  getPermissionTemplates: async (params: {
    page: number;
    pageSize: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    tenantId?: string;
  }) => {
    return graphqlClient.request<{ getPermissionTemplates: unknown }>(GET_PERMISSION_TEMPLATES, {
      page: params.page,
      pageSize: params.pageSize,
      search: params.search ?? null,
      criteria: params.criteria ?? 'version',
      status: params.status ?? 'all',
      sortBy: params.sortBy ?? 'version',
      sortOrder: params.sortOrder ?? 'asc',
      tenantId: params.tenantId ?? null,
    });
  },

  getPermissionTemplateById: async (templateId: string) => {
    return graphqlClient.request<{ getPermissionTemplateById: unknown }>(GET_PERMISSION_TEMPLATE_BY_ID, {
      templateId,
    });
  },
};
