import React, { useState, useEffect } from 'react';
import { KpiCard } from '../components/common/KpiCard';
import { ChartCard } from '../components/common/ChartCard';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import {
  mockWorkers,
  mockAssessments,
  mockModules,
  mockCompetencyWeaknesses,
  mockCompetencyDistribution,
} from '../mockData';
import { fetchDashboardSummary, type DashboardSummary } from '../services/api';
import type { DateRange, DateRangePreset } from '../types';
import { Users, ShieldCheck, Dumbbell, Award, AlertOctagon, TrendingUp } from 'lucide-react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts';

interface RecentActivityRow {
  id: string;
  workerId: string;
  workerName: string;
  employeeId: string;
  plant: string;
  sector: string;
  moduleName: string;
  stage: string;
  score: number;
  status: string;
  lastActivity: string;
}

interface OverviewScreenProps {
  onNavigateToWorker: (workerId: string) => void;
  onNavigateToScreen: (screen: string) => void;
  dateRange?: DateRangePreset;
  customRange?: DateRange | null;
}

const TOOLTIP_STYLE = {
  backgroundColor: '#1E1814',
  borderColor: '#342821',
  borderRadius: '8px',
  color: '#F5EFEA',
  fontSize: '12px',
};

const scoreColor = (score: number) =>
  score >= 85 ? 'text-emerald-400' : score >= 70 ? 'text-amber-400' : 'text-rose-400';

