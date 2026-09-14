import React from 'react';
import { ShieldAlert, RefreshCw } from 'lucide-react';

interface EmptyStateProps {
  title?: string;
  description?: string;
  onReset?: () => void;
  actionText?: string;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  title = 'No Records Found',
  description = 'No data matching your current filter criteria was found in the system registry.',
  onReset,
  actionText = 'Reset Filters',
}) => {
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-suraksha-border bg-suraksha-card p-12 text-center">
      <div className="rounded-full bg-suraksha-surface border border-suraksha-border p-4 text-suraksha-amber mb-3">
        <ShieldAlert className="w-8 h-8" />
      </div>
      <h4 className="text-base font-bold text-white mb-1">{title}</h4>
      <p className="text-xs text-suraksha-subtext max-w-md mb-5">{description}</p>
      {onReset && (
        <button
          onClick={onReset}
          className="inline-flex items-center gap-2 rounded-lg bg-suraksha-surface border border-suraksha-border px-4 py-2 text-xs font-semibold text-white hover:bg-suraksha-hover transition"
        >
          <RefreshCw className="w-3.5 h-3.5 text-suraksha-amber" />
          <span>{actionText}</span>
        </button>
      )}
    </div>
  );
};
