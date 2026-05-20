import React, { useState } from 'react';
import { useCreateTenant } from '../../../application/identity/hooks/use-tenant';
import { useI18n } from '../../../application/i18n/use-i18n';
import { M3Button } from '../../shared/components/M3Button';
import { M3TextField } from '../../shared/components/M3TextField';
import { M3Select } from '../../shared/components/M3Select';
import { X, Building } from 'lucide-react';
import { sanitizeCode, sanitizeInput } from '../../../application/utils/security';

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
  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});

    const newErrors: { [key: string]: string } = {};
    const sanitizedCode = sanitizeCode(code);
    const sanitizedName = sanitizeInput(name);
    const sanitizedRef = sanitizeInput(companyReference);

    if (sanitizedCode.length < 3) newErrors.code = t.tenantCodeError;
    if (!/^[A-Z0-9_]+$/.test(sanitizedCode)) newErrors.code = t.tenantCodeInvalidFormat;
    if (sanitizedName.length < 3) newErrors.name = t.tenantNameError;

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    try {
      const response = await createTenantMutation.mutateAsync({
        code: sanitizedCode,
        name: sanitizedName,
        type,
        companyReference: sanitizedRef || undefined,
      });
      setCode(''); setName(''); setType('INTERNAL'); setCompanyReference('');
      onSuccess(response.tenantId);
      onClose();
    } catch {
      setErrors({ submit: t.tenantCreateFailed });
    }
  };

  return (
    <div className="fixed inset-0 z-50 overflow-hidden flex items-center justify-center p-4 select-none">
      <div className="absolute inset-0 bg-black/45 backdrop-blur-sm" onClick={onClose} />

      <div className="bg-m3-surface border border-m3-outline/25 w-full max-w-lg rounded-xl overflow-hidden shadow-2xl z-10">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-m3-outline/20">
          <div className="flex items-center gap-2 text-m3-primary">
            <Building className="w-5 h-5" />
            <h2 className="text-base font-semibold text-m3-on-surface">
              {t.createTenantTitle}
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-full hover:bg-m3-primary/10 text-m3-secondary transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Form body */}
        <form onSubmit={handleSubmit} className="p-6 space-y-1">
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

            <M3Select
              label={t.tenantType}
              value={type}
              onChange={(e) => setType(e.target.value)}
              className="mb-0"
            >
              {['INTERNAL', 'SUPPLIER', 'CLIENT'].map((tp) => (
                <option key={tp} value={tp}>{tp}</option>
              ))}
            </M3Select>
          </div>

          <div className="flex justify-end gap-3 pt-6 border-t border-m3-outline/10 mt-6">
            <M3Button variant="text" onClick={onClose} type="button">
              {t.cancelBtn}
            </M3Button>
            <M3Button variant="filled" type="submit" loading={createTenantMutation.isPending}>
              {t.registerTenantBtn}
            </M3Button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TenantForm;
