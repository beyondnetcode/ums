import { useState, useCallback } from 'react';

export interface QueryStateInitials<TFilter extends string, TSort extends string> {
  criteria: string;
  filter: TFilter;
  sortBy: TSort;
}

export function useQueryState<TFilter extends string, TSort extends string>(initials: QueryStateInitials<TFilter, TSort>) {
  const [searchCriteria, setSearchCriteria] = useState<string>(initials.criteria);
  const [searchValue, setSearchValue] = useState<string>('');
  const [activeFilter, setActiveFilter] = useState<TFilter>(initials.filter);
  const [sortBy, setSortBy] = useState<TSort>(initials.sortBy);
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [appliedQuery, setAppliedQuery] = useState<{ criteria: string; term: string }>({
    criteria: initials.criteria,
    term: '',
  });

  const handleQuerySubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    setAppliedQuery({ criteria: searchCriteria, term: searchValue.trim() });
  }, [searchCriteria, searchValue]);

  const handleResetQuery = useCallback(() => {
    setSearchValue('');
    setAppliedQuery({ criteria: initials.criteria, term: '' });
  }, [initials.criteria]);

  const toggleSortOrder = useCallback(() => {
    setSortOrder((o) => (o === 'asc' ? 'desc' : 'asc'));
  }, []);

  return {
    searchCriteria,
    setSearchCriteria,
    searchValue,
    setSearchValue,
    activeFilter,
    setActiveFilter,
    sortBy,
    setSortBy,
    sortOrder,
    setSortOrder,
    toggleSortOrder,
    appliedQuery,
    handleQuerySubmit,
    handleResetQuery,
  };
}
