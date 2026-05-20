import React from 'react';
import { useNotificationStore } from '../../../application/stores/notification.store';
import { X, Trash2, CheckCheck, Info, CheckCircle, AlertTriangle, XCircle } from 'lucide-react';
import { M3Button } from './M3Button';

export const NotificationCenter: React.FC = () => {
  const { notifications, isOpen, setIsOpen, clearAll, markAllAsRead, markAsRead } = useNotificationStore();

  if (!isOpen) return null;

  const icons = {
    info: <Info className="w-4 h-4 text-sky-500" />,
    success: <CheckCircle className="w-4 h-4 text-emerald-500" />,
    warning: <AlertTriangle className="w-4 h-4 text-amber-500" />,
    error: <XCircle className="w-4 h-4 text-rose-500" />,
  };

  const borders = {
    info: 'border-sky-500/20 bg-sky-500/5',
    success: 'border-emerald-500/20 bg-emerald-500/5',
    warning: 'border-amber-500/20 bg-amber-500/5',
    error: 'border-rose-500/20 bg-rose-500/5',
  };

  const unreadCount = notifications.filter((n) => !n.read).length;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden select-none">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/40 backdrop-blur-sm transition-opacity"
        onClick={() => setIsOpen(false)}
      />

      <div className="pointer-events-none fixed inset-y-0 right-0 flex max-w-full pl-10">
        <div className="pointer-events-auto w-screen max-w-md">
          <div className="flex h-full flex-col bg-m3-surface border-l border-m3-outline/25 shadow-2xl transition-all duration-300">
            {/* Header */}
            <div className="flex items-center justify-between px-6 py-5 border-b border-m3-outline/30">
              <div>
                <h2 className="text-base font-extrabold tracking-tight text-m3-on-surface">
                  Operations Audit Log
                </h2>
                {unreadCount > 0 && (
                  <p className="text-[10px] text-m3-primary font-bold tracking-wide uppercase mt-1">
                    {unreadCount} unread operations
                  </p>
                )}
              </div>
              <button
                onClick={() => setIsOpen(false)}
                className="p-2 rounded-full hover:bg-m3-primary/10 text-m3-secondary transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Actions Panel */}
            <div className="flex gap-2 px-6 py-3 bg-m3-surface-container/30 border-b border-m3-outline/10 text-xs">
              <button
                onClick={markAllAsRead}
                disabled={unreadCount === 0}
                className="inline-flex items-center gap-1.5 text-m3-primary hover:text-m3-primary/80 font-bold uppercase tracking-wider text-[10px] disabled:opacity-40"
              >
                <CheckCheck className="w-3.5 h-3.5" /> Read All
              </button>
              <span className="text-m3-outline">|</span>
              <button
                onClick={clearAll}
                disabled={notifications.length === 0}
                className="inline-flex items-center gap-1.5 text-m3-error hover:text-m3-error/80 font-bold uppercase tracking-wider text-[10px] disabled:opacity-40"
              >
                <Trash2 className="w-3.5 h-3.5" /> Clear All
              </button>
            </div>

            {/* Notifications List */}
            <div className="flex-1 overflow-y-auto p-6 space-y-4">
              {notifications.length === 0 ? (
                <div className="h-64 flex flex-col items-center justify-center text-center text-xs text-m3-secondary/70 gap-2">
                  <Info className="w-6 h-6 text-m3-outline" />
                  No operations audited yet.
                </div>
              ) : (
                notifications.map((n) => (
                  <div
                    key={n.id}
                    onClick={() => markAsRead(n.id)}
                    className={`p-4 rounded-2xl border transition-all cursor-pointer hover:border-m3-primary/30 relative overflow-hidden flex gap-3 ${
                      borders[n.type]
                    } ${!n.read ? 'ring-1 ring-m3-primary/30' : ''}`}
                  >
                    {!n.read && (
                      <span className="absolute top-3 right-3 w-2 h-2 rounded-full bg-m3-primary animate-pulse" />
                    )}
                    <div className="flex-shrink-0 mt-0.5">{icons[n.type]}</div>
                    <div className="flex-1">
                      <h4 className="text-xs font-bold text-m3-on-surface">{n.title}</h4>
                      <p className="text-[11px] text-m3-secondary mt-1 leading-relaxed">{n.message}</p>
                      <span className="block text-[9px] text-m3-secondary/50 font-bold tracking-wider uppercase mt-2.5">
                        {new Date(n.timestamp).toLocaleTimeString()}
                      </span>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
