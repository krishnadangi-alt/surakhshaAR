/**
 * Mock / sample data used when the FastAPI backend is unreachable.
 * Clearly separated so it is easy to replace with real API calls.
 */

import type {
  Assessment,
  Certificate,
  DashboardSummary,
  Worker,
  WorkerDetail,
  WorkerListResponse,
} from '../types';

export const mockSummary: DashboardSummary = {
  total_workers: 24,
  workers_in_training: 14,
  certified_workers: 8,
  total_assessments: 31,
  pass_rate: 74.2,
  module_stats: [
    { module_id: 1, module_name: 'Fire & Explosion Response', workers_enrolled: 16, certified: 5 },
    { module_id: 2, module_name: 'Gas Leak & Confined Space Protocol', workers_enrolled: 12, certified: 3 },
  ],
};

export const mockWorkers: WorkerListResponse = {
  workers: [
    {
      id: 1, name: 'Ramesh Kumar', employee_id: 'EMP001', role: 'Fire Safety Worker',
      progress: [{ module_id: 1, module_code: 'fire', module_name: 'Fire & Explosion Response', stage: 'certify', status: 'completed', last_updated: '2026-09-07T08:30:00Z' }],
      certified_modules: ['fire'],
    },
    {
      id: 2, name: 'Sunita Devi', employee_id: 'EMP002', role: 'Gas Plant Operator',
      progress: [{ module_id: 2, module_code: 'gas', module_name: 'Gas Leak & Confined Space Protocol', stage: 'retrain', status: 'in_progress', last_updated: '2026-09-07T09:15:00Z' }],
      certified_modules: [],
    },
    {
      id: 3, name: 'Amit Patel', employee_id: 'EMP003', role: 'Maintenance Technician',
      progress: [
        { module_id: 1, module_code: 'fire', module_name: 'Fire & Explosion Response', stage: 'certify', status: 'completed', last_updated: '2026-09-06T14:20:00Z' },
        { module_id: 2, module_code: 'gas', module_name: 'Gas Leak & Confined Space Protocol', stage: 'assess', status: 'in_progress', last_updated: '2026-09-06T16:45:00Z' },
      ],
      certified_modules: ['fire'],
    },
    {
      id: 4, name: 'Priya Sharma', employee_id: 'EMP004', role: 'Safety Supervisor',
      progress: [
        { module_id: 1, module_code: 'fire', module_name: 'Fire & Explosion Response', stage: 'certify', status: 'completed', last_updated: '2026-09-05T11:00:00Z' },
        { module_id: 2, module_code: 'gas', module_name: 'Gas Leak & Confined Space Protocol', stage: 'certify', status: 'completed', last_updated: '2026-09-05T13:30:00Z' },
      ],
      certified_modules: ['fire', 'gas'],
    },
    {
      id: 5, name: 'Vikram Singh', employee_id: 'EMP005', role: 'Fire Safety Worker',
      progress: [{ module_id: 1, module_code: 'fire', module_name: 'Fire & Explosion Response', stage: 'diagnose', status: 'in_progress', last_updated: '2026-09-07T07:50:00Z' }],
      certified_modules: [],
    },
    {
      id: 6, name: 'Lakshmi Nair', employee_id: 'EMP006', role: 'Gas Plant Operator',
      progress: [{ module_id: 2, module_code: 'gas', module_name: 'Gas Leak & Confined Space Protocol', stage: 'certify', status: 'completed', last_updated: '2026-09-04T10:20:00Z' }],
      certified_modules: ['gas'],
    },
    {
      id: 7, name: 'Rajesh Gupta', employee_id: 'EMP007', role: 'Maintenance Technician',
      progress: [
        { module_id: 1, module_code: 'fire', module_name: 'Fire & Explosion Response', stage: 'retrain', status: 'in_progress', last_updated: '2026-09-07T06:40:00Z' },
        { module_id: 2, module_code: 'gas', module_name: 'Gas Leak & Confined Space Protocol', stage: 'practice', status: 'completed', last_updated: '2026-09-03T09:10:00Z' },
      ],
      certified_modules: [],
    },
    {
      id: 8, name: 'Anita Kumari', employee_id: 'EMP008', role: 'Fire Safety Worker',
      progress: [{ module_id: 1, module_code: 'fire', module_name: 'Fire & Explosion Response', stage: 'certify', status: 'completed', last_updated: '2026-09-06T15:55:00Z' }],
      certified_modules: ['fire'],
    },
  ],
};

