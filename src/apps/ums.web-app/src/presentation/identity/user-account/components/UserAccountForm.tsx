import React, { useState } from 'react';
import { useCreateUserAccount } from '@app/identity/hooks/use-user-account';
import { useI18n } from '@app/i18n/use-i18n';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { M3Select } from '@shared/components/M3Select';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { USER_CATEGORIES, IDENTITY_REFERENCE_TYPES } from '@domain/identity/constants/user-account.constants';
import { CreateUserAccountPayloadSchema } from '@domain/identity/schemas/user-account.schema';
import { UserPlus } from 'lucide-react';

interface UserAccountFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const UserAccountForm: React.FC<UserAccountFormProps> = ({ isOpen, onClose, onSuccess }) => {
  const createUserAccountMutation = useCreateUserAccount();
  const t = useI18n();

  const [email, setEmail] = useState('');
  const [category, setCategory] = useState('Internal');
  const [identityReference, setIdentityReference] = useState('');
  const [identityReferenceType, setIdentityReferenceType] = useState('HrId');
  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});

    const payload = {
      tenantId: 'f3e2d1c0-b9a8-7f6e-5d4c-321098765432',
      branchId: null,
      email,
      category: category as 'Internal' | 'External' | 'B2B' | 'Partner' | 'ServiceAccount',
      identityReference: identityReference || undefined,
      identityReferenceType: identityReference ? (identityReferenceType as 'HrId' | 'VendorCode' | 'GovernmentId' | 'PartnerRef') : undefined,
    };

    const result = CreateUserAccountPayloadSchema.safeParse(payload);
    if (!result.success) {
      const fieldErrors: { [key: string]: string } = {};
      const flattened = result.error.flatten();
      if (flattened.fieldErrors.email?.[0]) fieldErrors.email = flattened.fieldErrors.email[0];
      if (flattened.fieldErrors.category?.[0]) fieldErrors.category = flattened.fieldErrors.category[0];
      setErrors(fieldErrors);
      return;
    }

    try {
      await createUserAccountMutation.mutateAsync(payload);
      setEmail(''); setCategory('Internal'); setIdentityReference(''); setIdentityReferenceType('HrId');
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
          <M3Select
            label={t.userCategory}
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            className="mb-0"
          >
            {USER_CATEGORIES.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </M3Select>

          <M3Select
            label={t.identityReferenceType}
            value={identityReferenceType}
            onChange={(e) => setIdentityReferenceType(e.target.value)}
            className="mb-0"
          >
            {IDENTITY_REFERENCE_TYPES.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </M3Select>
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
