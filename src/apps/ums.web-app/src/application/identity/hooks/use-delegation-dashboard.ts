import React, { useState, useEffect, useCallback } from 'react';
import { useGetDelegationsByDelegatedAdmin, useGetDelegationsByDelegatingAdmin } from './use-delegation';
import { Delegation } from '@domain/identity/models/delegation.model';
import { useLocalOverrides } from '@app/hooks/use-local-overrides';

export type DelegationViewType = 'received' | 'granted';

// In a real app, these come from an auth context.
const CURRENT_USER_ID = '5f4e3d01-1b0a-9f8e-7d6c-543210987654'; // UNIMAR admin
const CURRENT_TENANT_ID = '5f4e3d2c-1b0a-9f8e-7d6c-543210987654'; // UNIMAR tenant

export interface DelegationDashboardState {
  selectedId: string;
  showDiscardDialog: boolean;
  pendingNavigationId: string | null;
  activeConsoleTab: 'overview' | 'permissions';
  isEditing: boolean;
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
  delegationViewType: DelegationViewType;
}

export interface DelegationDashboardActions {
  setSelectedId: React.Dispatch<React.SetStateAction<string>>;
  setShowDiscardDialog: React.Dispatch<React.SetStateAction<boolean>>;
  setPendingNavigationId: React.Dispatch<React.SetStateAction<string | null>>;
  setActiveConsoleTab: React.Dispatch<React.SetStateAction<'overview' | 'permissions'>>;
  setIsEditing: React.Dispatch<React.SetStateAction<boolean>>;
  setIsCreateOpen: React.Dispatch<React.SetStateAction<boolean>>;
  setViewMode: React.Dispatch<React.SetStateAction<'list' | 'thumbnail'>>;
  setSearchCriteria: React.Dispatch<React.SetStateAction<string>>;
  setSearchValue: React.Dispatch<React.SetStateAction<string>>;
  setAppliedQuery: React.Dispatch<React.SetStateAction<{ criteria: string; term: string }>>;
  setActiveFilter: React.Dispatch<React.SetStateAction<string>>;
  setSortBy: React.Dispatch<React.SetStateAction<string>>;
  setSortOrder: React.Dispatch<React.SetStateAction<'asc' | 'desc'>>;
  setPage: React.Dispatch<React.SetStateAction<number>>;
  setDelegationViewType: React.Dispatch<React.SetStateAction<DelegationViewType>>;
  handleSelectDelegation: (id: string) => void;
  confirmDiscard: () => void;
  patchDelegation: (id: string, patch: Partial<Delegation>) => void;
  handleCreateSuccess: () => void;
  handleQuerySubmit: (e: React.FormEvent) => void;
  handleResetQuery: () => void;
}

