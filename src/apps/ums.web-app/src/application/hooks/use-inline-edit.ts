import { useState, useCallback } from 'react';

export interface UseInlineEditReturn<T> {
  isEditing: boolean;
  draft: T;
  setDraft: (updater: T | ((prev: T) => T)) => void;
  startEdit: () => void;
  cancelEdit: () => void;
  saveEdit: (onSave: (draft: T) => void) => void;
}

export function useInlineEdit<T>(initialValue: T): UseInlineEditReturn<T> {
  const [isEditing, setIsEditing] = useState(false);
  const [draft, setDraft] = useState<T>(initialValue);

  const startEdit = useCallback(() => setIsEditing(true), []);

  const cancelEdit = useCallback(() => {
    setDraft(initialValue);
    setIsEditing(false);
  }, [initialValue]);

  const saveEdit = useCallback(
    (onSave: (draft: T) => void) => {
      onSave(draft);
      setIsEditing(false);
    },
    [draft],
  );

  return { isEditing, draft, setDraft, startEdit, cancelEdit, saveEdit };
}
