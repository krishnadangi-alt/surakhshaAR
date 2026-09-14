import React, { useState } from 'react';
import { FileSpreadsheet, Download, FileText, Filter, CheckCircle2, RefreshCw } from 'lucide-react';
import { DateRangePicker } from '../components/common/DateRangePicker';
import { mockWorkers, mockAssessments, mockCertificates, mockRetrainingRecords, mockRetentionRecords, getWorkerCertificateStatus } from '../mockData';
import type { DateRange, DateRangePreset } from '../types';
import { filterAssessmentsByRange } from '../utils/dateRange';
import { downloadCsv } from '../utils/export';

const REPORT_TYPES = [
  'Workers Directory',
  'Assessment Performance',
  'Competency Breakdown',
  'Retraining Audit',
  'Certificates Register',
  'Retention Log',
];



export const ReportsScreen: React.FC = () => {
  const [reportType, setReportType] = useState(REPORT_TYPES[0]);
  const [selectedDateRange, setSelectedDateRange] = useState<DateRangePreset>('Last 30 Days');
  const [customRange, setCustomRange] = useState<DateRange | null>(null);
  const [sector, setSector] = useState('ALL');
  const [workerFilter, setWorkerFilter] = useState('ALL');
  const [moduleFilter, setModuleFilter] = useState('ALL');
  const [passFailFilter, setPassFailFilter] = useState('ALL');
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [competencyFilter, setCompetencyFilter] = useState('ALL');
  const [retrainingFilter, setRetrainingFilter] = useState('ALL');
  const [certFilter, setCertFilter] = useState('ALL');
  const [retentionFilter, setRetentionFilter] = useState('ALL');
  const [isGenerating, setIsGenerating] = useState(false);
  const [generatedSuccess, setGeneratedSuccess] = useState(false);
  
    

  // Date + filter scoped datasets (mock preview only).
  const filteredWorkers = mockWorkers.filter(
    (w) =>
      (sector === 'ALL' || w.sector === sector) &&
      (moduleFilter === 'ALL' || w.moduleProgressList.some((m) => m.moduleId === moduleFilter)) &&
      (statusFilter === 'ALL' || w.overallStatus === statusFilter) &&
      (competencyFilter === 'ALL' || w.overallCompetency === competencyFilter) &&
      (retrainingFilter === 'ALL' || w.retrainingStatus === retrainingFilter) &&
      (certFilter === 'ALL' || getWorkerCertificateStatus(w.id) === certFilter) &&
      (workerFilter === 'ALL' || w.name === workerFilter || w.employeeId === workerFilter),
  );

  const filteredAssessments = filterAssessmentsByRange(mockAssessments, selectedDateRange, customRange ?? undefined).filter(
    (a) => (moduleFilter === 'ALL' || a.moduleId === moduleFilter) && (passFailFilter === 'ALL' || a.passFail === passFailFilter),
  );

  const filteredCerts = mockCertificates.filter(
    (c) => (certFilter === 'ALL' || c.status === certFilter) && (sector === 'ALL' || c.sector === sector),
  );

  const filteredRetraining = mockRetrainingRecords.filter((r) => retrainingFilter === 'ALL' || r.status === retrainingFilter);

  const filteredRetention = mockRetentionRecords.filter((r) => {
    if (retentionFilter === 'ALL') return true;
    const cleared = r.day1Status === 'Completed' && r.day7Status === 'Completed' && r.day30Status === 'Completed';
    return retentionFilter === 'Completed' ? cleared : !cleared;
  });

  const getRecordCount = () => {
    switch (reportType) {
      case 'Workers Directory':
      case 'Competency Breakdown':
        return filteredWorkers.length;
      case 'Assessment Performance':
        return filteredAssessments.length;
      case 'Retraining Audit':
        return filteredRetraining.length;
      case 'Certificates Register':
        return filteredCerts.length;
      case 'Retention Log':
        return filteredRetention.length;
      default:
        return 0;
    }
  };
const handleGenerate = (e: React.FormEvent) => {
    e.preventDefault();
    if (isGenerating) return;
    setIsGenerating(true);
    setGeneratedSuccess(false);
   setTimeout(() => {
  setIsGenerating(false);
  setGeneratedSuccess(true);
}, 800);
  };

  const handleExportCsv = () => {
    switch (reportType) {
      case 'Workers Directory':
      case 'Competency Breakdown':
        downloadCsv(
          reportType.toLowerCase().replace(/\s+/g, '-'),
          ['Worker Name', 'Employee ID', 'Sector', 'Latest Score %', 'Competency', 'Status'],
          filteredWorkers.map((w) => [w.name, w.employeeId, w.sector, w.latestScore, w.overallCompetency, w.overallStatus]),
        );
        break;
      case 'Assessment Performance':
        downloadCsv(
          'assessment-performance',
          ['Worker', 'Employee ID', 'Module', 'Scenario', 'Score %', 'Pass/Fail', 'Date'],
          filteredAssessments.map((a) => [a.workerName, a.employeeId, a.moduleName, a.scenarioName, a.score, a.passFail, a.dateTime]),
        );
        break;
      case 'Retraining Audit':
        downloadCsv(
          'retraining-audit',
          ['Worker', 'Employee ID', 'Module', 'Weakness', 'Status', 'Initial Score %'],
          filteredRetraining.map((r) => [r.workerName, r.employeeId, r.moduleName, r.weakArea, r.status, r.initialScore]),
        );
        break;
      case 'Certificates Register':
        downloadCsv(
          'certificates-register',
          ['Worker', 'Employee ID', 'Certificate ID', 'Module', 'Grade', 'Status', 'Expiry'],
          filteredCerts.map((c) => [c.workerName, c.employeeId, c.certificateId, c.moduleName, c.resultGrade, c.status, c.expiryDate]),
        );
        break;
      case 'Retention Log':
        downloadCsv(
          'retention-log',
          ['Worker', 'Employee ID', 'Module', 'Day 1', 'Day 7', 'Day 30', 'Cleared'],
          filteredRetention.map((r) => [r.workerName, r.employeeId, r.moduleName, r.day1Status, r.day7Status, r.day30Status, r.auditCleared ? 'Yes' : 'No']),
        );
        break;
    }
  };
    

return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h3 className="text-lg font-bold text-white uppercase tracking-wider">
          Compliance Reports & Export Center
        </h3>
        <p className="text-xs text-suraksha-subtext">
          Configure granular filters and generate CSV/PDF demo report previews for state administration.
        </p>
      </div>

      {/* Main Form & Configurator Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Report Generator Configurator Panel (2 cols wide) */}
        <div className="lg:col-span-2 rounded-2xl border border-suraksha-border bg-suraksha-card p-6 shadow-card">
          <div className="flex items-center justify-between pb-4 border-b border-suraksha-border mb-6">
            <div className="flex items-center gap-3">
              <div className="p-2.5 rounded-xl bg-blue-500/10 text-suraksha-blue border border-blue-500/20">
                <FileSpreadsheet className="w-5 h-5" />
              </div>
              <div>
                <h4 className="text-sm font-bold text-white uppercase tracking-wider">Report Configurator</h4>
                <p className="text-xs text-suraksha-subtext">Select report category & dataset scope</p>
              </div>
            </div>
            <DateRangePicker
              selected={selectedDateRange}
              onChange={setSelectedDateRange}
              customRange={customRange ?? undefined}
              onCustomRangeChange={setCustomRange}
            />
          </div>

          <form onSubmit={handleGenerate} className="space-y-5">
            {/* Report Type Selector Buttons */}
            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-suraksha-subtext mb-2">
                1. Select Report Type
              </label>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                {REPORT_TYPES.map((type) => (
                  <button
                    type="button"
                    key={type}
                    onClick={() => setReportType(type)}
                    className={`p-3 rounded-xl border text-xs font-bold transition text-left ${
                      reportType === type
                        ? 'border-suraksha-amber bg-amber-500/10 text-suraksha-amber shadow-subtle'
                        : 'border-suraksha-border bg-suraksha-surface/60 text-suraksha-subtext hover:bg-suraksha-hover hover:text-white'
                    }`}
                  >
                    {type}
                  </button>
                ))}
              </div>
            </div>
{/* Granular Filter Matrix */}
            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-suraksha-subtext mb-2">
                2. Apply Dataset Filter Parameters
              </label>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Worker</label>
                  <select
                    value={workerFilter}
                    onChange={(e) => setWorkerFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All Workers</option>
                    {mockWorkers.map((w) => (
                      <option key={w.id} value={w.name}>
                        {w.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Industrial Unit</label>
                  <select
                    value={sector}
                    onChange={(e) => setSector(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All Industrial Units</option>
                    <option value="Bokaro Steel Plant">Bokaro Steel Plant</option>
                    <option value="Dhanbad Coal Fields">Dhanbad Coal Fields</option>
                    <option value="Jamshedpur Metallurgy">Jamshedpur Metallurgy</option>
                    <option value="Ranchi Heavy Electricals">Ranchi Heavy Electricals</option>
                    <option value="Ramgarh Chemical Works">Ramgarh Chemical Works</option>
                  </select>
                </div>

                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Safety Module</label>
                  <select
                    value={moduleFilter}
                    onChange={(e) => setModuleFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All Modules</option>
                    <option value="m-fire">Fire Safety</option>
                    <option value="m-gas">Gas Safety</option>
                    <option value="m-mach">Machinery Safety</option>
                  </select>
                </div>

                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Pass / Fail</label>
                  <select
                    value={passFailFilter}
                    onChange={(e) => setPassFailFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All Results</option>
                    <option value="Pass">Pass</option>
                    <option value="Fail">Fail</option>
                  </select>
                </div>
<div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Competency</label>
                  <select
                    value={competencyFilter}
                    onChange={(e) => setCompetencyFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All Competency</option>
                    <option value="Strong">Strong</option>
                    <option value="Developing">Developing</option>
                    <option value="Needs Retraining">Needs Retraining</option>
                  </select>
                </div>

                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Retraining</label>
                  <select
                    value={retrainingFilter}
                    onChange={(e) => setRetrainingFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All States</option>
                    <option value="Recommended">Recommended</option>
                    <option value="Assigned">Assigned</option>
                    <option value="In Progress">In Progress</option>
                    <option value="Completed">Completed</option>
                    <option value="Reassessment Pending">Reassessment Pending</option>
                  </select>
                </div>

                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Certificate Status</label>
                  <select
                    value={certFilter}
                    onChange={(e) => setCertFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All Statuses</option>
                    <option value="Active">Active</option>
                    <option value="Expiring Soon">Expiring Soon</option>
                    <option value="Expired">Expired</option>
                    <option value="Pending">Pending</option>
                  </select>
                </div>

                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Retention Status</label>
                  <select
                    value={retentionFilter}
                    onChange={(e) => setRetentionFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All Stages</option>
                    <option value="Completed">Cleared</option>
                    <option value="Pending">In Evaluation</option>
                  </select>
                </div>

                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Overall Status</label>
                  <select
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
                  >
                    <option value="ALL">All Statuses</option>
                    <option value="Certified">Certified</option>
                    <option value="Passed">Passed</option>
                    <option value="In Training">In Training</option>
                    <option value="Retraining Required">Retraining Required</option>
                    <option value="Failed">Failed</option>
                  </select>
                </div>
              </div>
            </div>
{/* Active Parameters Banner */}
            <div className="p-3.5 rounded-xl bg-suraksha-surface/80 border border-suraksha-border flex items-center justify-between text-xs">
              <div className="flex items-center gap-2 text-suraksha-subtext">
                <Filter className="w-4 h-4 text-suraksha-amber" />
                <span>
                  Scope: <strong className="text-white">{reportType}</strong> | Date:{' '}
                  <strong className="text-white">{selectedDateRange}</strong> | Records:{' '}
                  <strong className="text-white">{getRecordCount()}</strong>
                </span>
              </div>
              {generatedSuccess && (
                <span className="flex items-center gap-1 font-bold text-emerald-400">
                  <CheckCircle2 className="w-4 h-4" /> Generated &amp; Ready
                </span>
              )}
            </div>

            {/* Action Buttons */}
            <div className="flex flex-wrap items-center justify-end gap-3 pt-4 border-t border-suraksha-border">
              <button
                type="submit"
                disabled={isGenerating}
                className="flex items-center gap-2 rounded-xl border border-suraksha-border bg-suraksha-card px-5 py-2.5 text-xs font-bold text-white hover:bg-suraksha-hover transition disabled:opacity-50"
              >
                <RefreshCw className="w-4 h-4 text-suraksha-amber" />
                <span>{isGenerating ? 'Generating...' : 'Generate Report'}</span>
              </button>

              <button
                type="button"
                onClick={handleExportCsv}
                className="flex items-center gap-2 rounded-xl bg-suraksha-surface border border-suraksha-border px-5 py-2.5 text-xs font-bold text-white hover:bg-suraksha-hover transition"
              >
                <Download className="w-4 h-4 text-suraksha-blue" />
                <span>Export CSV</span>
              </button>

              <button
                type="button"
                onClick={() => setGeneratedSuccess(true)}
                className="flex items-center gap-2 rounded-xl bg-suraksha-blue px-6 py-2.5 text-xs font-bold text-white hover:bg-suraksha-blueHover transition shadow-subtle"
              >
                <FileText className="w-4 h-4" />
                <span>PDF Preview (Simulated)</span>
              </button>
            </div>


          {/* End action buttons */}
        </form>
      </div>
    </div>
  </div>
);
};
export default ReportsScreen;