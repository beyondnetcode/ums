import { httpClient } from '@infra/http/httpClient';
import { graphqlRoleQueries } from '@infra/authorization/queries/role.graphql';
import {
  CreateRoleResponseSchema,
  RoleListSchema,
  type CreateRolePayload,
  type CreateRoleResponse,
  type Role,
  type UpdateRolePayload,
} from '@domain/authorization/schemas/role.schema';

export const roleService = {
  getBySystemSuite: async (systemSuiteId: string): Promise<Role[]> => {
    const response = await graphqlRoleQueries.getRolesBySystemSuite(systemSuiteId);
    return RoleListSchema.parse(response.rolesBySystemSuite);
  },

  create: async (systemSuiteId: string, payload: CreateRolePayload): Promise<CreateRoleResponse> => {
    const { data } = await httpClient.post(`/system-suites/${systemSuiteId}/roles`, {
      systemSuiteId,
      ...payload,
    });
    return CreateRoleResponseSchema.parse(data);
  },

  update: async (systemSuiteId: string, roleId: string, payload: UpdateRolePayload): Promise<void> => {
    await httpClient.put(`/system-suites/${systemSuiteId}/roles/${roleId}`, { roleId, ...payload });
  },

  setActive: async (systemSuiteId: string, roleId: string, isActive: boolean): Promise<void> => {
    const action = isActive ? 'activate' : 'deactivate';
    await httpClient.post(`/system-suites/${systemSuiteId}/roles/${roleId}/${action}`);
  },
};

export default roleService;
