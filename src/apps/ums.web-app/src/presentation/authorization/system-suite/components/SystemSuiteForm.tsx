import React, { useState } from 'react';
import { useCreateSystemSuite } from '@app/authorization/hooks/use-system-suite';
import { useI18n } from '@app/i18n/use-i18n';
import { useFormValidation } from '@app/hooks';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { CreateSystemSuitePayloadSchema } from '@domain/authorization/schemas/system-suite.schema';
import { Box } from 'lucide-react';

interface SystemSuiteFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  tenantId?: string;
}

export const SystemSuiteForm: React.FC<SystemSuiteFormProps> = ({ isOpen, onClose, onSuccess, tenantId }) => {
  const createSystemSuiteMutation = useCreateSystemSuite();
  const t = useI18n();

  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

  const { errors, validate, clearErrors } = useFormValidation(CreateSystemSuitePayloadSchema);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const payload = {
      tenantId: tenantId || 'f3e2d1c0-b9a8-7f6e-5d4c-321098765432',
      code,
      name,
      description: description || undefined,
    };

    const validData = validate(payload);
    if (!validData) return;

    try {
      await createSystemSuiteMutation.mutateAsync(validData);
      setCode(''); setName(''); setDescription('');
      clearErrors();
      onSuccess();
      onClose();
    } catch {
      // Handled by mutation hook
    }
  };

  return (
    <M3FormDialog
      open={isOpen}
      onClose={onClose}
      title={t.createSystemSuiteTitle}
      icon={<Box className="w-5 h-5" />}
      footer={
        <>
          <M3Button variant="text" onClick={onClose} type="button">
            {t.cancelBtn}
          </M3Button>
          <M3Button variant="filled" onClick={handleSubmit} loading={createSystemSuiteMutation.isPending}>
            {t.registerSystemSuiteBtn}
          </M3Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-0">
        <M3TextField
          label={t.systemSuiteCode}
          required
          value={code}
          onChange={(e) => setCode(e.target.value.toUpperCase())}
          placeholder="e.g. SUITE_CRM"
          error={errors.code}
          helperText={t.systemSuiteCodeHelper}
        />

        <M3TextField
          label={t.systemSuiteName}
          required
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="e.g. CRM System Suite"
          error={errors.name}
          helperText={t.systemSuiteNameHelper}
        />

        <M3TextField
          label={t.description}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="e.g. Customer relationship management module"
          className="mt-4"
        />
      </form>
    </M3FormDialog>
  );
};