export function useDelegationDashboard(): DelegationDashboardState & DelegationDashboardActions & {
  knownDelegations: Delegation[];
  isLoadingList: boolean;
  listError: Error | null;
  activeDelegation: Delegation | undefined;
  consoleTabs: Array<'overview' | 'permissions'>;
  totalItems: number;
  totalPages: number;
  startIndex: number;
  currentUserId: string;
  currentTenantId: string;
} {
  const [selectedId, setSelectedId] = useState('');
  const [showDiscardDialog, setShowDiscardDialog] = useState(false);
  const [pendingNavigationId, setPendingNavigationId] = useState<string | null>(null);
  const [activeConsoleTab, setActiveConsoleTab] = useState<'overview' | 'permissions'>('overview');
  const [isEditing, setIsEditing] = useState(false);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewMode, setViewMode] = useState<'list' | 'thumbnail'>('list');
  const [searchCriteria, setSearchCriteria] = useState('id');
  const [searchValue, setSearchValue] = useState('');
  const [appliedQuery, setAppliedQuery] = useState<{ criteria: string; term: string }>({ criteria: 'id', term: '' });
  const [activeFilter, setActiveFilter] = useState('all');
  const [sortBy, setSortBy] = useState('status');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState(1);
  const [delegationViewType, setDelegationViewType] = useState<DelegationViewType>('received');
  
  const pageSize = 10;

  const receivedQuery = useGetDelegationsByDelegatedAdmin(CURRENT_USER_ID, CURRENT_TENANT_ID);
  const grantedQuery = useGetDelegationsByDelegatingAdmin(CURRENT_USER_ID, CURRENT_TENANT_ID);

  const isReceived = delegationViewType === 'received';
  const activeQuery = isReceived ? receivedQuery : grantedQuery;

  // We are not handling pagination locally for the mocked hook array here to keep it simple,
  // but we provide the properties so the UI component works correctly.
  let filteredItems = activeQuery.data ?? [];
  
  if (appliedQuery.term) {
    const term = appliedQuery.term.toLowerCase();
    filteredItems = filteredItems.filter(d => 
      d.delegationId.toLowerCase().includes(term) || d.scopeType.toLowerCase().includes(term)
    );
  }

  if (activeFilter !== 'all') {
    filteredItems = filteredItems.filter(d => d.status === activeFilter);
  }

  const { items: knownDelegations, patchItem: patchLocalDelegation } = useLocalOverrides<Delegation>(
    filteredItems,
    'delegationId',
  );

  const activeDelegation = knownDelegations.find((d) => d.delegationId === selectedId);
  const consoleTabs: Array<'overview' | 'permissions'> = ['overview', 'permissions'];
  const hasPendingChanges = isEditing;

  const applyDelegationSelection = useCallback((id: string) => {
    setIsEditing(false);
    setActiveConsoleTab('overview');
    setSelectedId(id);
  }, []);

  const handleSelectDelegation = useCallback((id: string) => {
    if (id === selectedId) return;
    if (hasPendingChanges) {
      setPendingNavigationId(id);
      setShowDiscardDialog(true);
      return;
    }
    applyDelegationSelection(id);
  }, [selectedId, hasPendingChanges, applyDelegationSelection]);

  const confirmDiscard = useCallback(() => {
    if (pendingNavigationId) applyDelegationSelection(pendingNavigationId);
    setPendingNavigationId(null);
    setShowDiscardDialog(false);
  }, [pendingNavigationId, applyDelegationSelection]);

  useEffect(() => {
    if (!selectedId && knownDelegations.length > 0) {
      applyDelegationSelection(knownDelegations[0].delegationId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [knownDelegations]);

  const patchDelegation = useCallback((id: string, patch: Partial<Delegation>) => {
    patchLocalDelegation(id, patch);
  }, [patchLocalDelegation]);

  const handleCreateSuccess = useCallback(() => {
    setIsCreateOpen(false);
    setDelegationViewType('granted'); // Usually after creating, you want to see granted.
    setPage(1);
    setAppliedQuery({ criteria: 'id', term: '' });
    setSearchValue('');
  }, []);

  const handleQuerySubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    if (searchCriteria === 'id' && searchValue.trim()) handleSelectDelegation(searchValue.trim());
    setAppliedQuery({ criteria: searchCriteria, term: searchValue });
  }, [searchCriteria, searchValue, handleSelectDelegation]);

  const handleResetQuery = useCallback(() => {
    setSearchValue('');
    setAppliedQuery({ criteria: 'id', term: '' });
    setPage(1);
  }, []);

  const totalItems = knownDelegations.length;
  const totalPages = Math.ceil(totalItems / pageSize) || 1;
  const startIndex = (page - 1) * pageSize;
  
  // Paginate local array
  const paginatedDelegations = knownDelegations.slice(startIndex, startIndex + pageSize);

  return {
    selectedId, setSelectedId,
    showDiscardDialog, setShowDiscardDialog,
    pendingNavigationId, setPendingNavigationId,
    activeConsoleTab, setActiveConsoleTab,
    isEditing, setIsEditing,
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
    delegationViewType, setDelegationViewType,
    handleSelectDelegation,
    confirmDiscard,
    patchDelegation,
    handleCreateSuccess,
    handleQuerySubmit,
    handleResetQuery,
    knownDelegations: paginatedDelegations,
    isLoadingList: activeQuery.isLoading,
    listError: activeQuery.error ?? null,
    activeDelegation,
    consoleTabs,
    totalItems,
    totalPages,
    startIndex,
    currentUserId: CURRENT_USER_ID,
    currentTenantId: CURRENT_TENANT_ID,
  };
}
