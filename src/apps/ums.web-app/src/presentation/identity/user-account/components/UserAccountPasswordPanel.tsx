import React, { useState } from 'react';
import { KeyRound, ShieldCheck } from 'lucide-react';
import { useSetUserAccountPassword } from '@app/identity/hooks/use-user-account';
import { useI18n } from '@app/i18n/use-i18n';
import { UserAccount } from '@domain/identity/models/user-account.model';
import { InlineAddForm } from '@shared/components/InlineAddForm';
import { M3TextField } from '@shared/components/M3TextField';

interface UserAccountPasswordPanelProps {
  account: UserAccount;
}

export const UserAccountPasswordPanel: React.FC<UserAccountPasswordPanelProps> = ({ account }) => {
  const t = useI18n();
  const [isOpen, setIsOpen] = useState(false);
  const [password, setPassword] = useState('');
  const [confirmation, setConfirmation] = useState('');
  const [validationError, setValidationError] = useState<string>();
  const mutation = useSetUserAccountPassword(account.userAccountId);
  const isFederated = Boolean(account.identityReference);

  const closeForm = () => {
    setIsOpen(false);
    setPassword('');
    setConfirmation('');
    setValidationError(undefined);
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (password.length < 12) {
      setValidationError(t.passwordMinLength);
      return;
    }
    if (password !== confirmation) {
      setValidationError(t.passwordMismatch);
      return;
    }

    try {
      await mutation.mutateAsync(password);
      closeForm();
    } catch {
      // The notification hook renders the localized and correlated API error.
    }
  };

  return (
    <div className="space-y-3 animate-fadeIn">
      <div className="p-3.5 rounded-xl border border-m3-outline/20 bg-m3-surface-container/5 space-y-3">
        <div className="flex items-center gap-2 border-b border-m3-outline/10 pb-2">
          <ShieldCheck className="w-4 h-4 text-m3-primary" />
          <span className="text-xs font-semibold text-m3-on-surface">{t.passwordManagement}</span>
        </div>

        {isFederated ? (
          <p className="text-xs text-m3-secondary">{t.federatedPasswordManagedExternally}</p>
        ) : (
          <dl className="space-y-2 text-xs">
            <div className="flex items-center justify-between">
              <dt className="text-m3-secondary">{t.localPassword}</dt>
              <dd className={account.hasActivePassword ? 'text-emerald-500 font-semibold' : 'text-m3-secondary'}>
                {account.hasActivePassword ? t.active : t.notConfigured}
              </dd>
            </div>
            {account.passwordUpdatedAtUtc && (
              <div className="flex items-center justify-between">
                <dt className="text-m3-secondary">{t.lastPasswordRotation}</dt>
                <dd className="text-m3-on-surface">
                  {new Date(account.passwordUpdatedAtUtc).toLocaleDateString()}
                </dd>
              </div>
            )}
          </dl>
        )}
      </div>

      {!isFederated && (
        <InlineAddForm
          isOpen={isOpen}
          onToggle={(open) => open ? setIsOpen(true) : closeForm()}
          onSubmit={handleSubmit}
          addLabel={account.hasActivePassword ? t.rotatePassword : t.setTemporaryPassword}
          title={account.hasActivePassword ? t.rotatePassword : t.setTemporaryPassword}
          submitLabel={t.savePassword}
          cancelLabel={t.cancelBtn}
          isLoading={mutation.isPending}
          error={validationError}
          triggerEmphasis="quiet"
          icon={<KeyRound className="w-3.5 h-3.5" />}
        >
          <M3TextField
            label={t.temporaryPassword}
            type="password"
            autoComplete="new-password"
            required
            compact
            value={password}
            onChange={(event) => {
              setPassword(event.target.value);
              setValidationError(undefined);
            }}
            helperText={t.passwordRule}
          />
          <M3TextField
            label={t.confirmPassword}
            type="password"
            autoComplete="new-password"
            required
            compact
            value={confirmation}
            onChange={(event) => {
              setConfirmation(event.target.value);
              setValidationError(undefined);
            }}
          />
        </InlineAddForm>
      )}
    </div>
  );
};
