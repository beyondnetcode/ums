/**
 * useNotifiedMutation — DRY factory for TanStack mutations
 *
 * Every mutation in UMS follows the same ceremony:
 *   1. Call the service
 *   2. Invalidate one or more query keys on success
 *   3. Show a success notification
 *   4. Show an error notification (extracting API message)
 *
 * This factory encapsulates that boilerplate so each hook only declares
 * the domain-specific bits (fn, keys, labels).
 */
import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
  type QueryKey,
} from '@tanstack/react-query';
import { useNotificationStore } from '@app/stores/notification.store';
import { getHttpErrorMessage } from '@app/errors/http-error';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface NotifiedMutationNotif {
  title: string;
  message: string;
  type?: 'success' | 'info' | 'warning' | 'error';
}

export interface UseNotifiedMutationOptions<TData, TVariables> {
  /** The async function that performs the operation. */
  mutationFn: (variables: TVariables) => Promise<TData>;
  /** Query keys to invalidate on success. */
  invalidateKeys?: QueryKey[];
  /** Notification shown on success. May use the response data. */
  successNotif: (data: TData) => NotifiedMutationNotif;
  /** Notification shown on error. Receives the raw error. */
  errorNotif: (error: unknown) => NotifiedMutationNotif;
  /**
   * Optional additional TanStack mutation options.
   * `onSuccess` / `onError` from here run AFTER the built-in notification logic.
   */
  options?: Omit<
    UseMutationOptions<TData, unknown, TVariables>,
    'mutationFn' | 'onSuccess' | 'onError'
  >;
}

// ─── Hook ───────────────────────────────────────────────────────────────────

export function useNotifiedMutation<TData = void, TVariables = void>({
  mutationFn,
  invalidateKeys,
  successNotif,
  errorNotif,
  options,
}: UseNotifiedMutationOptions<TData, TVariables>) {
  const queryClient = useQueryClient();
  const addNotification = useNotificationStore((s) => s.addNotification);

  return useMutation<TData, unknown, TVariables>({
    mutationFn,
    onSuccess: (data) => {
      if (invalidateKeys) {
        invalidateKeys.forEach((key) =>
          queryClient.invalidateQueries({ queryKey: key }),
        );
      }
      const notif = successNotif(data);
      addNotification({
        title: notif.title,
        message: notif.message,
        type: notif.type ?? 'success',
      });
    },
    onError: (error: unknown) => {
      const notif = errorNotif(error);
      addNotification({
        title: notif.title,
        message: getHttpErrorMessage(error, notif.message),
        type: notif.type ?? 'error',
      });
    },
    ...options,
  });
}
