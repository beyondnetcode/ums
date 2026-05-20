import React, { useState } from 'react';
import { useCreateTenant } from '../../../application/identity/hooks/use-tenant';
import { M3Button } from '../../shared/components/M3Button';
import { M3TextField } from '../../shared/components/M3TextField';
import { X, Building } from 'lucide-react';

interface TenantFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (tenantId: string) => void;
}

export const TenantForm: React.FC<TenantFormProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const createTenantMutation = useCreateTenant();

  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [type, setType] = useState('Standard');
  const [companyReference, setCompanyReference] = useState('');
  const [parentTenantId, setParentTenantId] = useState('');
  
  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});

    const newErrors: { [key: string]: string } = {};
    if (code.length < 3) {
      newErrors.code = 'Code must be at least 3 characters.';
    }
    if (name.length < 3) {
      newErrors.name = 'Name must be at least 3 characters.';
    }

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    try {
      const response = await createTenantMutation.mutateAsync({
        code,
        name,
        type,
        companyReference: companyReference || undefined,
        parentTenantId: parentTenantId || undefined,
      });

      setCode('');
      setName('');
      setType('Standard');
      setCompanyReference('');
      setParentTenantId('');

      onSuccess(response.tenantId);
      onClose();
    } catch (err) {
      // Handled by custom hook mutation alerts
    }
  };

  return (
    <div className="fixed inset-0 z-50 overflow-hidden flex items-center justify-center p-4 select-none">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/45 backdrop-blur-sm transition-opacity"
        onClick={onClose}
      />

      {/* Modal Card */}
      <div className="bg-m3-surface border border-m3-outline/25 w-full max-w-lg rounded-3xl overflow-hidden shadow-2xl z-10 transition-all duration-300 transform scale-100">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-5 border-b border-m3-outline/20">
          <div className="flex items-center gap-2 text-m3-primary">
            <Building className="w-5 h-5" />
            <h2 className="text-base font-extrabold tracking-tight text-m3-on-surface">
              Create New Tenant
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-full hover:bg-m3-primary/10 text-m3-secondary transition-colors"
          >
            <X className="w-4.5 h-4.5" />
          </button>
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          <M3TextField
            label="Tenant Code"
            required
            value={code}
            onChange={(e) => setCode(e.target.value.toUpperCase())}
            placeholder="e.g. TENANT_A"
            error={errors.code}
            helperText="Uppercase alphanumeric characters, underscores only."
          />

          <M3TextField
            label="Tenant Name"
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Acme Corporation"
            error={errors.name}
            helperText="Friendly name describing the organization."
          />

          <div className="grid grid-cols-2 gap-4">
            <M3TextField
              label="Company Reference"
              value={companyReference}
              onChange={(e) => setCompanyReference(e.target.value)}
              placeholder="e.g. REF_992"
            />

            <div>
              <label className="block text-[11px] font-bold text-m3-primary uppercase tracking-wider mb-2 ml-1">
                Tenant Type
              </label>
              <select
                value={type}
                onChange={(e) => setType(e.target.value)}
                className="w-full px-4 py-3.5 text-sm rounded-2xl border border-m3-outline bg-m3-surface-container/40 dark:bg-m3-surface-container/20 text-m3-on-surface focus:outline-none focus:ring-2 focus:ring-m3-primary/20 transition-all"
              >
                {['Standard', 'Enterprise', 'Partner', 'Trial'].map((t) => (
                  <option key={t} value={t}>
                    {t}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <M3TextField
            label="Parent Tenant ID (Optional Guid)"
            value={parentTenantId}
            onChange={(e) => setParentTenantId(e.target.value)}
            placeholder="e.g. 00000000-0000-0000-0000-000000000000"
            helperText="Leave empty if this is a top-level parent tenant."
          />

          {/* Action Row */}
          <div className="flex justify-end gap-3 pt-4 border-t border-m3-outline/10">
            <M3Button variant="text" onClick={onClose} type="button">
              Cancel
            </M3Button>
            <M3Button variant="filled" type="submit" loading={createTenantMutation.isPending}>
              Register Tenant
            </M3Button>
          </div>
        </form>
      </div>
    </div>
  );
};
export default TenantForm;
