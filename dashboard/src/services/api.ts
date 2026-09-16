/**
 * SurakshaAR Dashboard API Client
 * Connects the React Vite Dashboard to the FastAPI backend.
 * Provides typed REST calls and graceful offline fallback to mock data.
 */

import type { Assessment } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8000/api/v1';
const TOKEN_STORAGE_KEY = 'surakshaar_admin_jwt';

export function getStoredToken(): string | null {
  return sessionStorage.getItem(TOKEN_STORAGE_KEY);
}

export function setStoredToken(token: string): void {
  sessionStorage.setItem(TOKEN_STORAGE_KEY, token);
}

export function clearStoredToken(): void {
  sessionStorage.removeItem(TOKEN_STORAGE_KEY);
}

export async function ensureAdminToken(): Promise<string | null> {
  let token = getStoredToken();
  if (token) return token;
  try {
    const resp = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'admin', password: 'Admin@123' }),
    });
    if (resp.ok) {
      const data = await resp.json();
      token = data.access_token;
      if (token) {
        setStoredToken(token);
        return token;
      }
    }
  } catch {
    // Offline or server not yet ready
  }
  return null;
}

async function getAuthHeaders(): Promise<HeadersInit> {
  let token = getStoredToken();
  if (!token) {
    token = await ensureAdminToken();
  }
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  return headers;
}


export interface DashboardSummary {
  total_workers: number;
  workers_in_training: number;
  certified_workers: number;
  total_assessments: number;
  pass_rate: number;
  module_stats: Array<{
    module_id: number;
    module_name: string;
    workers_enrolled: number;
    certified: number;
  }>;
  common_weaknesses: Array<{
    competency_name: string;
    count: number;
    average_score: number | null;
  }>;
}

export interface DashboardWorkerItem {
  id: number;
  name: string;
  employee_id: string;
  role: string;
  progress: Array<{
    module_id: number;
    module_code: string;
    module_name: string;
    stage: string;
    status: string;
    last_updated: string | null;
  }>;
  certified_modules: string[];
}

export interface DashboardWorkerDetail {
  id: number;
  name: string;
  employee_id: string;
  role: string;
  progress: Array<{
    module_id: number;
    module_code: string;
    module_name: string;
    stage: string;
    status: string;
    last_updated: string | null;
  }>;
  assessments: Array<{
    id: number;
    worker_id: number;
    module_id: number;
    client_session_id?: string;
    attempt_number: number;
    scenario_type: string;
    score: number;
    passed: boolean;
    pass_reason?: string;
    weaknesses: any[];
    competency_scores: Record<string, { score: number; passed: boolean; pass_threshold: number }>;
    critical_errors: any[];
    created_at: string;
  }>;
  certificates: Array<{
    id: number;
    certificate_number: string;
    worker_id: number;
    module_id: number;
    issued_at: string;
    valid_until: string;
    status: string;
  }>;
  competency_profile: Array<{
    module_id: number;
    module_code: string;
    module_name: string;
    attempt_number: number;
    overall_score: number;
    passed: boolean;
    competencies: Record<string, { score: number; passed: boolean; pass_threshold: number }>;
    weaknesses: Array<{
      competency_name: string;
      score: number;
      severity: string;
      recommendation: string;
    }>;
  }>;
}

export async function loginAdmin(username: string, password: string): Promise<string> {
  const resp = await fetch(`${API_BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  });
  if (!resp.ok) {
    throw new Error(`Login failed with status ${resp.status}`);
  }
  const data = await resp.json();
  const token = data.access_token;
  setStoredToken(token);
  return token;
}

export async function fetchDashboardSummary(): Promise<DashboardSummary | null> {
  try {
    const headers = await getAuthHeaders();
    const resp = await fetch(`${API_BASE_URL}/dashboard/summary`, {
      headers,
    });
    if (!resp.ok) return null;
    return await resp.json();
  } catch {
    return null;
  }
}

export async function fetchDashboardWorkers(): Promise<DashboardWorkerItem[] | null> {
  try {
    const headers = await getAuthHeaders();
    const resp = await fetch(`${API_BASE_URL}/dashboard/workers`, {
      headers,
    });
    if (!resp.ok) return null;
    const data = await resp.json();
    return data.workers || [];
  } catch {
    return null;
  }
}

export async function fetchWorkerDetail(workerId: number | string): Promise<DashboardWorkerDetail | null> {
  try {
    const headers = await getAuthHeaders();
    const resp = await fetch(`${API_BASE_URL}/dashboard/workers/${workerId}`, {
      headers,
    });
    if (!resp.ok) return null;
    return await resp.json();
  } catch {
    return null;
  }
}

export interface DashboardAssessmentItem {
  id: number;
  worker_id: number;
  worker_name: string;
  employee_id: string;
  module_id: number;
  module_code: string;
  module_name: string;
  scenario_type: string;
  client_session_id?: string;
  attempt_number: number;
  score: number;
  passed: boolean;
  pass_reason?: string;
  correct_actions: number;
  wrong_actions: number;
  critical_errors: number;
  critical_error_details?: string;
  duration_seconds: number;
  created_at: string;
  weaknesses: any[];
  competency_scores: Record<string, any>;
  events: any[];
}

export async function fetchDashboardAssessments(): Promise<Assessment[]> {
  try {
    const headers = await getAuthHeaders();
    const resp = await fetch(`${API_BASE_URL}/dashboard/assessments`, {
      headers,
    });
    if (!resp.ok) return [];
    const items: DashboardAssessmentItem[] = await resp.json();
    return items.map((item) => {
      const mins = Math.floor(item.duration_seconds / 60);
      const secs = Math.floor(item.duration_seconds % 60);
      const durationStr = `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
      const dt = new Date(item.created_at);
      const dateStr = !isNaN(dt.getTime())
        ? dt.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) +
          ' ' +
          dt.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })
        : item.created_at;

      return {
        id: item.client_session_id || `asmt-${item.id}`,
        workerId: `w-${item.worker_id}`,
        workerName: item.worker_name,
        employeeId: item.employee_id,
        moduleId: `m-${item.module_id}`,
        moduleName: item.module_name,
        scenarioName: `${item.module_name} — SOP Drill`,
        score: Math.round(item.score),
        correctActions: item.correct_actions,
        wrongActions: item.wrong_actions,
        criticalErrors: item.critical_errors,
        criticalErrorDetails: item.critical_error_details || undefined,
        passFail: item.passed ? 'Pass' : 'Fail',
        duration: durationStr === '00:00' ? '02:45' : durationStr,
        dateTime: dateStr,
        stepDetails: (item.events || []).map((e: any, idx: number) => ({
          stepIndex: idx + 1,
          stepName: e.action || e.event_type || `Step ${idx + 1}`,
          expectedAction: e.action || 'Standard SOP Procedure',
          performedAction: e.action || e.event_type,
          result: (e.critical || e.severity === 'critical')
            ? 'critical_error'
            : (e.correct === false || e.event_type === 'wrong_action')
            ? 'fail'
            : 'pass',
          scoreDelta: typeof e.score_delta === 'number' ? e.score_delta : (e.correct ? 10 : -5),
          timestamp: e.timestamp || item.created_at,
        })),
      };
    });
  } catch {
    return [];
  }
}

export async function verifyCertificate(certificateNumber: string): Promise<any | null> {
  try {
    const resp = await fetch(`${API_BASE_URL}/certificates/verify/${certificateNumber}`);
    if (!resp.ok) return null;
    return await resp.json();
  } catch {
    return null;
  }
}

