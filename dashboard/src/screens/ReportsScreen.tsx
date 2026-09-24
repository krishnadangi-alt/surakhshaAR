import React, { useState, useEffect } from 'react';
import { FileSpreadsheet, Download, FileText, Filter, CheckCircle2, RefreshCw } from 'lucide-react';
import { DateRangePicker } from '../components/common/DateRangePicker';
import { fetchDashboardWorkers, fetchDashboardAssessments, fetchDashboardCertificates } from '../services/api';
import type { Worker, Assessment, Certificate, RetrainingRecord, RetentionRecord, DateRange, DateRangePreset } from '../types';
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
  const [workers, setWorkers] = useState<Worker[]>([]);
  const [assessments, setAssessments] = useState<Assessment[]>([]);
  const [certificates, setCertificates] = useState<Certificate[]>([]);

  useEffect(() => {
    Promise.all([fetchDashboardWorkers(), fetchDashboardAssessments(), fetchDashboardCertificates()]).then(
      ([rawWorkers, rawAssessments, rawCerts]) => {
        if (rawAssessments) setAssessments(rawAssessments);
        if (rawCerts) setCertificates(rawCerts);
        if (rawWorkers) {
          const mapped: Worker[] = rawWorkers.map((w) => {
            const wAsmts = (rawAssessments || []).filter((a) => a.workerId === `w-${w.id}` || a.employeeId === w.employee_id);
            const latestAsmt = wAsmts[0];
            const latestScore = latestAsmt ? latestAsmt.score : (w.certified_modules.length > 0 ? 85 : 0);
            return {
              id: `w-${w.id}`,
              employeeId: w.employee_id,
              name: w.name,
              sector: 'Dhanbad Region-1',
              plant: 'Jharia Shaft Mine #4',
              role: w.role,
              email: `${w.name.toLowerCase().replace(/[^a-z0-9]/g, '.')}@mining.jh.gov.in`,
              phone: '+91 98765 43210',
              joinedDate: '2026-01-15',
              safetyOfficer: 'Inspector R. K. Soren',
              overallStatus: w.certified_modules.length > 0 ? 'Certified' : (latestAsmt ? (latestAsmt.passFail === 'Pass' ? 'Passed' : 'Failed') : 'In Training'),
              modulesCompleted: w.certified_modules.length,
              latestScore,
              overallCompetency: latestScore >= 75 ? 'Competent' : 'Needs Retraining',
              lastAssessmentDate: latestAsmt ? latestAsmt.dateTime : '2026-09-17',
              certificatesCount: w.certified_modules.length,
              retrainingStatus: latestScore < 75 ? 'Assigned' : 'Completed',
              moduleProgressList: [],
              weakAreas: [],
              retentionDay1: 'Completed',
              retentionDay7: 'Scheduled',
              retentionDay30: 'Scheduled',
            };
          });
          setWorkers(mapped);
        }
      }
    );
  }, []);

  const getWorkerCertStatus = (w: Worker) => (w.certificatesCount > 0 ? 'Active' : 'Pending');

  // Filter scoped live datasets
  const filteredWorkers = workers.filter(
    (w) =>
      (sector === 'ALL' || w.sector === sector) &&
      (statusFilter === 'ALL' || w.overallStatus === statusFilter) &&
      (competencyFilter === 'ALL' || w.overallCompetency === competencyFilter) &&
      (retrainingFilter === 'ALL' || w.retrainingStatus === retrainingFilter) &&
      (certFilter === 'ALL' || getWorkerCertStatus(w) === certFilter) &&
      (workerFilter === 'ALL' || w.name === workerFilter || w.employeeId === workerFilter),
  );

  const filteredAssessments = filterAssessmentsByRange(assessments, selectedDateRange, customRange ?? undefined).filter(
    (a) => (moduleFilter === 'ALL' || a.moduleId === moduleFilter) && (passFailFilter === 'ALL' || a.passFail === passFailFilter),
  );

  const filteredCerts = certificates.filter(
    (c) => (certFilter === 'ALL' || c.status === certFilter) && (sector === 'ALL' || c.sector === sector),
  );

  const liveRetraining: RetrainingRecord[] = assessments
    .filter((a) => a.passFail === 'Fail' || a.criticalErrors > 0)
    .map((a, idx) => ({
      id: `ret-${idx + 1}`,
      workerId: a.workerId,
      workerName: a.workerName,
      employeeId: a.employeeId,
      sector: 'Dhanbad Region-1',
      moduleId: a.moduleId,
      moduleName: a.moduleName,
      weakArea: a.criticalErrorDetails || 'SOP Compliance',
      recommendation: 'Targeted AR SOP Retraining',
      status: 'Recommended' as const,
      assignedDate: a.dateTime ? a.dateTime.slice(0, 10) : '',
      initialScore: a.score,
    }));

  const filteredRetraining = liveRetraining.filter((r) => retrainingFilter === 'ALL' || r.status === retrainingFilter);

  const liveRetention: RetentionRecord[] = workers.map((w) => ({
    id: w.id,
    workerId: w.id,
    workerName: w.name,
    employeeId: w.employeeId,
    sector: w.sector,
    moduleName: 'Fire & Explosion Response',
    lastTrainingDate: w.lastAssessmentDate || '2026-09-17',
    day1Status: 'Completed',
    day1Score: w.latestScore,
    day7Status: w.latestScore >= 80 ? 'Completed' : 'Scheduled',
    day7Score: w.latestScore >= 80 ? w.latestScore : undefined,
    day30Status: 'Scheduled',
    auditCleared: w.latestScore >= 80,
  }));

  const filteredRetention = liveRetention.filter((r) => {
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
        <h3 className="text-lg font-bold text-suraksha-heading uppercase tracking-wider">
          Compliance Reports & Export Center
        </h3>
        <p className="text-xs text-suraksha-subtext font-medium">
          Configure granular filters and generate CSV/PDF demo report previews for state administration.
        </p>
      </div>

      {/* Main Form & Configurator Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Report Generator Configurator Panel (2 cols wide) */}
        <div className="lg:col-span-2 rounded-2xl border border-suraksha-border bg-suraksha-card p-6 shadow-card">
          <div className="flex items-center justify-between pb-4 border-b border-suraksha-border mb-6">
            <div className="flex items-center gap-3">
              <div className="p-2.5 rounded-xl bg-blue-50 text-suraksha-blue border border-blue-200">
                <FileSpreadsheet className="w-5 h-5" />
              </div>
              <div>
                <h4 className="text-sm font-bold text-suraksha-heading uppercase tracking-wider">Report Configurator</h4>
                <p className="text-xs text-suraksha-subtext font-medium">Select report category & dataset scope</p>
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
                        : 'border-suraksha-border bg-suraksha-surface/60 text-suraksha-subtext hover:bg-suraksha-hover hover:text-suraksha-heading'
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
                    {workers.map((w) => (
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
                    <option value="Dhanbad Region-1">Dhanbad Region-1</option>
                    <option value="Dhanbad Region-2">Dhanbad Region-2</option>
                    <option value="Dhanbad Region-3">Dhanbad Region-3</option>
                    <option value="Koderma Region">Koderma Region</option>
                    <option value="Ranchi Region">Ranchi Region</option>
                    <option value="Chaibasa Region">Chaibasa Region</option>
                    <option value="Bhubaneswar Region-1">Bhubaneswar Region-1</option>
                    <option value="Bhubaneswar Region-2">Bhubaneswar Region-2</option>
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
                    <option value="m-fire">Fire &amp; Explosion Response</option>
                    <option value="m-gas">Gas Leak &amp; Confined Space</option>
                    <option value="m-mach">Machinery</option>
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
                    <option value="Competent">Competent</option>
                    <option value="Needs Retraining">Needs Retraining</option>
                  </select>
                </div>

                <div>
                  <label className="block text-[10px] text-suraksha-subtext uppercase font-bold mb-1">Retraining</label>
                  <select
                    value={retrainingFilter}
                    onChange={(e) => setRetrainingFilter(e.target.value)}
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-semibold text-suraksha-text focus:border-suraksha-blue focus:outline-none"
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
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-semibold text-suraksha-text focus:border-suraksha-blue focus:outline-none"
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
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-semibold text-suraksha-text focus:border-suraksha-blue focus:outline-none"
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
                    className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-semibold text-suraksha-text focus:border-suraksha-blue focus:outline-none"
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
              <div className="flex items-center gap-2 text-suraksha-subtext font-medium">
                <Filter className="w-4 h-4 text-suraksha-amber" />
                <span>
                  Scope: <strong className="text-suraksha-heading font-bold">{reportType}</strong> | Date:{' '}
                  <strong className="text-suraksha-heading font-bold">{selectedDateRange}</strong> | Records:{' '}
                  <strong className="text-suraksha-heading font-bold">{getRecordCount()}</strong>
                </span>
              </div>
              {generatedSuccess && (
                <span className="flex items-center gap-1 font-bold text-emerald-700">
                  <CheckCircle2 className="w-4 h-4" /> Generated &amp; Ready
                </span>
              )}
            </div>

            {/* Action Buttons */}
            <div className="flex flex-wrap items-center justify-end gap-3 pt-4 border-t border-suraksha-border">
              <button
                type="submit"
                disabled={isGenerating}
                className="flex items-center gap-2 rounded-xl border border-suraksha-border bg-suraksha-card px-5 py-2.5 text-xs font-bold text-suraksha-text hover:bg-suraksha-hover transition disabled:opacity-50"
              >
                <RefreshCw className="w-4 h-4 text-suraksha-amber" />
                <span>{isGenerating ? 'Generating...' : 'Generate Report'}</span>
              </button>

              <button
                type="button"
                onClick={handleExportCsv}
                className="flex items-center gap-2 rounded-xl bg-suraksha-surface border border-suraksha-border px-5 py-2.5 text-xs font-bold text-suraksha-heading hover:bg-suraksha-hover transition"
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
          </form>
        </div>
      </div>
    </div>
  );
};
export default ReportsScreen;