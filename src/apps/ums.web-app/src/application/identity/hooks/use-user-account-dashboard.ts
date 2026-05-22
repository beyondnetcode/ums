/**
 * useUserAccountDashboard.ts — Orchestrates state and API calls for UserAccountDashboardScreen.
 *
 * Manages selection, search, pagination, dialogs, and mutations for the
 * UserAccount bounded context.
 */
import React, { useState, useEffect, useCallback } from 'react';
import { useGetAllUserAccounts, useActivateUserAccount, useBlockUserAccount, useRestoreUserAccount } from '@app/identity/hooks/use-user-account';
import { useLocalOverrides } from '@app/hooks/use-local-overrides';
import { useNotificationStore } from '@app/stores/notification.store';
import { UserAccount } from '@domain/identity/models/user-account.model';
import { USER_ACCOUNT_PAGE_SIZE } from '@domain/identity/constants/user-account.constants';

export interface UserAccountDashboardState {
  selectedId: string;
  showBlockDialog: boolean;
  showRestoreDialog: boolean;
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
  blockReason: string;
}

export interface UserAccountDashboardActions {
  setSelectedId: React.Dispatch<React.SetStateAction<string>>;
  setShowBlockDialog: React.Dispatch<React.SetStateAction<boolean>>;
  setShowRestoreDialog: React.Dispatch<React.SetStateAction<boolean>>;
  setIsCreateOpen: React.Dispatch<React.SetStateAction<boolean>>;
  setViewMode: React.Dispatch<React.SetStateAction<'list' | 'thumbnail'>>;
  setSearchCriteria: React.Dispatch<React.SetStateAction<string>>;
  setSearchValue: React.Dispatch<React.SetStateAction<string>>;
  setAppliedQuery: React.Dispatch<React.SetStateAction<{ criteria: string; term: string }>>;
  setActiveFilter: React.Dispatch<React.SetStateAction<string>>;
  setSortBy: React.Dispatch<React.SetStateAction<string>>;
  setSortOrder: React.Dispatch<React.SetStateAction<'asc' | 'desc'>>;
  setPage: React.Dispatch<React.SetStateAction<number>>;
  setBlockReason: React.Dispatch<React.SetStateAction<string>>;
  handleSelectAccount: (id: string) => void;
  handleActivate: () => void;
  handleBlockRequest: (userAccountId: string) => void;
  handleRestoreRequest: (userAccountId: string) => void;
  confirmBlock: () => void;
  confirmRestore: () => void;
  handleCreateSuccess: () => void;
  handleQuerySubmit: (e: React.FormEvent) => void;
  handleResetQuery: () => void;
}

export function useUserAccountDashboard(): UserAccountDashboardState & UserAccountDashboardActions & {
  knownAccounts: UserAccount[];
  isLoadingList: boolean;
  activeAccount: UserAccount | undefined;
  totalItems: number;
  totalPages: number;
  startIndex: number;
} {
  const [selectedId, setSelectedId] = useState('');
  const [showBlockDialog, setShowBlockDialog] = useState(false);
  const [showRestoreDialog, setShowRestoreDialog] = useState(false);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [viewMode, setViewMode] = useState<'list' | 'thumbnail'>('list');
  const [searchCriteria, setSearchCriteria] = useState('email');
  const [searchValue, setSearchValue] = useState('');
  const [appliedQuery, setAppliedQuery] = useState<{ criteria: string; term: string }>({ criteria: 'email', term: '' });
  const [activeFilter, setActiveFilter] = useState('all');
  const [sortBy, setSortBy] = useState('email');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState(1);
  const [blockReason, setBlockReason] = useState('');
  const pageSize = USER_ACCOUNT_PAGE_SIZE;

  const addNotification = useNotificationStore((s) => s.addNotification);

  const { data: accountPage, isLoading: isLoadingList } = useGetAllUserAccounts({
    page,
    pageSize,
    search: appliedQuery.term,
    criteria: appliedQuery.criteria,
    status: activeFilter,
    sortBy,
    sortOrder,
  });

  const { items: knownAccounts } = useLocalOverrides<UserAccount>(
    accountPage?.items,
    'userAccountId',
  );

  const activeAccount = knownAccounts.find((account) => account.userAccountId === selectedId);

  const activateMutation = useActivateUserAccount(selectedId);
  const blockMutation = useBlockUserAccount(selectedId);
  const restoreMutation = useRestoreUserAccount(selectedId);

  const handleSelectAccount = useCallback((id: string) => {
    setSelectedId(id);
  }, []);

  const handleActivate = useCallback(() => {
    activateMutation.mutate(undefined, {
      onSuccess: () => {
        addNotification({
          title: 'Activated',
          message: 'User account has been activated.',
          type: 'success',
        });
      },
    });
  }, [activateMutation, addNotification]);

  const handleBlockRequest = useCallback((userAccountId: string) => {
    setSelectedId(userAccountId);
    setShowBlockDialog(true);
  }, []);

  const handleRestoreRequest = useCallback((userAccountId: string) => {
    setSelectedId(userAccountId);
    setShowRestoreDialog(true);
  }, []);

  const confirmBlock = useCallback(() => {
    if (!blockReason.trim()) return;
    blockMutation.mutate(blockReason.trim(), {
      onSuccess: () => {
        addNotification({
          title: 'Blocked',
          message: 'User account has been blocked.',
          type: 'warning',
        });
        setShowBlockDialog(false);
        setBlockReason('');
      },
    });
  }, [blockMutation, blockReason, addNotification]);

  const confirmRestore = useCallback(() => {
    restoreMutation.mutate(undefined, {
      onSuccess: () => {
        addNotification({
          title: 'Restored',
          message: 'User account has been restored.',
          type: 'success',
        });
        setShowRestoreDialog(false);
      },
    });
  }, [restoreMutation, addNotification]);

  const handleCreateSuccess = useCallback(() => {
    setPage(1);
    setAppliedQuery({ criteria: 'email', term: '' });
    setSearchValue('');
    setIsCreateOpen(false);
  }, []);

  const handleQuerySubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    setAppliedQuery({ criteria: searchCriteria, term: searchValue });
  }, [searchCriteria, searchValue]);

  const handleResetQuery = useCallback(() => {
    setSearchValue('');
    setAppliedQuery({ criteria: 'email', term: '' });
    setPage(1);
  }, []);

  useEffect(() => {
    if (!selectedId && knownAccounts.length > 0) {
      setSelectedId(knownAccounts[0].userAccountId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [knownAccounts]);

  const totalItems = accountPage?.totalItems ?? 0;
  const totalPages = accountPage?.totalPages ?? 0;
  const startIndex = (page - 1) * pageSize;

  return {
    selectedId, setSelectedId,
    showBlockDialog, setShowBlockDialog,
    showRestoreDialog, setShowRestoreDialog,
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
    blockReason, setBlockReason,
    handleSelectAccount,
    handleActivate,
    handleBlockRequest,
    handleRestoreRequest,
    confirmBlock,
    confirmRestore,
    handleCreateSuccess,
    handleQuerySubmit,
    handleResetQuery,
    knownAccounts,
    isLoadingList,
    activeAccount,
    totalItems,
    totalPages,
    startIndex,
  };
}
