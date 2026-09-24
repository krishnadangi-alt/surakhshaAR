import React, { useState, useEffect } from 'react';
import { ChartCard } from '../components/common/ChartCard';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { fetchDashboardSummary, fetchDashboardWorkers, fetchDashboardAssessments } from '../services/api';
import type { DashboardSummary } from '../services/api';
import type { Worker, Assessment } from '../types';
import { AlertOctagon, CheckCircle2, ArrowRight } from 'lucide-react';
import {
  ResponsiveContainer,
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis,
  Radar,
  Legend,
  Tooltip,
} from 'recharts';

interface CompetencyScreenProps {
  onNavigateToScreen?: (screen: string, param?: string) => void;
}

export const CompetencyScreen: React.FC<CompetencyScreenProps> = ({ onNavigateToScreen }) => {
  const [liveSummary, setLiveSummary] = useState<DashboardSummary | null>(null);
  const [liveWorkers, setLiveWorkers] = useState<Worker[]>([]);
  const [liveAssessments, setLiveAssessments] = useState<Assessment[]>([]);

  useEffect(() => {
    Promise.all([fetchDashboardSummary(), fetchDashboardWorkers(), fetchDashboardAssessments()]).then(
      ([summary, workers, assessments]) => {
        if (summary) setLiveSummary(summary);
        if (assessments) setLiveAssessments(assessments);
        if (workers && workers.length > 0) {
          const mapped: Worker[] = workers.map((w) => {
            const workerAssessments = (assessments || []).filter(
              (a) => a.workerId === `w-${w.id}` || a.employeeId === w.employee_id
            );
            const latestAsmt = workerAssessments[0];
            const latestScore = latestAsmt ? latestAsmt.score : (w.certified_modules.length > 0 ? 85 : 0);
            const overallCompetency: 'Competent' | 'Needs Retraining' = latestScore >= 75 ? 'Competent' : 'Needs Retraining';
            return {
              id: `w-${w.id}`,
              employeeId: w.employee_id,
              name: w.name,
              sector: 'Dhanbad Region-1',
              plant: w.employee_id.startsWith('GUEST') ? 'SurakshaAR AR Testing Hub' : 'Jharia Deep Shaft Mine #4',
              role: w.role,
              email: `${w.name.toLowerCase().replace(/[^a-z0-9]/g, '.')}@mining.jh.gov.in`,
              phone: '+91 98765 43210',
              joinedDate: '2026-01-15',
              safetyOfficer: 'Inspector R. K. Soren',
              overallStatus: w.certified_modules.length > 0 ? 'Certified' : (latestAsmt ? (latestAsmt.passFail === 'Pass' ? 'Passed' : 'Failed') : 'In Training'),
              modulesCompleted: w.certified_modules.length || (latestAsmt ? 1 : 0),
              latestScore,
              overallCompetency,
              lastAssessmentDate: latestAsmt ? latestAsmt.dateTime : '2026-09-16',
              certificatesCount: w.certified_modules.length,
              retrainingStatus: overallCompetency === 'Needs Retraining' ? 'Assigned' : 'Completed',
              moduleProgressList: w.progress.map((p) => ({
                moduleId: p.module_code,
                moduleName: p.module_name,
                stage: p.stage,
                status: p.status === 'completed' ? 'Completed' : 'In Progress',
                completionPercentage: p.status === 'completed' ? 100 : 50,
                score: latestScore,
                lastUpdated: p.last_updated || '2026-09-16',
              })),
              weakAreas: latestAsmt && latestAsmt.wrongActions > 0 ? ['PASS Extinguisher Technique'] : [],
              retentionDay1: 'Completed',
              retentionDay7: 'Scheduled',
              retentionDay30: 'Scheduled',
            };
          });
          setLiveWorkers(mapped);
        }
      }
    );
  }, []);
  const workerCompetencyColumns: Column<Worker>[] = [
    {
      key: 'name',
      header: 'Worker',
      sortable: true,
      render: (w) => (
        <div>
          <h5 className="font-bold text-suraksha-heading hover:text-suraksha-amber transition">{w.name}</h5>
          <p className="text-[10px] font-mono text-suraksha-subtext font-medium">{w.employeeId}</p>
        </div>
      ),
    },
    {
      key: 'sector',
      header: 'Industrial Unit',
      sortable: true,
      render: (w) => <span className="text-xs font-semibold text-suraksha-text">{w.sector}</span>,
    },
    {
      key: 'latestScore',
      header: 'Latest Score',
      align: 'center',
      sortable: true,
      render: (w) => (
        <span
          className={`font-bold ${
            w.latestScore >= 85 ? 'text-emerald-700' : w.latestScore >= 70 ? 'text-amber-700' : 'text-rose-700'
          }`}
        >
          {w.latestScore}%
        </span>
      ),
    },
    {
      key: 'overallCompetency',
      header: 'Competency Grade',
      sortable: true,
      render: (w) => <StatusBadge status={w.overallCompetency} size="sm" />,
    },
    {
      key: 'weakAreas',
      header: 'Flagged Weaknesses',
      align: 'center',
      render: (w) => (
        <span className={`font-bold text-xs ${w.weakAreas.length > 0 ? 'text-rose-700' : 'text-emerald-700'}`}>
          {w.weakAreas.length > 0 ? `${w.weakAreas.length}` : 'None'}
        </span>
      ),
    },
    {
      key: 'actions',
      header: 'Inspect',
      align: 'center',
      render: (w) => (
        <button
          onClick={(e) => {
            e.stopPropagation();
            onNavigateToScreen?.('worker-details', w.id);
          }}
          className="p-1.5 rounded-lg border border-suraksha-border text-suraksha-amber hover:bg-suraksha-hover transition"
          title="Open worker dossier"
        >
          <ArrowRight className="w-3.5 h-3.5" />
        </button>
      ),
    },
  ];

  const retrainingActions = [
    { id: 'ra-1', weakness: 'PPE Selection in Hazard Zone', workers: 128, modules: 'Gas Leak & Confined Space', action: 'Targeted PPE Training → PPE Practice → Reassess' },
    { id: 'ra-2', weakness: 'Extinguisher Selection & Use', workers: 96, modules: 'Fire & Explosion Response', action: 'Extinguisher selection & PASS drill → Reassess' },
    { id: 'ra-3', weakness: 'Evacuation Sequence', workers: 61, modules: 'Fire & Explosion Response', action: 'Evacuation route AR drill → Reassess' },
    { id: 'ra-4', weakness: 'Hazard Zone Recognition', workers: 47, modules: 'Gas Leak & Confined Space', action: 'Hazard zone demarcation practice → Reassess' },
  ];

  const avgScore =
    liveAssessments.length > 0
      ? (liveAssessments.reduce((acc, a) => acc + a.score, 0) / liveAssessments.length).toFixed(1)
      : '84.6';
  const passRate = liveSummary ? liveSummary.pass_rate.toFixed(1) : '76.0';
  const passedCount = liveSummary ? Math.round((liveSummary.total_assessments * liveSummary.pass_rate) / 100) : 92;
  const retrainingRate = liveSummary ? (100 - liveSummary.pass_rate).toFixed(1) : '24.0';
  const retrainingCount = liveSummary ? liveSummary.total_assessments - passedCount : 28;
  const workersSource = liveWorkers;

  const coreCompetencies = [
    { key: 'hazard_identification', label: 'Hazard Identification' },
    { key: 'ppe_selection', label: 'PPE Selection' },
    { key: 'procedure_compliance', label: 'Procedure Compliance' },
    { key: 'equipment_use', label: 'Equipment Operation' },
    { key: 'decision_making', label: 'Decision Making' },
  ];

  const competencyRadarData = coreCompetencies.map(({ key, label }) => {
    let sum = 0;
    let count = 0;
    for (const a of liveAssessments) {
      const cScores = (a as any).competencyScores || (a as any).competency_scores;
      if (cScores && cScores[key]) {
        sum += cScores[key].score;
        count++;
      } else if (a.score) {
        sum += a.score;
        count++;
      }
    }
    return {
      dimension: label,
      score: count > 0 ? Math.round(sum / count) : 80,
      benchmark: 75,
    };
  });

  const liveWeaknesses = liveSummary?.common_weaknesses && liveSummary.common_weaknesses.length > 0
    ? liveSummary.common_weaknesses.slice(0, 3)
    : [];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h3 className="text-lg font-bold text-suraksha-heading uppercase tracking-wider">
          Workforce Competency
        </h3>
        <p className="text-xs text-suraksha-subtext font-medium">
          Workforce safety competency summary, skill breakdown, and weak area analysis.
        </p>
      </div>

      {/* Top Metrics Strip */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 text-center">
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Statewide Competency Index</p>
          <p className="text-2xl font-black text-suraksha-amber mt-1">{avgScore} / 100</p>
          <span className="text-[10px] text-emerald-700 font-bold">Live Assessment Index</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Competent Ratio</p>
          <p className="text-2xl font-black text-emerald-700 mt-1">{passRate}%</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">{passedCount} Passed &amp; Cleared</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Needs Retraining</p>
          <p className="text-2xl font-black text-rose-700 mt-1">{retrainingRate}%</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">{retrainingCount} Action Items</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Modules Monitored</p>
          <p className="text-2xl font-black text-suraksha-blue mt-1">3 Modules</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">Fire / Gas / Machinery</span>
        </div>
      </div>

      {/* Grid: Skill Radar & Strong/Weak Breakdown */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Radar Chart (2 cols wide) */}
        <ChartCard
          title="Competency Overview"
          subtitle="Workforce average vs state safety benchmark"
          className="lg:col-span-2"
        >
          <div className="h-80 w-full pt-2">
            <ResponsiveContainer width="100%" height="100%">
              <RadarChart cx="50%" cy="50%" outerRadius="75%" data={competencyRadarData}>
                <PolarGrid stroke="#342821" />
                <PolarAngleAxis dataKey="dimension" stroke="#A8998C" fontSize={11} />
                <PolarRadiusAxis angle={30} domain={[0, 100]} stroke="#342821" fontSize={10} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#1E1814',
                    borderColor: '#342821',
                    borderRadius: '8px',
                    color: '#F5EFEA',
                    fontSize: '12px',
                  }}
                />
                <Legend wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }} />
                <Radar
                  name="Jharkhand Workforce Score"
                  dataKey="score"
                  stroke="#E88024"
                  fill="#E88024"
                  fillOpacity={0.4}
                />
                <Radar
                  name="State Compliance Benchmark"
                  dataKey="benchmark"
                  stroke="#E87722"
                  fill="#E87722"
                  fillOpacity={0.25}
                />
              </RadarChart>
            </ResponsiveContainer>
          </div>
        </ChartCard>

        {/* Strongest & Weakest Competency Areas */}
        <div className="space-y-6">
          {/* Strongest Areas */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <div className="flex items-center gap-2 pb-3 border-b border-suraksha-border text-emerald-700 mb-3">
              <CheckCircle2 className="w-5 h-5" />
              <h4 className="text-xs font-bold uppercase tracking-wider text-suraksha-heading">Strongest Competency Areas</h4>
            </div>

            <div className="space-y-2 text-xs">
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-emerald-50 border border-emerald-200">
                <span className="font-semibold text-suraksha-heading">Select & Use Extinguisher</span>
                <span className="font-bold text-emerald-700">91.0%</span>
              </div>
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-emerald-50 border border-emerald-200">
                <span className="font-semibold text-suraksha-heading">Evacuation Sequence</span>
                <span className="font-bold text-emerald-700">89.0%</span>
              </div>
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-emerald-50 border border-emerald-200">
                <span className="font-semibold text-suraksha-heading">Hazard Recognition</span>
                <span className="font-bold text-emerald-700">88.0%</span>
              </div>
            </div>
          </div>

          {/* Weakest Vulnerabilities */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <div className="flex items-center gap-2 pb-3 border-b border-suraksha-border text-rose-700 mb-3">
              <AlertOctagon className="w-5 h-5" />
              <h4 className="text-xs font-bold uppercase tracking-wider text-suraksha-heading">Weakest Skill Vulnerabilities</h4>
            </div>

            <div className="space-y-2 text-xs">
              {liveWeaknesses.map((w, idx) => (
                <div key={idx} className="flex items-center justify-between p-2.5 rounded-lg bg-rose-50 border border-rose-200">
                  <span className="font-semibold text-suraksha-heading">{w.competency_name}</span>
                  <span className="font-bold text-rose-700">
                    {w.average_score !== null ? `${w.average_score}%` : `${w.count} errors`}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Industrial Sector Competency Heatmap Matrix */}
      <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
        <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading mb-4">
          Competency Matrix across DGMS Jurisdictions
        </h4>

        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead>
              <tr className="border-b border-suraksha-border bg-suraksha-surface/60 uppercase font-bold text-suraksha-subtext">
                <th className="px-4 py-3">DGMS Jurisdiction Region</th>
                <th className="px-4 py-3 text-center">Hazard ID</th>
                <th className="px-4 py-3 text-center">PPE Selection</th>
                <th className="px-4 py-3 text-center">Fire &amp; Extinguisher</th>
                <th className="px-4 py-3 text-center">Evacuation</th>
                <th className="px-4 py-3 text-center">Overall Index</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-suraksha-border/40">
              {[
                { zone: 'Dhanbad Region-1', hs: 88, gas: 72, fire: 90, loto: 88, overall: 84.5 },
                { zone: 'Dhanbad Region-2', hs: 84, gas: 68, fire: 86, loto: 74, overall: 78.0 },
                { zone: 'Koderma Region', hs: 90, gas: 84, fire: 94, loto: 88, overall: 89.0 },
                { zone: 'Ranchi Region', hs: 92, gas: 88, fire: 96, loto: 92, overall: 92.0 },
                { zone: 'Chaibasa Region', hs: 86, gas: 78, fire: 85, loto: 82, overall: 82.8 },
              ].map((row, idx) => (
                <tr key={idx} className="hover:bg-suraksha-surface/40">
                  <td className="px-4 py-3 font-bold text-suraksha-heading">{row.zone}</td>
                  <td className="px-4 py-3 text-center text-emerald-700 font-bold">{row.hs}%</td>
                  <td className={`px-4 py-3 text-center font-bold ${row.gas < 75 ? 'text-rose-700' : 'text-emerald-700'}`}>
                    {row.gas}%
                  </td>
                  <td className="px-4 py-3 text-center text-emerald-700 font-bold">{row.fire}%</td>
                  <td className={`px-4 py-3 text-center font-bold ${row.loto < 80 ? 'text-amber-700' : 'text-emerald-700'}`}>
                    {row.loto}%
                  </td>
                  <td className="px-4 py-3 text-center font-extrabold text-suraksha-amber">{row.overall}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Worker Competency Overview Table */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading">
            Worker Competency Overview
          </h4>
          {onNavigateToScreen && (
            <button
              onClick={() => onNavigateToScreen('workers')}
              className="text-xs font-semibold text-suraksha-blue hover:underline"
            >
              Full Workers Directory →
            </button>
          )}
        </div>
        <DataTable
          columns={workerCompetencyColumns}
          data={workersSource}
          pageSize={8}
          onRowClick={(w) => onNavigateToScreen?.('worker-details', w.id)}
          emptyMessage="No workers available for competency review."
        />
      </div>

      {/* Targeted Retraining */}
      <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
        <div className="flex items-start gap-3 pb-3 border-b border-suraksha-border/60 mb-4">
          <div className="p-2 rounded-lg bg-amber-500/10 text-suraksha-amber border border-amber-500/30 shrink-0">
            <AlertOctagon className="w-5 h-5" />
          </div>
          <div>
            <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading">
              Targeted Retraining
            </h4>
            <p className="text-xs text-suraksha-subtext font-medium">
              Workers requiring targeted retraining based on weak areas.
            </p>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          {retrainingActions.map((ra) => (
            <div
              key={ra.id}
              className="p-4 rounded-xl border border-suraksha-border bg-suraksha-surface/60 flex flex-col justify-between"
            >
              <div>
                <h5 className="text-xs font-bold text-suraksha-heading">{ra.weakness}</h5>
                <p className="text-[11px] text-suraksha-subtext font-medium mt-1">
                  {ra.workers} workers · {ra.modules}
                </p>
              </div>
              <div className="flex items-center justify-between mt-3 pt-3 border-t border-suraksha-border/40">
                <span className="text-[11px] text-amber-800 font-semibold">{ra.action}</span>
                <button
                  onClick={() => onNavigateToScreen?.('retraining')}
                  className="shrink-0 ml-2 flex items-center gap-1 rounded-lg border border-amber-500/30 bg-amber-500/10 px-2.5 py-1 text-[11px] font-bold text-suraksha-amber hover:bg-amber-500/20 transition"
                >
                  Queue <ArrowRight className="w-3 h-3" />
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
