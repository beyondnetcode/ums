import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import React from 'react';
import { UserAccountListPanel } from './UserAccountListPanel';
import { Tenant } from '@domain/identity/models/tenant.model';
import { UserAccount } from '@domain/identity/models/user-account.model';

const mockTenants: Tenant[] = [
  {
    tenantId: 'tenant-1',
    code: 'RANSA',
    name: 'Ransa Comercial',
    type: 'INTERNAL',
    status: 'Active',
    parentTenantId: null,
    companyReference: null,
  },
  {
    tenantId: 'tenant-2',
    code: 'NEPTUNIA',
    name: 'Neptunia S.A.',
    type: 'CLIENT',
    status: 'Active',
    parentTenantId: null,
    companyReference: null,
  },
];

const mockAccounts: UserAccount[] = [
  {
    userAccountId: 'user-1',
    tenantId: 'tenant-1',
    branchId: null,
    email: 'admin@ransa.pe',
    category: 'Internal',
    status: 'Active',
    identityReference: null,
    identityReferenceType: null,
  },
];

describe('UserAccountListPanel', () => {
  it('renders title, search input, and active tenant selector dropdown', () => {
    const onTenantChange = vi.fn();
    render(
      <UserAccountListPanel
        accounts={mockAccounts}
        selectedId="user-1"
        isLoading={false}
        error={null}
        viewMode="list"
        onViewModeChange={() => {}}
        searchCriteria="email"
        onSearchCriteriaChange={() => {}}
        searchValue=""
        onSearchValueChange={() => {}}
        onSearchSubmit={(e) => e.preventDefault()}
        onRegisterNew={() => {}}
        sortBy="email"
        onSortByChange={() => {}}
        sortOrder="asc"
        onSortOrderToggle={() => {}}
        activeFilter="all"
        onFilterChange={() => {}}
        page={1}
        pageSize={20}
        totalItems={1}
        totalPages={1}
        appliedTerm=""
        onPageChange={() => {}}
        onSelectAccount={() => {}}
        criteriaOptions={[{ label: 'Email', value: 'email' }]}
        filterOptions={[{ label: 'All', value: 'all' }]}
        sortOptions={[{ label: 'Email', value: 'email' }]}
        tenants={mockTenants}
        selectedTenantId="tenant-1"
        onTenantChange={onTenantChange}
      />
    );

    // Verify title and subtitle are present
    expect(screen.getByText('Cuentas de Usuario')).toBeInTheDocument();

    // Verify active tenant selector is rendered
    expect(screen.getByLabelText(/Active Tenant/i)).toBeInTheDocument();
    expect(screen.getByText('Ransa Comercial (RANSA)')).toBeInTheDocument();
    expect(screen.getByText('Neptunia S.A. (NEPTUNIA)')).toBeInTheDocument();

    // Verify account row renders
    expect(screen.getByText('admin@ransa.pe')).toBeInTheDocument();
  });

  it('triggers onTenantChange callback when changing selected tenant', () => {
    const onTenantChange = vi.fn();
    render(
      <UserAccountListPanel
        accounts={mockAccounts}
        selectedId="user-1"
        isLoading={false}
        error={null}
        viewMode="list"
        onViewModeChange={() => {}}
        searchCriteria="email"
        onSearchCriteriaChange={() => {}}
        searchValue=""
        onSearchValueChange={() => {}}
        onSearchSubmit={(e) => e.preventDefault()}
        onRegisterNew={() => {}}
        sortBy="email"
        onSortByChange={() => {}}
        sortOrder="asc"
        onSortOrderToggle={() => {}}
        activeFilter="all"
        onFilterChange={() => {}}
        page={1}
        pageSize={20}
        totalItems={1}
        totalPages={1}
        appliedTerm=""
        onPageChange={() => {}}
        onSelectAccount={() => {}}
        criteriaOptions={[{ label: 'Email', value: 'email' }]}
        filterOptions={[{ label: 'All', value: 'all' }]}
        sortOptions={[{ label: 'Email', value: 'email' }]}
        tenants={mockTenants}
        selectedTenantId="tenant-1"
        onTenantChange={onTenantChange}
      />
    );

    const select = screen.getByLabelText(/Active Tenant/i);
    fireEvent.change(select, { target: { value: 'tenant-2' } });

    expect(onTenantChange).toHaveBeenCalledWith('tenant-2');
  });
});
