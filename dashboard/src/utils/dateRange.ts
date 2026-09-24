import type { Assessment, DateRange, DateRangePreset } from '../types';

/**
 * Real dynamic date filtering utilities for live dashboard records.
 */

function toIso(date: Date): string {
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${m}-${d}`;
}

export function getTodayIso(): string {
  return toIso(new Date());
}

// Kept for backward compatibility with existing imports
export const MOCK_TODAY_ISO = getTodayIso();

export function getMockToday(): Date {
  return new Date();
}

export function addDaysIso(iso: string, days: number): string {
  const d = new Date(`${iso}T12:00:00`);
  d.setDate(d.getDate() + days);
  return toIso(d);
}

export function getPresetDefaultRange(preset: DateRangePreset): DateRange {
  const today = getTodayIso();
  // Include tomorrow to account for UTC vs local timezone offsets
  const tomorrow = addDaysIso(today, 1);

  switch (preset) {
    case 'Today':
      return { from: today, to: tomorrow };
    case 'Last 7 Days':
      return { from: addDaysIso(today, -6), to: tomorrow };
    case 'Last 30 Days':
      return { from: addDaysIso(today, -29), to: tomorrow };
    case 'Custom Range':
    default:
      return { from: addDaysIso(today, -90), to: tomorrow };
  }
}

export function resolveRange(preset: DateRangePreset, custom?: DateRange): DateRange {
  if (preset === 'Custom Range' && custom?.from && custom?.to) {
    return { from: custom.from, to: custom.to };
  }
  return getPresetDefaultRange(preset);
}

export function parseIsoDate(dateTimeStr: string): string {
  if (!dateTimeStr) return '';
  const match = dateTimeStr.match(/^(\d{4}-\d{2}-\d{2})/);
  if (match) return match[1];
  const d = new Date(dateTimeStr);
  if (!isNaN(d.getTime())) return toIso(d);
  return dateTimeStr.slice(0, 10);
}

export function assessmentInRange(assessment: Assessment, range: DateRange): boolean {
  if (!range || (!range.from && !range.to)) return true;
  const date = parseIsoDate(assessment.dateTime);
  if (!date) return true;
  if (range.from && date < range.from) return false;
  if (range.to && date > range.to) return false;
  return true;
}

export function filterAssessmentsByRange(
  assessments: Assessment[],
  preset: DateRangePreset,
  custom?: DateRange,
): Assessment[] {
  const range = resolveRange(preset, custom);
  return assessments.filter((a) => assessmentInRange(a, range));
}