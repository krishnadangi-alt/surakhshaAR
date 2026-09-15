/**
 * SurakshaAR Dashboard API Client
 * Connects the React Vite Dashboard to the FastAPI backend.
 * Provides typed REST calls and graceful offline fallback to mock data.
 */

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

function getAuthHeaders(): HeadersInit {
  const token = getStoredToken();
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
    const resp = await fetch(`${API_BASE_URL}/dashboard/summary`, {
      headers: getAuthHeaders(),
    });
    if (!resp.ok) return null;
    return await resp.json();
  } catch {
    return null;
  }
}

export async function fetchDashboardWorkers(): Promise<DashboardWorkerItem[] | null> {
  try {
    const resp = await fetch(`${API_BASE_URL}/dashboard/workers`, {
      headers: getAuthHeaders(),
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
    const resp = await fetch(`${API_BASE_URL}/dashboard/workers/${workerId}`, {
      headers: getAuthHeaders(),
    });
    if (!resp.ok) return null;
    return await resp.json();
  } catch {
    return null;
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
