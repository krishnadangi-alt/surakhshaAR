export type WorkerStatus = 'Passed' | 'Failed' | 'In Training' | 'Certified' | 'Needs Retraining';
export type CompetencyGrade = 'Competent' | 'Needs Retraining';
export type RetrainingStatus = 'Recommended' | 'Assigned' | 'In Progress' | 'Completed' | 'Reassessment Pending';
export type CertificateStatus = 'Active' | 'Expiring Soon' | 'Expired' | 'Pending';
export type RetentionStatus = 'Scheduled' | 'Completed' | 'Pending' | 'Overdue';
export type SectorName = 
  | 'Dhanbad Region-1'
  | 'Dhanbad Region-2'
  | 'Dhanbad Region-3'
  | 'Koderma Region'
  | 'Ranchi Region'
  | 'Chaibasa Region'
  | 'Bhubaneswar Region-1'
  | 'Bhubaneswar Region-2';

export interface ModuleProgress {
  moduleId: string;
  moduleName: string;
  stage: string;
  status: 'Completed' | 'In Progress' | 'Not Started' | 'Retraining';
  completionPercentage: number;
  score: number;
  lastUpdated: string;
}

export interface AssessmentStep {
  stepIndex: number;
  stepName: string;
  expectedAction: string;
  performedAction: string;
  result: 'pass' | 'fail' | 'critical_error';
  scoreDelta: number;
  timestamp: string;
}

export interface Assessment {
  id: string;
  workerId: string;
  workerName: string;
  employeeId: string;
  moduleId: string;
  moduleName: string;
  scenarioName: string;
  score: number;
  correctActions: number;
  wrongActions: number;
  criticalErrors: number;
  criticalErrorDetails?: string;
  passFail: 'Pass' | 'Fail';
  duration: string;
  dateTime: string;
  stepDetails: AssessmentStep[];
  competencyScores?: Record<string, { name: string; score: number; passed: boolean; pass_threshold: number }>;
  weaknesses?: Array<{ competency_name: string; score?: number; threshold?: number; severity?: string; reason?: string }>;
}

export interface Worker {
  id: string;
  employeeId: string;
  name: string;
  sector: SectorName;
  plant: string;
  role: string;
  email: string;
  phone: string;
  joinedDate: string;
  safetyOfficer: string;
  overallStatus: WorkerStatus;
  modulesCompleted: number;
  latestScore: number;
  overallCompetency: CompetencyGrade;
  lastAssessmentDate: string;
  certificatesCount: number;
  retrainingStatus: RetrainingStatus;
  moduleProgressList: ModuleProgress[];
  weakAreas: string[];
  retentionDay1: RetentionStatus;
  retentionDay7: RetentionStatus;
  retentionDay30: RetentionStatus;
}

export interface CompetencyWeakness {
  id: string;
  name: string;
  occurrenceCount: number;
  averageScore: number;
  severity: 'Critical' | 'Warning' | 'Moderate';
  affectedModules: string[];
  recommendedAction: string;
}

export interface RetrainingRecord {
  id: string;
  workerId: string;
  workerName: string;
  employeeId: string;
  sector: SectorName;
  moduleId: string;
  moduleName: string;
  weakArea: string;
  recommendation: string;
  status: RetrainingStatus;
  assignedDate: string;
  reassessmentDate?: string;
  initialScore: number;
  reassessmentScore?: number;
  scoreImprovement?: number;
}

export interface Certificate {
  id: string;
  certificateId: string;
  workerId: string;
  workerName: string;
  employeeId: string;
  sector: SectorName;
  moduleId: string;
  moduleName: string;
  resultGrade: string;
  issueDate: string;
  expiryDate: string;
  status: CertificateStatus;
  verificationCode: string;
  issuerDepartment: string;
  score?: number;
  competencyStatus?: string;
  publicImageUrl?: string;
  hasImage?: boolean;
  hasPdf?: boolean;
}

export interface RetentionRecord {
  id: string;
  workerId: string;
  workerName: string;
  employeeId: string;
  sector: SectorName;
  moduleName: string;
  lastTrainingDate: string;
  day1Status: RetentionStatus;
  day1Score?: number;
  day7Status: RetentionStatus;
  day7Score?: number;
  day30Status: RetentionStatus;
  day30Score?: number;
  auditCleared: boolean;
}

export interface NotificationItem {
  id: string;
  title: string;
  message: string;
  timestamp: string;
  type: 'critical' | 'warning' | 'info' | 'success';
  read: boolean;
}

export type DateRangePreset = 'Today' | 'Last 7 Days' | 'Last 30 Days' | 'Custom Range';

export interface DateRange {
  from: string;
  to: string;
}

export interface AdminSession {
  name: string;
  email: string;
  role: 'administrator';
  roleLabel: string;
  domain: string;
}

export type AllowedRole = 'administrator';
