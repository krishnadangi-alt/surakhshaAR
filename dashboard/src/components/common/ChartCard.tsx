import React from 'react';

interface ChartCardProps {
  title: string;
  subtitle?: string;
  action?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
}

export const ChartCard: React.FC<ChartCardProps> = ({
  title,
  subtitle,
  action,
  children,
  className = '',
}) => {
  return (
    <div className={`rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card ${className}`}>
      <div className="flex items-center justify-between pb-4 border-b border-suraksha-border/60 mb-4">
        <div>
          <h4 className="text-sm font-bold uppercase tracking-wider text-white">{title}</h4>
          {subtitle && <p className="text-xs text-suraksha-subtext mt-0.5">{subtitle}</p>}
        </div>
        {action && <div>{action}</div>}
      </div>
      <div className="w-full">{children}</div>
    </div>
  );
};
