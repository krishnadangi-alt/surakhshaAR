import { useEffect, useMemo, useState } from 'react';
import KpiCard from '../components/KpiCard';
import BarChart from '../components/BarChart';
import DonutChart from '../components/DonutChart';
import { getDashboardSummary, getWorkerList } from '../services/api';
import type { DashboardSummary, Worker } from '../types';

export default function Analytics() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [workers, setWorkers] = useState<Worker[]>([]);

  useEffect(() => {
    getDashboardSummary().then(setSummary);
    getWorkerList().then((d) => setWorkers(d.workers));
  }, []);

  const weaknessRanking = useMemo(() => {
    const map = new Map<string, number>();
    workers.forEach((w) => {
      w.progress.forEach((p) => {
        if (p.stage === 'retrain' || p.stage === 'diagnose') {
          map.set('PPE Selection', (map.get('PPE Selection') ?? 0) + 1);
          map.set('Hazard Recognition', (map.get('Hazard Recognition') ?? 0) + 1);
        }
      });
    });
    return Array.from(map.entries()).sort((a, b) => b[1] - a[1]);
  }, [workers]);

  const retrainingCount = useMemo(() => {
    return workers.filter((w) => w.progress.some((p) => p.stage === 'retrain')).length;
  }, [workers]);

  const certificatesIssued = useMemo(() => {
    return workers.filter((w) => w.certified_modules.length > 0).length;
  }, [workers]);

  const passedCount = useMemo(() => {
    return workers.filter((w) => {
      const stage = w.progress[0]?.stage ?? '';
      return stage === 'certify' || stage === 'retain';
    }).length;
  }, [workers]);

  const failedCount = workers.length - passedCount;

  if (!summary) {
    return <div className="text-sm text-muted">Loading analytics...</div>;
  }

  return (
    <div className="page-enter">
      <div className="kpi-row">
        <KpiCard label="Average Pass Rate" value={`${summary.pass_rate}%`} tone="success" />
        <KpiCard label="Total Assessments" value={summary.total_assessments} />
        <KpiCard label="Retraining Required" value={retrainingCount} tone="warning" />
        <KpiCard label="Certificates Issued" value={certificatesIssued} tone="success" />
      </div>

      <div className="grid-2">
        <div className="card animate-in delay-1">
          <div className="card-header">Pass / Fail Distribution</div>
          <div className="card-body">
            <DonutChart
              data={[
                { label: 'Passed', value: passedCount, color: '#15803d' },
                { label: 'Failed / Retraining', value: failedCount, color: '#b95309' },
              ]}
            />
          </div>
        </div>

        <div className="card animate-in delay-2">
          <div className="card-header">Module Enrollment vs Certified</div>
          <div className="card-body">
            <BarChart
              data={summary.module_stats.map((m) => ({
                label: m.module_name.length > 18 ? m.module_name.slice(0, 16) + '...' : m.module_name,
                value: m.certified,
                max: Math.max(m.workers_enrolled, 1),
                tone: 'success',
              }))}
            />
          </div>
        </div>
      </div>

      <div className="card mt-4 animate-in delay-3">
        <div className="card-header">Common Weaknesses</div>
        <div className="card-body">
          {weaknessRanking.length > 0 ? (
            <BarChart
              data={weaknessRanking.map(([label, value]) => ({
                label,
                value,
                max: Math.max(...weaknessRanking.map(([, v]) => v), 1),
                tone: 'accent',
              }))}
            />
          ) : (
            <div className="text-sm text-muted">
              Weakness data will appear as workers complete assessments.
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
