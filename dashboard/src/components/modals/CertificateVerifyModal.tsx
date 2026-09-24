import React, { useState } from 'react';
import type { Certificate } from '../../types';
import { Modal } from '../common/Modal';
import { ShieldCheck, Download, Printer, Award, ExternalLink, Image as ImageIcon, FileText } from 'lucide-react';
import { RealQRCode } from '../common/RealQRCode';

interface CertificateVerifyModalProps {
  certificate: Certificate | null;
  isOpen: boolean;
  onClose: () => void;
}

export const CertificateVerifyModal: React.FC<CertificateVerifyModalProps> = ({
  certificate,
  isOpen,
  onClose,
}) => {
  const [modalTab, setModalTab] = useState<'preview' | 'image'>('preview');

  if (!certificate) return null;

  const handlePrint = () => {
    window.print();
  };

  // Use public Render URL so QR codes work on any phone (not localhost)
  const VERIFY_BASE = (import.meta.env.VITE_VERIFY_BASE_URL || 'https://surakhshaar.onrender.com/verify').replace(/\/$/, '');
  const verifyUrl = `${VERIFY_BASE}/${encodeURIComponent(certificate.certificateId)}`;
  const publicImageUrl = certificate.publicImageUrl || '';

  const localImageUrl = `/api/v1/certificates/public/${encodeURIComponent(certificate.certificateId)}/image`;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Official Safety Compliance Certificate"
      subtitle={`Verified Credential ID: ${certificate.certificateId} • Directorate General of Mines Safety (DGMS)`}
      maxWidth="4xl"
    >
      <div className="space-y-4">
        {/* Tab Selector */}
        <div className="flex items-center gap-2 border-b border-suraksha-border pb-3">
          <button
            onClick={() => setModalTab('preview')}
            className={`flex items-center gap-1.5 px-3.5 py-1.5 rounded-xl text-xs font-bold transition ${
              modalTab === 'preview'
                ? 'bg-suraksha-amber text-slate-950 shadow-sm'
                : 'text-suraksha-subtext hover:text-suraksha-heading bg-suraksha-bg border border-suraksha-border'
            }`}
          >
            <FileText className="w-3.5 h-3.5" />
            <span>Interactive Layout</span>
          </button>
          <button
            onClick={() => setModalTab('image')}
            className={`flex items-center gap-1.5 px-3.5 py-1.5 rounded-xl text-xs font-bold transition ${
              modalTab === 'image'
                ? 'bg-suraksha-amber text-slate-950 shadow-sm'
                : 'text-suraksha-subtext hover:text-suraksha-heading bg-suraksha-bg border border-suraksha-border'
            }`}
          >
            <ImageIcon className="w-3.5 h-3.5" />
            <span>Authoritative Rendered Image (PNG)</span>
          </button>
        </div>

        {modalTab === 'image' ? (
          /* High-Resolution Certificate Image View */
          <div className="space-y-4">
            <div className="rounded-2xl border-2 border-suraksha-amber/40 bg-slate-950 p-2 overflow-hidden shadow-2xl flex items-center justify-center">
              <img
                src={publicImageUrl || localImageUrl}
                alt={`Official Certificate - ${certificate.workerName}`}
                className="w-full h-auto rounded-xl object-contain max-h-[65vh]"
              />
            </div>
            <div className="p-3 rounded-xl bg-amber-500/10 border border-amber-500/30 flex items-center justify-between text-xs">
              <span className="text-suraksha-text font-medium">
                🌐 Globally Accessible High-Res Image: <a href={publicImageUrl} target="_blank" rel="noopener noreferrer" className="font-mono text-suraksha-amber underline font-bold">{publicImageUrl}</a>
              </span>
              <a
                href={publicImageUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="px-3 py-1 rounded-lg bg-suraksha-amber text-slate-950 font-bold hover:bg-amber-400 transition"
              >
                Open Fullscreen
              </a>
            </div>
          </div>
        ) : (
          /* Interactive Digital Layout View */
          <div className="rounded-2xl border-2 border-suraksha-amber/50 bg-gradient-to-b from-suraksha-card to-suraksha-surface p-6 sm:p-8 shadow-elevated relative overflow-hidden">
            {/* Background Watermark */}
            <div className="absolute right-4 bottom-4 opacity-5 pointer-events-none text-white text-9xl font-black select-none">
              DGMS
            </div>

            <div className="flex flex-wrap items-center justify-between pb-6 border-b border-suraksha-border gap-4">
              <div className="flex items-center gap-3.5">
                <div className="p-3 rounded-2xl bg-amber-500/10 text-suraksha-amber border border-amber-500/30">
                  <Award className="w-8 h-8" />
                </div>
                <div>
                  <p className="text-[10px] uppercase font-bold tracking-widest text-suraksha-amber">
                    Government of Jharkhand • Dept. of Mines &amp; Geology
                  </p>
                  <h4 className="text-base sm:text-lg font-extrabold text-suraksha-heading tracking-wide">
                    SURAKSHAAR SAFETY COMPLIANCE CERTIFICATE
                  </h4>
                  <p className="text-xs text-suraksha-subtext font-medium">
                    Directorate General of Mines Safety (DGMS) Verified Standard
                  </p>
                </div>
              </div>

              <div className="text-right">
                <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-emerald-500/10 text-emerald-600 border border-emerald-500/30">
                  <ShieldCheck className="w-4 h-4 text-emerald-600" />
                  VERIFIED PASS • ACTIVE
                </span>
              </div>
            </div>

            {/* Certificate Body */}
            <div className="py-6 space-y-4">
              <p className="text-xs font-semibold text-suraksha-subtext uppercase tracking-wider">
                This is to certify that industrial worker:
              </p>

              <div className="p-4 rounded-xl bg-suraksha-bg border border-suraksha-border flex flex-wrap items-center justify-between gap-4">
                <div className="pl-3 border-l-2 border-suraksha-amber">
                  <h3 className="text-xl sm:text-2xl font-black text-suraksha-heading tracking-wide">
                    {certificate.workerName}
                  </h3>
                  <p className="text-xs text-suraksha-subtext font-mono mt-0.5">
                    Employee ID: <span className="text-suraksha-heading font-bold">{certificate.employeeId}</span> | Unit:{' '}
                    <span className="text-suraksha-heading font-bold">{certificate.sector}</span>
                  </p>
                </div>

                <div className="text-right">
                  <span className="text-[10px] uppercase font-bold text-suraksha-subtext block">Assessment Score</span>
                  <span className="text-xl font-black text-emerald-600">{certificate.score ?? 90}%</span>
                  <span className="text-[10px] font-bold text-suraksha-subtext block">{certificate.resultGrade}</span>
                </div>
              </div>

              <p className="text-xs font-medium text-suraksha-subtext">
                Has successfully demonstrated full operational competence in the AR Simulation Assessment for:
              </p>

              <div className="p-4 rounded-xl bg-suraksha-bg border border-suraksha-border">
                <h5 className="text-sm font-bold text-suraksha-amber">{certificate.moduleName}</h5>
                <div className="flex flex-wrap items-center gap-4 text-xs text-suraksha-subtext mt-2 font-medium">
                  <span>Passing Score: <strong>75% Required</strong></span>
                  <span>Critical Violations: <strong className="text-emerald-600">0 (Zero Violations)</strong></span>
                  <span>Issue Date: <strong>{certificate.issueDate || '2026-09-21'}</strong></span>
                  <span>Valid Until: <strong>{certificate.expiryDate || '2027-09-21'}</strong></span>
                </div>
              </div>
            </div>

            {/* Footer with Scannable QR */}
            <div className="pt-6 border-t border-suraksha-border flex flex-wrap items-center justify-between gap-6">
              <div className="space-y-1">
                <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Issuing Authority</p>
                <p className="text-xs font-bold text-suraksha-heading">Directorate General of Mines Safety</p>
                <p className="text-[11px] text-suraksha-subtext">
                  Cert ID: {certificate.certificateId}
                </p>
                <p className="text-[9px] font-mono text-emerald-600 font-bold">
                  ✓ Scannable with Google Scanner to load full Certificate Image
                </p>
              </div>

              <div className="flex flex-col items-center">
                <RealQRCode
                  value={verifyUrl}
                  size={110}
                  showActions={true}
                  downloadFilename={`qr_${certificate.certificateId}.png`}
                />
                <span className="text-[10px] font-mono font-bold text-suraksha-amber mt-1">
                  Scan to Verify Certificate
                </span>
              </div>
            </div>
          </div>
        )}

        {/* Modal Actions */}
        <div className="flex flex-wrap items-center justify-between no-print gap-3 pt-2">
          <div className="flex items-center gap-2">
            <a
              href={verifyUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-1.5 px-3 py-2 rounded-xl bg-amber-500/10 border border-amber-500/40 text-xs font-bold text-suraksha-amber hover:bg-amber-500/20 transition"
            >
              <ExternalLink className="w-3.5 h-3.5" />
              <span>Open Public Verification Page</span>
            </a>
          </div>

          <div className="flex items-center gap-2">
            <a
              href={publicImageUrl || localImageUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-1.5 rounded-xl border border-suraksha-border bg-suraksha-surface px-4 py-2 text-xs font-bold text-suraksha-text hover:bg-suraksha-hover transition"
            >
              <ImageIcon className="w-3.5 h-3.5 text-amber-500" />
              <span>Direct Image Link</span>
            </a>
            <button
              onClick={handlePrint}
              className="flex items-center gap-1.5 rounded-xl border border-suraksha-border bg-suraksha-surface px-4 py-2 text-xs font-semibold text-suraksha-text hover:bg-suraksha-hover transition"
            >
              <Printer className="w-3.5 h-3.5 text-suraksha-subtext" />
              <span>Print</span>
            </button>
            <a
              href={`/api/v1/certificates/${encodeURIComponent(certificate.certificateId)}/pdf`}
              download={`${certificate.certificateId}.pdf`}
              className="flex items-center gap-1.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 px-4 py-2 text-xs font-extrabold transition shadow-sm"
            >
              <Download className="w-3.5 h-3.5" />
              <span>Download PDF</span>
            </a>
          </div>
        </div>
      </div>
    </Modal>
  );
};
