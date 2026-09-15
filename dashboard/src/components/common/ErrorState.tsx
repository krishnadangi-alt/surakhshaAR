import React from 'react';
import { AlertTriangle, RotateCcw } from 'lucide-react';

interface ErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
}

export const ErrorState: React.FC<ErrorStateProps> = ({
  title = 'System Synchronization Warning',
  message = 'Unable to fetch real-time telemetry from Jharkhand Industrial Safety Node. Please verify network status.',
  onRetry,
}) => {
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-rose-500/30 bg-rose-500/5 p-8 text-center">
      <div className="rounded-full bg-rose-500/10 p-3 text-rose-400 mb-3">
        <AlertTriangle className="w-6 h-6" />
      </div>
      <h4 className="text-sm font-bold text-white mb-1">{title}</h4>
      <p className="text-xs text-suraksha-subtext max-w-md mb-4">{message}</p>
      {onRetry && (
        <button
          onClick={onRetry}
          className="inline-flex items-center gap-1.5 rounded-lg border border-rose-500/30 bg-rose-500/10 px-3.5 py-1.5 text-xs font-semibold text-rose-300 hover:bg-rose-500/20 transition"
        >
          <RotateCcw className="w-3.5 h-3.5" />
          <span>Retry Sync</span>
        </button>
      )}
    </div>
  );
};
