import { graphqlClient } from '@infra/http/graphqlClient';

const GET_PROFILES = `
  query GetProfiles(
    $page: Int!
    $pageSize: Int!
    $search: String
    $criteria: String
    $status: String
    $sortBy: String
    $sortOrder: String
    $tenantId: UUID
    $userId: UUID
  ) {
    profiles(
      page: $page
      pageSize: $pageSize
      search: $search
      criteria: $criteria
      status: $status
      sortBy: $sortBy
      sortOrder: $sortOrder
      tenantId: $tenantId
      userId: $userId
    ) {
      items {
        profileId
        tenantId
        tenantCode
        tenantName
        userId
        userEmail
        roleId
        roleCode
        roleName
        systemSuiteId
        systemSuiteCode
        systemSuiteName
        branchId
        branchName
        scope
        isActive
        permissionCount
      }
      page
      pageSize
      totalItems
      totalPages
    }
  }
`;

const GET_PROFILE_BY_ID = `
  query GetProfileById($profileId: UUID!) {
    profileById(profileId: $profileId) {
      profileId
      tenantId
      tenantCode
      tenantName
      userId
      userEmail
      roleId
      roleCode
      roleName
      systemSuiteId
      systemSuiteCode
      systemSuiteName
      branchId
      branchName
      scope
      isActive
      permissionCount
      permissions {
        permissionId
        profileId
        templateId
        targetType
        targetId
        targetName
        actionId
        actionName
        isAllowed
        isDenied
        isActive
        isOverride
      }
    }
  }
`;

export const graphqlProfileQueries = {
  getProfiles: async (params: {
    page: number;
    pageSize: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    tenantId?: string;
    userId?: string;
  }) => {
    return graphqlClient.request<{ profiles: unknown }>(GET_PROFILES, {
      page: params.page,
      pageSize: params.pageSize,
      search: params.search ?? null,
      criteria: params.criteria ?? 'userId',
      status: params.status ?? 'all',
      sortBy: params.sortBy ?? 'userId',
      sortOrder: params.sortOrder ?? 'asc',
      tenantId: params.tenantId ?? null,
      userId: params.userId ?? null,
    });
  },

  getProfileById: async (profileId: string) => {
    return graphqlClient.request<{ profileById: unknown }>(GET_PROFILE_BY_ID, {
      profileId,
    });
  },
};
