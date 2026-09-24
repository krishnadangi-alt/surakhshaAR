import React, { useState, useEffect } from 'react';
import { Drawer } from '../common/Drawer';
import { fetchDashboardAssessments } from '../../services/api';
import type { NotificationItem } from '../../types';
import { AlertOctagon, AlertTriangle, CheckCircle, Info } from 'lucide-react';

interface NotificationsDrawerProps {
  isOpen: boolean;
  onClose: () => void;
}

export const NotificationsDrawer: React.FC<NotificationsDrawerProps> = ({ isOpen, onClose }) => {
  const [alerts, setAlerts] = useState<NotificationItem[]>([]);

  useEffect(() => {
    if (isOpen) {
      fetchDashboardAssessments().then((assessments) => {
        const derived: NotificationItem[] = [];
        (assessments || []).forEach((a, idx) => {
          if (a.criticalErrors > 0) {
            derived.push({
              id: `crit-${a.id || idx}`,
              title: `Critical Safety Flag: ${a.workerName}`,
              message: a.criticalErrorDetails || `Recorded ${a.criticalErrors} critical error(s) during ${a.moduleName}`,
              timestamp: a.dateTime,
              type: 'critical',
              read: false,
            });
          } else if (a.passFail === 'Fail') {
            derived.push({
              id: `fail-${a.id || idx}`,
              title: `Remedial Retraining Order: ${a.workerName}`,
              message: `Scored ${a.score}% in ${a.moduleName} (required 70%). Assigned remedial AR training module.`,
              timestamp: a.dateTime,
              type: 'warning',
              read: false,
            });
          } else if (a.score >= 90 && idx < 5) {
            derived.push({
              id: `pass-${a.id || idx}`,
              title: `Exemplary Assessment: ${a.workerName}`,
              message: `Achieved ${a.score}% score in ${a.moduleName} with zero errors. Certified status active.`,
              timestamp: a.dateTime,
              type: 'success',
              read: true,
            });
          }
        });
        setAlerts(derived.slice(0, 10));
      });
    }
  }, [isOpen]);

  return (
    <Drawer isOpen={isOpen} onClose={onClose} title="Safety & Compliance Alerts" subtitle="Real-time telemetric notifications from industrial nodes">
      <div className="space-y-3">
        {alerts.length > 0 ? (
          alerts.map((n) => (
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
          ))
        ) : (
          <div className="p-8 text-center text-xs text-suraksha-subtext bg-suraksha-surface/30 rounded-xl">
            ✓ All active mines and facilities operating normally. No active safety flags.
          </div>
        )}
      </div>
    </Drawer>
  );
};
