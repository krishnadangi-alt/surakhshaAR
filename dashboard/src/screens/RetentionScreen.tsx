import React, { useState } from 'react';
import { ChartCard } from '../components/common/ChartCard';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { mockRetentionRecords, mockRetentionCurveData } from '../mockData';
import type { RetentionRecord } from '../types';
import { CheckCircle2, Clock, ShieldCheck, ClipboardCheck } from 'lucide-react';
import { addDaysIso } from '../utils/dateRange';
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from 'recharts';

export const RetentionScreen: React.FC = () => {
  const [records, setRecords] = useState<RetentionRecord[]>(mockRetentionRecords);

  const getNextDueInfo = (r: RetentionRecord) => {
    if (r.day7Status !== 'Completed') return { stage: 'Day 7', due: addDaysIso(r.lastTrainingDate, 7) };
    if (r.day30Status !== 'Completed') return { stage: 'Day 30', due: addDaysIso(r.lastTrainingDate, 30) };
    return { stage: 'Cleared', due: '—' };
  };

  const logAudit = (workerId: string) => {
    setRecords((prev) =>
      prev.map((r) => {
        if (r.id !== workerId) return r;
        if (r.day7Status !== 'Completed') {
          return { ...r, day7Status: 'Completed' as const, day7Score: 90, auditCleared: true };
        }
        if (r.day30Status !== 'Completed') {
          return { ...r, day30Status: 'Completed' as const, day30Score: 88, auditCleared: true };
        }
        return r;
      }),
    );
  };

  const columns: Column<RetentionRecord>[] = [
    {
      key: 'workerName',
      header: 'Worker',
      sortable: true,
      render: (r) => (
        <div>
          <h5 className="font-bold text-suraksha-heading hover:text-suraksha-amber transition">{r.workerName}</h5>
          <p className="text-[10px] text-suraksha-subtext font-medium">{r.employeeId} • {r.sector}</p>
        </div>
      ),
    },
    {
      key: 'moduleName',
      header: 'Module',
      sortable: true,
      render: (r) => <span className="text-xs font-semibold text-suraksha-heading">{r.moduleName}</span>,
    },
    {
      key: 'lastTrainingDate',
      header: 'Initial Training Date',
      sortable: true,
      render: (r) => <span className="text-xs text-suraksha-subtext font-mono font-medium">{r.lastTrainingDate}</span>,
    },
    {
      key: 'day1Status',
      header: 'Day 1 Recall Audit',
      align: 'center',
      render: (r) => (
        <div className="flex flex-col items-center gap-0.5">
          <StatusBadge status={r.day1Status} size="sm" />
          {r.day1Score && <span className="text-[10px] font-bold text-emerald-700">{r.day1Score}%</span>}
        </div>
      ),
    },
    {
      key: 'day7Status',
      header: 'Day 7 Shift Audit',
      align: 'center',
      render: (r) => (
        <div className="flex flex-col items-center gap-0.5">
          <StatusBadge status={r.day7Status} size="sm" />
          {r.day7Score ? (
            <span className="text-[10px] font-bold text-emerald-700">{r.day7Score}%</span>
          ) : (
            <span className="text-[10px] text-suraksha-subtext font-medium">—</span>
          )}
        </div>
      ),
    },
    {
      key: 'day30Status',
      header: 'Day 30 Compliance Check',
      align: 'center',
      render: (r) => (
        <div className="flex flex-col items-center gap-0.5">
          <StatusBadge status={r.day30Status} size="sm" />
          {r.day30Score ? (
            <span className="text-[10px] font-bold text-emerald-700">{r.day30Score}%</span>
          ) : (
            <span className="text-[10px] text-suraksha-subtext font-medium">—</span>
          )}
        </div>
      ),
    },
    {
      key: 'nextDue',
      header: 'Retention Stage / Due',
      align: 'center',
      render: (r) => {
        const next = getNextDueInfo(r);
        return (
          <div className="text-center">
            <span className="text-[11px] font-bold text-suraksha-heading block">{next.stage}</span>
            <span className="text-[10px] text-suraksha-subtext font-mono font-medium">{next.due}</span>
          </div>
        );
      },
    },
    {
      key: 'auditCleared',
      header: 'Protocol Status',
      align: 'center',
      render: (r) =>
        r.auditCleared ? (
          <span className="inline-flex items-center gap-1 text-xs font-bold text-emerald-700 bg-emerald-50 px-2.5 py-1 rounded-md border border-emerald-200">
            <CheckCircle2 className="w-3.5 h-3.5" /> Cleared
          </span>
        ) : (
          <span className="inline-flex items-center gap-1 text-xs font-bold text-amber-700 bg-amber-50 px-2.5 py-1 rounded-md border border-amber-200">
            <Clock className="w-3.5 h-3.5" /> In Evaluation
          </span>
        ),
    },
    {
      key: 'logAudit',
      header: 'Audit Action',
      align: 'center',
      render: (r) => {
        const allDone = r.day7Status === 'Completed' && r.day30Status === 'Completed';
        return (
          <button
            disabled={allDone}
            onClick={(e) => {
              e.stopPropagation();
              logAudit(r.id);
            }}
            className="inline-flex items-center gap-1.5 rounded-lg border border-suraksha-blue/30 bg-suraksha-blue/10 px-2.5 py-1 text-[11px] font-bold text-suraksha-blue hover:bg-suraksha-blue/20 transition disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-suraksha-blue/10"
            title={allDone ? 'All retention stages completed' : 'Simulate logging the next scheduled audit'}
          >
            <ClipboardCheck className="w-3.5 h-3.5" />
            {allDone ? 'Completed' : 'Log Audit'}
          </button>
        );
      },
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h3 className="text-lg font-bold text-suraksha-heading uppercase tracking-wider">
          Knowledge Retention Monitoring (Ebbinghaus Protocol)
        </h3>
        <p className="text-xs text-suraksha-subtext font-medium">
          Track post-training safety recall decay across Day 1, Day 7, and Day 30 workplace audits.
        </p>
      </div>

      {/* Mock state note */}
      <div className="flex items-start gap-2 rounded-xl border border-amber-500/30 bg-amber-500/10 p-3 text-[11px] font-medium text-amber-800">
        <ShieldCheck className="w-4 h-4 text-suraksha-amber shrink-0 mt-0.5" />
        <span>
          Retention audits shown here are simulation previews. &quot;Log Audit&quot; updates local demo state only and is
          not connected to a compliance system.
        </span>
      </div>

      {/* Primary Section Cards: Day 1, Day 7, Day 30 */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
          <div className="flex items-center justify-between pb-3 border-b border-suraksha-border text-emerald-700 mb-3">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-5 h-5" />
              <h4 className="text-xs font-bold uppercase tracking-wider text-suraksha-heading">Day 1 Audit (Initial)</h4>
            </div>
            <span className="text-xs font-extrabold text-emerald-700">96.2% Retention</span>
          </div>
          <div className="grid grid-cols-2 gap-2 text-center text-xs text-suraksha-subtext">
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Target</span>
              <span className="font-bold text-suraksha-blue text-sm">92%</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Actual</span>
              <span className="font-bold text-emerald-700 text-sm">96.2%</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Completed</span>
              <span className="font-bold text-emerald-700 text-sm">14,120</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Pending</span>
              <span className="font-bold text-amber-700 text-sm">160</span>
            </div>
          </div>
        </div>

        <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
          <div className="flex items-center justify-between pb-3 border-b border-suraksha-border text-suraksha-amber mb-3">
            <div className="flex items-center gap-2">
              <Clock className="w-5 h-5" />
              <h4 className="text-xs font-bold uppercase tracking-wider text-suraksha-heading">Day 7 Audit (1-Week)</h4>
            </div>
            <span className="text-xs font-extrabold text-suraksha-amber">88.4% Retention</span>
          </div>
          <div className="grid grid-cols-2 gap-2 text-center text-xs text-suraksha-subtext">
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Target</span>
              <span className="font-bold text-suraksha-blue text-sm">82%</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Actual</span>
              <span className="font-bold text-amber-700 text-sm">88.4%</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Completed</span>
              <span className="font-bold text-emerald-700 text-sm">11,940</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Pending</span>
              <span className="font-bold text-amber-700 text-sm">1,820</span>
            </div>
          </div>
        </div>

        <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
          <div className="flex items-center justify-between pb-3 border-b border-suraksha-border text-suraksha-blue mb-3">
            <div className="flex items-center gap-2">
              <ShieldCheck className="w-5 h-5" />
              <h4 className="text-xs font-bold uppercase tracking-wider text-suraksha-heading">Day 30 Audit (1-Month)</h4>
            </div>
            <span className="text-xs font-extrabold text-suraksha-blue">82.1% Retention</span>
          </div>
          <div className="grid grid-cols-2 gap-2 text-center text-xs text-suraksha-subtext">
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Target</span>
              <span className="font-bold text-suraksha-blue text-sm">70%</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Actual</span>
              <span className="font-bold text-suraksha-blue text-sm">82.1%</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Completed</span>
              <span className="font-bold text-emerald-700 text-sm">9,850</span>
            </div>
            <div className="p-2 rounded-lg bg-suraksha-surface">
              <span className="text-[10px] block font-bold text-suraksha-subtext">Pending</span>
              <span className="font-bold text-amber-700 text-sm">3,410</span>
            </div>
          </div>
        </div>
      </div>

      {/* Retention Curve Recharts Visualization */}
      <ChartCard
        title="Ebbinghaus Memory Retention Curve vs Target Benchmark"
        subtitle="Comparing actual Jharkhand workforce memory retention curve against target safety threshold"
      >
        <div className="h-72 w-full pt-2">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={mockRetentionCurveData} margin={{ top: 10, right: 10, left: -10, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#E2E8F0" opacity={0.8} />
              <XAxis dataKey="day" stroke="#64748B" fontSize={11} tickLine={false} />
              <YAxis stroke="#64748B" fontSize={11} domain={[60, 100]} tickLine={false} />
              <Tooltip
                contentStyle={{
                  backgroundColor: '#FFFFFF',
                  borderColor: '#CBD5E1',
                  borderRadius: '8px',
                  color: '#0F172A',
                  fontSize: '12px',
                  boxShadow: '0 4px 12px rgba(15, 23, 42, 0.1)',
                }}
              />
              <Legend wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }} />
              <Line
                type="monotone"
                dataKey="actualRetention"
                stroke="#059669"
                strokeWidth={3}
                dot={{ r: 5 }}
                name="Actual Workforce Retention (%)"
              />
              <Line
                type="monotone"
                dataKey="expectedRetention"
                stroke="#D97706"
                strokeWidth={2}
                strokeDasharray="5 5"
                dot={{ r: 3 }}
                name="Target Compliance Threshold (%)"
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </ChartCard>

      {/* Retention Audit Queue Table */}
      <div className="space-y-3">
        <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading">
          Active Retention Audit Queue
        </h4>
        <DataTable columns={columns} data={records} pageSize={5} />
      </div>
    </div>
  );
};