const assessments: Record<number, Assessment[]> = {
  1: [
    { id: 1, worker_id: 1, module_id: 1, attempt_number: 1, score: 86, passed: true, weaknesses: [], created_at: '2026-09-07T08:30:00Z' },
  ],
  2: [
    { id: 2, worker_id: 2, module_id: 2, attempt_number: 1, score: 62, passed: false, weaknesses: ['PPE Selection', 'Hazard Recognition'], created_at: '2026-09-06T10:00:00Z' },
    { id: 3, worker_id: 2, module_id: 2, attempt_number: 2, score: 58, passed: false, weaknesses: ['PPE Selection', 'Emergency Response'], created_at: '2026-09-07T09:15:00Z' },
  ],
  3: [
    { id: 4, worker_id: 3, module_id: 1, attempt_number: 1, score: 91, passed: true, weaknesses: [], created_at: '2026-09-06T14:20:00Z' },
    { id: 5, worker_id: 3, module_id: 2, attempt_number: 1, score: 74, passed: true, weaknesses: ['Hazard Recognition'], created_at: '2026-09-06T16:45:00Z' },
  ],
  4: [
    { id: 6, worker_id: 4, module_id: 1, attempt_number: 1, score: 95, passed: true, weaknesses: [], created_at: '2026-09-05T11:00:00Z' },
    { id: 7, worker_id: 4, module_id: 2, attempt_number: 1, score: 88, passed: true, weaknesses: [], created_at: '2026-09-05T13:30:00Z' },
  ],
  5: [
    { id: 8, worker_id: 5, module_id: 1, attempt_number: 1, score: 45, passed: false, weaknesses: ['Emergency Response', 'PPE Selection', 'Hazard Recognition'], created_at: '2026-09-07T07:50:00Z' },
  ],
  6: [
    { id: 9, worker_id: 6, module_id: 2, attempt_number: 1, score: 82, passed: true, weaknesses: [], created_at: '2026-09-04T10:20:00Z' },
  ],
  7: [
    { id: 10, worker_id: 7, module_id: 1, attempt_number: 1, score: 70, passed: true, weaknesses: ['Emergency Response'], created_at: '2026-09-06T12:00:00Z' },
    { id: 11, worker_id: 7, module_id: 2, attempt_number: 1, score: 55, passed: false, weaknesses: ['Hazard Recognition', 'PPE Selection'], created_at: '2026-09-03T09:10:00Z' },
  ],
  8: [
    { id: 12, worker_id: 8, module_id: 1, attempt_number: 1, score: 78, passed: true, weaknesses: [], created_at: '2026-09-06T15:55:00Z' },
  ],
};


const certificates: Record<number, Certificate[]> = {
  1: [
    { id: 1, certificate_number: 'SUR-2026-0001', worker_id: 1, module_id: 1, issued_at: '2026-09-07T09:00:00Z', valid_until: '2027-09-07T09:00:00Z', status: 'active' },
  ],
  3: [
    { id: 2, certificate_number: 'SUR-2026-0002', worker_id: 3, module_id: 1, issued_at: '2026-09-06T15:00:00Z', valid_until: '2027-09-06T15:00:00Z', status: 'active' },
  ],
  4: [
    { id: 3, certificate_number: 'SUR-2026-0003', worker_id: 4, module_id: 1, issued_at: '2026-09-05T12:00:00Z', valid_until: '2027-09-05T12:00:00Z', status: 'active' },
    { id: 4, certificate_number: 'SUR-2026-0004', worker_id: 4, module_id: 2, issued_at: '2026-09-05T14:00:00Z', valid_until: '2027-09-05T14:00:00Z', status: 'active' },
  ],
  6: [
    { id: 5, certificate_number: 'SUR-2026-0005', worker_id: 6, module_id: 2, issued_at: '2026-09-04T11:00:00Z', valid_until: '2027-09-04T11:00:00Z', status: 'active' },
  ],
  8: [
    { id: 6, certificate_number: 'SUR-2026-0006', worker_id: 8, module_id: 1, issued_at: '2026-09-06T16:30:00Z', valid_until: '2027-09-06T16:30:00Z', status: 'active' },
  ],
};

export function getMockWorkerDetail(id: number): WorkerDetail | null {
  const worker = mockWorkers.workers.find((w) => w.id === id);
  if (!worker) return null;
  return {
    ...worker,
    assessments: assessments[id] ?? [],
    certificates: certificates[id] ?? [],
  };
}

