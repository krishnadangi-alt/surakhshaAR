import { useEffect, useMemo, useState } from 'react';
import KpiCard from '../components/KpiCard';
import BarChart from '../components/BarChart';
import DonutChart from '../components/DonutChart';
import { getDashboardSummary, getWorkerList } from '../services/api';
import { formatDate } from '../utils/format';
import type { DashboardSummary, Worker } from '../types';

const STAGE_LABELS: Record<string, string> = {
  learn: 'Training',
  practice: 'Practice',
  assess: 'Assessment',
  diagnose: 'Diagnosis',
  retrain: 'Retraining',
  reassess: 'Reassessment',
  certify: 'Certification',
  retain: 'Certified',
};

const STAGE_BADGE: Record<string, string> = {
  certify: 'badge-passed',
  retain: 'badge-passed',
  learn: 'badge-in-training',
  practice: 'badge-in-training',
  assess: 'badge-in-training',
  reassess: 'badge-in-training',
  diagnose: 'badge-retraining',
  retrain: 'badge-retraining',
};

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

  const recentWorkers = useMemo(() => {
    return [...workers]
      .sort((a, b) => {
        const ta = a.progress[0]?.last_updated ?? '';
        const tb = b.progress[0]?.last_updated ?? '';
        return tb.localeCompare(ta);
      })
      .slice(0, 8);
  }, [workers]);

  if (!summary) {
    return <div className="text-sm text-muted">Loading dashboard...</div>;
  }

  return (
    <div>
      <div className="kpi-row">
        <KpiCard label="Total Workers" value={summary.total_workers} />
        <KpiCard label="Certified" value={summary.certified_workers} tone="success" />
        <KpiCard label="In Training" value={summary.workers_in_training} />
        <KpiCard label="Pass Rate" value={`${summary.pass_rate}%`} tone="success" />
        <KpiCard label="Total Assessments" value={summary.total_assessments} />
      </div>

      <div className="grid-2">
        <div className="card">
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

        <div className="card">
          <div className="card-header">Assessment Overview</div>
          <div className="card-body">
            <DonutChart
              data={[
                { label: 'Passed', value: stats?.passed ?? 0, color: '#15803d' },
                { label: 'Failed', value: stats?.failed ?? 0, color: '#b91c1c' },
              ]}
            />
            <div className="overview-stats">
              <div>Average Pass Rate: <strong>{summary.pass_rate}%</strong></div>
              <div>Total Assessments: <strong>{summary.total_assessments}</strong></div>
            </div>
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-header">Recent Worker Activity</div>
        <div className="card-body" style={{ padding: 0 }}>
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Worker</th>
                  <th>Employee ID</th>
                  <th>Module</th>
                  <th>Stage</th>
                  <th>Last Activity</th>
                </tr>
              </thead>
              <tbody>
                {recentWorkers.map((w) => {
                  const stage = w.progress[0]?.stage ?? '';
                  const stageLabel = STAGE_LABELS[stage] ?? stage;
                  const stageBadge = STAGE_BADGE[stage] ?? 'badge-in-training';
                  return (
                    <tr key={w.id}>
                      <td>{w.name}</td>
                      <td>{w.employee_id}</td>
                      <td>{w.progress[0]?.module_name ?? '\u2014'}</td>
                      <td>{stage ? <span className={`badge ${stageBadge}`}>{stageLabel}</span> : '\u2014'}</td>
                      <td className="text-secondary">
                        {w.progress[0] ? formatDate(w.progress[0].last_updated) : '\u2014'}
                      </td>
                    </tr>
                  );
                })}
                {recentWorkers.length === 0 && (
                  <tr>
                    <td colSpan={5} style={{ textAlign: 'center', padding: 24 }} className="text-muted">
                      No worker activity recorded.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
