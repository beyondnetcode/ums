import React, { useState } from 'react';
import { useAuthStore } from '@app/stores/auth.store';
import { useDevToolsStore } from '@app/stores/devTools.store';
import { useI18n } from '@app/i18n/use-i18n';
import { M3Card } from '@shared/components/M3Card';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { KeyValueRow } from '@shared/components/KeyValueRow';
import { M3SegmentedButton } from '@shared/components/M3SegmentedButton';
import type { SegmentOption } from '@shared/components/M3SegmentedButton';
import { ShieldCheck, Cpu, Key } from 'lucide-react';
import { useNotificationStore } from '@app/stores/notification.store';
import { DEV_PROFILES } from '@domain/identity/constants/dev-profiles.constants';

export const LoginScreen: React.FC = () => {
  const { login, isAuthenticated, user, logout } = useAuthStore();
  const { devUserId, setDevUserId } = useDevToolsStore();
  const addNotification = useNotificationStore((state) => state.addNotification);
  const t = useI18n();

  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [role, setRole] = useState('admin');

  const roleOptions: SegmentOption[] = [
    { value: 'user', label: 'user' },
    { value: 'moderator', label: 'moderator' },
    { value: 'admin', label: 'admin' },
  ];

  const devProfiles = DEV_PROFILES.map((p) => ({
    name: t[p.nameKey as keyof typeof t] as string,
    id: p.id,
    role: p.role,
    email: p.email,
    username: p.username,
  }));

  const handleDevProfileSelect = (p: typeof devProfiles[0]) => {
    setDevUserId(p.id);
    login({ id: p.id, username: p.username, email: p.email, role: p.role });
    addNotification({
      title: t.profileSwitched,
      message: t.profileSwitchedMsg(p.name, p.id.substring(0, 8)),
      type: 'info',
    });
  };

  const handleCustomLogin = (e: React.FormEvent) => {
    e.preventDefault();
    if (username.length < 3) return;

    const generatedId = crypto.randomUUID();
    setDevUserId(generatedId);
    login({ id: generatedId, username, email: email || 'custom@ums.pe', role });
    addNotification({
      title: t.sessionAuth,
      message: t.sessionAuthMsg(username, role),
      type: 'success',
    });
  };

  if (isAuthenticated && user) {
    return (
      <div className="max-w-2xl mx-auto space-y-6 select-none">
        <M3Card variant="elevated" className="text-center p-8 border border-m3-outline/25 bg-m3-surface-container/20">
          <div className="w-16 h-16 rounded-full bg-emerald-500/10 border border-emerald-500/30 flex items-center justify-center mx-auto text-emerald-500 mb-4 shadow">
            <ShieldCheck className="w-8 h-8" />
          </div>
          <h2 className="text-xl font-extrabold tracking-tight text-m3-on-surface">
            {t.activeSessionTitle}
          </h2>
          <p className="text-xs text-m3-secondary mt-1">
            {t.activeSessionSubtitle}
          </p>

          <div className="my-6 p-5 bg-m3-surface-container/70 rounded-2xl border border-m3-outline/30 text-left space-y-3.5 text-xs">
            <KeyValueRow label={t.labelUsername} value={user.username} />
            <KeyValueRow label={t.labelEmail} value={user.email} />
            <KeyValueRow
              label={t.labelRole}
              value={
                <span className="px-2.5 py-0.5 rounded-full font-bold uppercase text-[9px] bg-m3-primary/10 border border-m3-primary/20 text-m3-primary">
                  {user.role}
                </span>
              }
            />
            <KeyValueRow label={t.labelXUserId} value={<span className="font-mono text-m3-primary font-bold">{devUserId}</span>} bordered={false} />
          </div>

          <div className="flex justify-center gap-3">
            <M3Button variant="outlined" onClick={logout}>
              {t.logoutBtn}
            </M3Button>
          </div>
        </M3Card>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto grid grid-cols-1 md:grid-cols-12 gap-8 select-none">

      {/* Left Pane: Custom Credential Simulator */}
      <M3Card variant="elevated" className="md:col-span-7 p-7 border border-m3-outline/25 bg-m3-surface-container/20">
        <div className="flex items-center gap-2 text-m3-primary mb-5">
          <Key className="w-5 h-5" />
          <h3 className="text-sm font-extrabold uppercase tracking-widest text-m3-on-surface">
            {t.sessionCredentials}
          </h3>
        </div>

        <form onSubmit={handleCustomLogin} className="space-y-4">
          <M3TextField
            label={t.devUsername}
            required
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="e.g. operador_callao"
          />

          <M3TextField
            label={t.emailAddress}
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="e.g. operador@ransa.pe"
          />

          <div>
            <label className="block text-[11px] font-bold text-m3-primary uppercase tracking-wider mb-2 ml-1">
              {t.devRole}
            </label>
            <M3SegmentedButton options={roleOptions} value={role} onChange={setRole} />
          </div>

          <div className="pt-3">
            <M3Button variant="filled" className="w-full" type="submit">
              {t.loginBtn}
            </M3Button>
          </div>
        </form>
      </M3Card>

      {/* Right Pane: Quick Dev Profiles */}
      <div className="md:col-span-5 flex flex-col gap-5">
        <div className="px-1">
          <h3 className="text-xs font-extrabold uppercase tracking-widest text-m3-on-surface flex items-center gap-1">
            <Cpu className="w-4 h-4 text-m3-primary" /> {t.mockDevProfiles}
          </h3>
          <p className="text-[10px] text-m3-secondary leading-relaxed mt-1">
            {t.mockDevSubtitle}
          </p>
        </div>

        <div className="space-y-3.5">
          {devProfiles.map((p) => (
            <M3Card
              key={p.id}
              variant="outlined"
              hoverable
              onClick={() => handleDevProfileSelect(p)}
              className="p-[18px] rounded-2xl hover:bg-m3-primary-container/20 border-m3-outline/40"
            >
              <div className="flex justify-between items-center mb-2">
                <span className="font-extrabold text-xs text-m3-on-surface">{p.name}</span>
                <span className="text-[8px] font-extrabold uppercase tracking-wider px-2 py-0.5 rounded-full border bg-m3-primary/10 border-m3-primary/20 text-m3-primary">
                  {p.role}
                </span>
              </div>
              <p className="text-[9px] font-mono text-m3-secondary">ID: {p.id.substring(0, 16)}...</p>
            </M3Card>
          ))}
        </div>
      </div>
    </div>
  );
};
