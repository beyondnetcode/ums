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
    getProfiles(
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
        userId
        roleId
        branchId
        scope
        isActive
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
    getProfileById(profileId: $profileId) {
      profileId
      tenantId
      userId
      roleId
      branchId
      scope
      isActive
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
    return graphqlClient.request<{ getProfiles: unknown }>(GET_PROFILES, {
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
    return graphqlClient.request<{ getProfileById: unknown }>(GET_PROFILE_BY_ID, {
      profileId,
    });
  },
};
