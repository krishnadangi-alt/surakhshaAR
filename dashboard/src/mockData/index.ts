import { mockWorkers } from './workers';
import { mockAssessments } from './assessments';
import { mockModules } from './modules';
import { mockCompetencyWeaknesses, mockCompetencyRadarData, mockCompetencyDistribution } from './competency';
import { mockRetrainingRecords } from './retraining';
import { mockCertificates } from './certificates';
import { mockRetentionRecords, mockRetentionCurveData } from './retention';
import { mockNotifications } from './notifications';
import type { CertificateStatus } from '../types';

export {
  mockWorkers,
  mockAssessments,
  mockModules,
  mockCompetencyWeaknesses,
  mockCompetencyRadarData,
  mockCompetencyDistribution,
  mockRetrainingRecords,
  mockCertificates,
  mockRetentionRecords,
  mockRetentionCurveData,
  mockNotifications
};

export const getWorkerById = (id: string) => {
  return mockWorkers.find(w => w.id === id || w.employeeId === id) || mockWorkers[0];
};

export const getAssessmentsByWorkerId = (workerId: string) => {
  return mockAssessments.filter(a => a.workerId === workerId || a.employeeId === workerId);
};

export const getCertificatesByWorkerId = (workerId: string) => {
  return mockCertificates.filter(c => c.workerId === workerId || c.employeeId === workerId);
};

export const getRetrainingByWorkerId = (workerId: string) => {
  return mockRetrainingRecords.filter(r => r.workerId === workerId || r.employeeId === workerId);
};

export const getRetentionByWorkerId = (workerId: string) => {
  return mockRetentionRecords.find(r => r.workerId === workerId || r.employeeId === workerId);
};

/**
 * Derives an aggregate certificate status for a worker based on their
 * simulated certificate records. Useful for directory-level filtering.
 */
export const getWorkerCertificateStatus = (workerId: string): CertificateStatus => {
  const certs = getCertificatesByWorkerId(workerId);
  if (certs.length === 0) return 'Pending';
  if (certs.some(c => c.status === 'Expired')) return 'Expired';
  if (certs.some(c => c.status === 'Expiring Soon')) return 'Expiring Soon';
  if (certs.some(c => c.status === 'Active')) return 'Active';
  return 'Pending';
};
