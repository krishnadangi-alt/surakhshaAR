import React, { useState } from 'react';
import { FilterBar } from '../components/common/FilterBar';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { AssignRetrainingModal } from '../components/modals/AssignRetrainingModal';
import { mockRetrainingRecords, mockWorkers } from '../mockData';
import type { RetrainingRecord, Worker } from '../types';
import { TrendingUp, Plus } from 'lucide-react';
import { MOCK_TODAY_ISO } from '../utils/dateRange';

export const RetrainingScreen: React.FC = () => {
  const [records, setRecords] = useState<RetrainingRecord[]>(mockRetrainingRecords);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [selectedWorker, setSelectedWorker] = useState<Worker | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

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
          <h5 className="font-bold text-white hover:text-suraksha-amber transition">{r.workerName}</h5>
          <p className="text-[10px] text-suraksha-subtext">{r.employeeId} • {r.sector}</p>
        </div>
      ),
    },
    {
      key: 'moduleName',
      header: 'Target Module',
      sortable: true,
      render: (r) => <span className="text-xs font-semibold text-white">{r.moduleName}</span>,
    },
    {
      key: 'weakArea',
      header: 'Identified Weak Protocol',
      render: (r) => (
        <span className="text-xs font-medium text-rose-300 truncate max-w-[200px] block">
          {r.weakArea}
        </span>
      ),
    },
    {
      key: 'recommendation',
      header: 'Recommended Remedial Plan',
      render: (r) => (
        <span className="text-[11px] text-suraksha-subtext truncate max-w-[220px] block">
          {r.recommendation}
        </span>
      ),
    },
    {
      key: 'initialScore',
      header: 'Initial vs Reassessment Score',
      align: 'center',
      render: (r) => (
        <div className="text-xs font-semibold">
          <span className="text-rose-400">{r.initialScore}%</span>
          {r.reassessmentScore !== undefined ? (
            <>
              <span className="text-suraksha-subtext px-1">→</span>
              <span className="text-emerald-400 font-bold">{r.reassessmentScore}%</span>
            </>
          ) : (
            <span className="text-suraksha-subtext text-[10px] ml-1">(Pending)</span>
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
          <span className="inline-flex items-center gap-1 font-bold text-emerald-400 text-xs bg-emerald-500/10 px-2 py-0.5 rounded border border-emerald-500/30">
            <TrendingUp className="w-3 h-3" /> +{r.scoreImprovement}%
          </span>
        ) : (
          <span className="text-[10px] text-suraksha-subtext">—</span>
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
      render: (r) => <span className="text-[10px] text-suraksha-subtext">{r.assignedDate}</span>,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h3 className="text-lg font-bold text-white uppercase tracking-wider">Safety Retraining & Mandatory Re-evaluation</h3>
          <p className="text-xs text-suraksha-subtext">
            Operational queue for workers flagged for remedial AR training, re-assessments, and score improvement tracking.
          </p>
        </div>

        <button
          onClick={() => {
            setSelectedWorker(mockWorkers[1]);
            setIsModalOpen(true);
          }}
          className="flex items-center gap-1.5 rounded-lg bg-suraksha-blue px-3.5 py-2 text-xs font-semibold text-white hover:bg-suraksha-blueHover transition shadow-subtle"
        >
          <Plus className="w-3.5 h-3.5" />
          <span>Assign Retraining Order</span>
        </button>
      </div>

      {/* Metric Summary Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Flagged for Retraining</p>
          <p className="text-2xl font-black text-white mt-1">{records.length} Workers</p>
          <span className="text-[10px] text-amber-400 font-semibold">{flaggedCount} Recommended</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">In Progress</p>
          <p className="text-2xl font-black text-amber-400 mt-1">{inProgressCount} Active</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">Assigned + Training</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Reassessment Pending</p>
          <p className="text-2xl font-black text-suraksha-blue mt-1">{pendingReassessCount} Scheduled</p>
          <span className="text-[10px] text-suraksha-subtext font-medium">Awaiting re-eval run</span>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Completed</p>
          <p className="text-2xl font-black text-emerald-400 mt-1">{completedCount} Resolved</p>
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
          const worker = mockWorkers.find((w) => w.id === data.workerId);
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
              assignedDate: MOCK_TODAY_ISO,
              initialScore: worker.latestScore,
            },
            ...prev,
          ]);
        }}
      />
    </div>
  );
};
