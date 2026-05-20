import React from 'react';

export interface StatusBadgeProps {
  status: string;
  labels: { active: string; suspended: string; pending: string };
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, labels }) => {
  const label = status === 'Active' ? labels.active : status === 'Suspended' ? labels.suspended : labels.pending;
  const cls = status === 'Active'
    ? 'bg-emerald-500/10 border-emerald-500/25 text-emerald-500'
    : status === 'Suspended'
    ? 'bg-rose-500/10 border-rose-500/25 text-rose-500'
    : 'bg-amber-500/10 border-amber-500/25 text-amber-500';
  return <span className={`text-[10px] font-medium px-2.5 py-0.5 rounded-full border ${cls}`}>{label}</span>;
};
