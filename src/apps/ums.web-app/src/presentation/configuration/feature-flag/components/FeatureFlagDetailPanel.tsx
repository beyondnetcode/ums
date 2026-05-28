import React, { useState } from 'react';
import {
  Flag, Trash2, Plus, CheckCircle2, Archive,
  ToggleLeft, ToggleRight, Info,
} from 'lucide-react';
import type { FeatureFlag, FeatureFlagCriteria } from '@domain/configuration/models/feature-flag.model';
import {
  FLAG_TYPE_LABELS,
  CRITERIA_TYPE_LABELS,
  CRITERIA_OPERATOR_LABELS,
} from '@domain/configuration/constants/feature-flag.constants';

import { CodeBadge } from '@shared/components/CodeBadge';
import { KeyValueRow } from '@shared/components/KeyValueRow';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import {
  useActivateFlag,
  useDeactivateFlag,
  useArchiveFlag,
  useAddFeatureFlagCriteria,
  useRemoveFeatureFlagCriteria,
} from '@app/configuration/hooks/use-feature-flag';

interface Props {
  flag: FeatureFlag | undefined;
}

const STATUS_LABEL: Record<string, string> = {
  Inactive: 'Inactivo',
  Active:   'Activo',
  Archived: 'Archivado',
};


const TYPE_COLOR: Record<string, string> = {
  Boolean:    'bg-blue-500/10 text-blue-500',
  Variant:    'bg-purple-500/10 text-purple-500',
  Percentage: 'bg-emerald-500/10 text-emerald-500',
};

const CriteriaRow: React.FC<{
  criteria: FeatureFlagCriteria;
  onRemove: () => void;
  isDraft: boolean;
}> = ({ criteria, onRemove, isDraft }) => (
  <div className="flex items-center gap-3 p-2 rounded-lg border border-m3-outline/10 bg-m3-surface-container/5">
    <div className="flex-1 grid grid-cols-3 gap-2 text-xs">
      <div>
        <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Tipo</span>
        <p className="text-m3-on-surface">{CRITERIA_TYPE_LABELS[criteria.criteriaType] ?? criteria.criteriaType}</p>
      </div>
      <div>
        <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Operador</span>
        <p className="text-m3-on-surface">{CRITERIA_OPERATOR_LABELS[criteria.operator] ?? criteria.operator}</p>
      </div>
      <div>
        <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Valor</span>
        <CodeBadge code={criteria.value} size="sm" />
      </div>
    </div>
    {isDraft && (
      <button
        onClick={onRemove}
        className="p-1 rounded text-m3-secondary hover:text-rose-500 hover:bg-rose-500/10 transition-colors"
        title="Eliminar criterio"
      >
        <Trash2 className="w-3.5 h-3.5" />
      </button>
    )}
  </div>
);

