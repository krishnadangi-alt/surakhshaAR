import React from 'react';
import { Lock } from 'lucide-react';

interface UnauthorizedStateProps {
  title?: string;
  message?: string;
  onBackToOverview?: () => void;
}

export const UnauthorizedState: React.FC<UnauthorizedStateProps> = ({
  title = 'Access Restricted',
  message = 'Your current role does not permit access to this administrative screen. Authorized dashboard navigation is limited to your assigned access profile.',
  onBackToOverview,
}) => {
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-rose-500/30 bg-rose-500/5 p-12 text-center min-h-[320px]">
      <div className="rounded-full bg-rose-500/10 p-4 text-rose-400 border border-rose-500/30 mb-4">
        <Lock className="w-8 h-8" />
      </div>
      <h4 className="text-base font-bold text-white mb-2">{title}</h4>
      <p className="text-xs text-suraksha-subtext max-w-md mb-5">{message}</p>
      {onBackToOverview && (
        <button
          onClick={onBackToOverview}
          className="rounded-lg bg-suraksha-blue px-4 py-2 text-xs font-semibold text-white hover:bg-suraksha-blueHover transition shadow-subtle"
        >
          Return to Overview
        </button>
      )}
    </div>
  );
};