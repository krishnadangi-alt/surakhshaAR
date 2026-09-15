import React from 'react';
import type { LucideIcon } from 'lucide-react';
import { TrendingUp, TrendingDown } from 'lucide-react';

interface KpiCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  change?: string;
  changeType?: 'positive' | 'negative' | 'neutral';
  icon: LucideIcon;
  variant?: 'default' | 'accent' | 'wide';
  accentColor?: 'blue' | 'amber' | 'green' | 'red';
  className?: string;
}

export const KpiCard: React.FC<KpiCardProps> = ({
  title,
  value,
  subtitle,
  change,
  changeType = 'positive',
  icon: Icon,
  variant = 'default',
  accentColor = 'blue',
  className = '',
}) => {
  const colorMap = {
    blue: {
      bg: 'bg-suraksha-card',
      iconBg: 'bg-blue-500/10 text-suraksha-blue border-blue-500/20',
      accentBar: 'border-l-4 border-l-suraksha-blue',
    },
    amber: {
      bg: 'bg-suraksha-card',
      iconBg: 'bg-amber-500/10 text-suraksha-amber border-amber-500/20',
      accentBar: 'border-l-4 border-l-suraksha-amber',
    },
    green: {
      bg: 'bg-suraksha-card',
      iconBg: 'bg-emerald-500/10 text-suraksha-green border-emerald-500/20',
      accentBar: 'border-l-4 border-l-suraksha-green',
    },
    red: {
      bg: 'bg-suraksha-card',
      iconBg: 'bg-rose-500/10 text-suraksha-red border-rose-500/20',
      accentBar: 'border-l-4 border-l-suraksha-red',
    },
  };

  const selectedColor = colorMap[accentColor];

  return (
    <div
      className={`relative rounded-xl border border-suraksha-border p-5 transition-all duration-200 hover:border-suraksha-borderLight ${selectedColor.bg} ${variant === 'accent' ? selectedColor.accentBar : ''} ${className}`}
    >
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs font-bold uppercase tracking-wider text-suraksha-subtext">{title}</p>
          <h3 className="mt-2 text-2xl font-bold tracking-tight text-suraksha-heading">{value}</h3>
        </div>
        <div className={`p-2.5 rounded-lg border ${selectedColor.iconBg}`}>
          <Icon className="w-5 h-5" />
        </div>
      </div>

      {(subtitle || change) && (
        <div className="mt-4 flex items-center justify-between text-xs text-suraksha-subtext pt-3 border-t border-suraksha-border/60">
          {subtitle && <span className="font-medium text-suraksha-subtext">{subtitle}</span>}
          {change && (
            <span
              className={`flex items-center gap-1 font-bold ${
                changeType === 'positive'
                  ? 'text-emerald-700'
                  : changeType === 'negative'
                  ? 'text-rose-700'
                  : 'text-suraksha-subtext'
              }`}
            >
              {changeType === 'positive' ? (
                <TrendingUp className="w-3.5 h-3.5" />
              ) : changeType === 'negative' ? (
                <TrendingDown className="w-3.5 h-3.5" />
              ) : null}
              {change}
            </span>
          )}
        </div>
      )}
    </div>
  );
};
