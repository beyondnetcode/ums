import React from 'react';
import { useAuthStore } from '../../../application/stores/auth.store';
import { useNotificationStore } from '../../../application/stores/notification.store';
import { M3Card } from '../../shared/components/M3Card';
import { M3Button } from '../../shared/components/M3Button';
import { 
  User, 
  ShieldCheck, 
  Globe, 
  Clock, 
  Terminal, 
  Activity, 
  CheckCircle,
  AlertCircle
} from 'lucide-react';

export const ProfileScreen: React.FC = () => {
  const { user, devUserId, devLanguage, isAuthenticated } = useAuthStore();
  const { notifications, addNotification } = useNotificationStore();

  const handleTriggerTest = (type: 'success' | 'warning' | 'error' | 'info') => {
    addNotification({
      title: `Developer ${type.charAt(0).toUpperCase() + type.slice(1)} Event`,
      message: `Self-triggered development telemetry verification at ${new Date().toLocaleTimeString()}`,
      type,
    });
  };

  const activeUser = isAuthenticated && user ? user : {
    username: 'anonymous_developer',
    email: 'dev@beyondnetcode.local',
    role: 'administrator (unauthenticated)'
  };

  return (
    <div className="space-y-6 select-none animate-fadeIn">
      {/* Top Banner */}
      <div>
        <h2 className="text-xl font-extrabold tracking-tight text-m3-on-surface">
          Developer Profile & Diagnostics
        </h2>
        <p className="text-xs text-m3-secondary">
          Live workspace status, authorization headers, and telemetry logs.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
        
        {/* LEFT CARD: User Session Info (7 columns) */}
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
            <div className="flex justify-between items-center border-b border-m3-outline/20 pb-2.5">
              <span className="text-m3-secondary font-bold uppercase tracking-wider text-[10px] flex items-center gap-1.5">
                <User className="w-3.5 h-3.5" /> Security Role
              </span>
              <span className="px-2.5 py-0.5 rounded-full font-bold uppercase text-[9px] bg-m3-primary/10 border border-m3-primary/20 text-m3-primary">
                {activeUser.role}
              </span>
            </div>

            <div className="flex justify-between items-center border-b border-m3-outline/20 pb-2.5">
              <span className="text-m3-secondary font-bold uppercase tracking-wider text-[10px] flex items-center gap-1.5">
                <ShieldCheck className="w-3.5 h-3.5" /> X-User-Id Header
              </span>
              <span className="font-mono text-m3-primary font-bold text-[11px] bg-m3-surface-container px-2 py-1 rounded border border-m3-outline/30 max-w-[200px] truncate" title={devUserId}>
                {devUserId}
              </span>
            </div>

            <div className="flex justify-between items-center border-b border-m3-outline/20 pb-2.5">
              <span className="text-m3-secondary font-bold uppercase tracking-wider text-[10px] flex items-center gap-1.5">
                <Globe className="w-3.5 h-3.5" /> X-Language Header
              </span>
              <span className="font-extrabold uppercase text-m3-on-surface bg-m3-surface-container px-2 py-0.5 rounded border border-m3-outline/30">
                {devLanguage}
              </span>
            </div>

            <div className="flex justify-between items-center pb-1">
              <span className="text-m3-secondary font-bold uppercase tracking-wider text-[10px] flex items-center gap-1.5">
                <Clock className="w-3.5 h-3.5" /> Connection State
              </span>
              <span className="text-emerald-500 font-extrabold flex items-center gap-1">
                <CheckCircle className="w-3.5 h-3.5" /> Synced with Local API
              </span>
            </div>
          </div>

          <div className="mt-6 pt-5 border-t border-m3-outline/20">
            <h4 className="text-[10px] font-extrabold uppercase tracking-wider text-m3-secondary mb-3">
              Trigger UI Diagnostic Telemetry
            </h4>
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
              <M3Button variant="tonal" onClick={() => handleTriggerTest('success')} className="py-1 px-2 text-[10px] h-8">
                Success
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

        {/* RIGHT CARD: Diagnostics & Core Standard Telemetry (5 columns) */}
        <div className="md:col-span-5 flex flex-col gap-6">
          <M3Card variant="filled" className="p-5 rounded-2xl bg-m3-surface-container/40">
            <div className="flex items-center gap-2 mb-3.5">
              <Activity className="w-4 h-4 text-m3-primary" />
              <h3 className="text-xs font-bold uppercase tracking-widest text-m3-on-surface">
                Workspace Health
              </h3>
            </div>

            <div className="space-y-3 text-[11px] text-m3-secondary font-medium">
              <div className="p-3 bg-m3-surface-container/60 rounded-xl border border-m3-outline/25 flex items-center justify-between">
                <span>API Port Listener</span>
                <span className="font-mono text-m3-primary font-bold">5293 [HTTP]</span>
              </div>
              <div className="p-3 bg-m3-surface-container/60 rounded-xl border border-m3-outline/25 flex items-center justify-between">
                <span>Database Status</span>
                <span className="text-emerald-500 font-extrabold flex items-center gap-1">
                  <CheckCircle className="w-3 h-3" /> PostgreSQL 16
                </span>
              </div>
              <div className="p-3 bg-m3-surface-container/60 rounded-xl border border-m3-outline/25 flex items-center justify-between">
                <span>Architecture</span>
                <span className="font-extrabold text-m3-on-surface">Clean Hexagonal (.NET 8)</span>
              </div>
            </div>
          </M3Card>

          <M3Card variant="outlined" className="p-5 rounded-2xl border-m3-outline/40">
            <div className="flex items-center gap-2 mb-3">
              <Terminal className="w-4 h-4 text-indigo-400" />
              <h3 className="text-xs font-bold uppercase tracking-widest text-m3-on-surface">
                Recent Audit Trail
              </h3>
            </div>

            <div className="space-y-2.5 max-h-[140px] overflow-y-auto pr-1">
              {notifications.slice(0, 3).map((n) => (
                <div key={n.id} className="p-2.5 bg-m3-surface-container/30 rounded-xl border border-m3-outline/20 text-[10px] leading-relaxed flex gap-2">
                  {n.type === 'success' ? (
                    <CheckCircle className="w-3.5 h-3.5 text-emerald-500 flex-shrink-0 mt-0.5" />
                  ) : n.type === 'error' ? (
                    <AlertCircle className="w-3.5 h-3.5 text-rose-500 flex-shrink-0 mt-0.5" />
                  ) : (
                    <Clock className="w-3.5 h-3.5 text-indigo-400 flex-shrink-0 mt-0.5" />
                  )}
                  <div>
                    <p className="font-bold text-m3-on-surface">{n.title}</p>
                    <p className="text-m3-secondary">{n.message}</p>
                  </div>
                </div>
              ))}
              {notifications.length === 0 && (
                <p className="text-[10px] text-m3-secondary text-center py-4">No audit notifications recorded.</p>
              )}
            </div>
          </M3Card>
        </div>
      </div>
    </div>
  );
};

export default ProfileScreen;
