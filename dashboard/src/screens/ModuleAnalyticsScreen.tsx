import React, { useState } from 'react';
import { ChartCard } from '../components/common/ChartCard';
import { mockModules } from '../mockData';
import { Flame, Wind, Cog } from 'lucide-react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
  Legend,
} from 'recharts';

export const ModuleAnalyticsScreen: React.FC = () => {
  const [selectedModuleId, setSelectedModuleId] = useState<string>('ALL');

  const selectedModule = mockModules.find((m) => m.moduleId === selectedModuleId);

  // Combined trend line data
  const combinedTrendData = [
    { month: 'Apr', Fire: 88, Gas: 81, Machinery: 85 },
    { month: 'May', Fire: 89, Gas: 82, Machinery: 86 },
    { month: 'Jun', Fire: 87, Gas: 83, Machinery: 87 },
    { month: 'Jul', Fire: 90, Gas: 84, Machinery: 88 },
    { month: 'Aug', Fire: 91, Gas: 85, Machinery: 89 },
    { month: 'Sep', Fire: 92, Gas: 86, Machinery: 90 },
  ];

  return (
    <div className="space-y-6">
      {/* Top Header & Module Filter Tabs */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h3 className="text-lg font-bold text-white uppercase tracking-wider">Module Performance Analytics</h3>
          <p className="text-xs text-suraksha-subtext">
            In-depth performance, completion rates, and critical error heatmaps for Fire, Gas & Machinery AR modules.
          </p>
        </div>

        <div className="flex items-center gap-1.5 p-1 rounded-xl bg-suraksha-card border border-suraksha-border">
          <button
            onClick={() => setSelectedModuleId('ALL')}
            className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition ${
              selectedModuleId === 'ALL'
                ? 'bg-suraksha-blue text-white shadow-subtle'
                : 'text-suraksha-subtext hover:text-white'
            }`}
          >
            All Modules
          </button>
          {mockModules.map((m) => (
            <button
              key={m.moduleId}
              onClick={() => setSelectedModuleId(m.moduleId)}
              className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition ${
                selectedModuleId === m.moduleId
                  ? 'bg-suraksha-blue text-white shadow-subtle'
                  : 'text-suraksha-subtext hover:text-white'
              }`}
            >
              {m.moduleName}
            </button>
          ))}
        </div>
      </div>

      {/* Module KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {mockModules.map((m) => {
          const isSelected = selectedModuleId === 'ALL' || selectedModuleId === m.moduleId;
          return (
            <div
              key={m.moduleId}
              onClick={() => setSelectedModuleId(m.moduleId)}
              className={`cursor-pointer rounded-2xl border p-5 transition-all duration-200 ${
                isSelected
                  ? 'border-suraksha-amber bg-suraksha-card shadow-card'
                  : 'border-suraksha-border bg-suraksha-card/50 opacity-60 hover:opacity-100'
              }`}
            >
              <div className="flex items-center justify-between pb-3 border-b border-suraksha-border">
                <div className="flex items-center gap-3">
                  <div className="p-2.5 rounded-xl bg-amber-500/10 text-suraksha-amber border border-amber-500/20">
                    {m.moduleId === 'm-fire' && <Flame className="w-5 h-5" />}
                    {m.moduleId === 'm-gas' && <Wind className="w-5 h-5" />}
                    {m.moduleId === 'm-mach' && <Cog className="w-5 h-5" />}
                  </div>
                  <div>
                    <h4 className="text-sm font-bold text-white">{m.moduleName}</h4>
                    <p className="text-[10px] text-suraksha-subtext">{m.totalEnrolled.toLocaleString()} Workers Enrolled</p>
                  </div>
                </div>
                <span className="text-sm font-bold text-emerald-400">{m.passRate}% Pass</span>
              </div>

              <div className="grid grid-cols-2 gap-3 pt-4 text-xs">
                <div>
                  <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Avg Score</span>
                  <span className="font-bold text-white">{m.averageScore} / 100</span>
                </div>
                <div>
                  <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Certified</span>
                  <span className="font-bold text-suraksha-amber">{m.certifiedCount.toLocaleString()}</span>
                </div>
                <div>
                  <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Completion</span>
                  <span className="font-bold text-emerald-400">
                    {Math.round((m.completedCount / m.totalEnrolled) * 100)}%
                  </span>
                </div>
                <div>
                  <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Critical Errors</span>
                  <span className="font-bold text-rose-400">{m.criticalErrorCount} logs</span>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Main Charts Area */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Chart 1: Average Score Monthly Trend Line Chart */}
        <ChartCard
          title="Average Score Trend (6 Months)"
          subtitle="Monthly progression across Fire, Gas and Machinery safety evaluations"
        >
          <div className="h-72 w-full pt-2">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={combinedTrendData} margin={{ top: 10, right: 10, left: -10, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#342821" opacity={0.6} />
                <XAxis dataKey="month" stroke="#A8998C" fontSize={11} tickLine={false} />
                <YAxis stroke="#A8998C" fontSize={11} domain={[70, 100]} tickLine={false} />
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
                <Line type="monotone" dataKey="Fire" stroke="#E5484D" strokeWidth={2} dot={{ r: 4 }} />
                <Line type="monotone" dataKey="Gas" stroke="#E88024" strokeWidth={2} dot={{ r: 4 }} />
                <Line type="monotone" dataKey="Machinery" stroke="#E87722" strokeWidth={2} dot={{ r: 4 }} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </ChartCard>

        {/* Chart 2: Scenario Performance & Critical Errors Bar Chart */}
        <ChartCard
          title={selectedModule ? `${selectedModule.moduleName} — AR Scenarios` : 'AR Scenarios Pass Rates'}
          subtitle="Pass rates and critical error counts for specific AR simulation scenarios"
        >
          <div className="h-72 w-full pt-2">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={(selectedModule || mockModules[0]).scenarios}
                layout="vertical"
                margin={{ top: 5, right: 20, left: 40, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#1E3A5F" opacity={0.5} />
                <XAxis type="number" domain={[0, 100]} stroke="#94A3B8" fontSize={11} />
                <YAxis dataKey="name" type="category" stroke="#94A3B8" fontSize={10} width={130} tickLine={false} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#101C2E',
                    borderColor: '#1E3A5F',
                    borderRadius: '8px',
                    color: '#FFF',
                    fontSize: '12px',
                  }}
                />
                <Bar dataKey="passRate" fill="#10B981" radius={[0, 4, 4, 0]} name="Pass Rate (%)" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </ChartCard>
      </div>

      {/* Sector & Completion Time Analytics Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartCard
          title="Performance by Sector"
          subtitle="Average assessment scores across industrial zones and modules"
        >
          <div className="h-72 w-full pt-2">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={[
                { sector: 'Bokaro Steel', Fire: 88, Gas: 84, Machinery: 87 },
                { sector: 'Dhanbad Coal', Fire: 82, Gas: 71, Machinery: 76 },
                { sector: 'Jamshedpur', Fire: 92, Gas: 86, Machinery: 91 },
                { sector: 'Ranchi H.E.', Fire: 84, Gas: 77, Machinery: 80 },
                { sector: 'Ramgarh Chem', Fire: 87, Gas: 82, Machinery: 78 },
              ]} margin={{ top: 10, right: 10, left: -10, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#1E3A5F" opacity={0.5} />
                <XAxis dataKey="sector" stroke="#94A3B8" fontSize={10} tickLine={false} />
                <YAxis stroke="#94A3B8" fontSize={11} domain={[0, 100]} tickLine={false} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#101C2E',
                    borderColor: '#1E3A5F',
                    borderRadius: '8px',
                    color: '#FFF',
                    fontSize: '12px',
                  }}
                />
                <Legend wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }} />
                <Bar dataKey="Fire" fill="#EF4444" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Gas" fill="#F59E0B" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Machinery" fill="#1D6BF3" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </ChartCard>

        <ChartCard
          title="Completion Time Distribution"
          subtitle="Share of AR simulation runs by completion duration"
        >
          <div className="h-72 w-full pt-2">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={[
                { bucket: '< 3 min', share: 17 },
                { bucket: '3–4 min', share: 33 },
                { bucket: '4–5 min', share: 29 },
                { bucket: '5–6 min', share: 13 },
                { bucket: '6–8 min', share: 6 },
                { bucket: '> 8 min', share: 2 },
              ]} margin={{ top: 10, right: 10, left: -10, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#1E3A5F" opacity={0.5} />
                <XAxis dataKey="bucket" stroke="#94A3B8" fontSize={10} tickLine={false} />
                <YAxis stroke="#94A3B8" fontSize={11} tickLine={false} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#101C2E',
                    borderColor: '#1E3A5F',
                    borderRadius: '8px',
                    color: '#FFF',
                    fontSize: '12px',
                  }}
                  formatter={(value) => [`${value ?? 0}%`, 'Runs Share']}
                />
                <Bar dataKey="share" fill="#1D6BF3" radius={[4, 4, 0, 0]} name="Share of Runs (%)" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </ChartCard>
      </div>

      {/* Scenario Telemetry Detail List */}
      <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
        <h4 className="text-sm font-bold uppercase tracking-wider text-white mb-4">
          AR Scenario Breakdown & Attempt Metrics
        </h4>

        <div className="space-y-3">
          {(selectedModule ? selectedModule.scenarios : mockModules.flatMap((m) => m.scenarios)).map(
            (sc, idx) => (
              <div
                key={idx}
                className="flex flex-wrap items-center justify-between gap-3 p-4 rounded-xl border border-suraksha-border bg-suraksha-surface/60"
              >
                <div className="flex items-start gap-3">
                  <div className="p-2 rounded-lg bg-suraksha-card border border-suraksha-border font-bold text-xs text-suraksha-amber shrink-0">
                    #{idx + 1}
                  </div>
                  <div>
                    <h5 className="text-xs font-bold text-white">{sc.name}</h5>
                    <p className="text-[11px] text-suraksha-subtext mt-0.5">
                      {sc.attempts.toLocaleString()} total simulation runs conducted
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-6 text-xs">
                  <div className="text-right">
                    <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Pass Rate</span>
                    <span className="font-bold text-emerald-400">{sc.passRate}%</span>
                  </div>
                  <div className="text-right">
                    <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Avg Score</span>
                    <span className="font-bold text-white">{sc.avgScore} / 100</span>
                  </div>
                  <div className="text-right">
                    <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Critical Errors</span>
                    <span className="font-bold text-rose-400">{sc.criticalErrors} logs</span>
                  </div>
                </div>
              </div>
            )
          )}
        </div>
      </div>
    </div>
  );
};
