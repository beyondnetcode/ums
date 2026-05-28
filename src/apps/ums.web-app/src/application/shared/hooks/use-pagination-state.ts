import { useState, useCallback, useMemo } from 'react';

export interface UsePaginationStateOptions {
  initialPage?: number;
  initialPageSize?: number;
}

export function usePaginationState(options: UsePaginationStateOptions = {}) {
  const [page, setPage] = useState<number>(options.initialPage ?? 1);
  const [pageSize, setPageSize] = useState<number>(options.initialPageSize ?? 10);

  const startIndex = useMemo(() => (page - 1) * pageSize, [page, pageSize]);

  const handlePageChange = useCallback((newPage: number) => {
    setPage(newPage);
  }, []);

  const handlePageSizeChange = useCallback((newPageSize: number) => {
    setPageSize(newPageSize);
    setPage(1); // Reset to first page when page size changes
  }, []);

  return {
    page,
    setPage,
    pageSize,
    setPageSize,
    startIndex,
    handlePageChange,
    handlePageSizeChange,
  };
}
