/**
 * API client for the SurakshaAR FastAPI backend.
 *
 * Each function first attempts the real backend endpoint.
 * If the backend is unreachable, it falls back to mock data
 * so the dashboard remains usable during development / demos.
 *
 * To force mock data (e.g. for offline demos), set:
 *   VITE_USE_MOCK=true  in the dashboard .env file.
 */

import type {
  DashboardSummary,
  WorkerDetail,
  WorkerListResponse,
} from '../types';
import { getMockWorkerDetail, mockSummary, mockWorkers } from './mockData';

const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:8000/api/v1';
const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

async function fetchJson<T>(url: string): Promise<T> {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json() as Promise<T>;
}

async function tryBackend<T>(path: string): Promise<T | null> {
  if (USE_MOCK) return null;
  try {
    return await fetchJson<T>(`${API_BASE}${path}`);
  } catch {
    return null;
  }
}

/* ----------------------------- Dashboard summary ---------------------------- */

export async function getDashboardSummary(): Promise<DashboardSummary> {
  const data = await tryBackend<DashboardSummary>('/dashboard/summary');
  if (data) return data;
  // Fallback: mock data
  return mockSummary;
}

/* ------------------------------- Worker list ------------------------------- */

export async function getWorkerList(): Promise<WorkerListResponse> {
  const data = await tryBackend<WorkerListResponse>('/dashboard/workers');
  if (data) return data;
  return mockWorkers;
}

/* ------------------------------ Worker detail ------------------------------ */

export async function getWorkerDetail(id: number): Promise<WorkerDetail | null> {
  const data = await tryBackend<WorkerDetail>(`/dashboard/workers/${id}`);
  if (data) return data;
  return getMockWorkerDetail(id);
}
