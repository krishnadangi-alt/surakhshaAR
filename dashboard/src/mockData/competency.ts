import type { CompetencyWeakness } from '../types';

export interface CompetencyRadarPoint {
  dimension: string;
  score: number;
  benchmark: number;
}

export const mockCompetencyWeaknesses: CompetencyWeakness[] = [
  {
    id: 'cw-1',
    name: 'SCBA Cylinder Pressure Zeroing & Seal Check',
    occurrenceCount: 248,
    averageScore: 61.5,
    severity: 'Critical',
    affectedModules: ['Gas Safety'],
    recommendedAction: 'Mandatory 15-min physical SCBA don/doff drill prior to AR simulation',
  },
  {
    id: 'cw-2',
    name: 'LOTO Lock Verification Before Pinch Point Entry',
    occurrenceCount: 186,
    averageScore: 68.2,
    severity: 'Critical',
    affectedModules: ['Machinery Safety', 'Gas Safety'],
    recommendedAction: 'Re-assign Lockout/Tagout 3D spatial interactive scenario',
  },
  {
    id: 'cw-3',
    name: 'Standing Downwind During Chemical/Foam Discharge',
    occurrenceCount: 142,
    averageScore: 71.0,
    severity: 'Warning',
    affectedModules: ['Fire Safety'],
    recommendedAction: 'Wind direction indicator awareness module review',
  },
  {
    id: 'cw-4',
    name: 'Methane Sensor Span Calibration Potentiometer Adjustment',
    occurrenceCount: 119,
    averageScore: 73.8,
    severity: 'Warning',
    affectedModules: ['Gas Safety'],
    recommendedAction: 'Calibration bench micro-drill simulation',
  },
  {
    id: 'cw-5',
    name: 'Secondary Emergency Stop Button Confirmation',
    occurrenceCount: 94,
    averageScore: 76.4,
    severity: 'Moderate',
    affectedModules: ['Machinery Safety'],
    recommendedAction: 'E-stop verification checklist refresher',
  }
];

export const mockCompetencyRadarData: CompetencyRadarPoint[] = [
  { dimension: 'Hazard Identification', score: 88, benchmark: 82 },
  { dimension: 'SCBA & Gas Handling', score: 76, benchmark: 80 },
  { dimension: 'Fire Suppression', score: 92, benchmark: 85 },
  { dimension: 'LOTO Equipment Protocol', score: 81, benchmark: 84 },
  { dimension: 'Emergency Evacuation', score: 90, benchmark: 86 },
  { dimension: 'Incident Escalation', score: 87, benchmark: 83 },
];

export const mockCompetencyDistribution = {
  strongCount: 9710,     // 68%
  developingCount: 3427, // 24%
  retrainingCount: 1143, // 8%
  totalWorkers: 14280,
  averageCompetencyScore: 84.6,
};
