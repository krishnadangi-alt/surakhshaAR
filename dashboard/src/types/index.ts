/**
 * Shared TypeScript types mirroring the FastAPI backend schemas.
 * Keep these in sync with backend/app/schemas/*.py
 */

export interface ModuleStat {
  module_id: number;
  module_name: string;
  workers_enrolled: number;
  certified: number;
}

export interface DashboardSummary {
  total_workers: number;
  workers_in_training: number;
  certified_workers: number;
  total_assessments: number;
  pass_rate: number;
  module_stats: ModuleStat[];
}

export interface ProgressItem {
  module_id: number;
  module_code: string;
  module_name: string;
  stage: string;
  status: string;
  last_updated: string;
}

export interface Worker {
  id: number;
  name: string;
  employee_id: string;
  role: string;
  progress: ProgressItem[];
  certified_modules: string[];
}

export interface WorkerListResponse {
  workers: Worker[];
}

export interface Assessment {
  id: number;
  worker_id: number;
  module_id: number;
  attempt_number: number;
  score: number;
  passed: boolean;
  weaknesses: string[];
  created_at: string;
}

export interface Certificate {
  id: number;
  certificate_number: string;
  worker_id: number;
  module_id: number;
  issued_at: string;
  valid_until: string;
  status: string;
}

export interface WorkerDetail extends Worker {
  assessments: Assessment[];
  certificates: Certificate[];
}
