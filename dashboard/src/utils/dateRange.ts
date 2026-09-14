import type { Assessment, DateRange, DateRangePreset } from '../types';

/**
 * Mock-data date filtering utilities.
 *
 * The dashboard is in DEMO (mock data) phase. These helpers centralise the
 * mock "today" anchor (2026-09-14) so the global date range selector
 * genuinely changes the visible mock state. Swap this module for real
 * backend date queries during API integration without touching screens.
 */
export const MOCK_TODAY_ISO = '2026-09-14';

export function getMockToday(): Date {
  return new Date(`${MOCK_TODAY_ISO}T00:00:00`);
}

function toIso(date: Date): string {
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${m}-${d}`;
}

export function addDaysIso(iso: string, days: number): string {
  const d = new Date(`${iso}T12:00:00`);
  d.setDate(d.getDate() + days);
  return toIso(d);
}

export function getPresetDefaultRange(preset: DateRangePreset): DateRange {
 
  switch (preset) {
    case 'Today':
      return { from: MOCK_TODAY_ISO, to: MOCK_TODAY_ISO };
    case 'Last 7 Days': {
      return { from: addDaysIso(MOCK_TODAY_ISO, -6), to: MOCK_TODAY_ISO };
    }
    case 'Last 30 Days': {
      return { from: addDaysIso(MOCK_TODAY_ISO, -29), to: MOCK_TODAY_ISO };
    }
    case 'Custom Range':
    default:
      return { from: addDaysIso(MOCK_TODAY_ISO, -29), to: MOCK_TODAY_ISO };
  }
}

export function resolveRange(preset: DateRangePreset, custom?: DateRange): DateRange {
  if (preset === 'Custom Range' && custom?.from && custom?.to) {
    return { from: custom.from, to: custom.to };
  }
  return getPresetDefaultRange(preset);
}

export function assessmentInRange(assessment: Assessment, range: DateRange): boolean {
  const date = assessment.dateTime.slice(0, 10);
  return date >= range.from && date <= range.to;
}

export function filterAssessmentsByRange(
  assessments: Assessment[],
  preset: DateRangePreset,
  custom?: DateRange,
): Assessment[] {
  const range = resolveRange(preset, custom);
  return assessments.filter((a) => assessmentInRange(a, range));
}