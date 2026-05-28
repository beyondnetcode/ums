import React, { useState } from 'react';
import { Flag } from 'lucide-react';
import { M3Button, M3Select } from '@shared/components';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { M3TextField } from '@shared/components/M3TextField';
import { useCreateFeatureFlag } from '@app/configuration/hooks/use-feature-flag';
import { useGetAllSystemSuites } from '@app/authorization/hooks/use-system-suite';
import {
  FLAG_TYPE_LABELS,
} from '@domain/configuration/constants/feature-flag.constants';
import type { CreateFeatureFlagPayload, FlagType } from '@domain/configuration/models/feature-flag.model';

interface Props {
  isOpen:    boolean;
  onClose:   () => void;
  onSuccess: (featureFlagId: string) => void;
}

interface SelectProps {
  label:    string;
  value:    string;
  onChange: (v: string) => void;
  options:  { value: string; label: string }[];
  disabled?: boolean;
  required?: boolean;
  error?:    string;
}

const FieldSelect: React.FC<SelectProps> = ({ label, value, onChange, options, disabled, required, error }) => (
  <M3Select
    label={label}
    value={value}
    onChange={e => onChange(e.target.value)}
    disabled={disabled}
    required={required}
    error={error}
  >
    <option value="">— Seleccionar —</option>
    {options.map(o => (
      <option key={o.value} value={o.value}>{o.label}</option>
    ))}
  </M3Select>
);

export const FeatureFlagForm: React.FC<Props> = ({ isOpen, onClose, onSuccess }) => {
  const [flagCode, setFlagCode] = useState('');
  const [flagType, setFlagType] = useState<'Boolean' | 'Variant' | 'Percentage'>('Boolean');
  const [systemSuiteId, setSystemSuiteId] = useState('');
  const [flagTargets, setFlagTargets] = useState('*');
  const [rolloutPercentage, setRolloutPercentage] = useState('');
  const [error, setError] = useState('');

  const createMutation = useCreateFeatureFlag();
  const { data: suitesPage, isLoading: loadingSuites } = useGetAllSystemSuites({
    page: 1, pageSize: 100,
  });

  const suiteOptions = (suitesPage?.items ?? []).map(s => ({
    value: s.systemSuiteId,
    label: `${s.name} (${s.code})`,
  }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!flagCode.trim()) { setError('Código requerido'); return; }
    if (!systemSuiteId.trim()) { setError('Suite del Sistema requerida'); return; }

    const payload: CreateFeatureFlagPayload = {
      flagCode: flagCode.trim(),
      flagType,
      systemSuiteId: systemSuiteId.trim(),
      flagTargets: flagTargets.trim() || '*',
      ...(rolloutPercentage ? { rolloutPercentage: parseInt(rolloutPercentage, 10) } : {}),
    };

    try {
      const result = await createMutation.mutateAsync(payload);
      onSuccess(result.featureFlagId);
      setFlagCode('');
      setFlagType('Boolean');
      setSystemSuiteId('');
      setFlagTargets('*');
      setRolloutPercentage('');
    } catch { /* handled by hook */ }
  };

  return (
    <M3FormDialog
      open={isOpen}
      onClose={onClose}
      title="Nuevo Feature Flag"
      icon={<Flag className="w-4 h-4" />}
      maxWidth="max-w-md"
      footer={
        <>
          <M3Button type="button" variant="text" onClick={onClose} disabled={createMutation.isPending}>
            Cancelar
          </M3Button>
          <M3Button
            type="submit"
            form="feature-flag-form"
            variant="filled"
            disabled={createMutation.isPending}
          >
            {createMutation.isPending ? 'Creando…' : 'Crear Flag'}
          </M3Button>
        </>
      }
    >
      <form id="feature-flag-form" onSubmit={handleSubmit} className="space-y-4">
        <p className="text-[11px] text-m3-secondary">
          El flag se crea en estado <span className="font-bold text-amber-500">Inactivo</span>.
          Active después de configurar targeting y criterios.
        </p>

        <M3TextField
          label="Código del Flag"
          required
          value={flagCode}
          onChange={e => { setFlagCode(e.target.value); setError(''); }}
          placeholder="e.g. dark-mode, new-checkout"
        />

        <FieldSelect
          label="Tipo de Flag"
          value={flagType}
          onChange={v => setFlagType(v as FlagType)}
          options={Object.entries(FLAG_TYPE_LABELS).map(([k, v]) => ({ value: k, label: v }))}
          required
        />

        <FieldSelect
          label={loadingSuites ? 'Cargando suites…' : 'Suite del Sistema'}
          value={systemSuiteId}
          onChange={v => { setSystemSuiteId(v); setError(''); }}
          options={suiteOptions}
          disabled={loadingSuites}
          required
        />

        <M3TextField
          label="Targeting Rules"
          value={flagTargets}
          onChange={e => setFlagTargets(e.target.value)}
          placeholder="e.g. * (all), role:admin, tenant:xyz"
        />

        <M3TextField
          label="Rollout Percentage (0-100, opcional)"
          type="number"
          value={rolloutPercentage}
          onChange={e => setRolloutPercentage(e.target.value)}
          placeholder="e.g. 50"
        />

        {error && <p className="text-[11px] text-m3-error">{error}</p>}
      </form>
    </M3FormDialog>
  );
};