export const FeatureFlagDetailPanel: React.FC<Props> = ({ flag }) => {
  const [criteriaType, setCriteriaType] = useState('TenantId');
  const [criteriaOperator, setCriteriaOperator] = useState('Equals');
  const [criteriaValue, setCriteriaValue] = useState('');
  const [criteriaError, setCriteriaError] = useState('');

  // Hooks must be called unconditionally at the top level
  const activateFlag = useActivateFlag(flag?.featureFlagId ?? '');
  const deactivateFlag = useDeactivateFlag(flag?.featureFlagId ?? '');
  const archiveFlag = useArchiveFlag(flag?.featureFlagId ?? '');
  const addCriteria = useAddFeatureFlagCriteria(flag?.featureFlagId ?? '');
  const removeCriteria = useRemoveFeatureFlagCriteria(flag?.featureFlagId ?? '');

  if (!flag) {
    return (
      <div className="flex flex-col items-center justify-center h-full text-m3-secondary/50 p-6 text-center">
        <Flag className="w-12 h-12 mb-3 opacity-20" />
        <p className="text-sm font-medium">Seleccione un feature flag</p>
        <p className="text-xs mt-1 max-w-[250px]">Los feature flags permiten controlar funcionalidades por tenant, rol, ambiente y más.</p>
      </div>
    );
  }

  const isDraft = flag.status === 'Inactive';
  const StatusIcon = flag.status === 'Active' ? CheckCircle2 : flag.status === 'Archived' ? Archive : ToggleLeft;
  const stateColor = flag.status === 'Active' ? 'text-emerald-500 bg-emerald-500/10'
    : flag.status === 'Archived' ? 'text-rose-500 bg-rose-500/10'
    : 'text-amber-500 bg-amber-500/10';

  const handleAddCriteria = async (e: React.FormEvent) => {
    e.preventDefault();
    setCriteriaError('');
    if (!criteriaValue.trim()) { setCriteriaError('Valor requerido'); return; }
    try {
      await addCriteria.mutateAsync({
        criteriaType: criteriaType as FeatureFlagCriteria['criteriaType'],
        operator: criteriaOperator as FeatureFlagCriteria['operator'],
        value: criteriaValue.trim(),
      });
      setCriteriaValue('');
    } catch { /* handled by hook */ }
  };

  return (
    <div className="p-5 space-y-6">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2 mb-1">
          <div className={`p-1.5 rounded ${TYPE_COLOR[flag.flagType]}`}>
            <Flag className="w-4 h-4" />
          </div>
          <h3 className="text-sm font-bold text-m3-on-surface">{flag.flagCode}</h3>
          <span className={`text-[10px] font-bold uppercase px-1.5 py-0.5 rounded ${TYPE_COLOR[flag.flagType]}`}>
            {FLAG_TYPE_LABELS[flag.flagType]}
          </span>
        </div>
        <p className="text-[10px] font-mono text-m3-secondary/60">{flag.featureFlagId}</p>
      </div>

      {/* Status */}
      <div className={`p-4 rounded-xl border border-m3-outline/20 bg-m3-surface-container/30 flex items-start gap-3`}>
        <StatusIcon className={`w-6 h-6 shrink-0 mt-0.5 ${stateColor.split(' ')[0]}`} />
        <div>
          <p className="text-xs font-bold text-m3-on-surface">Estado: {STATUS_LABEL[flag.status]}</p>
          <p className="text-[11px] text-m3-secondary mt-1">
            {flag.status === 'Active' && 'El flag está activo y evaluando targeting rules.'}
            {flag.status === 'Inactive' && 'El flag está inactivo. Puede ser activado o archivado.'}
            {flag.status === 'Archived' && 'El flag está archivado (estado terminal). No puede ser reactivado.'}
          </p>
        </div>
      </div>

      {/* Key-Value Info */}
      <div className="space-y-2">
        <KeyValueRow label="System Suite" value={flag.systemSuiteId.slice(0, 8) + '…'} />
        <KeyValueRow label="Flag Type" value={FLAG_TYPE_LABELS[flag.flagType]} />
        <KeyValueRow label="Targeting" value={flag.flagTargets} />
        {flag.rolloutPercentage != null && (
          <KeyValueRow label="Rollout %" value={`${flag.rolloutPercentage}%`} />
        )}
        {flag.linkedResourceType && (
          <KeyValueRow label="Linked Resource" value={`${flag.linkedResourceType} (${flag.linkedResourceId?.slice(0, 8)}…)`} />
        )}
      </div>

      {/* Actions */}
      <div className="space-y-3">
        <h4 className="text-xs font-bold uppercase tracking-wider text-m3-secondary">Acciones</h4>
        <div className="flex gap-2">
          {flag.status === 'Inactive' && (
            <>
              <button
                onClick={() => activateFlag.mutate()}
                disabled={activateFlag.isPending}
                className="flex-1 py-2 px-3 rounded-lg border border-emerald-500/30 text-xs font-semibold text-emerald-600 bg-emerald-500/10 flex items-center justify-center gap-2 hover:bg-emerald-500/20 transition-colors disabled:opacity-50"
              >
                <ToggleRight className="w-4 h-4" /> Activar
              </button>
              <button
                onClick={() => archiveFlag.mutate()}
                disabled={archiveFlag.isPending}
                className="flex-1 py-2 px-3 rounded-lg border border-rose-500/30 text-xs font-semibold text-rose-600 bg-rose-500/10 flex items-center justify-center gap-2 hover:bg-rose-500/20 transition-colors disabled:opacity-50"
              >
                <Archive className="w-4 h-4" /> Archivar
              </button>
            </>
          )}
          {flag.status === 'Active' && (
            <button
              onClick={() => deactivateFlag.mutate()}
              disabled={deactivateFlag.isPending}
              className="flex-1 py-2 px-3 rounded-lg border border-amber-500/30 text-xs font-semibold text-amber-600 bg-amber-500/10 flex items-center justify-center gap-2 hover:bg-amber-500/20 transition-colors disabled:opacity-50"
            >
              <ToggleLeft className="w-4 h-4" /> Desactivar
            </button>
          )}
          {flag.status === 'Archived' && (
            <div className="flex-1 py-2 px-3 rounded-lg border border-m3-outline/20 text-xs text-m3-secondary/50 text-center">
              Archivado — acción terminal
            </div>
          )}
        </div>
      </div>

      {/* Criteria */}
      <div className="space-y-3">
        <h4 className="text-xs font-bold uppercase tracking-wider text-m3-secondary">
          Criterios de Evaluación ({flag.criteria.length})
        </h4>

        {flag.criteria.length === 0 && (
          <div className="flex items-center gap-2 text-[11px] text-m3-secondary/70 bg-m3-surface-variant/30 p-2 rounded">
            <Info className="w-3.5 h-3.5" />
            Sin criterios. El flag se evalúa solo por targeting rules.
          </div>
        )}

        <div className="space-y-1.5">
          {flag.criteria.map(c => (
            <CriteriaRow
              key={c.criteriaId}
              criteria={c}
              onRemove={() => removeCriteria.mutate(c.criteriaId)}
              isDraft={isDraft}
            />
          ))}
        </div>

        {isDraft && (
          <form onSubmit={handleAddCriteria} className="p-3 rounded-lg border border-m3-outline/20 bg-m3-surface-container/20 space-y-2">
            <div className="flex items-center gap-1 text-xs font-bold text-m3-on-surface">
              <Plus className="w-3.5 h-3.5" /> Agregar Criterio
            </div>
            <div className="grid grid-cols-2 gap-2">
              <select
                value={criteriaType}
                onChange={e => setCriteriaType(e.target.value)}
                className="w-full h-9 px-2 rounded-lg bg-m3-surface border border-m3-outline/20 text-xs focus:border-m3-primary outline-none"
              >
                {Object.entries(CRITERIA_TYPE_LABELS).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
              <select
                value={criteriaOperator}
                onChange={e => setCriteriaOperator(e.target.value)}
                className="w-full h-9 px-2 rounded-lg bg-m3-surface border border-m3-outline/20 text-xs focus:border-m3-primary outline-none"
              >
                {Object.entries(CRITERIA_OPERATOR_LABELS).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
            </div>
            <M3TextField
              label="Valor"
              value={criteriaValue}
              onChange={e => { setCriteriaValue(e.target.value); setCriteriaError(''); }}
              placeholder="e.g. tenant-id, role-code, percentage"
            />
            {criteriaError && <p className="text-[11px] text-m3-error">{criteriaError}</p>}
            <div className="flex justify-end">
              <M3Button
                type="submit"
                variant="filled"
                disabled={addCriteria.isPending || !criteriaValue.trim()}
              >
                {addCriteria.isPending ? 'Agregando…' : 'Agregar'}
              </M3Button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
