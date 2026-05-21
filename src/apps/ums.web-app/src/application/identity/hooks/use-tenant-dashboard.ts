/**
 * useTenantDashboard.ts — Orchestrates state and API calls for TenantDashboardScreen.
 *
 * H-2: Extracted control state, selection logic, and API wiring from the screen component.
 */
import React, { useState, useEffect, useCallback } from 'react';
import { useGetAllTenants } from '@app/identity/hooks/use-tenant';
import { useLocalOverrides } from '@app/hooks/use-local-overrides';
import { Tenant } from '@domain/identity/models/tenant.model';
import { TENANT_PAGE_SIZE } from '@domain/identity/constants/tenant.constants';

export interface TenantDashboardState {
  selectedId: string;
  showDiscardDialog: boolean;
  pendingNavigationId: string | null;
  activeConsoleTab: 'branches' | 'providers' | 'branding';
  isTenantEditing: boolean;
  isCreateOpen: boolean;
  viewMode: 'list' | 'thumbnail';
  searchCriteria: string;
  searchValue: string;
  appliedQuery: { criteria: string; term: string };
  activeFilter: string;
  sortBy: string;
  sortOrder: 'asc' | 'desc';
  page: number;
  pageSize: number;
}

export interface TenantDashboardActions {
  setSelectedId: React.Dispatch<React.SetStateAction<string>>;
  setShowDiscardDialog: React.Dispatch<React.SetStateAction<boolean>>;
  setPendingNavigationId: React.Dispatch<React.SetStateAction<string | null>>;
  setActiveConsoleTab: React.Dispatch<React.SetStateAction<'branches' | 'providers' | 'branding'>>;
  setIsTenantEditing: React.Dispatch<React.SetStateAction<boolean>>;
  setIsCreateOpen: React.Dispatch<React.SetStateAction<boolean>>;
  setViewMode: React.Dispatch<React.SetStateAction<'list' | 'thumbnail'>>;
  setSearchCriteria: React.Dispatch<React.SetStateAction<string>>;
  setSearchValue: React.Dispatch<React.SetStateAction<string>>;
  setAppliedQuery: React.Dispatch<React.SetStateAction<{ criteria: string; term: string }>>;
  setActiveFilter: React.Dispatch<React.SetStateAction<string>>;
  setSortBy: React.Dispatch<React.SetStateAction<string>>;
  setSortOrder: React.Dispatch<React.SetStateAction<'asc' | 'desc'>>;
  setPage: React.Dispatch<React.SetStateAction<number>>;
  handleSelectTenant: (id: string) => void;
  confirmDiscard: () => void;
  patchTenant: (tenantId: string, patch: Partial<Tenant>) => void;
  handleCreateSuccess: (newTenantId: string) => void;
  handleQuerySubmit: (e: React.FormEvent) => void;
  handleResetQuery: () => void;
}

