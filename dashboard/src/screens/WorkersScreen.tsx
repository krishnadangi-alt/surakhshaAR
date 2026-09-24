import React, { useState, useEffect } from 'react';
import { FilterBar } from '../components/common/FilterBar';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { AssignRetrainingModal } from '../components/modals/AssignRetrainingModal';
import { fetchDashboardWorkers, fetchDashboardAssessments } from '../services/api';
import type { Worker } from '../types';
import { UserPlus, Download, Eye, RotateCcw } from 'lucide-react';
import { downloadCsv } from '../utils/export';

interface WorkersScreenProps {
  onSelectWorker: (workerId: string) => void;
}

export const WorkersScreen: React.FC<WorkersScreenProps> = ({ onSelectWorker }) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [moduleFilter, setModuleFilter] = useState('ALL');
  const [sectorFilter, setSectorFilter] = useState('ALL');
  const [plantFilter, setPlantFilter] = useState('ALL');
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [competencyFilter, setCompetencyFilter] = useState('ALL');
  const [retrainingFilter, setRetrainingFilter] = useState('ALL');
  const [certFilter, setCertFilter] = useState('ALL');
  const [selectedWorker, setSelectedWorker] = useState<Worker | null>(null);
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [liveWorkers, setLiveWorkers] = useState<Worker[]>([]);

  useEffect(() => {
    let isMounted = true;
    const loadWorkers = () => {
      Promise.all([fetchDashboardWorkers(), fetchDashboardAssessments()]).then(([items, liveAssessments]) => {
        if (!isMounted) return;
        if (items && items.length > 0) {
          const mapped: Worker[] = items.map((w) => {
            const workerAssessments = (liveAssessments || []).filter(
              (a) => a.workerId === `w-${w.id}` || a.employeeId === w.employee_id
            );
            const latestAsmt = workerAssessments[0];
            const latestScore = latestAsmt ? latestAsmt.score : (w.certified_modules.length > 0 ? 85 : 0);
            const overallCompetency: 'Competent' | 'Needs Retraining' = latestScore >= 75 ? 'Competent' : 'Needs Retraining';
            const lastDate = latestAsmt ? latestAsmt.dateTime : '2026-09-16';
            const weakAreas = latestAsmt && latestAsmt.wrongActions > 0 ? ['PASS Extinguisher Technique'] : [];

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
              lastAssessmentDate: lastDate,
              certificatesCount: w.certified_modules.length,
              retrainingStatus: overallCompetency === 'Needs Retraining' ? 'Assigned' : 'Completed',
              moduleProgressList: w.progress.map((p) => ({
                moduleId: p.module_code,
                moduleName: p.module_name,
                stage: p.stage,
                status: p.status === 'completed' ? 'Completed' : 'In Progress',
                completionPercentage: p.status === 'completed' ? 100 : 50,
                score: latestScore,
                lastUpdated: p.last_updated || lastDate,
              })),
              weakAreas,
              retentionDay1: 'Completed',
              retentionDay7: 'Scheduled',
              retentionDay30: 'Scheduled',
            };
          });
          setLiveWorkers(mapped);
        }
      });
    };

    loadWorkers();
    const interval = setInterval(loadWorkers, 3000);
    return () => {
      isMounted = false;
      clearInterval(interval);
    };
  }, []);

  const getWorkerCertStatus = (w: Worker) => (w.certificatesCount > 0 ? 'Active' : 'Pending');
  const workersSource = liveWorkers;

  // Filter logic
  const filteredWorkers = workersSource.filter((w) => {
    const matchesSearch =
      w.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      w.employeeId.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesModule = moduleFilter === 'ALL' || w.moduleProgressList.some((m) => m.moduleId === moduleFilter);
    const matchesSector = sectorFilter === 'ALL' || w.sector === sectorFilter;
    const matchesPlant = plantFilter === 'ALL' || w.plant === plantFilter;
    const matchesStatus = statusFilter === 'ALL' || w.overallStatus === statusFilter;
    const matchesCompetency = competencyFilter === 'ALL' || w.overallCompetency === competencyFilter;
    const matchesRetraining = retrainingFilter === 'ALL' || w.retrainingStatus === retrainingFilter;
    const matchesCert = certFilter === 'ALL' || getWorkerCertStatus(w) === certFilter;

    return (
      matchesSearch &&
      matchesModule &&
      matchesSector &&
      matchesPlant &&
      matchesStatus &&
      matchesCompetency &&
      matchesRetraining &&
      matchesCert
    );
  });

  const handleExportCsv = () => {
    downloadCsv(
      'surakshaar-workers-directory',
      ['Worker Name', 'Employee ID', 'Sector', 'Plant', 'Modules Completed', 'Latest Score', 'Overall Status', 'Competency', 'Retraining', 'Certificate', 'Last Assessment'],
      filteredWorkers.map((w) => [
        w.name,
        w.employeeId,
        w.sector,
        w.plant,
        `${w.modulesCompleted}/3`,
        `${w.latestScore}%`,
        w.overallStatus,
        w.overallCompetency,
        w.retrainingStatus,
        getWorkerCertStatus(w),
        w.lastAssessmentDate,
      ]),
    );
  };
