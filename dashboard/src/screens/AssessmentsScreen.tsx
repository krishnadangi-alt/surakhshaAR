import React, { useState } from 'react';
import { FilterBar } from '../components/common/FilterBar';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { AssessmentDetailModal } from '../components/modals/AssessmentDetailModal';
import { DateRangePicker } from '../components/common/DateRangePicker';
import { mockAssessments } from '../mockData';
import type { Assessment, DateRange, DateRangePreset } from '../types';
import { AlertOctagon, Eye, Download } from 'lucide-react';
import { filterAssessmentsByRange } from '../utils/dateRange';
import { downloadCsv } from '../utils/export';

export const AssessmentsScreen: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [moduleFilter, setModuleFilter] = useState('ALL');
  const [resultFilter, setResultFilter] = useState('ALL');
  const [criticalFilter, setCriticalFilter] = useState('ALL');
  const [dateRangePreset, setDateRangePreset] = useState<DateRangePreset>('Last 30 Days');
  const [customRange, setCustomRange] = useState<DateRange | null>(null);
  const [selectedAssessment, setSelectedAssessment] = useState<Assessment | null>(null);

  const dateFiltered = filterAssessmentsByRange(mockAssessments, dateRangePreset, customRange ?? undefined);

  // Filtered dataset
  const filteredAssessments = dateFiltered.filter((a) => {
    const matchesSearch =
      a.workerName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      a.employeeId.toLowerCase().includes(searchQuery.toLowerCase()) ||
      a.scenarioName.toLowerCase().includes(searchQuery.toLowerCase());

    const matchesModule = moduleFilter === 'ALL' || a.moduleId === moduleFilter;
    const matchesResult = resultFilter === 'ALL' || a.passFail === resultFilter;
    const matchesCritical =
      criticalFilter === 'ALL' ||
      (criticalFilter === 'YES' && a.criticalErrors > 0) ||
      (criticalFilter === 'NO' && a.criticalErrors === 0);

    return matchesSearch && matchesModule && matchesResult && matchesCritical;
  });

  const handleExportCsv = () => {
    downloadCsv(
      'surakshaar-assessment-logs',
      ['Worker', 'Employee ID', 'Module', 'Scenario', 'Score %', 'Correct', 'Wrong', 'Critical Errors', 'Pass/Fail', 'Duration', 'Date & Time'],
      filteredAssessments.map((a) => [
        a.workerName,
        a.employeeId,
        a.moduleName,
        a.scenarioName,
        a.score,
        a.correctActions,
        a.wrongActions,
        a.criticalErrors,
        a.passFail,
        a.duration,
        a.dateTime,
      ]),
    );
  };

  const columns: Column<Assessment>[] = [
    {
      key: 'workerName',
      header: 'Worker',
      sortable: true,
      render: (a) => (
        <div>
          <h5 className="font-bold text-white hover:text-suraksha-amber transition">{a.workerName}</h5>
          <p className="text-[10px] font-mono text-suraksha-subtext">{a.employeeId}</p>
        </div>
      ),
    },
    {
      key: 'moduleName',
      header: 'Module',
      sortable: true,
      render: (a) => <span className="text-xs font-semibold text-suraksha-text">{a.moduleName}</span>,
    },
    {
      key: 'scenarioName',
      header: 'AR Scenario',
      sortable: true,
      render: (a) => <span className="text-xs font-medium text-white truncate max-w-[220px] block">{a.scenarioName}</span>,
    },
    {
      key: 'score',
      header: 'Score',
      align: 'center',
      sortable: true,
      render: (a) => (
        <span
          className={`font-bold ${
            a.score >= 85 ? 'text-emerald-400' : a.score >= 70 ? 'text-amber-400' : 'text-rose-400'
          }`}
        >
          {a.score}%
        </span>
      ),
    },
    {
      key: 'actionsRatio',
      header: 'Actions (C/W)',
      align: 'center',
      render: (a) => (
        <span className="text-xs font-semibold">
          <span className="text-emerald-400">{a.correctActions}</span> /{' '}
          <span className="text-rose-400">{a.wrongActions}</span>
        </span>
      ),
    },
    {
      key: 'criticalErrors',
      header: 'Critical Breach',
      align: 'center',
      render: (a) =>
        a.criticalErrors > 0 ? (
          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold bg-rose-500/10 text-rose-400 border border-rose-500/30">
            <AlertOctagon className="w-3 h-3" /> {a.criticalErrors} ERR
          </span>
        ) : (
          <span className="text-[10px] text-suraksha-subtext">None</span>
        ),
    },
    {
      key: 'passFail',
      header: 'Result',
      sortable: true,
      render: (a) => <StatusBadge status={a.passFail} size="sm" />,
    },
    {
      key: 'duration',
      header: 'Duration',
      render: (a) => <span className="text-[11px] text-suraksha-subtext">{a.duration}</span>,
    },
    {
      key: 'dateTime',
      header: 'Date & Time',
      sortable: true,
      render: (a) => <span className="text-[10px] text-suraksha-subtext">{a.dateTime}</span>,
    },
    {
      key: 'actions',
      header: 'Audit',
      align: 'center',
      render: (a) => (
        <button
          onClick={(e) => {
            e.stopPropagation();
            setSelectedAssessment(a);
          }}
          className="p-1.5 rounded-lg border border-suraksha-border text-suraksha-amber hover:bg-suraksha-surface transition"
          title="Inspect Telemetry Log"
        >
          <Eye className="w-4 h-4" />
        </button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* Top Header */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h3 className="text-lg font-bold text-white uppercase tracking-wider">Assessment Logs & Telemetry</h3>
          <p className="text-xs text-suraksha-subtext">
            Audit AR simulation assessment runs, step telemetry, and critical safety error flags.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <DateRangePicker
            selected={dateRangePreset}
            onChange={setDateRangePreset}
            customRange={customRange ?? undefined}
            onCustomRangeChange={setCustomRange}
          />
          <button
            onClick={handleExportCsv}
            className="flex items-center gap-1.5 rounded-lg border border-suraksha-border bg-suraksha-card px-3.5 py-2 text-xs font-semibold text-suraksha-subtext hover:bg-suraksha-hover hover:text-white transition"
          >
            <Download className="w-3.5 h-3.5" />
            <span>Export CSV</span>
          </button>
        </div>
      </div>

      {/* Summary KPI Strip */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Pass Rate</p>
          <p className="text-2xl font-black text-emerald-400 mt-1">87.4%</p>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Average Score</p>
          <p className="text-2xl font-black text-suraksha-amber mt-1">88.2 / 100</p>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Total Assessments</p>
          <p className="text-2xl font-black text-white mt-1">38,420</p>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Critical Errors Recorded</p>
          <p className="text-2xl font-black text-rose-400 mt-1">322 Logs</p>
        </div>
      </div>

      {/* Filter Bar */}
      <FilterBar
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Filter by Worker, Employee ID, Scenario..."
        filters={[
          {
            key: 'module',
            label: 'Module',
            value: moduleFilter,
            onChange: setModuleFilter,
            options: [
              { label: 'Fire Safety', value: 'm-fire' },
              { label: 'Gas Safety', value: 'm-gas' },
              { label: 'Machinery Safety', value: 'm-mach' },
            ],
          },
          {
            key: 'result',
            label: 'Result',
            value: resultFilter,
            onChange: setResultFilter,
            options: [
              { label: 'Pass', value: 'Pass' },
              { label: 'Fail', value: 'Fail' },
            ],
          },
          {
            key: 'critical',
            label: 'Critical Error',
            value: criticalFilter,
            onChange: setCriticalFilter,
            options: [
              { label: 'Has Critical Error', value: 'YES' },
              { label: 'No Critical Error', value: 'NO' },
            ],
          },
        ]}
        onReset={() => {
          setSearchQuery('');
          setModuleFilter('ALL');
          setResultFilter('ALL');
          setCriticalFilter('ALL');
        }}
      />

      {/* Assessment Data Table */}
      <DataTable
        columns={columns}
        data={filteredAssessments}
        pageSize={7}
        onRowClick={(a) => setSelectedAssessment(a)}
        emptyMessage="No assessment logs found matching filter criteria."
      />

      {/* Inspection Modal */}
      <AssessmentDetailModal
        assessment={selectedAssessment}
        isOpen={!!selectedAssessment}
        onClose={() => setSelectedAssessment(null)}
      />
    </div>
  );
};
