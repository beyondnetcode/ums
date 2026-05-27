import { graphqlClient, GraphQlValidationError } from '@infra/http/graphqlClient';
import type { Role } from '@domain/authorization/schemas/role.schema';

const GET_ROLES_BY_SYSTEM_SUITE = `
  query RolesBySystemSuite($systemSuiteId: UUID!) {
    rolesBySystemSuite(systemSuiteId: $systemSuiteId) {
      roleId
      tenantId
      systemSuiteId
      parentRoleId
      code
      value
      description
      hierarchyLevel
      promotionOrder
      isActive
    }
  }
`;

interface RolesBySystemSuiteResponse {
  rolesBySystemSuite: Role[];
}

export const graphqlRoleQueries = {
  getRolesBySystemSuite: async (systemSuiteId: string): Promise<RolesBySystemSuiteResponse> => {
    if (!systemSuiteId.trim()) {
      throw new GraphQlValidationError('Invalid systemSuiteId parameter', ['systemSuiteId must be provided']);
    }

    return graphqlClient.request<RolesBySystemSuiteResponse>(GET_ROLES_BY_SYSTEM_SUITE, { systemSuiteId });
  },
};
