import React from 'react';
import { KpiCard } from '../components/common/KpiCard';
import { ChartCard } from '../components/common/ChartCard';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { EmptyState } from '../components/common/EmptyState';
import {
  mockWorkers,
  mockAssessments,
  mockModules,
  mockCompetencyWeaknesses,
  mockCompetencyDistribution,
} from '../mockData';
import { filterAssessmentsByRange, resolveRange } from '../utils/dateRange';
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
  dateRange: DateRangePreset;
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
  dateRange,
  customRange,
}) => {
  // The global date range selector drives the mock state below.
  const range = resolveRange(dateRange, customRange ?? undefined);
  const rangeLabel = `${range.from} → ${range.to}`;
  const inRangeAssessments = filterAssessmentsByRange(mockAssessments, dateRange, customRange ?? undefined);

  const coverage =
    inRangeAssessments.length === 0 ? 0 : Math.max(inRangeAssessments.length / mockAssessments.length, 0.15);

  const passCount = inRangeAssessments.filter((a) => a.passFail === 'Pass').length;
  const failCount = inRangeAssessments.length - passCount;
  const hasRangeData = inRangeAssessments.length > 0;
  const rangePassRate = hasRangeData ? Math.round((passCount / inRangeAssessments.length) * 100) : 0;

  const kpi = {
    totalWorkers: hasRangeData ? Math.round(14280 * coverage).toLocaleString() : '0',
    certified: hasRangeData ? Math.round(11850 * coverage).toLocaleString() : '0',
    inTraining: hasRangeData ? Math.round(1640 * coverage).toLocaleString() : '0',
    passRate: hasRangeData ? `${rangePassRate}%` : '—',
    assessments: hasRangeData ? Math.round(38420 * coverage).toLocaleString() : '0',
  };

  const modulePerformanceData = mockModules.map((m) => ({
    name: m.moduleName,
    Enrolled: m.totalEnrolled,
    Completed: m.completedCount,
    Certified: m.certifiedCount,
  }));

  const assessmentOverviewData = [
    { name: 'Pass', value: hasRangeData ? passCount : 33579, color: '#10B981' },
    { name: 'Fail', value: hasRangeData ? failCount : 4841, color: '#EF4444' },
  ];

  // Recent worker activity derived from the date-scoped assessment logs.
  const recentActivityRows: RecentActivityRow[] = inRangeAssessments
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
        <div>
          <span className="font-bold text-white hover:text-suraksha-amber transition">{r.workerName}</span>
          <p className="text-[10px] text-suraksha-subtext">{r.plant}</p>
        </div>
      ),
    },
    {
      key: 'employeeId',
      header: 'Employee ID',
      render: (r) => <span className="font-mono text-suraksha-subtext">{r.employeeId}</span>,
    },
    {
      key: 'sector',
      header: 'Plant/Sector',
      render: (r) => <span className="text-suraksha-subtext font-medium">{r.sector}</span>,
    },
    {
      key: 'moduleName',
      header: 'Module',
      render: (r) => <span className="font-medium text-white">{r.moduleName}</span>,
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
      {/* Date Scope Banner */}
      {!hasRangeData && (
        <div className="flex items-start gap-3 rounded-xl border border-amber-500/30 bg-amber-500/5 p-4 text-amber-200">
          <AlertOctagon className="w-5 h-5 text-suraksha-amber shrink-0 mt-0.5" />
          <div className="text-xs">
            <p className="font-bold text-white">No assessment activity in the selected range ({rangeLabel}).</p>
            <p className="text-suraksha-subtext mt-0.5">
              The KPI cards below reflect registry-level aggregates. Expand the date range to see assessment-level activity.
            </p>
          </div>
        </div>
      )}

      {/* Top KPI Section */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        <KpiCard
          title="Total Workers"
          value={kpi.totalWorkers}
          subtitle={`Range: ${dateRange}`}
          change={hasRangeData ? (coverage >= 1 ? 'Full registry scope' : `${Math.round(coverage * 100)}% of registry`) : 'No activity'}
          changeType="neutral"
          icon={Users}
          accentColor="blue"
        />
        <KpiCard
          title="Certified Workers"
          value={kpi.certified}
          subtitle="Aggregate compliance estimate"
          change={hasRangeData ? `${Math.round((11850 * coverage) / Math.max(14280 * coverage, 1) * 100)}% compliance` : '—'}
          changeType="positive"
          icon={ShieldCheck}
          variant="accent"
          accentColor="green"
        />
        <KpiCard
          title="In Training"
          value={kpi.inTraining}
          subtitle="Active AR Module Sessions"
          change={hasRangeData ? `${Math.round(320 * coverage)} pending evaluations` : '—'}
          changeType="neutral"
          icon={Dumbbell}
          accentColor="amber"
        />
        <KpiCard
          title="Pass Rate"
          value={kpi.passRate}
          subtitle={hasRangeData ? `From ${inRangeAssessments.length} in-range runs` : 'No in-range assessments'}
          change={hasRangeData ? `${passCount} passed in range` : '—'}
          changeType="positive"
          icon={TrendingUp}
          accentColor="green"
        />
        <KpiCard
          title="Total Assessments"
          value={kpi.assessments}
          subtitle={hasRangeData ? `In-range: ${inRangeAssessments.length}` : 'No in-range assessments'}
          change={`Scope ${rangeLabel}`}
          changeType={hasRangeData ? 'positive' : 'neutral'}
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
          subtitle={hasRangeData ? `Distribution across ${rangeLabel}` : 'Registry-level aggregate (no in-range runs)'}
          action={
            <button
              onClick={() => onNavigateToScreen('assessments')}
              className="text-xs font-semibold text-suraksha-amber hover:underline"
            >
              Logs →
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
              <span className="text-xl font-bold text-white">{hasRangeData ? `${rangePassRate}%` : '87.4%'}</span>

          {/* End of balance structured sections */}

              <span className="text-[10px] uppercase font-bold text-suraksha-subtext">
                {hasRangeData ? 'Range Pass Rate' : 'Avg Pass Rate'}
              </span>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-2 pt-3 border-t border-suraksha-border/60 text-center text-xs">
            <div className="p-2 rounded-lg bg-emerald-500/10 border border-emerald-500/20">
              <p className="text-[10px] font-bold text-emerald-400 uppercase">Passed</p>
              <p className="text-sm font-bold text-white">{hasRangeData ? passCount : '33,579'}</p>
            </div>
            <div className="p-2 rounded-lg bg-rose-500/10 border border-rose-500/20">
              <p className="text-[10px] font-bold text-rose-400 uppercase">Failed</p>
              <p className="text-sm font-bold text-white">{hasRangeData ? failCount : '4,841'}</p>
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
              <h4 className="text-sm font-bold uppercase tracking-wider text-white">
                Common Competency Weaknesses
              </h4>
              <p className="text-xs text-suraksha-subtext">
                Top recurring hazards identified during AR assessment trials
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
                    <h5 className="text-xs font-bold text-white">{cw.name}</h5>
                    <p className="text-[11px] text-suraksha-subtext mt-0.5">{cw.recommendedAction}</p>
                  </div>
                </div>

                <div className="flex items-center gap-4 text-xs">
                  <div className="text-right">
                    <span className="text-[10px] text-suraksha-subtext block">Occurrences</span>
                    <span className="font-bold text-white">{cw.occurrenceCount} runs</span>
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
              <h4 className="text-sm font-bold uppercase tracking-wider text-white">
                Competency Distribution
              </h4>
              <p className="text-xs text-suraksha-subtext">Workforce technical readiness classification</p>
            </div>

            <div className="space-y-4">
              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="font-semibold text-emerald-400">Strong (Score 85%+)</span>
                  <span className="font-bold text-white">68.0% ({mockCompetencyDistribution.strongCount})</span>
                </div>
                <div className="h-2.5 w-full bg-suraksha-surface rounded-full overflow-hidden border border-suraksha-border">
                  <div className="h-full bg-emerald-500 rounded-full" style={{ width: '68%' }} />
                </div>
              </div>

              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="font-semibold text-amber-400">Developing (Score 70-84%)</span>
                  <span className="font-bold text-white">24.0% ({mockCompetencyDistribution.developingCount})</span>
                </div>
                <div className="h-2.5 w-full bg-suraksha-surface rounded-full overflow-hidden border border-suraksha-border">
                  <div className="h-full bg-amber-500 rounded-full" style={{ width: '24%' }} />
                </div>
              </div>

              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="font-semibold text-rose-400">Needs Retraining (&lt;70%)</span>
                  <span className="font-bold text-white">8.0% ({mockCompetencyDistribution.retrainingCount})</span>
                </div>
                <div className="h-2.5 w-full bg-suraksha-surface rounded-full overflow-hidden border border-suraksha-border">
                  <div className="h-full bg-rose-500 rounded-full" style={{ width: '8%' }} />
                </div>
              </div>
            </div>
          </div>

          <div className="mt-6 pt-4 border-t border-suraksha-border/60 text-center bg-suraksha-surface/40 p-3 rounded-xl">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Statewide Competency Index</p>
            <p className="text-2xl font-black text-suraksha-amber mt-0.5">84.6 / 100</p>
            <p className="text-[10px] text-emerald-400 font-semibold mt-1">↑ +2.8 points above safety target</p>
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

        {hasRangeData ? (
          <DataTable
            columns={recentColumns}
            data={recentActivityRows}
            pageSize={5}
            onRowClick={(r) => onNavigateToWorker(r.workerId)}
            emptyMessage="No recent worker activity recorded."
          />
        ) : (
          <EmptyState
            title="No Activity in Selected Range"
            description={`No assessment logs fall between ${rangeLabel}. Select a wider date range to view worker activity.`}
          />
        )}
      </div>
    </div>
  );
};

export default OverviewScreen;