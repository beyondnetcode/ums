/**
 * useSystemSuiteDashboard.ts — Orchestrates state and API calls for SystemSuiteDashboardScreen.
 */
import React, { useState, useEffect, useCallback } from 'react';
import { useGetAllSystemSuites } from '@app/authorization/hooks/use-system-suite';
import { useLocalOverrides } from '@app/hooks/use-local-overrides';
import { SystemSuite } from '@domain/authorization/models/system-suite.model';
import { SYSTEM_SUITE_PAGE_SIZE } from '@domain/authorization/constants/system-suite.constants';

export interface SystemSuiteDashboardState {
  selectedId: string;
  showDiscardDialog: boolean;
  pendingNavigationId: string | null;
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
}

export interface SystemSuiteDashboardActions {
  setSelectedId: React.Dispatch<React.SetStateAction<string>>;
  setShowDiscardDialog: React.Dispatch<React.SetStateAction<boolean>>;
  setPendingNavigationId: React.Dispatch<React.SetStateAction<string | null>>;
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
  handleSelectSystemSuite: (id: string) => void;
  confirmDiscard: () => void;
  patchSystemSuite: (id: string, patch: Partial<SystemSuite>) => void;
  handleCreateSuccess: () => void;
  handleQuerySubmit: (e: React.FormEvent) => void;
  handleResetQuery: () => void;
}

export function useSystemSuiteDashboard(): SystemSuiteDashboardState & SystemSuiteDashboardActions & {
  knownSystemSuites: SystemSuite[];
  isLoadingList: boolean;
  listError: Error | null;
  activeSystemSuite: SystemSuite | undefined;
  totalItems: number;
  totalPages: number;
  startIndex: number;
} {
  const [selectedId, setSelectedId] = useState('');
  const [showDiscardDialog, setShowDiscardDialog] = useState(false);
  const [pendingNavigationId, setPendingNavigationId] = useState<string | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewMode, setViewMode] = useState<'list' | 'thumbnail'>('list');
  const [searchCriteria, setSearchCriteria] = useState('name');
  const [searchValue, setSearchValue] = useState('');
  const [appliedQuery, setAppliedQuery] = useState<{ criteria: string; term: string }>({ criteria: 'name', term: '' });
  const [activeFilter, setActiveFilter] = useState('all');
  const [sortBy, setSortBy] = useState('name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState(1);
  const pageSize = SYSTEM_SUITE_PAGE_SIZE;

  const { data: systemSuitePage, isLoading: isLoadingList, error: listError } = useGetAllSystemSuites({
    page,
    pageSize,
    search: appliedQuery.term,
    criteria: appliedQuery.criteria,
    status: activeFilter,
    sortBy,
    sortOrder,
  });

  const { items: knownSystemSuites, patchItem: patchLocalSystemSuite } = useLocalOverrides<SystemSuite>(
    systemSuitePage?.items,
    'systemSuiteId',
  );

  const activeSystemSuite = knownSystemSuites.find((s) => s.systemSuiteId === selectedId);

  const hasPendingChanges = isEditing;

  const applySystemSuiteSelection = useCallback((id: string) => {
    setIsEditing(false);
    setSelectedId(id);
  }, []);

  const handleSelectSystemSuite = useCallback((id: string) => {
    if (id === selectedId) return;
    if (hasPendingChanges) {
      setPendingNavigationId(id);
      setShowDiscardDialog(true);
      return;
    }
    applySystemSuiteSelection(id);
  }, [selectedId, hasPendingChanges, applySystemSuiteSelection]);

  const confirmDiscard = useCallback(() => {
    if (pendingNavigationId) applySystemSuiteSelection(pendingNavigationId);
    setPendingNavigationId(null);
    setShowDiscardDialog(false);
  }, [pendingNavigationId, applySystemSuiteSelection]);

  useEffect(() => {
    if (!selectedId && knownSystemSuites.length > 0) {
      applySystemSuiteSelection(knownSystemSuites[0].systemSuiteId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [knownSystemSuites]);

  const patchSystemSuite = useCallback((id: string, patch: Partial<SystemSuite>) => {
    patchLocalSystemSuite(id, patch);
  }, [patchLocalSystemSuite]);

  const handleCreateSuccess = useCallback(() => {
    setPage(1);
    setAppliedQuery({ criteria: 'name', term: '' });
    setSearchValue('');
    setIsCreateOpen(false);
  }, []);

  const handleQuerySubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    if (searchCriteria === 'id' && searchValue.trim()) handleSelectSystemSuite(searchValue.trim());
    setAppliedQuery({ criteria: searchCriteria, term: searchValue });
  }, [searchCriteria, searchValue, handleSelectSystemSuite]);

  const handleResetQuery = useCallback(() => {
    setSearchValue('');
    setAppliedQuery({ criteria: 'name', term: '' });
    setPage(1);
  }, []);

  const totalItems = systemSuitePage?.totalItems ?? 0;
  const totalPages = systemSuitePage?.totalPages ?? 0;
  const startIndex = (page - 1) * pageSize;

  return {
    selectedId, setSelectedId,
    showDiscardDialog, setShowDiscardDialog,
    pendingNavigationId, setPendingNavigationId,
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
    handleSelectSystemSuite,
    confirmDiscard,
    patchSystemSuite,
    handleCreateSuccess,
    handleQuerySubmit,
    handleResetQuery,
    knownSystemSuites,
    isLoadingList,
    listError,
    activeSystemSuite,
    totalItems,
    totalPages,
    startIndex,
  };
}
