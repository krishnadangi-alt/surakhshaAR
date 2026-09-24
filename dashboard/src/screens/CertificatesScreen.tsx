import React, { useState, useEffect } from 'react';
import { FilterBar } from '../components/common/FilterBar';
import { DataTable } from '../components/common/DataTable';
import type { Column } from '../components/common/DataTable';
import { StatusBadge } from '../components/common/StatusBadge';
import { CertificateVerifyModal } from '../components/modals/CertificateVerifyModal';
import {
  fetchDashboardCertificates,
  fetchReviewQueue,
  fetchAttemptDetailedReview,
  approveCertificate,
  rejectCertificate,
  type ReviewQueueItem,
} from '../services/api';
import type { Certificate } from '../types';
import {
  QrCode,
  ClipboardCheck,
  CheckCircle2,
  XCircle,
  Clock,
  AlertTriangle,
  FileText,
  ExternalLink,
  Image as ImageIcon,
  Download,
  Globe,
} from 'lucide-react';
import { RealQRCode } from '../components/common/RealQRCode';

export const CertificatesScreen: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'registry' | 'queue'>('registry');
  const [certificates, setCertificates] = useState<Certificate[]>([]);
  const [reviewQueue, setReviewQueue] = useState<ReviewQueueItem[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null);
  const [viewingImageCert, setViewingImageCert] = useState<Certificate | null>(null);

  // Review modal state
  const [reviewingItem, setReviewingItem] = useState<ReviewQueueItem | null>(null);
  const [attemptDetails, setAttemptDetails] = useState<any | null>(null);
  const [loadingDetails, setLoadingDetails] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [showRejectBox, setShowRejectBox] = useState(false);
  const [feedbackMessage, setFeedbackMessage] = useState<string | null>(null);

  const [lastUpdated, setLastUpdated] = useState<string | null>(null);

  const loadData = () => {
    fetchDashboardCertificates().then((data) => {
      setCertificates(data || []);
    });
    fetchReviewQueue('PENDING_REVIEW').then((data) => {
      setReviewQueue(data || []);
    });
    setLastUpdated(new Date().toLocaleTimeString());
  };

  useEffect(() => {
    loadData();
    const interval = setInterval(loadData, 5000);
    return () => clearInterval(interval);
  }, []);

  const handleOpenReview = async (item: ReviewQueueItem) => {
    setReviewingItem(item);
    setShowRejectBox(false);
    setRejectReason('');
    setFeedbackMessage(null);
    setLoadingDetails(true);
    const idToFetch = item.attempt_id || (item.assessment_id ? String(item.assessment_id) : String(item.certificate_id));
    const details = await fetchAttemptDetailedReview(idToFetch);
    setAttemptDetails(details);
    setLoadingDetails(false);
  };

  const handleApprove = async () => {
    if (!reviewingItem) return;
    setActionLoading(true);
    try {
      const idToApprove = reviewingItem.attempt_id || String(reviewingItem.assessment_id || reviewingItem.certificate_id);
      await approveCertificate(idToApprove);
      setFeedbackMessage('✓ Certificate approved and issued successfully! PDF & QR generated.');
      setTimeout(() => {
        setReviewingItem(null);
        loadData();
      }, 1500);
    } catch (err: any) {
      alert(`Approval error: ${err.message}`);
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    if (!reviewingItem) return;
    if (!rejectReason.trim()) {
      alert('Please enter a specific reason for rejection');
      return;
    }
    setActionLoading(true);
    try {
      const idToReject = reviewingItem.attempt_id || String(reviewingItem.assessment_id || reviewingItem.certificate_id);
      await rejectCertificate(idToReject, rejectReason);
      setFeedbackMessage('✕ Certificate rejected. Retraining requirement recorded.');
      setTimeout(() => {
        setReviewingItem(null);
        loadData();
      }, 1500);
    } catch (err: any) {
      alert(`Rejection error: ${err.message}`);
    } finally {
      setActionLoading(false);
    }
  };

  // Filtered Certificates for Registry View
  const filteredCertificates = certificates.filter((c) => {
    const matchesSearch =
      c.workerName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      c.employeeId.toLowerCase().includes(searchQuery.toLowerCase()) ||
      c.certificateId.toLowerCase().includes(searchQuery.toLowerCase());

    const matchesStatus = statusFilter === 'ALL' || c.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  // Filtered Review Queue Items
  const filteredQueue = reviewQueue.filter((item) => {
    return (
      item.worker_name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.employee_id.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.certificate_number.toLowerCase().includes(searchQuery.toLowerCase())
    );
  });

  // Registry columns
  const registryColumns: Column<Certificate>[] = [
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
      render: (c) => <span className="text-[11px] text-suraksha-subtext font-medium">{c.issueDate || 'Pending'}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      sortable: true,
      render: (c) => <StatusBadge status={c.status} size="sm" />,
    },
    {
      key: 'verification',
      header: 'Actions',
      align: 'center',
      render: (c) => (
        <div className="flex items-center gap-1.5 justify-center">
          <button
            onClick={(e) => {
              e.stopPropagation();
              setSelectedCertificate(c);
            }}
            className="flex items-center gap-1 px-2.5 py-1 rounded-lg border border-suraksha-border bg-suraksha-surface text-xs font-semibold text-suraksha-text hover:bg-suraksha-hover transition"
          >
            <QrCode className="w-3.5 h-3.5 text-suraksha-amber" />
            <span>Verify & QR</span>
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              setViewingImageCert(c);
            }}
            title="View Official Certificate Image (PNG)"
            className="flex items-center gap-1 px-2.5 py-1 rounded-lg border border-amber-500/40 bg-amber-500/10 text-xs font-bold text-suraksha-amber hover:bg-amber-500/20 transition"
          >
            <ImageIcon className="w-3.5 h-3.5" />
            <span>View Image</span>
          </button>
          <a
            href={`/verify/${encodeURIComponent(c.certificateId)}`}
            target="_blank"
            rel="noopener noreferrer"
            title="Open Public Verification Page"
            className="p-1 rounded-lg border border-suraksha-border bg-suraksha-surface text-slate-400 hover:text-suraksha-amber transition"
          >
            <ExternalLink className="w-3.5 h-3.5" />
          </a>
        </div>
      ),
    },
  ];

  // Queue columns
  const queueColumns: Column<ReviewQueueItem>[] = [
    {
      key: 'worker_name',
      header: 'Worker & Employee ID',
      sortable: true,
      render: (q) => (
        <div>
          <h5 className="font-bold text-suraksha-heading">{q.worker_name}</h5>
          <p className="text-[11px] text-suraksha-subtext font-mono">{q.employee_id}</p>
        </div>
      ),
    },
    {
      key: 'module_name',
      header: 'Module & Scenario',
      sortable: true,
      render: (q) => (
        <span className="text-xs font-semibold text-suraksha-heading">{q.module_name}</span>
      ),
    },
    {
      key: 'score',
      header: 'Score & Competency',
      align: 'center',
      render: (q) => (
        <div>
          <span className="text-xs font-bold text-suraksha-blue">{Math.round(q.score)} / 100</span>
          <p className="text-[10px] font-bold text-emerald-600">{q.competency_status}</p>
        </div>
      ),
    },
    {
      key: 'critical_errors',
      header: 'Critical Errors',
      align: 'center',
      render: (q) => (
        <span className={`text-xs font-bold ${q.critical_errors > 0 ? 'text-rose-600' : 'text-slate-600'}`}>
          {q.critical_errors}
        </span>
      ),
    },
    {
      key: 'duration_seconds',
      header: 'Training Duration',
      align: 'center',
      render: (q) => {
        const mins = Math.floor(q.duration_seconds / 60);
        const secs = Math.floor(q.duration_seconds % 60);
        return <span className="text-xs font-mono">{mins}:{secs.toString().padStart(2, '0')}</span>;
      },
    },
    {
      key: 'status',
      header: 'Review State',
      align: 'center',
      render: () => (
        <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-500/10 text-amber-700 border border-amber-500/30">
          PENDING REVIEW
        </span>
      ),
    },
    {
      key: 'actions',
      header: 'Admin Action',
      align: 'center',
      render: (q) => (
        <button
          onClick={(e) => {
            e.stopPropagation();
            handleOpenReview(q);
          }}
          className="flex items-center gap-1 px-3 py-1.5 rounded-lg bg-suraksha-amber text-slate-950 text-xs font-bold hover:bg-amber-400 transition shadow-sm"
        >
          <ClipboardCheck className="w-3.5 h-3.5" />
          <span>Inspect & Review</span>
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
            Industrial Safety Certification & Governance
          </h3>
          <p className="text-xs text-suraksha-subtext">
            Server-authoritative assessment scoring, detailed behavioral timeline inspection, and admin review queue.
            {lastUpdated && <span className="ml-2 font-mono text-[10px] text-suraksha-amber font-semibold">• Live: {lastUpdated}</span>}
          </p>
        </div>

        {/* Tab Switcher */}
        <div className="flex items-center gap-2 bg-suraksha-card p-1 rounded-xl border border-suraksha-border">
          <button
            onClick={() => setActiveTab('queue')}
            className={`flex items-center gap-1.5 px-4 py-2 rounded-lg text-xs font-bold transition ${
              activeTab === 'queue'
                ? 'bg-suraksha-amber text-slate-950 shadow-sm'
                : 'text-suraksha-subtext hover:text-suraksha-heading'
            }`}
          >
            <Clock className="w-3.5 h-3.5" />
            <span>Review Queue</span>
            {reviewQueue.length > 0 && (
              <span className="px-1.5 py-0.2 rounded-full text-[10px] font-black bg-rose-500 text-white ml-1">
                {reviewQueue.length}
              </span>
            )}
          </button>
          <button
            onClick={() => setActiveTab('registry')}
            className={`flex items-center gap-1.5 px-4 py-2 rounded-lg text-xs font-bold transition ${
              activeTab === 'registry'
                ? 'bg-suraksha-amber text-slate-950 shadow-sm'
                : 'text-suraksha-subtext hover:text-suraksha-heading'
            }`}
          >
            <FileText className="w-3.5 h-3.5" />
            <span>Issued Registry</span>
          </button>
        </div>
      </div>

      {activeTab === 'queue' ? (
        <div className="space-y-4">
          <div className="flex items-start gap-2 rounded-xl border border-blue-500/30 bg-blue-500/10 p-3 text-[11px] font-medium text-blue-900">
            <ClipboardCheck className="w-4 h-4 text-blue-600 shrink-0 mt-0.5" />
            <span>
              <strong>Rule 82 Mandatory Admin Approval:</strong> Completed AR training attempts remain in
              PENDING_REVIEW until an authorized safety officer inspects actual performance (timings, mistakes, continuous spray contact) and issues an explicit decision.
            </span>
          </div>

          <DataTable
            columns={queueColumns}
            data={filteredQueue}
            pageSize={8}
            emptyMessage="No attempts currently awaiting admin review."
          />
        </div>
      ) : (
        <div className="space-y-6">
          {/* Live Scannable QR Codes Showcase */}
          <div className="rounded-2xl border-2 border-amber-500/40 bg-gradient-to-b from-suraksha-card to-suraksha-surface p-5 shadow-elevated space-y-4">
            <div className="flex flex-wrap items-center justify-between gap-3 pb-3 border-b border-suraksha-border">
              <div className="flex items-center gap-3">
                <div className="p-2.5 rounded-xl bg-amber-500/10 text-suraksha-amber border border-amber-500/30">
                  <QrCode className="w-5 h-5" />
                </div>
                <div>
                  <h4 className="text-sm font-bold text-suraksha-heading uppercase tracking-wider flex items-center gap-2">
                    Live Scannable QR Safety Credentials
                    <span className="px-2 py-0.5 rounded-full text-[10px] font-black bg-emerald-500/20 text-emerald-600 border border-emerald-500/30">
                      2 ISSUED CERTIFICATES
                    </span>
                    <span className="px-2 py-0.5 rounded-full text-[10px] font-black bg-emerald-500/20 text-emerald-500 border border-emerald-500/30 flex items-center gap-1">
                      <Globe className="w-3 h-3" /> PUBLIC HTTPS VERIFICATION
                    </span>
                  </h4>
                  <p className="text-xs text-suraksha-subtext">
                    Scan with any smartphone camera or <strong>Google Scanner / Lens</strong> to open the official public SurakshaAR verification webpage.
                  </p>
                </div>
              </div>

              {/* Direct Public Scannable Status Indicator */}
              <div className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-bold shadow-sm">
                <Globe className="w-4 h-4 text-emerald-400 shrink-0" />
                <span>Google Scanner Scannable • Live Verification Registry</span>
              </div>
            </div>

            {/* 2 Worker QR Cards Side-by-Side */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
              {/* Card 1: Krishna */}
              <div className="rounded-xl border border-suraksha-border bg-suraksha-bg p-4 flex flex-col sm:flex-row items-center gap-4 hover:border-amber-500/40 transition">
                <div className="flex flex-col items-center shrink-0">
                  <RealQRCode
                    value="https://surakhshaar.onrender.com/verify/SUR-2026-0002"
                    size={128}
                    showActions={true}
                    downloadFilename="qr_krishna_SUR-2026-0002.png"
                  />
                  <span className="text-[10px] font-mono text-emerald-400 font-bold mt-1.5 flex items-center gap-0.5">
                    ✓ Public Verify QR
                  </span>
                  <a
                    href="https://surakhshaar.onrender.com/verify/SUR-2026-0002"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-[10px] font-mono text-suraksha-amber underline hover:text-amber-300 mt-0.5 max-w-[150px] truncate text-center block"
                    title="Public URL: https://surakhshaar.onrender.com/verify/SUR-2026-0002"
                  >
                    /verify/SUR-2026-0002
                  </a>
                </div>

                <div className="flex-1 space-y-2 text-center sm:text-left w-full">
                  <div className="flex items-center justify-between gap-2">
                    <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/10 text-emerald-600 border border-emerald-500/30">
                      ✓ Grade A (Competent)
                    </span>
                    <span className="font-mono text-xs font-bold text-suraksha-amber">SUR-2026-0002</span>
                  </div>

                  <div>
                    <h5 className="text-base font-extrabold text-suraksha-heading">Krishna</h5>
                    <p className="text-xs text-suraksha-subtext font-mono">EMP-PROD-CERT • Dhanbad Region-1</p>
                  </div>

                  <div className="p-2.5 rounded-lg bg-suraksha-surface border border-suraksha-border text-xs space-y-1">
                    <div className="flex justify-between text-suraksha-subtext">
                      <span>Module:</span>
                      <strong className="text-suraksha-heading">Fire &amp; Explosion Response</strong>
                    </div>
                    <div className="flex justify-between text-suraksha-subtext">
                      <span>Score:</span>
                      <strong className="text-emerald-600 font-bold">90% (Zero Critical Errors)</strong>
                    </div>
                    <div className="flex justify-between text-suraksha-subtext">
                      <span>Issue Date:</span>
                      <strong className="text-suraksha-heading">2026-09-21</strong>
                    </div>
                  </div>

                  <div className="flex flex-wrap items-center gap-2 pt-1">
                    <a
                      href="https://surakhshaar.onrender.com/verify/SUR-2026-0002"
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex-1 flex items-center justify-center gap-1.5 py-2 px-3 rounded-lg bg-suraksha-amber text-slate-950 text-xs font-extrabold hover:bg-amber-400 transition shadow-sm"
                    >
                      <ExternalLink className="w-3.5 h-3.5" />
                      <span>Open Verification Page</span>
                    </a>
                    <button
                      onClick={() => {
                        const cert = certificates.find((c) => c.certificateId === 'SUR-2026-0002') || {
                          id: 'cert-2',
                          certificateId: 'SUR-2026-0002',
                          workerId: 'w-2',
                          workerName: 'Krishna',
                          employeeId: 'EMP-PROD-CERT',
                          sector: 'Dhanbad Region-1',
                          moduleId: 'm-1',
                          moduleName: 'Fire & Explosion Response',
                          resultGrade: 'Grade A (Competent)',
                          issueDate: '2026-09-21',
                          expiryDate: '2027-09-21',
                          status: 'Active' as any,
                          verificationCode: 'SHA256:0002',
                          issuerDepartment: 'Directorate General of Mines Safety (DGMS)',
                          publicImageUrl: 'https://files.catbox.moe/t1l5lb.png',
                        };
                        setViewingImageCert(cert);
                      }}
                      className="flex-1 flex items-center justify-center gap-1.5 py-2 px-3 rounded-lg bg-amber-500/10 border border-amber-500/40 text-suraksha-amber text-xs font-bold hover:bg-amber-500/20 transition shadow-sm"
                    >
                      <ImageIcon className="w-3.5 h-3.5" />
                      <span>Preview Image Modal</span>
                    </button>
                  </div>
                </div>
              </div>

              {/* Card 2: Birsa Munda */}
              <div className="rounded-xl border border-suraksha-border bg-suraksha-bg p-4 flex flex-col sm:flex-row items-center gap-4 hover:border-amber-500/40 transition">
                <div className="flex flex-col items-center shrink-0">
                  <RealQRCode
                    value="https://surakhshaar.onrender.com/verify/SUR-2026-0001"
                    size={128}
                    showActions={true}
                    downloadFilename="qr_birsa_munda_SUR-2026-0001.png"
                  />
                  <span className="text-[10px] font-mono text-emerald-400 font-bold mt-1.5 flex items-center gap-0.5">
                    ✓ Public Verify QR
                  </span>
                  <a
                    href="https://surakhshaar.onrender.com/verify/SUR-2026-0001"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-[10px] font-mono text-suraksha-amber underline hover:text-amber-300 mt-0.5 max-w-[150px] truncate text-center block"
                    title="Public URL: https://surakhshaar.onrender.com/verify/SUR-2026-0001"
                  >
                    /verify/SUR-2026-0001
                  </a>
                </div>

                <div className="flex-1 space-y-2 text-center sm:text-left w-full">
                  <div className="flex items-center justify-between gap-2">
                    <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/10 text-emerald-600 border border-emerald-500/30">
                      ✓ Grade A (Competent)
                    </span>
                    <span className="font-mono text-xs font-bold text-suraksha-amber">SUR-2026-0001</span>
                  </div>

                  <div>
                    <h5 className="text-base font-extrabold text-suraksha-heading">Birsa Munda</h5>
                    <p className="text-xs text-suraksha-subtext font-mono">EMP-JH-001 • Dhanbad Region-1</p>
                  </div>

                  <div className="p-2.5 rounded-lg bg-suraksha-surface border border-suraksha-border text-xs space-y-1">
                    <div className="flex justify-between text-suraksha-subtext">
                      <span>Module:</span>
                      <strong className="text-suraksha-heading">Fire &amp; Explosion Response</strong>
                    </div>
                    <div className="flex justify-between text-suraksha-subtext">
                      <span>Score:</span>
                      <strong className="text-emerald-600 font-bold">90% (Zero Critical Errors)</strong>
                    </div>
                    <div className="flex justify-between text-suraksha-subtext">
                      <span>Issue Date:</span>
                      <strong className="text-suraksha-heading">2026-09-16</strong>
                    </div>
                  </div>

                  <div className="flex flex-wrap items-center gap-2 pt-1">
                    <a
                      href="https://surakhshaar.onrender.com/verify/SUR-2026-0001"
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex-1 flex items-center justify-center gap-1.5 py-2 px-3 rounded-lg bg-suraksha-amber text-slate-950 text-xs font-extrabold hover:bg-amber-400 transition shadow-sm"
                    >
                      <ExternalLink className="w-3.5 h-3.5" />
                      <span>Open Verification Page</span>
                    </a>
                    <button
                      onClick={() => {
                        const cert = certificates.find((c) => c.certificateId === 'SUR-2026-0001') || {
                          id: 'cert-1',
                          certificateId: 'SUR-2026-0001',
                          workerId: 'w-1',
                          workerName: 'Birsa Munda',
                          employeeId: 'EMP-JH-001',
                          sector: 'Dhanbad Region-1',
                          moduleId: 'm-1',
                          moduleName: 'Fire & Explosion Response',
                          resultGrade: 'Grade A (Competent)',
                          issueDate: '2026-09-16',
                          expiryDate: '2027-09-16',
                          status: 'Active' as any,
                          verificationCode: 'SHA256:0001',
                          issuerDepartment: 'Directorate General of Mines Safety (DGMS)',
                          publicImageUrl: 'https://files.catbox.moe/hge6s4.png',
                        };
                        setViewingImageCert(cert);
                      }}
                      className="flex-1 flex items-center justify-center gap-1.5 py-2 px-3 rounded-lg bg-amber-500/10 border border-amber-500/40 text-suraksha-amber text-xs font-bold hover:bg-amber-500/20 transition shadow-sm"
                    >
                      <ImageIcon className="w-3.5 h-3.5" />
                      <span>Preview Image Modal</span>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <FilterBar
            searchQuery={searchQuery}
            onSearchChange={setSearchQuery}
            searchPlaceholder="Search by Worker Name, Certificate ID..."
            filters={[
              {
                key: 'status',
                label: 'Status',
                value: statusFilter,
                onChange: setStatusFilter,
                options: [
                  { label: 'Active', value: 'Active' },
                  { label: 'Pending', value: 'Pending' },
                  { label: 'Expired', value: 'Expired' },
                ],
              },
            ]}
            onReset={() => {
              setSearchQuery('');
              setStatusFilter('ALL');
            }}
          />

          <DataTable
            columns={registryColumns}
            data={filteredCertificates}
            pageSize={8}
            onRowClick={(c) => setSelectedCertificate(c)}
            emptyMessage="No certificates found matching criteria."
          />
        </div>
      )}

      {/* Digital Certificate Verification Modal */}
      <CertificateVerifyModal
        certificate={selectedCertificate}
        isOpen={!!selectedCertificate}
        onClose={() => setSelectedCertificate(null)}
      />

      {/* DETAILED ATTEMPT REVIEW MODAL */}
      {reviewingItem && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
          <div className="w-full max-w-4xl max-h-[92vh] overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl border border-slate-200">
            {/* Modal Header */}
            <div className="flex items-center justify-between border-b pb-4">
              <div>
                <h4 className="text-lg font-black text-slate-900">
                  Performance & Competency Audit: {reviewingItem.worker_name}
                </h4>
                <p className="text-xs text-slate-500 font-mono">
                  Employee ID: {reviewingItem.employee_id} • Attempt: {reviewingItem.attempt_id || reviewingItem.certificate_number}
                </p>
              </div>
              <button
                onClick={() => setReviewingItem(null)}
                className="rounded-lg p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
              >
                <XCircle className="w-6 h-6" />
              </button>
            </div>

            {feedbackMessage && (
              <div className="my-4 p-3 rounded-xl bg-emerald-50 border border-emerald-300 text-emerald-800 text-xs font-bold text-center">
                {feedbackMessage}
              </div>
            )}

            {loadingDetails ? (
              <div className="py-16 text-center text-slate-500">
                <Clock className="w-8 h-8 animate-spin mx-auto text-amber-500 mb-2" />
                <p className="text-xs font-semibold">Loading authoritative event timeline and telemetry...</p>
              </div>
            ) : attemptDetails ? (
              <div className="space-y-6 pt-4">
                {/* KPI Metrics */}
                <div className="grid grid-cols-2 md:grid-cols-5 gap-3 text-center">
                  <div className="p-3 rounded-xl bg-slate-50 border border-slate-200">
                    <p className="text-[10px] font-bold text-slate-500 uppercase">Authoritative Score</p>
                    <p className="text-2xl font-black text-blue-600 mt-1">{Math.round(attemptDetails.overall_score)} / 100</p>
                  </div>
                  <div className="p-3 rounded-xl bg-slate-50 border border-slate-200">
                    <p className="text-[10px] font-bold text-slate-500 uppercase">Competency Grade</p>
                    <p className="text-sm font-black text-emerald-600 mt-2">{attemptDetails.competency_status}</p>
                  </div>
                  <div className="p-3 rounded-xl bg-slate-50 border border-slate-200">
                    <p className="text-[10px] font-bold text-slate-500 uppercase">Continuous Spray</p>
                    <p className="text-2xl font-black text-amber-600 mt-1">
                      {attemptDetails.spray_contact_duration.toFixed(1)}s
                    </p>
                    <p className="text-[9px] text-slate-400">Target: &gt;= 10.0s</p>
                  </div>
                  <div className="p-3 rounded-xl bg-slate-50 border border-slate-200">
                    <p className="text-[10px] font-bold text-slate-500 uppercase">Spray Resets</p>
                    <p className="text-2xl font-black text-rose-600 mt-1">{attemptDetails.spray_resets}</p>
                    <p className="text-[9px] text-slate-400">{attemptDetails.spray_interruptions} interruptions</p>
                  </div>
                  <div className="p-3 rounded-xl bg-slate-50 border border-slate-200">
                    <p className="text-[10px] font-bold text-slate-500 uppercase">Critical Errors</p>
                    <p className="text-2xl font-black text-rose-700 mt-1">{attemptDetails.critical_errors}</p>
                  </div>
                </div>

                {/* Behavioral Reaction Timings */}
                <div className="p-4 rounded-xl bg-slate-50 border border-slate-200 space-y-2">
                  <h5 className="text-xs font-bold uppercase text-slate-700 flex items-center gap-1.5">
                    <Clock className="w-4 h-4 text-amber-500" />
                    <span>Real Interaction Timings & Ergonomic Readiness</span>
                  </h5>
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-xs">
                    <div>
                      <span className="text-slate-500 text-[11px]">Hazard Recognition:</span>{' '}
                      <strong>{attemptDetails.hazard_response_time ? `${attemptDetails.hazard_response_time.toFixed(1)}s` : 'Recorded'}</strong>
                    </div>
                    <div>
                      <span className="text-slate-500 text-[11px]">Alarm Activation:</span>{' '}
                      <strong>{attemptDetails.alarm_response_time ? `${attemptDetails.alarm_response_time.toFixed(1)}s` : 'Recorded'}</strong>
                    </div>
                    <div>
                      <span className="text-slate-500 text-[11px]">Extinguisher Picked:</span>{' '}
                      <strong>{attemptDetails.extinguisher_selection_time ? `${attemptDetails.extinguisher_selection_time.toFixed(1)}s` : 'Recorded'}</strong>
                    </div>
                    <div>
                      <span className="text-slate-500 text-[11px]">Safety Pin Removed:</span>{' '}
                      <strong>{attemptDetails.pin_removal_time ? `${attemptDetails.pin_removal_time.toFixed(1)}s` : 'Recorded'}</strong>
                    </div>
                  </div>
                </div>

                {/* Event Timeline */}
                <div className="space-y-2">
                  <h5 className="text-xs font-bold uppercase text-slate-700 flex items-center gap-1.5">
                    <FileText className="w-4 h-4 text-blue-500" />
                    <span>Authoritative Event-by-Event Timeline ({attemptDetails.timeline?.length || 0} Events)</span>
                  </h5>
                  <div className="max-h-56 overflow-y-auto border rounded-xl divide-y text-xs">
                    {attemptDetails.timeline?.map((item: any, i: number) => (
                      <div key={i} className="flex items-center justify-between p-2.5 hover:bg-slate-50">
                        <div className="flex items-center gap-3">
                          <span className="font-mono text-[10px] text-slate-400 w-16">{item.timestamp}</span>
                          <span className="font-semibold text-slate-800">{item.action}</span>
                        </div>
                        <div className="flex items-center gap-3">
                          <span
                            className={`px-2 py-0.5 rounded text-[10px] font-bold ${
                              item.result === 'Correct'
                                ? 'bg-emerald-100 text-emerald-700'
                                : 'bg-rose-100 text-rose-700'
                            }`}
                          >
                            {item.result}
                          </span>
                          <span className={`font-mono font-bold ${item.score_delta >= 0 ? 'text-emerald-600' : 'text-rose-600'}`}>
                            {item.score_delta >= 0 ? `+${item.score_delta}` : item.score_delta}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Decision Actions */}
                <div className="border-t pt-4 space-y-3">
                  {showRejectBox ? (
                    <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 space-y-3">
                      <h6 className="text-xs font-bold text-rose-900">Mandatory Rejection Reason:</h6>
                      <textarea
                        value={rejectReason}
                        onChange={(e) => setRejectReason(e.target.value)}
                        placeholder="Specify reason (e.g. spray contact was unstable, repeated premature extinguisher attempts, safety interlock bypassed)..."
                        className="w-full text-xs p-2.5 border rounded-lg focus:outline-none focus:ring-2 focus:ring-rose-500"
                        rows={2}
                      />
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setShowRejectBox(false)}
                          className="px-3 py-1.5 text-xs font-semibold text-slate-600 hover:bg-slate-200 rounded-lg"
                        >
                          Cancel
                        </button>
                        <button
                          onClick={handleReject}
                          disabled={actionLoading}
                          className="px-4 py-1.5 text-xs font-bold bg-rose-600 text-white rounded-lg hover:bg-rose-700"
                        >
                          {actionLoading ? 'Rejecting...' : 'Confirm Rejection'}
                        </button>
                      </div>
                    </div>
                  ) : (
                    <div className="flex items-center justify-between">
                      <div className="text-xs text-slate-500">
                        {attemptDetails.certificate_eligible ? (
                          <span className="text-emerald-700 font-bold flex items-center gap-1">
                            <CheckCircle2 className="w-4 h-4" /> Eligible for Official Issuance
                          </span>
                        ) : (
                          <span className="text-rose-700 font-bold flex items-center gap-1">
                            <AlertTriangle className="w-4 h-4" /> Ineligible: Critical violations or insufficient contact
                          </span>
                        )}
                      </div>
                      <div className="flex items-center gap-3">
                        <button
                          onClick={() => setShowRejectBox(true)}
                          className="px-4 py-2 rounded-xl border border-rose-300 bg-rose-50 text-rose-700 font-bold text-xs hover:bg-rose-100 transition"
                        >
                          Reject Certificate
                        </button>
                        <button
                          onClick={handleApprove}
                          disabled={actionLoading || !attemptDetails.certificate_eligible}
                          className="px-5 py-2 rounded-xl bg-emerald-600 text-white font-bold text-xs hover:bg-emerald-700 transition disabled:opacity-50 shadow-sm"
                        >
                          {actionLoading ? 'Issuing Certificate...' : 'Approve & Issue Certificate'}
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            ) : null}
          </div>
        </div>
      )}

      {/* FULL-SIZE AUTHORITATIVE CERTIFICATE IMAGE MODAL */}
      {viewingImageCert && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/85 p-4 backdrop-blur-md">
          <div className="w-full max-w-4xl max-h-[95vh] overflow-y-auto rounded-3xl bg-slate-900 border-2 border-amber-500/50 p-6 shadow-2xl space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-slate-800">
              <div className="flex items-center gap-3">
                <div className="p-2.5 rounded-xl bg-amber-500/10 border border-amber-500/30 text-suraksha-amber">
                  <ImageIcon className="w-5 h-5" />
                </div>
                <div>
                  <h4 className="text-base font-black text-white flex items-center gap-2">
                    Official Certificate: {viewingImageCert.workerName}
                    <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/20 text-emerald-400 border border-emerald-500/30">
                      Grade A (Competent)
                    </span>
                  </h4>
                  <p className="text-xs text-slate-400 font-mono">
                    ID: {viewingImageCert.certificateId} • {viewingImageCert.employeeId} • Directorate General of Mines Safety
                  </p>
                </div>
              </div>
              <button
                onClick={() => setViewingImageCert(null)}
                className="rounded-xl p-1.5 text-slate-400 hover:bg-slate-800 hover:text-white transition"
              >
                <XCircle className="w-6 h-6" />
              </button>
            </div>

            <div className="rounded-2xl border-2 border-slate-800 bg-black/60 p-2 flex items-center justify-center shadow-inner">
              <img
                src={
                  viewingImageCert.publicImageUrl ||
                  `/api/v1/certificates/public/${encodeURIComponent(viewingImageCert.certificateId)}/image`
                }
                alt={`Official Certificate - ${viewingImageCert.workerName}`}
                className="w-full h-auto max-h-[66vh] object-contain rounded-xl shadow-2xl"
              />
            </div>

            <div className="flex flex-wrap items-center justify-between gap-3 pt-2">
              <div className="text-xs text-slate-400 flex items-center gap-2">
                <Globe className="w-4 h-4 text-emerald-400 shrink-0" />
                <span className="font-semibold">Public Link:</span>
                <a
                  href={
                    viewingImageCert.publicImageUrl ||
                    `/api/v1/certificates/public/${encodeURIComponent(viewingImageCert.certificateId)}/image`
                  }
                  target="_blank"
                  rel="noopener noreferrer"
                  className="font-mono text-amber-400 underline font-bold truncate max-w-xs sm:max-w-md"
                >
                  {viewingImageCert.publicImageUrl || 'Direct Image Link'}
                </a>
              </div>

              <div className="flex items-center gap-2">
                <a
                  href={`/api/v1/certificates/public/${encodeURIComponent(viewingImageCert.certificateId)}/image`}
                  download={`${viewingImageCert.certificateId}.png`}
                  className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-white text-xs font-bold transition border border-slate-700"
                >
                  <Download className="w-4 h-4 text-amber-400" />
                  <span>Download PNG</span>
                </a>
                <a
                  href={`/api/v1/certificates/${encodeURIComponent(viewingImageCert.certificateId)}/pdf`}
                  download={`${viewingImageCert.certificateId}.pdf`}
                  className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 text-xs font-extrabold transition shadow-sm"
                >
                  <Download className="w-4 h-4" />
                  <span>Download PDF</span>
                </a>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
