import type { CompetencyWeakness } from '../types';

export interface CompetencyRadarPoint {
  dimension: string;
  score: number;
  benchmark: number;
}

export const mockCompetencyWeaknesses: CompetencyWeakness[] = [
  {
    id: 'cw-1',
    name: 'PPE Selection in Hazard Zone',
    occurrenceCount: 18,
    averageScore: 62.5,
    severity: 'Critical',
    affectedModules: ['Gas Leak & Confined Space'],
    recommendedAction: 'Targeted PPE Selection AR Practice & Reassessment',
  },
  {
    id: 'cw-2',
    name: 'Extinguisher Selection & Use',
    occurrenceCount: 14,
    averageScore: 68.0,
    severity: 'Critical',
    affectedModules: ['Fire & Explosion Response'],
    recommendedAction: 'Extinguisher selection & use AR drill',
  },
  {
    id: 'cw-3',
    name: 'Hazard Zone Recognition',
    occurrenceCount: 11,
    averageScore: 71.5,
    severity: 'Warning',
    affectedModules: ['Gas Leak & Confined Space'],
    recommendedAction: 'Hazard zone recognition practice',
  },
  {
    id: 'cw-4',
    name: 'Evacuation Sequence',
    occurrenceCount: 9,
    averageScore: 74.2,
    severity: 'Warning',
    affectedModules: ['Fire & Explosion Response'],
    recommendedAction: 'Evacuation route AR practice',
  },
  {
    id: 'cw-5',
    name: 'Machinery',
    occurrenceCount: 7,
    averageScore: 76.8,
    severity: 'Moderate',
    affectedModules: ['Machinery'],
    recommendedAction: 'Machinery safety practice drill',
  }
];

export const mockCompetencyRadarData: CompetencyRadarPoint[] = [
  { dimension: 'Hazard Recognition', score: 88, benchmark: 82 },
  { dimension: 'PPE Selection', score: 78, benchmark: 80 },
  { dimension: 'Extinguisher Operation', score: 91, benchmark: 85 },
  { dimension: 'Evacuation Sequence', score: 89, benchmark: 86 },
  { dimension: 'Machinery Interaction', score: 86, benchmark: 84 },
];

export const mockCompetencyDistribution = {
  competentCount: 92,     // ~76% of 120 demo workers
  retrainingCount: 28,    // ~24% of 120 demo workers
  totalWorkers: 120,
  averageCompetencyScore: 84.6,
};
