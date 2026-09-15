import React, { useState } from 'react';
import { FilterBar } from '../components/common/FilterBar';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { CertificateVerifyModal } from '../components/modals/CertificateVerifyModal';
import { mockCertificates } from '../mockData';
import type { Certificate } from '../types';
import { Download, QrCode, ShieldAlert } from 'lucide-react';

export const CertificatesScreen: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null);

  const filteredCertificates = mockCertificates.filter((c) => {
    const matchesSearch =
      c.workerName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      c.employeeId.toLowerCase().includes(searchQuery.toLowerCase()) ||
      c.certificateId.toLowerCase().includes(searchQuery.toLowerCase());

    const matchesStatus = statusFilter === 'ALL' || c.status === statusFilter;

    return matchesSearch && matchesStatus;
  });

  const columns: Column<Certificate>[] = [
    {
      key: 'workerName',
      header: 'Certified Worker',
      sortable: true,
      render: (c) => (
        <div>
          <h5 className="font-bold text-suraksha-heading hover:text-suraksha-amber transition">{c.workerName}</h5>
          <p className="text-[10px] text-suraksha-subtext font-medium">{c.employeeId} • {c.sector}</p>
        </div>
      ),
    },
    {
      key: 'certificateId',
      header: 'Certificate ID',
      sortable: true,
      render: (c) => <span className="font-mono text-xs font-bold text-suraksha-amber">{c.certificateId}</span>,
    },
    {
      key: 'moduleName',
      header: 'Safety Qualification',
      sortable: true,
      render: (c) => <span className="text-xs font-semibold text-suraksha-heading">{c.moduleName}</span>,
    },
    {
      key: 'resultGrade',
      header: 'Grade / Score',
      align: 'center',
      render: (c) => <span className="text-xs font-bold text-emerald-700">{c.resultGrade}</span>,
    },
    {
      key: 'issueDate',
      header: 'Issue Date',
      sortable: true,
      render: (c) => <span className="text-[11px] text-suraksha-subtext font-medium">{c.issueDate}</span>,
    },
    {
      key: 'expiryDate',
      header: 'Expiration Date',
      sortable: true,
      render: (c) => <span className="text-[11px] text-suraksha-subtext font-medium">{c.expiryDate}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      sortable: true,
      render: (c) => <StatusBadge status={c.status} size="sm" />,
    },
    {
      key: 'verification',
      header: 'Digital Verify',
      align: 'center',
      render: (c) => (
        <button
          onClick={(e) => {
            e.stopPropagation();
            setSelectedCertificate(c);
          }}
          className="flex items-center gap-1.5 px-2.5 py-1 rounded-lg border border-suraksha-border bg-suraksha-surface text-xs font-semibold text-suraksha-text hover:bg-suraksha-hover transition"
        >
          <QrCode className="w-3.5 h-3.5 text-suraksha-amber" />
          <span>Verify</span>
        </button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h3 className="text-lg font-bold text-suraksha-heading uppercase tracking-wider">
            Compliance Certificate Registry & Verification
          </h3>
          <p className="text-xs text-suraksha-subtext">
            Simulated DEMO certificate registry for UI preview — not an official government-issued registry.
          </p>
        </div>

        <button
          onClick={() => alert('Demo: exporting registry CSV is simulated in this phase.')}
          className="flex items-center gap-1.5 rounded-lg border border-suraksha-border bg-suraksha-card px-3.5 py-2 text-xs font-semibold text-suraksha-text hover:bg-suraksha-hover transition"
        >
          <Download className="w-3.5 h-3.5" />
          <span>Export Registry CSV</span>
        </button>
      </div>

      {/* Demo data notice */}
      <div className="flex items-start gap-2 rounded-xl border border-amber-500/30 bg-amber-500/10 p-3 text-[11px] font-medium text-amber-800">
        <ShieldAlert className="w-4 h-4 text-suraksha-amber shrink-0 mt-0.5" />
        <span>
          All certificates shown here are simulated demo records. Verification uses a demo integrity code and is not a
          cryptographic signature or government seal.
        </span>
      </div>

      {/* Summary KPI Strip */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Active Certificates</p>
          <p className="text-2xl font-black text-emerald-700 mt-1">11,850 Active</p>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Expiring in 30 Days</p>
          <p className="text-2xl font-black text-amber-700 mt-1">412 Due</p>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Expired</p>
          <p className="text-2xl font-black text-rose-700 mt-1">86 Lapsed</p>
        </div>
        <div className="p-4 rounded-xl border border-suraksha-border bg-suraksha-card">
          <p className="text-[10px] font-bold text-suraksha-subtext uppercase">Pending Approval</p>
          <p className="text-2xl font-black text-suraksha-blue mt-1">140 Pending</p>
        </div>
      </div>

      {/* Filter Bar */}
      <FilterBar
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Search by Worker Name, Certificate ID (e.g. JH-SAF-2026)..."
        filters={[
          {
            key: 'status',
            label: 'Status',
            value: statusFilter,
            onChange: setStatusFilter,
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
          setStatusFilter('ALL');
        }}
      />

      {/* Certificate Table */}
      <DataTable
        columns={columns}
        data={filteredCertificates}
        pageSize={8}
        onRowClick={(c) => setSelectedCertificate(c)}
        emptyMessage="No certificates found matching criteria."
      />

      {/* Digital Certificate Verification Modal */}
      <CertificateVerifyModal
        certificate={selectedCertificate}
        isOpen={!!selectedCertificate}
        onClose={() => setSelectedCertificate(null)}
      />
    </div>
  );
};
