import React from 'react';
import type { Certificate } from '../../types';
import { Modal } from '../common/Modal';
import { ShieldCheck, QrCode, Download, Printer, CheckCircle, Award } from 'lucide-react';

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
  if (!certificate) return null;

  const handlePrint = () => {
    window.print();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Safety Compliance Certificate — Demo Preview"
      subtitle={`Simulated Registry Record ID: ${certificate.certificateId}`}
      maxWidth="2xl"
    >
      <div className="space-y-6">
        {/* Certificate Printable Area (SIMULATED) */}
        <div className="rounded-2xl border-2 border-suraksha-amber/40 bg-gradient-to-b from-suraksha-card to-suraksha-surface p-8 shadow-elevated relative overflow-hidden">
          {/* Subtle Background Watermark */}
          <div className="absolute right-4 bottom-4 opacity-5 pointer-events-none text-white text-9xl font-black">
            JH
          </div>

          <div className="flex items-center justify-between pb-6 border-b border-suraksha-border">
            <div className="flex items-center gap-3">
              <div className="p-3 rounded-xl bg-amber-500/10 text-suraksha-amber border border-amber-500/30">
                <Award className="w-7 h-7" />
              </div>
              <div>
                <p className="text-[10px] uppercase font-bold tracking-widest text-suraksha-amber">
                  Government of Jharkhand (Branding)
                </p>
                <h4 className="text-base font-extrabold text-white tracking-wide">
                  SURAKSHAAR SAFETY COMPLIANCE CERTIFICATE
                </h4>
                <p className="text-xs text-suraksha-subtext">
                  Industrial Safety Training &amp; Compliance Platform — Simulated Record
                </p>
              </div>
            </div>

            <div className="text-right">
              <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-emerald-500/10 text-emerald-400 border border-emerald-500/30">
                <CheckCircle className="w-3.5 h-3.5" /> DEMO — VERIFICATION PASSED
              </span>
            </div>
          </div>

          {/* Demo banner */}
          <div className="mt-4 flex items-center gap-2 rounded-lg border border-amber-500/30 bg-amber-500/10 px-3 py-2 text-[11px] font-semibold text-suraksha-amber">
            <ShieldCheck className="w-4 h-4 shrink-0" />
            This is a simulated demo UI preview. It is not an official government-issued certificate.
          </div>

          <div className="py-6 space-y-4">
            <p className="text-xs text-suraksha-subtext">This is to certify that industrial worker:</p>
            <div className="pl-4 border-l-2 border-suraksha-amber">
              <h3 className="text-xl font-bold text-white tracking-wide">{certificate.workerName}</h3>
              <p className="text-xs text-suraksha-subtext font-mono mt-0.5">
                Employee ID: <span className="text-white font-semibold">{certificate.employeeId}</span> | Unit:{' '}
                <span className="text-white font-semibold">{certificate.sector}</span>
              </p>
            </div>

            <p className="text-xs text-suraksha-subtext">
              Has successfully demonstrated full operational competence in the AR Simulation Assessment for:
            </p>

            <div className="p-4 rounded-xl bg-suraksha-bg border border-suraksha-border">
              <h5 className="text-sm font-bold text-suraksha-amber">{certificate.moduleName}</h5>
              <div className="flex items-center gap-4 text-xs text-suraksha-subtext mt-2">
                <span>Grade Achieved: <strong className="text-white">{certificate.resultGrade}</strong></span>
                <span>Issue Date: <strong className="text-white">{certificate.issueDate}</strong></span>
                <span>Valid Until: <strong className="text-white">{certificate.expiryDate}</strong></span>
              </div>
            </div>
          </div>

          {/* Verification Area (Simulated) */}
          <div className="pt-6 border-t border-suraksha-border flex items-end justify-between">
            <div className="space-y-1">
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Issuing Registry</p>
              <p className="text-xs font-semibold text-white">{certificate.issuerDepartment}</p>
              <p className="text-[10px] font-mono text-suraksha-subtext truncate max-w-xs">
                {certificate.verificationCode}
              </p>
            </div>

            <div className="flex flex-col items-center">
              <div className="p-2 rounded-lg bg-white text-black mb-1">
                <QrCode className="w-12 h-12" />
              </div>
              <span className="text-[9px] font-mono text-suraksha-subtext">Demo QR — Not Scannable</span>
            </div>
          </div>
        </div>

        {/* Modal Actions */}
        <div className="flex items-center justify-between no-print gap-3">
          <span className="text-xs text-suraksha-subtext flex items-center gap-1.5">
            <ShieldCheck className="w-4 h-4 text-amber-400" />
            Simulated integrity code — not a cryptographic signature.
          </span>
          <div className="flex items-center gap-2">
            <button
              onClick={handlePrint}
              className="flex items-center gap-1.5 rounded-lg border border-suraksha-border bg-suraksha-surface px-4 py-2 text-xs font-semibold text-white hover:bg-suraksha-hover transition"
            >
              <Printer className="w-3.5 h-3.5 text-suraksha-subtext" />
              <span>Print Preview</span>
            </button>
            <button
              onClick={() => alert(`Demo: PDF export of ${certificate.certificateId} is simulated in this phase.`)}
              className="flex items-center gap-1.5 rounded-lg bg-suraksha-blue px-4 py-2 text-xs font-semibold text-white hover:bg-suraksha-blueHover transition shadow-subtle"
            >
              <Download className="w-3.5 h-3.5" />
              <span>Download PDF (Demo)</span>
            </button>
          </div>
        </div>
      </div>
    </Modal>
  );
};
