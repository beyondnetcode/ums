import React, { useState } from 'react';
import { useCreateUserAccount } from '@app/identity/hooks/use-user-account';
import { useI18n } from '@app/i18n/use-i18n';
import { useFormValidation } from '@app/hooks';
import { useAuthStore } from '@app/stores/auth.store';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { SearchableSelect } from '@shared/components/SearchableSelect';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { USER_CATEGORIES, IDENTITY_REFERENCE_TYPES } from '@domain/identity/constants/user-account.constants';
import { CreateUserAccountPayloadSchema } from '@domain/identity/schemas/user-account.schema';
import { UserPlus } from 'lucide-react';

interface UserAccountFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  tenantId?: string;
}

export const UserAccountForm: React.FC<UserAccountFormProps> = ({ isOpen, onClose, onSuccess, tenantId }) => {
  const createUserAccountMutation = useCreateUserAccount();
  const t = useI18n();

  const sessionTenantId = useAuthStore((state) => state.user?.tenantId);
  const effectiveTenantId = tenantId || sessionTenantId;

  const [email, setEmail] = useState('');
  const [category, setCategory] = useState('Internal');
  const [identityReference, setIdentityReference] = useState('');
  const [identityReferenceType, setIdentityReferenceType] = useState('HrId');

  const { errors, validate, clearErrors } = useFormValidation(CreateUserAccountPayloadSchema);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!effectiveTenantId) {
      return;
    }

    const payload = {
      tenantId: effectiveTenantId,
      branchId: null,
      email,
      category: category as 'Internal' | 'External' | 'B2B' | 'Partner' | 'ServiceAccount',
      identityReference: identityReference || undefined,
      identityReferenceType: identityReference ? (identityReferenceType as 'HrId' | 'VendorCode' | 'GovernmentId' | 'PartnerRef') : undefined,
    };

    const validData = validate(payload);
    if (!validData) return;

    try {
      await createUserAccountMutation.mutateAsync(validData);
      setEmail(''); setCategory('Internal'); setIdentityReference(''); setIdentityReferenceType('HrId');
      clearErrors();
      onSuccess();
    } catch {
      // Handled by mutation hook
    }
  };

  return (
    <M3FormDialog
      open={isOpen}
      onClose={onClose}
      title={t.createUserAccountTitle}
      icon={<UserPlus className="w-5 h-5" />}
      footer={
        <>
          <M3Button variant="text" onClick={onClose} type="button">
            {t.cancelBtn}
          </M3Button>
          <M3Button variant="filled" onClick={handleSubmit} loading={createUserAccountMutation.isPending}>
            {t.createUserBtn}
          </M3Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-0">
        <M3TextField
          label={t.userEmail}
          required
          value={email}
          onChange={(e) => setEmail(e.target.value.toLowerCase())}
          placeholder="e.g. user@company.com"
          error={errors.email}
          helperText={t.userEmailHelper}
        />

        <div className="grid grid-cols-2 gap-4">
          <SearchableSelect
            label={t.userCategory}
            value={category}
            onChange={(val) => setCategory(val || 'Internal')}
            options={USER_CATEGORIES.map((c) => ({ value: c, label: c }))}
            placeholder="Seleccionar categoría..."
            className="mb-0"
          />

          <SearchableSelect
            label={t.identityReferenceType}
            value={identityReferenceType}
            onChange={(val) => setIdentityReferenceType(val || 'HrId')}
            options={IDENTITY_REFERENCE_TYPES.map((t) => ({ value: t, label: t }))}
            placeholder="Seleccionar tipo..."
            className="mb-0"
          />
        </div>

        <M3TextField
          label={t.identityReference}
          value={identityReference}
          onChange={(e) => setIdentityReference(e.target.value)}
          placeholder="e.g. EMP-12345"
          className="mt-4"
        />
      </form>
    </M3FormDialog>
  );
};
