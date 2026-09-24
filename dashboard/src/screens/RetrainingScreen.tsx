import React, { useState, useEffect } from 'react';
import { FilterBar } from '../components/common/FilterBar';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { AssignRetrainingModal } from '../components/modals/AssignRetrainingModal';
import { fetchDashboardAssessments, fetchDashboardWorkers } from '../services/api';
import type { RetrainingRecord, Worker } from '../types';
import { TrendingUp, Plus } from 'lucide-react';
import { getTodayIso } from '../utils/dateRange';

export const RetrainingScreen: React.FC = () => {
  const [records, setRecords] = useState<RetrainingRecord[]>([]);
  const [workers, setWorkers] = useState<Worker[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [selectedWorker, setSelectedWorker] = useState<Worker | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  useEffect(() => {
    let isMounted = true;
    const loadData = () => {
      Promise.all([fetchDashboardAssessments(), fetchDashboardWorkers()]).then(([assessments, liveWorkers]) => {
        if (!isMounted) return;
        if (liveWorkers) {
          const mappedWorkers: Worker[] = liveWorkers.map((w) => ({
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
            overallStatus: w.certified_modules.length > 0 ? 'Certified' : 'In Training',
            modulesCompleted: w.certified_modules.length,
            latestScore: 80,
            overallCompetency: 'Competent',
            lastAssessmentDate: '2026-09-17',
            certificatesCount: w.certified_modules.length,
            retrainingStatus: 'Completed',
            moduleProgressList: [],
            weakAreas: [],
            retentionDay1: 'Completed',
            retentionDay7: 'Scheduled',
            retentionDay30: 'Scheduled',
          }));
          setWorkers(mappedWorkers);
        }
        const derived: RetrainingRecord[] = (assessments || [])
          .filter((a) => a.passFail === 'Fail' || a.criticalErrors > 0)
          .map((a, idx) => ({
            id: `ret-${idx + 1}`,
            workerId: a.workerId,
            workerName: a.workerName,
            employeeId: a.employeeId,
            sector: 'Dhanbad Region-1',
            moduleId: a.moduleId,
            moduleName: a.moduleName,
            weakArea: a.criticalErrorDetails || 'SOP Procedure Compliance / Technique',
            recommendation: 'Mandatory AR SOP refresher & supervised drill',
            status: 'Recommended' as const,
            assignedDate: a.dateTime ? a.dateTime.slice(0, 10) : getTodayIso(),
            initialScore: a.score,
          }));
        setRecords(derived);
      });
    };

    loadData();
    const interval = setInterval(loadData, 3000);
    return () => {
      isMounted = false;
      clearInterval(interval);
    };
  }, []);

  // Filter logic
  const filteredRecords = records.filter((r) => {
    const matchesSearch =
      r.workerName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.employeeId.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.weakArea.toLowerCase().includes(searchQuery.toLowerCase());

    const matchesStatus = statusFilter === 'ALL' || r.status === statusFilter;

    return matchesSearch && matchesStatus;
  });

  const flaggedCount = records.filter((r) => r.status === 'Recommended').length;
  const inProgressCount = records.filter((r) => r.status === 'In Progress' || r.status === 'Assigned').length;
  const pendingReassessCount = records.filter((r) => r.status === 'Reassessment Pending').length;
  const completedCount = records.filter((r) => r.status === 'Completed').length;

  const columns: Column<RetrainingRecord>[] = [
    {
      key: 'workerName',
      header: 'Worker Profile',
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
      header: 'Target Module',
      sortable: true,
      render: (r) => <span className="text-xs font-semibold text-suraksha-heading">{r.moduleName}</span>,
    },
    {
      key: 'weakArea',
      header: 'Identified Weak Protocol',
      render: (r) => (
        <span className="text-xs font-semibold text-rose-700 truncate max-w-[200px] block">
          {r.weakArea}
        </span>
      ),
    },
    {
      key: 'recommendation',
      header: 'Recommended Remedial Plan',
      render: (r) => (
        <span className="text-[11px] text-suraksha-subtext font-medium truncate max-w-[220px] block">
          {r.recommendation}
        </span>
      ),
    },
    {
      key: 'initialScore',
      header: 'Initial vs Reassessment Score',
      align: 'center',
      render: (r) => (
        <div className="text-xs font-bold">
          <span className="text-rose-700">{r.initialScore}%</span>
          {r.reassessmentScore !== undefined ? (
            <>
              <span className="text-suraksha-subtext px-1">→</span>
              <span className="text-emerald-700 font-bold">{r.reassessmentScore}%</span>
            </>
          ) : (
            <span className="text-suraksha-subtext text-[10px] ml-1 font-medium">(Pending)</span>
          )}
        </div>
      ),
    },
    {
      key: 'scoreImprovement',
      header: 'Improvement Delta',
      align: 'center',
      render: (r) =>
        r.scoreImprovement !== undefined ? (
          <span className="inline-flex items-center gap-1 font-bold text-emerald-700 text-xs bg-emerald-50 px-2 py-0.5 rounded border border-emerald-200">
            <TrendingUp className="w-3 h-3" /> +{r.scoreImprovement}%
          </span>
        ) : (
          <span className="text-[10px] text-suraksha-subtext font-medium">—</span>
        ),
    },
    {
      key: 'status',
      header: 'Retraining Status',
      sortable: true,
      render: (r) => <StatusBadge status={r.status} size="sm" />,
    },
    {
      key: 'assignedDate',
      header: 'Assigned Date',
      sortable: true,
      render: (r) => <span className="text-[10px] text-suraksha-subtext font-medium">{r.assignedDate}</span>,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h3 className="text-lg font-bold text-suraksha-heading uppercase tracking-wider">Safety Retraining & Mandatory Re-evaluation</h3>
          <p className="text-xs text-suraksha-subtext">
            Operational queue for workers flagged for remedial AR training, re-assessments, and score improvement tracking.
          </p>
        </div>

        <button
          onClick={() => {
            if (workers.length > 0) setSelectedWorker(workers[0]);
            setIsModalOpen(true);
          }}
          className="flex items-center gap-1.5 rounded-lg bg-suraksha-blue px-3.5 py-2 text-xs font-bold text-white hover:bg-suraksha-blueHover transition shadow-subtle"
        >
          <Plus className="w-3.5 h-3.5" />
          <span>Assign Retraining Order</span>
        </button>
      </div>

      {/* Metric Summary Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Flagged for Retraining</p>
          <p className="text-2xl font-black text-suraksha-heading mt-1">{records.length} Workers</p>
          <span className="text-[10px] text-amber-700 font-bold">{flaggedCount} Recommended</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">In Progress</p>
          <p className="text-2xl font-black text-amber-700 mt-1">{inProgressCount} Active</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">Assigned + Training</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Reassessment Pending</p>
          <p className="text-2xl font-black text-suraksha-blue mt-1">{pendingReassessCount} Scheduled</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">Awaiting re-eval run</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Completed</p>
          <p className="text-2xl font-black text-emerald-700 mt-1">{completedCount} Resolved</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">Avg +21.4% score delta</span>
        </div>
      </div>

      {/* Filter Bar */}
      <FilterBar
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Search worker, employee ID, weak area..."
        filters={[
          {
            key: 'status',
            label: 'Status',
            value: statusFilter,
            onChange: setStatusFilter,
            options: [
              { label: 'Recommended', value: 'Recommended' },
              { label: 'Assigned', value: 'Assigned' },
              { label: 'In Progress', value: 'In Progress' },
              { label: 'Completed', value: 'Completed' },
              { label: 'Reassessment Pending', value: 'Reassessment Pending' },
            ],
          },
        ]}
        onReset={() => {
          setSearchQuery('');
          setStatusFilter('ALL');
        }}
      />

      {/* Retraining Data Table */}
      <DataTable
        columns={columns}
        data={filteredRecords}
        pageSize={8}
        emptyMessage="No retraining records found matching criteria."
      />

      {/* Modal */}
      <AssignRetrainingModal
        worker={selectedWorker}
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onAssign={(data) => {
          const worker = workers.find((w) => w.id === data.workerId);
          if (!worker) return;
          const moduleName =
            data.moduleId === 'm-fire'
              ? 'Fire Safety'
              : data.moduleId === 'm-gas'
              ? 'Gas Safety'
              : 'Machinery Safety';
          setRecords((prev) => [
            {
              id: `ret-${Date.now()}`,
              workerId: data.workerId,
              workerName: worker.name,
              employeeId: worker.employeeId,
              sector: worker.sector,
              moduleId: data.moduleId,
              moduleName,
              weakArea: data.weakArea,
              recommendation: 'Individualized remedial AR path assigned',
              status: 'Assigned',
              assignedDate: getTodayIso(),
              initialScore: worker.latestScore,
            },
            ...prev,
          ]);
        }}
      />
    </div>
  );
};
