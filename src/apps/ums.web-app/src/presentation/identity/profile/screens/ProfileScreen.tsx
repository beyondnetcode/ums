import React from 'react';
import { useAuthStore } from '@app/stores/auth.store';
import { useDevToolsStore } from '@app/stores/devTools.store';
import { useI18nStore } from '@app/stores/i18n.store';
import { useNotificationStore } from '@app/stores/notification.store';
import { useI18n } from '@app/i18n/use-i18n';
import { SYSTEM_INFO } from '@domain/identity/constants/system-info.constants';
import { M3Card } from '@shared/components/M3Card';
import { M3Button } from '@shared/components/M3Button';
import { KeyValueRow } from '@shared/components/KeyValueRow';
import {
  User,
  ShieldCheck,
  Globe,
  Clock,
  Terminal,
  Activity,
  CheckCircle,
} from 'lucide-react';
import { NOTIFICATION_ICONS, NOTIFICATION_ICON_CLASSES } from '@shared/theme/notification-theme';
import type { NotificationType } from '@shared/theme/notification-theme';

export default function ProfileScreen(): React.JSX.Element {
  const { user, isAuthenticated } = useAuthStore();
  const { devUserId } = useDevToolsStore();
  const { language } = useI18nStore();
  const { notifications, addNotification } = useNotificationStore();
  const t = useI18n();

  const handleTriggerTest = (type: 'success' | 'warning' | 'error' | 'info') => {
    const titles = { success: t.devTestSuccess, warning: t.devTestWarning, error: t.devTestError, info: t.devTestInfo };
    addNotification({
      title: titles[type],
      message: t.devTestMsg(new Date().toLocaleTimeString()),
      type,
    });
  };

  const activeUser = isAuthenticated && user ? user : {
    username: 'anonymous_developer',
    email: 'dev@logistica.pe',
    role: 'administrator (unauthenticated)'
  };

  return (
    <div className="space-y-6 select-none animate-fadeIn">
      <div>
        <h2 className="text-xl font-extrabold tracking-tight text-m3-on-surface">
          {t.profileTitle}
        </h2>
        <p className="text-xs text-m3-secondary">
          {t.profileSubtitle}
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-12 gap-6">

        {/* LEFT CARD: User Session Info */}
        <M3Card variant="elevated" className="md:col-span-7 p-6 border border-m3-outline/25 bg-m3-surface-container/20">
          <div className="flex items-center gap-4 mb-6">
            <div className="w-14 h-14 rounded-3xl bg-gradient-to-tr from-m3-primary to-indigo-600 flex items-center justify-center font-black text-lg text-white shadow-lg shadow-m3-primary/20">
              {activeUser.username.substring(0, 2).toUpperCase()}
            </div>
            <div>
              <h3 className="text-base font-extrabold text-m3-on-surface">{activeUser.username}</h3>
              <p className="text-[11px] text-m3-secondary">{activeUser.email}</p>
            </div>
          </div>

          <div className="space-y-3.5 text-xs">
            <KeyValueRow
              icon={<User className="w-3.5 h-3.5" />}
              label={t.securityRole}
              value={
                <span className="px-2.5 py-0.5 rounded-full font-bold uppercase text-[9px] bg-m3-primary/10 border border-m3-primary/20 text-m3-primary">
                  {activeUser.role}
                </span>
              }
            />
            <KeyValueRow
              icon={<ShieldCheck className="w-3.5 h-3.5" />}
              label={t.xUserIdHeader}
              value={
                <span
                  className="font-mono text-m3-primary font-bold text-[11px] bg-m3-surface-container px-2 py-1 rounded border border-m3-outline/30 cursor-help select-all"
                  title={devUserId}
                >
                  {devUserId.substring(0, 8)}…
                </span>
              }
            />
            <KeyValueRow
              icon={<Globe className="w-3.5 h-3.5" />}
              label={t.xLanguageHeader}
              value={
                <span className="font-extrabold uppercase text-m3-on-surface bg-m3-surface-container px-2 py-0.5 rounded border border-m3-outline/30">
                  {language}
                </span>
              }
            />
            <KeyValueRow
              icon={<Clock className="w-3.5 h-3.5" />}
              label={t.connectionState}
              value={
                <span className="text-emerald-500 font-extrabold flex items-center gap-1">
                  <CheckCircle className="w-3.5 h-3.5" /> {t.synced}
                </span>
              }
              bordered={false}
            />
          </div>

          <div className="mt-6 pt-5 border-t border-m3-outline/20">
            <h4 className="text-[10px] font-extrabold uppercase tracking-wider text-m3-secondary mb-3">
              {t.diagnosticTitle}
            </h4>
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
              <M3Button variant="tonal" onClick={() => handleTriggerTest('success')} className="py-1 px-2 text-[10px] h-8">
                {t.active}
              </M3Button>
              <M3Button variant="outlined" onClick={() => handleTriggerTest('info')} className="py-1 px-2 text-[10px] h-8">
                Info
              </M3Button>
              <M3Button variant="outlined" onClick={() => handleTriggerTest('warning')} className="py-1 px-2 text-[10px] h-8 text-amber-500 border-amber-500/30 hover:bg-amber-500/10">
                Warning
              </M3Button>
              <M3Button variant="outlined" onClick={() => handleTriggerTest('error')} className="py-1 px-2 text-[10px] h-8 text-rose-500 border-rose-500/30 hover:bg-rose-500/10">
                Error
              </M3Button>
            </div>
          </div>
        </M3Card>

        {/* RIGHT CARD: Diagnostics */}
        <div className="md:col-span-5 flex flex-col gap-6">
          <M3Card variant="filled" className="p-5 rounded-2xl bg-m3-surface-container/40">
            <div className="flex items-center gap-2 mb-3.5">
              <Activity className="w-4 h-4 text-m3-primary" />
              <h3 className="text-xs font-bold uppercase tracking-widest text-m3-on-surface">
                {t.workspaceHealth}
              </h3>
            </div>

            <div className="space-y-3 text-[11px] text-m3-secondary font-medium">
              <div className="p-3 bg-m3-surface-container/60 rounded-xl border border-m3-outline/25 flex items-center justify-between">
                <span>{t.apiPort}</span>
                <span className="font-mono text-m3-primary font-bold">{SYSTEM_INFO.apiPort}</span>
              </div>
              <div className="p-3 bg-m3-surface-container/60 rounded-xl border border-m3-outline/25 flex items-center justify-between">
                <span>{t.dbStatus}</span>
                <span className="text-emerald-500 font-extrabold flex items-center gap-1">
                  <CheckCircle className="w-3 h-3" /> {SYSTEM_INFO.dbEngine}
                </span>
              </div>
              <div className="p-3 bg-m3-surface-container/60 rounded-xl border border-m3-outline/25 flex items-center justify-between">
                <span>{t.architecture}</span>
                <span className="font-extrabold text-m3-on-surface">{SYSTEM_INFO.architecture}</span>
              </div>
            </div>
          </M3Card>

          <M3Card variant="outlined" className="p-5 rounded-2xl border-m3-outline/40">
            <div className="flex items-center gap-2 mb-3">
              <Terminal className="w-4 h-4 text-indigo-400" />
              <h3 className="text-xs font-bold uppercase tracking-widest text-m3-on-surface">
                {t.auditTrail}
              </h3>
            </div>

            <div className="space-y-2.5 max-h-[140px] overflow-y-auto pr-1">
              {notifications.slice(0, 3).map((n) => (
                <div key={n.id} className="p-2.5 bg-m3-surface-container/30 rounded-xl border border-m3-outline/20 text-[10px] leading-relaxed flex gap-2">
                  <span className="flex-shrink-0 mt-0.5">
                    {React.createElement(NOTIFICATION_ICONS[n.type as NotificationType], {
                      className: `w-3.5 h-3.5 ${NOTIFICATION_ICON_CLASSES[n.type as NotificationType]}`,
                    })}
                  </span>
                  <div>
                    <p className="font-bold text-m3-on-surface">{n.title}</p>
                    <p className="text-m3-secondary">{n.message}</p>
                  </div>
                </div>
              ))}
              {notifications.length === 0 && (
                <p className="text-[10px] text-m3-secondary text-center py-4">{t.noAudit}</p>
              )}
            </div>
          </M3Card>
        </div>
      </div>
    </div>
  );
};