export function useTenantDashboard(): TenantDashboardState & TenantDashboardActions & {
  knownTenants: Tenant[];
  isLoadingList: boolean;
  activeTenant: Tenant | undefined;
  parentTenant: Tenant | null;
  isRootTenant: boolean;
  consoleTabs: Array<'branches' | 'providers' | 'branding'>;
  totalItems: number;
  totalPages: number;
  startIndex: number;
} {
  const [selectedId, setSelectedId] = useState('');
  const [showDiscardDialog, setShowDiscardDialog] = useState(false);
  const [pendingNavigationId, setPendingNavigationId] = useState<string | null>(null);
  const [activeConsoleTab, setActiveConsoleTab] = useState<'branches' | 'providers' | 'branding'>('branches');
  const [isTenantEditing, setIsTenantEditing] = useState(false);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewMode, setViewMode] = useState<'list' | 'thumbnail'>('list');
  const [searchCriteria, setSearchCriteria] = useState('name');
  const [searchValue, setSearchValue] = useState('');
  const [appliedQuery, setAppliedQuery] = useState<{ criteria: string; term: string }>({ criteria: 'name', term: '' });
  const [activeFilter, setActiveFilter] = useState('all');
  const [sortBy, setSortBy] = useState('name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState(1);
  const pageSize = TENANT_PAGE_SIZE;

  const { data: tenantPage, isLoading: isLoadingList } = useGetAllTenants({
    page,
    pageSize,
    search: appliedQuery.term,
    criteria: appliedQuery.criteria,
    status: activeFilter,
    sortBy,
    sortOrder,
  });

  const { items: knownTenants, patchItem: patchLocalTenant } = useLocalOverrides<Tenant>(
    tenantPage?.items,
    'tenantId',
  );

  const activeTenant = knownTenants.find((tenant) => tenant.tenantId === selectedId);
  const isRootTenant = activeTenant?.parentTenantId === null;

  const consoleTabs = (
    ['branches', 'providers', 'branding'] as Array<'branches' | 'providers' | 'branding'>
  ).filter((tab) => tab !== 'branding' || isRootTenant);

  const parentTenant = activeTenant?.parentTenantId
    ? knownTenants.find((t) => t.tenantId === activeTenant.parentTenantId) ?? null
    : null;

  const hasPendingChanges = isTenantEditing;

  const applyTenantSelection = useCallback((id: string) => {
    setIsTenantEditing(false);
    setActiveConsoleTab('branches');
    setSelectedId(id);
  }, []);

  const handleSelectTenant = useCallback((id: string) => {
    if (id === selectedId) return;
    if (hasPendingChanges) {
      setPendingNavigationId(id);
      setShowDiscardDialog(true);
      return;
    }
    applyTenantSelection(id);
  }, [selectedId, hasPendingChanges, applyTenantSelection]);

  const confirmDiscard = useCallback(() => {
    if (pendingNavigationId) applyTenantSelection(pendingNavigationId);
    setPendingNavigationId(null);
    setShowDiscardDialog(false);
  }, [pendingNavigationId, applyTenantSelection]);

  useEffect(() => {
    if (!selectedId && knownTenants.length > 0) {
      const first = knownTenants.find((t) => t.parentTenantId === null) ?? knownTenants[0];
      applyTenantSelection(first.tenantId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [knownTenants]);

  const patchTenant = useCallback((tenantId: string, patch: Partial<Tenant>) => {
    patchLocalTenant(tenantId, patch);
  }, [patchLocalTenant]);

  const handleCreateSuccess = useCallback((newTenantId: string) => {
    setPage(1);
    setAppliedQuery({ criteria: 'name', term: '' });
    setSearchValue('');
    applyTenantSelection(newTenantId);
  }, [applyTenantSelection]);

  const handleQuerySubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    if (searchCriteria === 'id' && searchValue.trim()) handleSelectTenant(searchValue.trim());
    setAppliedQuery({ criteria: searchCriteria, term: searchValue });
  }, [searchCriteria, searchValue, handleSelectTenant]);

  const handleResetQuery = useCallback(() => {
    setSearchValue('');
    setAppliedQuery({ criteria: 'name', term: '' });
    setPage(1);
  }, []);

  const totalItems = tenantPage?.totalItems ?? 0;
  const totalPages = tenantPage?.totalPages ?? 0;
  const startIndex = (page - 1) * pageSize;

  return {
    selectedId, setSelectedId,
    showDiscardDialog, setShowDiscardDialog,
    pendingNavigationId, setPendingNavigationId,
    activeConsoleTab, setActiveConsoleTab,
    isTenantEditing, setIsTenantEditing,
    isCreateOpen, setIsCreateOpen,
    viewMode, setViewMode,
    searchCriteria, setSearchCriteria,
    searchValue, setSearchValue,
    appliedQuery, setAppliedQuery,
    activeFilter, setActiveFilter,
    sortBy, setSortBy,
    sortOrder, setSortOrder,
    page, setPage,
    pageSize,
    handleSelectTenant,
    confirmDiscard,
    patchTenant,
    handleCreateSuccess,
    handleQuerySubmit,
    handleResetQuery,
    knownTenants,
    isLoadingList,
    activeTenant,
    parentTenant,
    isRootTenant,
    consoleTabs,
    totalItems,
    totalPages,
    startIndex,
  };
}
