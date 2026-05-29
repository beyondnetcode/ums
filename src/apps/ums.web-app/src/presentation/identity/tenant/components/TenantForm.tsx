import React, { useState } from 'react';
import { useCreateTenant } from '@app/identity/hooks/use-tenant';
import { useI18n } from '@app/i18n/use-i18n';
import { useFormValidation } from '@app/hooks';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { SearchableSelect } from '@shared/components/SearchableSelect';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { TENANT_TYPES } from '@domain/identity/constants/tenant.constants';
import { CreateTenantPayloadSchema } from '@domain/identity/schemas/tenant.schema';
import { Building } from 'lucide-react';

interface TenantFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (tenantId: string) => void;
}

export const TenantForm: React.FC<TenantFormProps> = ({ isOpen, onClose, onSuccess }) => {
  const createTenantMutation = useCreateTenant();
  const t = useI18n();

  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [type, setType] = useState('INTERNAL');
  const [companyReference, setCompanyReference] = useState('');
  
  const { errors, validate, clearErrors } = useFormValidation(CreateTenantPayloadSchema);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const payload = {
      code,
      name,
      type,
      companyReference: companyReference || undefined,
    };

    const validData = validate(payload);
    if (!validData) return;

    try {
      const response = await createTenantMutation.mutateAsync(validData);
      setCode(''); setName(''); setType('INTERNAL'); setCompanyReference('');
      clearErrors();
      onSuccess(response.tenantId);
      onClose();
    } catch {
      // Handled by mutation hook
    }
  };

  return (
    <M3FormDialog
      open={isOpen}
      onClose={onClose}
      title={t.createTenantTitle}
      icon={<Building className="w-5 h-5" />}
      footer={
        <>
          <M3Button variant="text" onClick={onClose} type="button">
            {t.cancelBtn}
          </M3Button>
          <M3Button variant="filled" onClick={handleSubmit} loading={createTenantMutation.isPending}>
            {t.registerTenantBtn}
          </M3Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-0">
        <M3TextField
          label={t.tenantCode}
          required
          value={code}
          onChange={(e) => setCode(e.target.value.toUpperCase())}
          placeholder="e.g. TRANS_LIMA"
          error={errors.code}
          helperText={t.tenantCodeHelper}
        />

        <M3TextField
          label={t.tenantName}
          required
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="e.g. Transportes Lima S.A.C."
          error={errors.name}
          helperText={t.tenantNameHelper}
        />

        <div className="grid grid-cols-2 gap-4">
          <M3TextField
            label={t.companyReference}
            value={companyReference}
            onChange={(e) => setCompanyReference(e.target.value)}
            placeholder="e.g. RUC-20512345678"
            className="mb-0"
          />

          <SearchableSelect
            label={t.tenantType}
            value={type}
            onChange={(val) => setType(val || 'INTERNAL')}
            options={TENANT_TYPES.map((tp) => ({ value: tp, label: tp }))}
            placeholder="Seleccionar tipo..."
            className="mb-0"
          />
        </div>
      </form>
    </M3FormDialog>
  );
};

