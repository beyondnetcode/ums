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
    $systemSuiteId: UUID
    $roleId: UUID
  ) {
    permissionTemplates(
      page: $page
      pageSize: $pageSize
      search: $search
      criteria: $criteria
      status: $status
      sortBy: $sortBy
      sortOrder: $sortOrder
      tenantId: $tenantId
      systemSuiteId: $systemSuiteId
      roleId: $roleId
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
    permissionTemplateById(templateId: $templateId) {
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
    systemSuiteId?: string;
    roleId?: string;
  }) => {
    return graphqlClient.request<{ permissionTemplates: unknown }>(GET_PERMISSION_TEMPLATES, {
      page: params.page,
      pageSize: params.pageSize,
      search: params.search ?? null,
      criteria: params.criteria ?? 'version',
      status: params.status ?? 'all',
      sortBy: params.sortBy ?? 'version',
      sortOrder: params.sortOrder ?? 'asc',
      tenantId: params.tenantId ?? null,
      systemSuiteId: params.systemSuiteId ?? null,
      roleId: params.roleId ?? null,
    });
  },

  getPermissionTemplateById: async (templateId: string) => {
    return graphqlClient.request<{ permissionTemplateById: unknown }>(
      GET_PERMISSION_TEMPLATE_BY_ID,
      {
        templateId,
      }
    );
  },
};
