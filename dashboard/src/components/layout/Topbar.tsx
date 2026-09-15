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
    <header className="sticky top-0 z-20 flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 bg-white/95 backdrop-blur px-6 py-4 shadow-sm">
      <div>
        <div className="flex items-center gap-2">
          <h2 className="text-lg font-bold tracking-tight text-slate-900">{title}</h2>
          <span className="inline-flex items-center gap-1 rounded-full bg-blue-50 px-2.5 py-0.5 text-[10px] font-bold text-blue-700 border border-blue-200">
            <ShieldCheck className="w-3 h-3 text-blue-600" /> Govt. of Jharkhand Portal
          </span>
        </div>
        {description && <p className="text-xs text-slate-500 mt-0.5">{description}</p>}
      </div>

      <div className="flex items-center gap-3">
        {/* Quick Command Search Trigger */}
        <button
          onClick={onOpenSearch}
          className="flex items-center gap-2 rounded-lg border border-slate-200 bg-slate-50 px-3 py-1.5 text-xs text-slate-600 hover:border-slate-300 hover:bg-slate-100 transition"
        >
          <Search className="w-3.5 h-3.5 text-amber-600" />
          <span className="hidden sm:inline font-medium">Search portal...</span>
          <kbd className="hidden sm:inline px-1.5 py-0.5 text-[10px] font-bold text-slate-500 bg-white border border-slate-200 rounded">
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
          className="relative p-2 rounded-lg border border-slate-200 bg-slate-50 text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition"
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
        <div className="flex items-center gap-2 pl-2 border-l border-slate-200">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-amber-50 border border-amber-200 font-bold text-xs text-amber-700">
            JK
          </div>
        </div>
      </div>
    </header>
  );
};