const columns: Column<Worker>[] = [
    {
      key: 'name',
      header: 'Worker',
      sortable: true,
      render: (w) => (
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-suraksha-surface border border-suraksha-border font-bold text-xs text-suraksha-amber">
            {w.name.split(' ').map((n) => n[0]).join('')}
          </div>
          <div>
            <h5 className="font-bold text-suraksha-heading hover:text-suraksha-amber transition">{w.name}</h5>
            <p className="text-[10px] text-suraksha-subtext">{w.role}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'employeeId',
      header: 'Employee ID',
      sortable: true,
      render: (w) => <span className="font-mono text-xs text-suraksha-subtext font-medium">{w.employeeId}</span>,
    },
    {
      key: 'modulesCompleted',
      header: 'Modules Completed',
      align: 'center',
      sortable: true,
      render: (w) => (
        <span className="font-bold text-suraksha-heading bg-suraksha-surface px-2.5 py-1 rounded-md border border-suraksha-border">
          {w.modulesCompleted} / 3
        </span>
      ),
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
      key: 'overallStatus',
      header: 'Overall Status',
      sortable: true,
      render: (w) => <StatusBadge status={w.overallStatus} size="sm" />,
    },
    {
      key: 'overallCompetency',
      header: 'Competency',
      sortable: true,
      render: (w) => <StatusBadge status={w.overallCompetency} size="sm" />,
    },
    {
      key: 'retrainingStatus',
      header: 'Retraining',
      sortable: true,
      render: (w) => <StatusBadge status={w.retrainingStatus} size="sm" />,
    },
    {
      key: 'certificate',
      header: 'Certificate',
      sortable: true,
      render: (w) => <StatusBadge status={getWorkerCertStatus(w)} size="sm" />,
    },
    {
      key: 'lastAssessmentDate',
      header: 'Last Assessment',
      sortable: true,
      render: (w) => <span className="text-[11px] text-suraksha-subtext font-medium">{w.lastAssessmentDate}</span>,
    },
    {
      key: 'actions',
      header: 'Actions',
      align: 'center',
      render: (w) => (
        <div className="flex items-center justify-center gap-1.5">
          <button
            onClick={(e) => {
              e.stopPropagation();
              onSelectWorker(w.id);
            }}
            title="View Full Dossier"
            className="p-1.5 rounded-lg border border-suraksha-border text-suraksha-blue hover:bg-suraksha-hover transition"
          >
            <Eye className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              setSelectedWorker(w);
              setIsAssignModalOpen(true);
            }}
            title="Assign Retraining"
            className="p-1.5 rounded-lg border border-amber-500/30 text-suraksha-amber hover:bg-amber-500/10 transition"
          >
            <RotateCcw className="w-3.5 h-3.5" />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header & Actions */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h3 className="text-lg font-bold text-suraksha-heading uppercase tracking-wider">Industrial Workers Directory</h3>
          <p className="text-xs text-suraksha-subtext">
            Search, inspect dossiers, and manage AR training compliance for Jharkhand personnel.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={handleExportCsv}
            className="flex items-center gap-1.5 rounded-lg border border-suraksha-border bg-suraksha-card px-3.5 py-2 text-xs font-semibold text-suraksha-text hover:bg-suraksha-hover transition"
          >
            <Download className="w-3.5 h-3.5" />
            <span>Export CSV</span>
          </button>
          <button
            onClick={() => alert('Not available in demo phase: worker registration is handled by the worker app.')}
            className="flex items-center gap-1.5 rounded-lg bg-suraksha-blue px-3.5 py-2 text-xs font-semibold text-white hover:bg-suraksha-blueHover transition shadow-subtle"
          >
            <UserPlus className="w-3.5 h-3.5" />
            <span>Register Worker</span>
          </button>
        </div>
      </div>

      {/* Filter Bar */}
      <FilterBar
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Filter by Worker Name, Employee ID (e.g. JHK-BS-8842)..."
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
            key: 'sector',
            label: 'Sector',
            value: sectorFilter,
            onChange: setSectorFilter,
            options: [
              { label: 'Bokaro Steel Plant', value: 'Bokaro Steel Plant' },
              { label: 'Dhanbad Coal Fields', value: 'Dhanbad Coal Fields' },
              { label: 'Jamshedpur Metallurgy', value: 'Jamshedpur Metallurgy' },
              { label: 'Ranchi Heavy Electricals', value: 'Ranchi Heavy Electricals' },
              { label: 'Ramgarh Chemical Works', value: 'Ramgarh Chemical Works' },
            ],
          },
          {
            key: 'plant',
            label: 'Plant',
            value: plantFilter,
            onChange: setPlantFilter,
            options: Array.from(new Set(workersSource.map((w) => w.plant))).map((p) => ({ label: p, value: p })),
          },
          {
            key: 'status',
            label: 'Result/Status',
            value: statusFilter,
            onChange: setStatusFilter,
            options: [
              { label: 'Certified', value: 'Certified' },
              { label: 'Passed', value: 'Passed' },
              { label: 'In Training', value: 'In Training' },
              { label: 'Retraining Required', value: 'Retraining Required' },
              { label: 'Failed', value: 'Failed' },
            ],
          },
          {
            key: 'competency',
            label: 'Competency',
            value: competencyFilter,
            onChange: setCompetencyFilter,
            options: [
              { label: 'Strong', value: 'Strong' },
              { label: 'Developing', value: 'Developing' },
              { label: 'Needs Retraining', value: 'Needs Retraining' },
            ],
          },
          {
            key: 'retraining',
            label: 'Retraining',
            value: retrainingFilter,
            onChange: setRetrainingFilter,
            options: [
              { label: 'Recommended', value: 'Recommended' },
              { label: 'Assigned', value: 'Assigned' },
              { label: 'In Progress', value: 'In Progress' },
              { label: 'Completed', value: 'Completed' },
              { label: 'Reassessment Pending', value: 'Reassessment Pending' },
            ],
          },
          {
            key: 'cert',
            label: 'Certificate',
            value: certFilter,
            onChange: setCertFilter,
            options: [
              { label: 'Active', value: 'Active' },
              { label: 'Expiring Soon', value: 'Expiring Soon' },
              { label: 'Expired', value: 'Expired' },
              { label: 'Pending', value: 'Pending' },
            ],
          },
        ]}
        onReset={() => {
          setSearchQuery('');
          setModuleFilter('ALL');
          setSectorFilter('ALL');
          setPlantFilter('ALL');
          setStatusFilter('ALL');
          setCompetencyFilter('ALL');
          setRetrainingFilter('ALL');
          setCertFilter('ALL');
        }}
      />

      {/* Workers Data Table */}
      <DataTable
        columns={columns}
        data={filteredWorkers}
        pageSize={10}
        onRowClick={(w) => onSelectWorker(w.id)}
        emptyMessage="No workers found matching your filter criteria."
      />

      {/* Assign Retraining Modal */}
      <AssignRetrainingModal
        worker={selectedWorker}
        isOpen={isAssignModalOpen}
        onClose={() => setIsAssignModalOpen(false)}
        onAssign={() => {
          // Mock-only: no backend call is made in this phase.
        }}
      />
    </div>
  );
};