export const OverviewScreen: React.FC<OverviewScreenProps> = ({
  onNavigateToWorker,
  onNavigateToScreen,
}) => {
  const [liveSummary, setLiveSummary] = useState<DashboardSummary | null>(null);

  useEffect(() => {
    fetchDashboardSummary().then((data) => {
      if (data) setLiveSummary(data);
    });
  }, []);

  const kpi = {
    totalWorkers: liveSummary ? liveSummary.total_workers.toString() : '120',
    certified: liveSummary ? liveSummary.certified_workers.toString() : '85',
    inTraining: liveSummary ? liveSummary.workers_in_training.toString() : '20',
    passRate: liveSummary ? `${liveSummary.pass_rate}%` : '82%',
    assessments: liveSummary ? liveSummary.total_assessments.toString() : '185',
  };

  const modulePerformanceData = liveSummary && liveSummary.module_stats.length > 0
    ? liveSummary.module_stats.map((m) => ({
        name: m.module_name,
        Enrolled: m.workers_enrolled,
        Completed: m.certified,
        Certified: m.certified,
      }))
    : mockModules.map((m) => ({
        name: m.moduleName,
        Enrolled: m.totalEnrolled,
        Completed: m.completedCount,
        Certified: m.certifiedCount,
      }));

  const assessmentOverviewData = [
    { name: 'Passed', value: 151, color: '#10B981' },
    { name: 'Failed', value: 34, color: '#EF4444' },
  ];

  // Recent worker activity derived from assessment records.
  const recentActivityRows: RecentActivityRow[] = mockAssessments
    .map((a) => {
      const worker = mockWorkers.find((w) => w.id === a.workerId) || mockWorkers[0];
      const mod = worker.moduleProgressList.find((mp) => mp.moduleId === a.moduleId);
      return {
        id: a.id,
        workerId: worker.id,
        workerName: a.workerName,
        employeeId: a.employeeId,
        plant: worker.plant,
        sector: worker.sector,
        moduleName: a.moduleName,
        stage: mod?.stage ?? 'Assessment Run',
        score: a.score,
        status: a.passFail,
        lastActivity: a.dateTime,
      };
    })
    .sort((x, y) => (x.lastActivity < y.lastActivity ? 1 : -1));

  const recentColumns: Column<RecentActivityRow>[] = [
    {
      key: 'workerName',
      header: 'Worker',
      render: (r) => (
        <span className="font-bold text-suraksha-heading hover:text-suraksha-amber transition">{r.workerName}</span>
      ),
    },
    {
      key: 'employeeId',
      header: 'Employee ID',
      render: (r) => <span className="font-mono text-suraksha-subtext">{r.employeeId}</span>,
    },
    {
      key: 'moduleName',
      header: 'Module',
      render: (r) => <span className="font-medium text-suraksha-heading">{r.moduleName}</span>,
    },
    {
      key: 'stage',
      header: 'Stage',
      render: (r) => <span className="text-[11px] text-suraksha-subtext truncate max-w-[150px] block">{r.stage}</span>,
    },
    {
      key: 'score',
      header: 'Score',
      align: 'center',
      render: (r) => <span className={`font-bold ${scoreColor(r.score)}`}>{r.score}%</span>,
    },
    {
      key: 'status',
      header: 'Status',
      render: (r) => <StatusBadge status={r.status} size="sm" />,
    },
    {
      key: 'lastActivity',
      header: 'Last Activity',
      render: (r) => <span className="text-[10px] text-suraksha-subtext">{r.lastActivity}</span>,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Top KPI Section */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        <KpiCard
          title="Total Workers"
          value={kpi.totalWorkers}
          subtitle="Enrolled personnel"
          change="Demo cohort"
          changeType="neutral"
          icon={Users}
          accentColor="blue"
        />
        <KpiCard
          title="Certified Workers"
          value={kpi.certified}
          subtitle="Competencies completed"
          change="70.8% certified"
          changeType="positive"
          icon={ShieldCheck}
          variant="accent"
          accentColor="green"
        />
        <KpiCard
          title="In Training"
          value={kpi.inTraining}
          subtitle="Active AR modules"
          change="16.6% in training"
          changeType="neutral"
          icon={Dumbbell}
          accentColor="amber"
        />
        <KpiCard
          title="Pass Rate"
          value={kpi.passRate}
          subtitle="Assessment pass rate"
          change="151 passed"
          changeType="positive"
          icon={TrendingUp}
          accentColor="green"
        />
        <KpiCard
          title="Total Assessments"
          value={kpi.assessments}
          subtitle="Evaluations completed"
          change="34 failed or retrain"
          changeType="neutral"
          icon={Award}
          accentColor="blue"
        />
      </div>
      
                        

{/* Main Visualization Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Module Performance Card (2 cols wide) */}
        <ChartCard
          title="Module Performance — Fire / Gas / Machinery"
          subtitle="Registry-level enrollment, completion & certification across core modules"
          className="lg:col-span-2"
          action={
            <button
              onClick={() => onNavigateToScreen('modules')}
              className="text-xs font-semibold text-suraksha-blue hover:underline"
            >
              View Detailed Analytics →
            </button>
          }
        >
          <div className="h-72 w-full pt-2">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={modulePerformanceData} margin={{ top: 10, right: 10, left: -10, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#1E3A5F" opacity={0.5} />
                <XAxis dataKey="name" stroke="#94A3B8" fontSize={11} tickLine={false} />
                <YAxis stroke="#94A3B8" fontSize={11} tickLine={false} />
                <Tooltip contentStyle={TOOLTIP_STYLE} />
                <Legend wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }} />
                <Bar dataKey="Enrolled" fill="#1D6BF3" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Completed" fill="#F59E0B" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Certified" fill="#10B981" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
            </div>
            </ChartCard>
        {/* Assessment Overview Donut Card */}
        <ChartCard
          title="Assessment Pass / Fail"
          subtitle="Passed vs Failed evaluation runs"
          action={
            <button
              onClick={() => onNavigateToScreen('assessments')}
              className="text-xs font-semibold text-suraksha-amber hover:underline"
            >
              Records →
            </button>
          }
        >
          <div className="h-56 w-full flex items-center justify-center relative">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={assessmentOverviewData}
                  cx="50%"
                  cy="50%"
                  innerRadius={55}
                  outerRadius={80}
                  paddingAngle={4}
                  dataKey="value"
                >
                  {assessmentOverviewData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} stroke="#101C2E" strokeWidth={2} />
                  ))}
                </Pie>
                <Tooltip contentStyle={TOOLTIP_STYLE} />
              </PieChart>
            </ResponsiveContainer>

            <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
              <span className="text-xl font-bold text-suraksha-heading">82%</span>
              <span className="text-[10px] uppercase font-bold text-suraksha-subtext">Pass Rate</span>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-2 pt-3 border-t border-suraksha-border/60 text-center text-xs">
            <div className="p-2 rounded-lg bg-emerald-50 border border-emerald-200">
              <p className="text-[10px] font-bold text-emerald-700 uppercase">Passed</p>
              <p className="text-sm font-bold text-suraksha-heading">151</p>
            </div>
            <div className="p-2 rounded-lg bg-rose-50 border border-rose-200">
              <p className="text-[10px] font-bold text-rose-700 uppercase">Failed</p>
              <p className="text-sm font-bold text-suraksha-heading">34</p>
            </div>
          </div>
        </ChartCard>
      </div>
{/* Middle Section: Weaknesses & Competency Distribution */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Common Competency Weaknesses (2 cols wide) */}
        <div className="lg:col-span-2 rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
          <div className="flex items-center justify-between pb-3 border-b border-suraksha-border/60 mb-4">
            <div>
              <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading">
                Common Competency Weaknesses
              </h4>
              <p className="text-xs text-suraksha-subtext">
                Common weak areas identified during worker assessment runs
              </p>
            </div>
            <button
              onClick={() => onNavigateToScreen('competency')}
              className="text-xs font-semibold text-suraksha-amber hover:underline"
            >
              Competency Radar →
            </button>
          </div>

          <div className="space-y-3">
            {mockCompetencyWeaknesses.map((cw) => (
              <div
                key={cw.id}
                className="flex flex-wrap items-center justify-between gap-3 p-3 rounded-xl border border-suraksha-border bg-suraksha-surface/60 hover:border-suraksha-borderLight transition"
              >
                <div className="flex items-start gap-3">
                  <div
                    className={`p-2 rounded-lg border shrink-0 mt-0.5 ${
                      cw.severity === 'Critical'
                        ? 'bg-rose-500/10 text-rose-400 border-rose-500/30'
                        : 'bg-amber-500/10 text-amber-400 border-amber-500/30'
                    }`}
                  >
                    <AlertOctagon className="w-4 h-4" />
                  </div>
                  <div>
                    <h5 className="text-xs font-bold text-suraksha-heading">{cw.name}</h5>
                    <p className="text-[11px] text-suraksha-subtext mt-0.5">{cw.recommendedAction}</p>
                  </div>
                </div>

                <div className="flex items-center gap-4 text-xs">
                  <div className="text-right">
                    <span className="text-[10px] text-suraksha-subtext block">Occurrences</span>
                    <span className="font-bold text-suraksha-heading">{cw.occurrenceCount} runs</span>
                  </div>
                  <div className="text-right">
                    <span className="text-[10px] text-suraksha-subtext block">Avg Score</span>
                    <span className="font-bold text-suraksha-amber">{cw.averageScore}%</span>
                  </div>
                  <StatusBadge status={cw.severity} size="sm" />
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Competency Distribution (1 col) */}
        <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card flex flex-col justify-between">
          <div>
            <div className="pb-3 border-b border-suraksha-border/60 mb-4">
              <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading">
                Competency Distribution
              </h4>
              <p className="text-xs text-suraksha-subtext">Workforce technical readiness classification</p>
            </div>

            <div className="space-y-4">
              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="font-semibold text-emerald-700">Competent (Passed & Cleared Errors)</span>
                  <span className="font-bold text-suraksha-heading">76.0% ({mockCompetencyDistribution.competentCount})</span>
                </div>
                <div className="h-2.5 w-full bg-suraksha-surface rounded-full overflow-hidden border border-suraksha-border">
                  <div className="h-full bg-emerald-500 rounded-full" style={{ width: '76%' }} />
                </div>
              </div>

              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="font-semibold text-rose-700">Needs Retraining (Failed or Error Flagged)</span>
                  <span className="font-bold text-suraksha-heading">24.0% ({mockCompetencyDistribution.retrainingCount})</span>
                </div>
                <div className="h-2.5 w-full bg-suraksha-surface rounded-full overflow-hidden border border-suraksha-border">
                  <div className="h-full bg-rose-500 rounded-full" style={{ width: '24%' }} />
                </div>
              </div>
            </div>
          </div>

          <div className="mt-6 pt-4 border-t border-suraksha-border/60 text-center bg-suraksha-surface/40 p-3 rounded-xl">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Statewide Competency Index</p>
            <p className="text-2xl font-black text-suraksha-amber mt-0.5">84.6 / 100</p>
            <p className="text-[10px] text-emerald-700 font-semibold mt-1">↑ +2.8 points above safety target</p>
          </div>
        </div>
      </div>
{/* Bottom Section: Recent Worker Activity Table */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <h4 className="text-sm font-bold uppercase tracking-wider text-white">
            Recent Worker Activity
          </h4>
          <button
            onClick={() => onNavigateToScreen('workers')}
            className="text-xs font-semibold text-suraksha-blue hover:underline"
          >
            View Full Directory →
          </button>
        </div>

        <DataTable
          columns={recentColumns}
          data={recentActivityRows}
          pageSize={5}
          onRowClick={(r) => onNavigateToWorker(r.workerId)}
          emptyMessage="No recent worker activity recorded."
        />
      </div>
    </div>
  );
};

export default OverviewScreen;