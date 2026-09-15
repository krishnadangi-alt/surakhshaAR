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
    badgeStyle = 'bg-emerald-50 text-emerald-700 border-emerald-200';
    dotColor = 'bg-emerald-500';
  } else if (['in training', 'in progress', 'assigned', 'developing', 'scheduled', 'pending', 'moderate', 'expiring soon', 'recommended'].includes(normalized)) {
    badgeStyle = 'bg-amber-50 text-amber-800 border-amber-200';
    dotColor = 'bg-amber-500';
  } else if (['failed', 'retraining required', 'needs retraining', 'expired', 'critical', 'fail', 'reassessment pending', 'overdue'].includes(normalized)) {
    badgeStyle = 'bg-rose-50 text-rose-700 border-rose-200';
    dotColor = 'bg-rose-500';
  } else if (['blue'].includes(normalized)) {
    badgeStyle = 'bg-blue-50 text-blue-700 border-blue-200';
    dotColor = 'bg-blue-500';
  }

  const sizeClasses = {
    sm: 'px-2.5 py-0.5 text-[12px] font-semibold',
    md: 'px-3 py-1 text-[13px] font-semibold',
    lg: 'px-3.5 py-1.5 text-[14px] font-bold',
  };

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-md border ${sizeClasses[size]} ${badgeStyle} ${className}`}>
      <span className={`w-1.5 h-1.5 rounded-full ${dotColor}`} />
      {status}
    </span>
  );
};
