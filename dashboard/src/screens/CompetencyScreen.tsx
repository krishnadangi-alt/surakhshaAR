import React from 'react';
import { ChartCard } from '../components/common/ChartCard';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { mockCompetencyRadarData, mockWorkers } from '../mockData';
import type { Worker } from '../types';
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
  const workerCompetencyColumns: Column<Worker>[] = [
    {
      key: 'name',
      header: 'Worker',
      sortable: true,
      render: (w) => (
        <div>
          <h5 className="font-bold text-white hover:text-suraksha-amber transition">{w.name}</h5>
          <p className="text-[10px] font-mono text-suraksha-subtext">{w.employeeId}</p>
        </div>
      ),
    },
    {
      key: 'sector',
      header: 'Industrial Unit',
      sortable: true,
      render: (w) => <span className="text-xs font-medium text-suraksha-text">{w.sector}</span>,
    },
    {
      key: 'latestScore',
      header: 'Latest Score',
      align: 'center',
      sortable: true,
      render: (w) => (
        <span
          className={`font-bold ${
            w.latestScore >= 85 ? 'text-emerald-400' : w.latestScore >= 70 ? 'text-amber-400' : 'text-rose-400'
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
        <span className={`font-bold text-xs ${w.weakAreas.length > 0 ? 'text-rose-400' : 'text-emerald-400'}`}>
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
    { id: 'ra-1', weakness: 'SCBA Cylinder Pressure Verification', workers: 128, modules: 'Gas Safety', action: 'Batch SCBA practical drill & reassess' },
    { id: 'ra-2', weakness: 'LOTO Lock Verification Before Entry', workers: 96, modules: 'Machinery / Gas', action: 'Assign Level-2 LOTO spatial scenario' },
    { id: 'ra-3', weakness: 'CO2 Horn-Handle Frostbite Protocol', workers: 61, modules: 'Fire Safety', action: 'Micro-drill on CO2 extinguisher handling' },
    { id: 'ra-4', weakness: 'Methane Sensor Span Calibration', workers: 47, modules: 'Gas Safety', action: 'Calibration bench simulation refresher' },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h3 className="text-lg font-bold text-white uppercase tracking-wider">
          Workforce Competency & Technical Skill Analytics
        </h3>
        <p className="text-xs text-suraksha-subtext">
          Statewide safety competency assessment, skill dimension breakdown, and vulnerability mapping.
        </p>
      </div>

      {/* Top Metrics Strip */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 text-center">
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Statewide Competency Index</p>
          <p className="text-2xl font-black text-suraksha-amber mt-1">84.6 / 100</p>
          <span className="text-[10px] text-emerald-400 font-semibold">↑ +2.8 vs Target</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Strong Competency Ratio</p>
          <p className="text-2xl font-black text-emerald-400 mt-1">68.0%</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">9,710 Certified Workers</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Developing Competency Ratio</p>
          <p className="text-2xl font-black text-amber-400 mt-1">24.0%</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">3,427 Workers</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Retraining Required</p>
          <p className="text-2xl font-black text-rose-400 mt-1">8.0%</p>
          <span className="text-[10px] text-rose-300/80 font-medium">1,143 Action Items</span>
        </div>
      </div>

      {/* Grid: Skill Radar & Strong/Weak Breakdown */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Radar Chart (2 cols wide) */}
        <ChartCard
          title="6-Dimension Safety Skill Radar"
          subtitle="Workforce average vs state safety benchmark"
          className="lg:col-span-2"
        >
          <div className="h-80 w-full pt-2">
            <ResponsiveContainer width="100%" height="100%">
              <RadarChart cx="50%" cy="50%" outerRadius="75%" data={mockCompetencyRadarData}>
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
            <div className="flex items-center gap-2 pb-3 border-b border-suraksha-border text-emerald-400 mb-3">
              <CheckCircle2 className="w-5 h-5" />
              <h4 className="text-xs font-bold uppercase tracking-wider text-white">Strongest Competency Areas</h4>
            </div>

            <div className="space-y-2 text-xs">
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-emerald-500/10 border border-emerald-500/20">
                <span className="font-semibold text-white">Fire Suppression & PASS Method</span>
                <span className="font-bold text-emerald-400">92.0%</span>
              </div>
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-emerald-500/10 border border-emerald-500/20">
                <span className="font-semibold text-white">Emergency Evacuation Routing</span>
                <span className="font-bold text-emerald-400">90.0%</span>
              </div>
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-emerald-500/10 border border-emerald-500/20">
                <span className="font-semibold text-white">Hazard Spotting & Alarm Escalation</span>
                <span className="font-bold text-emerald-400">88.0%</span>
              </div>
            </div>
          </div>

          {/* Weakest Vulnerabilities */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <div className="flex items-center gap-2 pb-3 border-b border-suraksha-border text-rose-400 mb-3">
              <AlertOctagon className="w-5 h-5" />
              <h4 className="text-xs font-bold uppercase tracking-wider text-white">Weakest Skill Vulnerabilities</h4>
            </div>

            <div className="space-y-2 text-xs">
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-rose-500/10 border border-rose-500/20">
                <span className="font-semibold text-white">SCBA Mask Pressure Verification</span>
                <span className="font-bold text-rose-400">61.5%</span>
              </div>
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-rose-500/10 border border-rose-500/20">
                <span className="font-semibold text-white">LOTO Lock Verification Sequence</span>
                <span className="font-bold text-rose-400">68.2%</span>
              </div>
              <div className="flex items-center justify-between p-2.5 rounded-lg bg-amber-500/10 border border-amber-500/20">
                <span className="font-semibold text-white">Wind Drift Foam Positioning</span>
                <span className="font-bold text-amber-400">71.0%</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Industrial Sector Competency Heatmap Matrix */}
      <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
        <h4 className="text-sm font-bold uppercase tracking-wider text-white mb-4">
          Competency Matrix across Jharkhand Industrial Zones
        </h4>

        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead>
              <tr className="border-b border-suraksha-border bg-suraksha-surface/60 uppercase font-bold text-suraksha-subtext">
                <th className="px-4 py-3">Industrial Zone</th>
                <th className="px-4 py-3 text-center">Hazard ID</th>
                <th className="px-4 py-3 text-center">SCBA &amp; Gas</th>
                <th className="px-4 py-3 text-center">Fire Suppression</th>
                <th className="px-4 py-3 text-center">LOTO Protocol</th>
                <th className="px-4 py-3 text-center">Overall Index</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-suraksha-border/40">
              {[
                { zone: 'Bokaro Steel Zone', hs: 90, gas: 84, fire: 94, loto: 88, overall: 89.0 },
                { zone: 'Dhanbad Mining Circle', hs: 84, gas: 68, fire: 86, loto: 74, overall: 78.0 },
                { zone: 'Jamshedpur Metallurgy', hs: 92, gas: 88, fire: 96, loto: 92, overall: 92.0 },
                { zone: 'Ranchi Heavy Electricals', hs: 86, gas: 78, fire: 85, loto: 82, overall: 82.8 },
                { zone: 'Ramgarh Chemical Works', hs: 88, gas: 82, fire: 90, loto: 80, overall: 85.0 },
              ].map((row, idx) => (
                <tr key={idx} className="hover:bg-suraksha-surface/40">
                  <td className="px-4 py-3 font-bold text-white">{row.zone}</td>
                  <td className="px-4 py-3 text-center text-emerald-400 font-bold">{row.hs}%</td>
                  <td className={`px-4 py-3 text-center font-bold ${row.gas < 75 ? 'text-rose-400' : 'text-emerald-400'}`}>
                    {row.gas}%
                  </td>
                  <td className="px-4 py-3 text-center text-emerald-400 font-bold">{row.fire}%</td>
                  <td className={`px-4 py-3 text-center font-bold ${row.loto < 80 ? 'text-amber-400' : 'text-emerald-400'}`}>
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
          <h4 className="text-sm font-bold uppercase tracking-wider text-white">
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
          data={mockWorkers}
          pageSize={8}
          onRowClick={(w) => onNavigateToScreen?.('worker-details', w.id)}
          emptyMessage="No workers available for competency review."
        />
      </div>

      {/* Targeted Retraining Action Center */}
      <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
        <div className="flex items-start gap-3 pb-3 border-b border-suraksha-border/60 mb-4">
          <div className="p-2 rounded-lg bg-amber-500/10 text-suraksha-amber border border-amber-500/30 shrink-0">
            <AlertOctagon className="w-5 h-5" />
          </div>
          <div>
            <h4 className="text-sm font-bold uppercase tracking-wider text-white">
              Targeted Retraining Action Center
            </h4>
            <p className="text-xs text-suraksha-subtext">
              Prioritized remediation batches mapped to the weakest competency dimensions
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
                <h5 className="text-xs font-bold text-white">{ra.weakness}</h5>
                <p className="text-[11px] text-suraksha-subtext mt-1">
                  {ra.workers} workers · {ra.modules}
                </p>
              </div>
              <div className="flex items-center justify-between mt-3 pt-3 border-t border-suraksha-border/40">
                <span className="text-[11px] text-amber-300/90">{ra.action}</span>
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
