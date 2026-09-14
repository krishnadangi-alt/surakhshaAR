import React from 'react';
import type { Assessment } from '../../types';
import { Modal } from '../common/Modal';
import { StatusBadge } from '../common/StatusBadge';
import { CheckCircle2, XCircle, AlertOctagon, Clock, ShieldCheck, User, Award } from 'lucide-react';

interface AssessmentDetailModalProps {
  assessment: Assessment | null;
  isOpen: boolean;
  onClose: () => void;
}

export const AssessmentDetailModal: React.FC<AssessmentDetailModalProps> = ({
  assessment,
  isOpen,
  onClose,
}) => {
  if (!assessment) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="AR Assessment Audit Dossier"
      subtitle={`Trial Ref ID: ${assessment.id} | ${assessment.dateTime}`}
      maxWidth="4xl"
    >
      <div className="space-y-6">
        {/* Header Summary Banner */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 p-4 rounded-xl bg-suraksha-surface border border-suraksha-border">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-blue-500/10 text-suraksha-blue border border-blue-500/20">
              <User className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Worker</p>
              <h5 className="text-xs font-bold text-white">{assessment.workerName}</h5>
              <p className="text-[10px] text-suraksha-subtext">{assessment.employeeId}</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-amber-500/10 text-suraksha-amber border border-amber-500/20">
              <Award className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Module & Score</p>
              <h5 className="text-xs font-bold text-white">{assessment.moduleName}</h5>
              <p className="text-[11px] font-bold text-suraksha-amber">{assessment.score}% Overall Score</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
              <Clock className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Duration & Result</p>
              <div className="flex items-center gap-2 mt-0.5">
                <StatusBadge status={assessment.passFail} size="sm" />
                <span className="text-[11px] font-medium text-suraksha-subtext">{assessment.duration}</span>
              </div>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-purple-500/10 text-purple-400 border border-purple-500/20">
              <ShieldCheck className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Actions Breakdown</p>
              <p className="text-xs font-semibold text-white">
                <span className="text-emerald-400">{assessment.correctActions} Correct</span> /{' '}
                <span className="text-rose-400">{assessment.wrongActions} Wrong</span>
              </p>
            </div>
          </div>
        </div>

        {/* Critical Error Callout (if any) */}
        {assessment.criticalErrors > 0 && (
          <div className="p-4 rounded-xl bg-rose-500/10 border border-rose-500/30 flex items-start gap-3 text-rose-300">
            <AlertOctagon className="w-5 h-5 text-rose-400 shrink-0 mt-0.5" />
            <div>
              <h6 className="text-xs font-bold text-rose-200 uppercase tracking-wide">
                Critical Safety Breach Detected ({assessment.criticalErrors} Incident)
              </h6>
              <p className="text-xs mt-1 text-rose-300/90">{assessment.criticalErrorDetails}</p>
            </div>
          </div>
        )}

        {/* Step-by-Step Action Telemetry Timeline */}
        <div>
          <h5 className="text-xs font-bold uppercase tracking-wider text-suraksha-subtext mb-3">
            AR Trial Step Telemetry & Actions Log
          </h5>
          <div className="space-y-3">
            {assessment.stepDetails.map((step) => (
              <div
                key={step.stepIndex}
                className="flex items-start justify-between p-3.5 rounded-xl border border-suraksha-border bg-suraksha-surface/60 transition hover:border-suraksha-borderLight"
              >
                <div className="flex items-start gap-3">
                  <div className="flex items-center justify-center w-7 h-7 rounded-lg bg-suraksha-card border border-suraksha-border text-xs font-bold text-suraksha-amber shrink-0">
                    {step.stepIndex}
                  </div>
                  <div>
                    <h6 className="text-xs font-bold text-white">{step.stepName}</h6>
                    <p className="text-[11px] text-suraksha-subtext mt-0.5">
                      <span className="text-suraksha-subtext/70 font-medium">Target:</span> {step.expectedAction}
                    </p>
                    <p className="text-[11px] text-white mt-1 font-mono bg-suraksha-card px-2 py-0.5 rounded border border-suraksha-border inline-block">
                      Performed: {step.performedAction}
                    </p>
                  </div>
                </div>

                <div className="text-right shrink-0">
                  <div className="flex items-center justify-end gap-1.5">
                    {step.result === 'pass' && <CheckCircle2 className="w-4 h-4 text-emerald-400" />}
                    {step.result === 'fail' && <XCircle className="w-4 h-4 text-amber-400" />}
                    {step.result === 'critical_error' && <AlertOctagon className="w-4 h-4 text-rose-400" />}
                    <span
                      className={`text-xs font-bold ${
                        step.result === 'pass'
                          ? 'text-emerald-400'
                          : step.result === 'critical_error'
                          ? 'text-rose-400'
                          : 'text-amber-400'
                      }`}
                    >
                      {step.result === 'pass' ? `+${step.scoreDelta} PTS` : `${step.scoreDelta} PTS`}
                    </span>
                  </div>
                  <p className="text-[10px] text-suraksha-subtext mt-1">{step.timestamp}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </Modal>
  );
};
