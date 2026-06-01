import React, { useMemo, useState } from 'react';
import { CheckCircle2, Clock3, Mail, Building2, Loader2 } from 'lucide-react';
import { M3Card } from '@shared/components/M3Card';
import { M3Button } from '@shared/components/M3Button';
import { StatusBadge } from '@shared/components/StatusBadge';
import { useI18n } from '@app/i18n/use-i18n';
import { useApproveTenantSignupRequest, useGetTenantSignupRequests } from '@app/identity/hooks/use-tenant-signup-request';

export const TenantSignupRequestsPanel: React.FC = () => {
  const t = useI18n();
  const { data, isLoading, error } = useGetTenantSignupRequests(true);
  const approveMutation = useApproveTenantSignupRequest();
  const [approvedMessage, setApprovedMessage] = useState<string | null>(null);

  const pendingCount = useMemo(() => data?.length ?? 0, [data]);

  const handleApprove = async (tenantSignupRequestId: string) => {
    const response = await approveMutation.mutateAsync(tenantSignupRequestId);
    setApprovedMessage(
      `${response.message} Usuario: ${response.userAccountId}. Contraseña temporal: ${response.temporaryPassword}`,
    );
  };

  return (
    <M3Card variant="filled" className="p-4 border border-m3-outline/25 bg-m3-surface-container/20">
      <div className="flex flex-col gap-3">
        <div className="flex items-start justify-between gap-3">
          <div>
            <div className="flex items-center gap-2 text-m3-primary">
              <Building2 className="w-4 h-4" />
              <h3 className="text-sm font-semibold uppercase tracking-wider text-m3-on-surface">
                Solicitudes de tenant
              </h3>
            </div>
            <p className="text-xs text-m3-secondary mt-1">
              Revisa los onboardings pendientes y aprueba el alta del tenant y su usuario administrador.
            </p>
          </div>
          <StatusBadge status={pendingCount > 0 ? 'Pending' : 'Active'} label={pendingCount > 0 ? `${pendingCount} pendientes` : 'Sin pendientes'} />
        </div>

        {approvedMessage && (
          <div className="rounded-xl border border-emerald-500/25 bg-emerald-500/5 p-3 text-xs text-m3-secondary">
            <div className="flex items-center gap-2 text-emerald-600 mb-1">
              <CheckCircle2 className="w-4 h-4" />
              <span className="font-semibold">Aprobación procesada</span>
            </div>
            <p className="leading-relaxed">{approvedMessage}</p>
          </div>
        )}

        {error && (
          <div className="rounded-xl border border-m3-error/20 bg-m3-error-container/20 p-3 text-xs text-m3-error">
            No se pudieron cargar las solicitudes pendientes.
          </div>
        )}

        {isLoading ? (
          <div className="flex items-center gap-2 text-xs text-m3-secondary py-3">
            <Loader2 className="w-4 h-4 animate-spin" />
            Cargando solicitudes...
          </div>
        ) : data && data.length > 0 ? (
          <div className="grid grid-cols-1 xl:grid-cols-2 gap-3">
            {data.map((request) => (
              <div
                key={request.tenantSignupRequestId}
                className="rounded-xl border border-m3-outline/20 bg-m3-surface/70 p-4 flex flex-col gap-3"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <p className="text-sm font-semibold text-m3-on-surface line-clamp-1">
                      {request.companyName}
                    </p>
                    <p className="text-[11px] text-m3-secondary mt-1">
                      {request.companyReference}
                    </p>
                  </div>
                  <StatusBadge status={request.status} label={request.status} />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs text-m3-secondary">
                  <div className="flex items-start gap-2">
                    <Clock3 className="w-3.5 h-3.5 mt-0.5 shrink-0" />
                    <span>{new Date(request.requestedAtUtc).toLocaleString()}</span>
                  </div>
                  <div className="flex items-start gap-2">
                    <Mail className="w-3.5 h-3.5 mt-0.5 shrink-0" />
                    <span className="break-all">{request.contactEmail}</span>
                  </div>
                  <div className="flex items-start gap-2 sm:col-span-2">
                    <Building2 className="w-3.5 h-3.5 mt-0.5 shrink-0" />
                    <span>Contacto: {request.contactName}</span>
                  </div>
                </div>

                <M3Button
                  type="button"
                  variant="filled"
                  className="self-start"
                  disabled={approveMutation.isPending}
                  onClick={() => {
                    void handleApprove(request.tenantSignupRequestId);
                  }}
                >
                  Aprobar solicitud
                </M3Button>
              </div>
            ))}
          </div>
        ) : (
          <div className="rounded-xl border border-dashed border-m3-outline/20 bg-m3-surface/40 p-4 text-xs text-m3-secondary">
            No hay solicitudes pendientes en este momento.
          </div>
        )}
      </div>
    </M3Card>
  );
};
