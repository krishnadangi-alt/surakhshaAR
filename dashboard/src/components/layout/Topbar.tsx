import React from 'react';
import { DateRangePicker } from '../common/DateRangePicker';
import type { DateRange, DateRangePreset } from '../../types';
import { Bell, Search, ShieldCheck } from 'lucide-react';
import { mockNotifications } from '../../mockData';

interface TopbarProps {
  title: string;
  description?: string;
  selectedDateRange: DateRangePreset;
  onDateRangeChange: (preset: DateRangePreset) => void;
  customRange?: DateRange;
  onCustomRangeChange?: (range: DateRange) => void;
  onOpenNotifications: () => void;
  onOpenSearch: () => void;
}

export const Topbar: React.FC<TopbarProps> = ({
  title,
  description,
  selectedDateRange,
  onDateRangeChange,
  customRange,
  onCustomRangeChange,
  onOpenNotifications,
  onOpenSearch,
}) => {
  const unreadCount = mockNotifications.filter((n) => !n.read).length;

  return (
    <header className="sticky top-0 z-20 flex flex-wrap items-center justify-between gap-4 border-b border-suraksha-border bg-suraksha-dark/95 backdrop-blur px-6 py-4 shadow-subtle">
      <div>
        <div className="flex items-center gap-2">
          <h2 className="text-lg font-bold tracking-tight text-white">{title}</h2>
          <span className="inline-flex items-center gap-1 rounded-full bg-blue-500/10 px-2 py-0.5 text-[10px] font-semibold text-suraksha-blue border border-blue-500/20">
            <ShieldCheck className="w-3 h-3" /> Govt. of Jharkhand Portal
          </span>
        </div>
        {description && <p className="text-xs text-suraksha-subtext mt-0.5">{description}</p>}
      </div>

      <div className="flex items-center gap-3">
        {/* Quick Command Search Trigger */}
        <button
          onClick={onOpenSearch}
          className="flex items-center gap-2 rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-1.5 text-xs text-suraksha-subtext hover:border-suraksha-borderLight hover:bg-suraksha-hover transition"
        >
          <Search className="w-3.5 h-3.5 text-suraksha-amber" />
          <span className="hidden sm:inline">Search portal...</span>
          <kbd className="hidden sm:inline px-1.5 py-0.5 text-[10px] font-bold text-suraksha-subtext bg-suraksha-card border border-suraksha-border rounded">
            ⌘K
          </kbd>
        </button>

        {/* Date Range Selector */}
        <DateRangePicker
          selected={selectedDateRange}
          onChange={onDateRangeChange}
          customRange={customRange}
          onCustomRangeChange={onCustomRangeChange}
        />

        {/* Notifications Alert Trigger */}
        <button
          onClick={onOpenNotifications}
          className="relative p-2 rounded-lg border border-suraksha-border bg-suraksha-surface text-suraksha-subtext hover:bg-suraksha-hover hover:text-white transition"
          title="View Notifications"
        >
          <Bell className="w-4 h-4" />
          {unreadCount > 0 && (
            <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-rose-500 text-[9px] font-bold text-white shadow-sm">
              {unreadCount}
            </span>
          )}
        </button>

        {/* Admin Avatar Badge */}
        <div className="flex items-center gap-2 pl-2 border-l border-suraksha-border">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-suraksha-surface border border-suraksha-border font-bold text-xs text-suraksha-amber">
            JK
          </div>
        </div>
      </div>
    </header>
  );
};
