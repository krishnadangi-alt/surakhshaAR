import React, { useState } from 'react';
import { Calendar, ChevronDown } from 'lucide-react';
import type { DateRange, DateRangePreset } from '../../types';
import { getPresetDefaultRange } from '../../utils/dateRange';

interface DateRangePickerProps {
  selected: DateRangePreset;
  onChange: (preset: DateRangePreset) => void;
  customRange?: DateRange;
  onCustomRangeChange?: (range: DateRange) => void;
  className?: string;
}

export const DateRangePicker: React.FC<DateRangePickerProps> = ({
  selected,
  onChange,
  customRange,
  onCustomRangeChange,
  className = '',
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const options: DateRangePreset[] = ['Today', 'Last 7 Days', 'Last 30 Days', 'Custom Range'];
  const activeRange = customRange && customRange.from && customRange.to
    ? customRange
    : getPresetDefaultRange(selected);

  return (
    <div className={`relative ${className}`}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-1.5 text-xs font-semibold text-suraksha-text transition hover:border-suraksha-borderLight hover:bg-suraksha-hover focus:outline-none"
      >
        <Calendar className="w-3.5 h-3.5 text-suraksha-amber" />
        <span>{selected}</span>
        <ChevronDown className="w-3.5 h-3.5 text-suraksha-subtext" />
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} />
          <div className="absolute right-0 z-50 mt-2 w-56 rounded-lg border border-suraksha-border bg-suraksha-card p-2 shadow-elevated">
            {options.map((option) => (
              <button
                key={option}
                onClick={() => {
                  onChange(option);
                  if (option !== 'Custom Range') setIsOpen(false);
                }}
                className={`w-full text-left px-3 py-2 text-xs font-medium rounded-md transition ${
                  selected === option
                    ? 'bg-suraksha-blue/20 text-suraksha-blue font-bold'
                    : 'text-suraksha-subtext hover:bg-suraksha-hover hover:text-white'
                }`}
              >
                {option}
              </button>
            ))}

            {selected === 'Custom Range' && onCustomRangeChange && (
              <div className="mt-2 pt-2 border-t border-suraksha-border space-y-2">
                <label className="block text-[10px] uppercase font-bold tracking-wider text-suraksha-subtext">
                  From Date
                </label>
                <input
                  type="date"
                  value={activeRange.from}
                  max={activeRange.to}
                  onChange={(e) => {
                    if (!e.target.value) return;
                    onCustomRangeChange({ from: e.target.value, to: activeRange.to });
                  }}
                  className="w-full rounded-md border border-suraksha-border bg-suraksha-surface px-2.5 py-1.5 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none [color-scheme:dark]"
                />
                <label className="block text-[10px] uppercase font-bold tracking-wider text-suraksha-subtext">
                  To Date
                </label>
                <input
                  type="date"
                  value={activeRange.to}
                  min={activeRange.from}
                  onChange={(e) => {
                    if (!e.target.value) return;
                    onCustomRangeChange({ from: activeRange.from, to: e.target.value });
                  }}
                  className="w-full rounded-md border border-suraksha-border bg-suraksha-surface px-2.5 py-1.5 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none [color-scheme:dark]"
                />
                <button
                  onClick={() => setIsOpen(false)}
                  className="w-full mt-1 rounded-md bg-suraksha-blue px-3 py-1.5 text-xs font-bold text-white hover:bg-suraksha-blueHover transition"
                >
                  Apply Range
                </button>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
};
