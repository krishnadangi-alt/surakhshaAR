import React from 'react';
import { Drawer } from '../common/Drawer';
import { mockNotifications } from '../../mockData';
import { AlertOctagon, AlertTriangle, CheckCircle, Info } from 'lucide-react';

interface NotificationsDrawerProps {
  isOpen: boolean;
  onClose: () => void;
}

export const NotificationsDrawer: React.FC<NotificationsDrawerProps> = ({ isOpen, onClose }) => {
  return (
    <Drawer isOpen={isOpen} onClose={onClose} title="Safety & Compliance Alerts" subtitle="Real-time telemetric notifications from industrial nodes">
      <div className="space-y-3">
        {mockNotifications.map((n) => (
          <div
            key={n.id}
            className={`p-4 rounded-xl border transition ${
              n.type === 'critical'
                ? 'border-rose-500/30 bg-rose-500/10'
                : n.type === 'warning'
                ? 'border-amber-500/30 bg-amber-500/10'
                : n.type === 'success'
                ? 'border-emerald-500/30 bg-emerald-500/10'
                : 'border-suraksha-border bg-suraksha-surface'
            }`}
          >
            <div className="flex items-start gap-3">
              <div className="shrink-0 mt-0.5">
                {n.type === 'critical' && <AlertOctagon className="w-5 h-5 text-rose-400" />}
                {n.type === 'warning' && <AlertTriangle className="w-5 h-5 text-amber-400" />}
                {n.type === 'success' && <CheckCircle className="w-5 h-5 text-emerald-400" />}
                {n.type === 'info' && <Info className="w-5 h-5 text-blue-400" />}
              </div>
              <div className="flex-1">
                <div className="flex items-center justify-between">
                  <h5 className="text-xs font-bold text-white">{n.title}</h5>
                  <span className="text-[10px] text-suraksha-subtext">{n.timestamp}</span>
                </div>
                <p className="text-xs text-suraksha-text/90 mt-1">{n.message}</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </Drawer>
  );
};
