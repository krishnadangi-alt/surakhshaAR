import React from 'react';
import { Search, RotateCcw } from 'lucide-react';

interface FilterOption {
  key: string;
  label: string;
  options: { label: string; value: string }[];
  value: string;
  onChange: (value: string) => void;
}

interface FilterBarProps {
  searchQuery?: string;
  onSearchChange?: (query: string) => void;
  searchPlaceholder?: string;
  filters?: FilterOption[];
  onReset?: () => void;
  className?: string;
}

export const FilterBar: React.FC<FilterBarProps> = ({
  searchQuery,
  onSearchChange,
  searchPlaceholder = 'Search by Name or ID...',
  filters = [],
  onReset,
  className = '',
}) => {
  return (
    <div className={`flex flex-wrap items-center justify-between gap-3 rounded-xl border border-suraksha-border bg-suraksha-card p-3.5 ${className}`}>
      <div className="flex flex-1 items-center gap-3 min-w-[240px]">
        {onSearchChange !== undefined && (
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-suraksha-subtext" />
            <input
              type="text"
              value={searchQuery || ''}
              onChange={(e) => onSearchChange(e.target.value)}
              placeholder={searchPlaceholder}
              className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface pl-9 pr-4 py-1.5 text-xs font-semibold text-suraksha-text placeholder-suraksha-subtext focus:border-suraksha-blue focus:outline-none focus:ring-1 focus:ring-suraksha-blue"
            />
          </div>
        )}

        <div className="flex flex-wrap items-center gap-2">
          {filters.map((filter) => (
            <select
              key={filter.key}
              value={filter.value}
              onChange={(e) => filter.onChange(e.target.value)}
              className="rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-1.5 text-xs font-semibold text-suraksha-text focus:border-suraksha-blue focus:outline-none"
            >
              <option value="ALL">{filter.label}: All</option>
              {filter.options.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          ))}
        </div>
      </div>

      {onReset && (
        <button
          onClick={onReset}
          className="flex items-center gap-1.5 rounded-lg border border-suraksha-border px-3 py-1.5 text-xs font-semibold text-suraksha-subtext hover:bg-suraksha-hover hover:text-suraksha-text transition"
        >
          <RotateCcw className="w-3.5 h-3.5" />
          <span>Reset Filters</span>
        </button>
      )}
    </div>
  );
};
