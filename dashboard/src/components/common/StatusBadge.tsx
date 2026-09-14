import React from 'react';

interface StatusBadgeProps {
  status: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, size = 'md', className = '' }) => {
  let badgeStyle = 'bg-suraksha-surface text-suraksha-subtext border-suraksha-border';
  let dotColor = 'bg-gray-400';

  const normalized = status.toLowerCase();

  if (['passed', 'certified', 'active', 'completed', 'strong', 'pass', 'high'].includes(normalized)) {
    badgeStyle = 'bg-emerald-500/10 text-emerald-400 border-emerald-500/30';
    dotColor = 'bg-emerald-400';
  } else if (['in training', 'in progress', 'assigned', 'developing', 'scheduled', 'pending', 'moderate', 'expiring soon', 'recommended'].includes(normalized)) {
    badgeStyle = 'bg-amber-500/10 text-amber-400 border-amber-500/30';
    dotColor = 'bg-amber-400';
  } else if (['failed', 'retraining required', 'needs retraining', 'expired', 'critical', 'fail', 'reassessment pending', 'overdue'].includes(normalized)) {
    badgeStyle = 'bg-rose-500/10 text-rose-400 border-rose-500/30';
    dotColor = 'bg-rose-400';
  } else if (['blue'].includes(normalized)) {
    badgeStyle = 'bg-blue-500/10 text-blue-400 border-blue-500/30';
    dotColor = 'bg-blue-400';
  }

  const sizeClasses = {
    sm: 'px-2 py-0.5 text-xs font-medium',
    md: 'px-2.5 py-1 text-xs font-semibold',
    lg: 'px-3 py-1.5 text-sm font-semibold',
  };

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-md border ${sizeClasses[size]} ${badgeStyle} ${className}`}>
      <span className={`w-1.5 h-1.5 rounded-full ${dotColor}`} />
      {status}
    </span>
  );
};
