import React from 'react';

interface LoadingStateProps {
  rows?: number;
  className?: string;
}

export const LoadingState: React.FC<LoadingStateProps> = ({ rows = 4, className = '' }) => {
  return (
    <div className={`space-y-3 rounded-xl border border-suraksha-border bg-suraksha-card p-6 animate-pulse ${className}`}>
      <div className="h-5 bg-suraksha-surface rounded w-1/4 mb-4" />
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="flex gap-4">
          <div className="h-4 bg-suraksha-surface rounded w-1/3" />
          <div className="h-4 bg-suraksha-surface rounded w-1/4" />
          <div className="h-4 bg-suraksha-surface rounded w-1/6" />
          <div className="h-4 bg-suraksha-surface rounded w-1/4" />
        </div>
      ))}
    </div>
  );
};
