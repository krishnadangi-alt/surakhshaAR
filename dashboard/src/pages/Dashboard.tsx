import { useEffect, useMemo, useState } from 'react';
import KpiCard from '../components/KpiCard';
import BarChart from '../components/BarChart';
import DonutChart from '../components/DonutChart';
import { getDashboardSummary, getWorkerList } from '../services/api';
import { formatDate, deriveWorkerStatus } from '../utils/format';
import type { DashboardSummary, Worker } from '../types';

interface ActivityItem {
  id: number;
  text: string;
  time: string;
  type: 'success' | 'danger' | 'warning' | 'info';
}

export default function Dashboard() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [workers, setWorkers] = useState<Worker[]>([]);

  useEffect(() => {
    getDashboardSummary().then(setSummary);
    getWorkerList().then((d) => setWorkers(d.workers));
  }, []);

  const stats = useMemo(() => {
    if (!summary) return null;
    const passed = Math.round((summary.total_assessments * summary.pass_rate) / 100);
    const failed = summary.total_assessments - passed;
    return { passed, failed };
  }, [summary]);

  const attentionWorkers = useMemo(() => {
    return workers
      .filter((w) => {
        const stage = w.progress[0]?.stage ?? '';
        return stage === 'retrain' || stage === 'diagnose';
      })
      .map((w) => {
        const prog = w.progress[0];
        const scoreMap: Record<string, number> = {
          certify: 85, retain: 90, assess: 70, diagnose: 50, retrain: 55, practice: 65, learn: 40,
        };
        const score = scoreMap[prog?.stage ?? ''] ?? 60;
        const status = deriveWorkerStatus(prog?.stage ?? '', score);
        return { ...w, _score: score, _status: status, _module: prog?.module_name ?? '—' };
      })
      .slice(0, 5);
  }, [workers]);

  const recentActivity = useMemo<ActivityItem[]>(() => {
    const items: ActivityItem[] = [];
    workers.forEach((w) => {
      w.progress.forEach((p) => {
        const stageLabel: Record<string, string> = {
          certify: 'completed certification',
          assess: 'completed assessment',
          retrain: 'assigned retraining',
          diagnose: 'entered diagnosis',
          practice: 'completed practice',
          learn: 'started training',
        };
        const typeMap: Record<string, ActivityItem['type']> = {
          certify: 'success',
          assess: 'info',
          retrain: 'warning',
          diagnose: 'warning',
          practice: 'info',
          learn: 'info',
        };
        items.push({
          id: w.id * 100 + p.module_id,
          text: `${w.name} ${stageLabel[p.stage] ?? 'updated'} — ${p.module_name}`,
          time: formatDate(p.last_updated),
          type: typeMap[p.stage] ?? 'info',
        });
      });
    });
    return items.sort((a, b) => a.time.localeCompare(b.time)).reverse().slice(0, 6);
  }, [workers]);

  if (!summary) {
    return <div className="text-sm text-muted">Loading dashboard...</div>;
  }

  return (
    <div className="page-enter">
      <div className="kpi-row">
        <KpiCard label="Total Workers" value={summary.total_workers} />
        <KpiCard label="Certified" value={summary.certified_workers} tone="success" />
        <KpiCard label="In Training" value={summary.workers_in_training} />
        <KpiCard label="Assessments" value={summary.total_assessments} />
        <KpiCard label="Pass Rate" value={`${summary.pass_rate}%`} tone="success" />
        <KpiCard label="Failed / Retraining" value={stats?.failed ?? 0} tone="warning" />
      </div>

      <div className="grid-2">
        <div className="card animate-in delay-1">
          <div className="card-header">Training Outcomes</div>
          <div className="card-body">
            <DonutChart
              data={[
                { label: 'Passed', value: stats?.passed ?? 0, color: '#15803d' },
                { label: 'Failed / Retraining', value: stats?.failed ?? 0, color: '#b45309' },
              ]}
            />
          </div>
        </div>

        <div className="card animate-in delay-2">
          <div className="card-header">Module Performance</div>
          <div className="card-body">
            <BarChart
              data={summary.module_stats.map((m) => ({
                label: m.module_name.length > 22 ? m.module_name.slice(0, 20) + '...' : m.module_name,
                value: m.certified,
                max: Math.max(m.workers_enrolled, 1),
                tone: 'accent',
              }))}
            />
          </div>
        </div>
      </div>

      <div className="grid-2">
        <div className="card animate-in delay-3 attention-section">
          <div className="card-header">
            <span>Workers Requiring Attention</span>
            <span className="badge badge-retraining">{attentionWorkers.length}</span>
          </div>
          <div className="card-body" style={{ padding: 0 }}>
            <div className="table-wrap">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Worker</th>
                    <th>Module</th>
                    <th>Score</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {attentionWorkers.map((w) => (
                    <tr key={w.id}>
                      <td>{w.name}</td>
                      <td className="text-secondary">{w._module}</td>
                      <td>{w._score}%</td>
                      <td>
                        <span className={`badge badge-${w._status === 'retraining' ? 'retraining' : w._status === 'failed' ? 'failed' : 'in-training'}`}>
                          {w._status === 'retraining' ? 'Retraining' : w._status === 'failed' ? 'Failed' : 'In Training'}
                        </span>
                      </td>
                    </tr>
                  ))}
                  {attentionWorkers.length === 0 && (
                    <tr>
                      <td colSpan={4} style={{ textAlign: 'center', padding: 24 }} className="text-muted">
                        No workers require attention currently.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div className="card animate-in delay-4">
          <div className="card-header">Recent Activity</div>
          <div className="card-body">
            <ul className="activity-timeline">
              {recentActivity.map((item) => (
                <li key={item.id} className="activity-item">
                  <span className={`activity-dot ${item.type}`} />
                  <div className="activity-content">
                    <div className="activity-text">{item.text}</div>
                    <div className="activity-time">{item.time}</div>
                  </div>
                </li>
              ))}
              {recentActivity.length === 0 && (
                <li className="text-muted text-sm">No recent activity recorded.</li>
              )}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}