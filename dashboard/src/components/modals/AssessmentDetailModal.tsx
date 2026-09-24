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
            <div className="p-2.5 rounded-lg bg-blue-50 text-suraksha-blue border border-blue-200">
              <User className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Worker</p>
              <h5 className="text-xs font-bold text-suraksha-heading">{assessment.workerName}</h5>
              <p className="text-[10px] font-medium text-suraksha-subtext">{assessment.employeeId}</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-amber-50 text-suraksha-amber border border-amber-200">
              <Award className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Module & Score</p>
              <h5 className="text-xs font-bold text-suraksha-heading">{assessment.moduleName}</h5>
              <p className="text-[11px] font-bold text-suraksha-amber">{assessment.score}% Overall Score</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-emerald-50 text-emerald-700 border border-emerald-200">
              <Clock className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Duration & Result</p>
              <div className="flex items-center gap-2 mt-0.5">
                <StatusBadge status={assessment.passFail} size="sm" />
                <span className="text-[11px] font-semibold text-suraksha-subtext">{assessment.duration}</span>
              </div>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-purple-50 text-purple-700 border border-purple-200">
              <ShieldCheck className="w-5 h-5" />
            </div>
            <div>
              <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Actions Breakdown</p>
              <p className="text-xs font-bold text-suraksha-heading">
                <span className="text-emerald-700">{assessment.correctActions} Correct</span> /{' '}
                <span className="text-rose-700">{assessment.wrongActions} Wrong</span>
              </p>
            </div>
          </div>
        </div>

        {/* Critical Error Callout (if any) */}
        {assessment.criticalErrors > 0 && (
          <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 flex items-start gap-3 text-rose-900">
            <AlertOctagon className="w-5 h-5 text-rose-600 shrink-0 mt-0.5" />
            <div>
              <h6 className="text-xs font-bold text-rose-800 uppercase tracking-wide">
                Critical Safety Breach Detected ({assessment.criticalErrors} Incident)
              </h6>
              <p className="text-xs mt-1 text-rose-700 font-medium">{assessment.criticalErrorDetails}</p>
            </div>
          </div>
        )}

        {/* Competency Evaluation Dimensions Strip */}
        <div>
          <h5 className="text-xs font-bold uppercase tracking-wider text-suraksha-subtext mb-3">
            Safety Competency Evaluation (5 Core Dimensions)
          </h5>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
            {[
              { key: 'hazard_identification', label: 'Hazard Identification', defaultScore: 85, threshold: 75 },
              { key: 'ppe_selection', label: 'PPE Selection', defaultScore: 90, threshold: 80 },
              { key: 'procedure_compliance', label: 'Procedure Compliance', defaultScore: assessment.wrongActions === 0 ? 95 : 70, threshold: 75 },
              { key: 'equipment_use', label: 'Equipment Operation', defaultScore: assessment.score >= 80 ? 90 : 65, threshold: 70 },
              { key: 'decision_making', label: 'Decision Making', defaultScore: assessment.criticalErrors === 0 ? 88 : 50, threshold: 70 },
            ].map(({ key, label, defaultScore, threshold }) => {
              const comp = assessment.competencyScores?.[key];
              const score = comp ? Math.round(comp.score) : defaultScore;
              const passThresh = comp?.pass_threshold ?? threshold;
              const isPassed = comp ? comp.passed : score >= passThresh;

              return (
                <div
                  key={key}
                  className={`p-3 rounded-xl border transition ${
                    isPassed
                      ? 'bg-emerald-50/50 border-emerald-200 text-emerald-950'
                      : 'bg-rose-50/50 border-rose-200 text-rose-950'
                  }`}
                >
                  <div className="flex items-center justify-between gap-1 mb-1">
                    <span className="text-[10px] font-bold uppercase tracking-wider text-suraksha-subtext">
                      {label}
                    </span>
                    {isPassed ? (
                      <span className="inline-flex items-center gap-0.5 text-[10px] font-bold text-emerald-700">
                        <CheckCircle2 className="w-3.5 h-3.5" /> Pass
                      </span>
                    ) : (
                      <span className="inline-flex items-center gap-0.5 text-[10px] font-bold text-rose-700">
                        <XCircle className="w-3.5 h-3.5" /> Fail
                      </span>
                    )}
                  </div>
                  <div className="flex items-baseline justify-between mt-1">
                    <span className={`text-lg font-black ${isPassed ? 'text-emerald-700' : 'text-rose-700'}`}>
                      {score}%
                    </span>
                    <span className="text-[10px] font-medium text-suraksha-subtext">
                      Min {passThresh}%
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Flagged Weaknesses (if any) */}
        {assessment.weaknesses && assessment.weaknesses.length > 0 && (
          <div className="p-4 rounded-xl bg-amber-50 border border-amber-200">
            <h6 className="text-xs font-bold text-amber-900 uppercase tracking-wide mb-2 flex items-center gap-1.5">
              <AlertOctagon className="w-4 h-4 text-amber-700" />
              Identified Competency Weaknesses & Retraining Focus
            </h6>
            <div className="space-y-1.5 text-xs text-amber-950">
              {assessment.weaknesses.map((w, idx) => (
                <div key={idx} className="flex items-start justify-between gap-2 p-2 rounded-lg bg-white/70 border border-amber-200/60">
                  <span className="font-semibold">{w.competency_name}</span>
                  <span className="text-[11px] text-amber-800 font-medium">{w.reason || 'Score below qualification threshold'}</span>
                </div>
              ))}
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
                  <div className="flex items-center justify-center w-7 h-7 rounded-lg bg-suraksha-card border border-suraksha-border text-xs font-bold text-suraksha-amber shrink-0 shadow-sm">
                    {step.stepIndex}
                  </div>
                  <div>
                    <h6 className="text-xs font-bold text-suraksha-heading">{step.stepName}</h6>
                    <p className="text-[11px] text-suraksha-subtext mt-0.5 font-medium">
                      <span className="text-suraksha-subtext font-bold">Target:</span> {step.expectedAction}
                    </p>
                    <p className="text-[11px] text-suraksha-heading mt-1 font-mono font-semibold bg-white px-2 py-0.5 rounded border border-suraksha-border inline-block shadow-sm">
                      Performed: {step.performedAction}
                    </p>
                  </div>
                </div>

                <div className="text-right shrink-0">
                  <div className="flex items-center justify-end gap-1.5">
                    {step.result === 'pass' && <CheckCircle2 className="w-4 h-4 text-emerald-600" />}
                    {step.result === 'fail' && <XCircle className="w-4 h-4 text-amber-600" />}
                    {step.result === 'critical_error' && <AlertOctagon className="w-4 h-4 text-rose-600" />}
                    <span
                      className={`text-xs font-bold ${
                        step.result === 'pass'
                          ? 'text-emerald-700'
                          : step.result === 'critical_error'
                          ? 'text-rose-700'
                          : 'text-amber-700'
                      }`}
                    >
                      {step.result === 'pass' ? `+${step.scoreDelta} PTS` : `${step.scoreDelta} PTS`}
                    </span>
                  </div>
                  <p className="text-[10px] text-suraksha-subtext mt-1 font-medium">{step.timestamp}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </Modal>
  );
};
