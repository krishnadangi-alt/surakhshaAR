import React, { useState } from 'react';
import { getWorkerById, getAssessmentsByWorkerId, getCertificatesByWorkerId, getRetrainingByWorkerId } from '../mockData';
import { StatusBadge } from '../components/common/StatusBadge';
import { AssessmentDetailModal } from '../components/modals/AssessmentDetailModal';
import { CertificateVerifyModal } from '../components/modals/CertificateVerifyModal';
import { AssignRetrainingModal } from '../components/modals/AssignRetrainingModal';
import type { Assessment, Certificate } from '../types';
import {
  AlertOctagon,
  RotateCcw,
  ArrowLeft,
  Eye,
} from 'lucide-react';
import { ResponsiveContainer, RadarChart, PolarGrid, PolarAngleAxis, PolarRadiusAxis, Radar } from 'recharts';

interface WorkerDetailsScreenProps {
  workerId: string;
  onBack: () => void;
}

export const WorkerDetailsScreen: React.FC<WorkerDetailsScreenProps> = ({ workerId, onBack }) => {
  const worker = getWorkerById(workerId);
  const assessments = getAssessmentsByWorkerId(worker.id);
  const certificates = getCertificatesByWorkerId(worker.id);
  const retrainingPlans = getRetrainingByWorkerId(worker.id);

  const [selectedAssessment, setSelectedAssessment] = useState<Assessment | null>(null);
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null);
  const [isAssignRetrainingOpen, setIsAssignRetrainingOpen] = useState(false);

  // Radar data for this worker (aligned to the 6 competency dimensions)
  const workerRadarData = [
    { dimension: 'Hazard Identification', score: Math.min(100, worker.latestScore + 2) },
    { dimension: 'SCBA & Gas Handling', score: Math.max(0, worker.latestScore - 5) },
    { dimension: 'Fire Suppression', score: Math.min(100, worker.latestScore + 4) },
    { dimension: 'LOTO Equipment Protocol', score: Math.max(0, worker.latestScore - 2) },
    { dimension: 'Emergency Evacuation', score: Math.min(100, worker.latestScore + 1) },
    { dimension: 'Incident Escalation', score: Math.min(100, worker.latestScore + 3) },
  ];

  return (
    <div className="space-y-6">
      {/* Top Back Navigation & Actions */}
      <div className="flex items-center justify-between">
        <button
          onClick={onBack}
          className="flex items-center gap-2 rounded-lg border border-suraksha-border bg-suraksha-card px-3.5 py-2 text-xs font-semibold text-suraksha-subtext hover:bg-suraksha-hover hover:text-white transition"
        >
          <ArrowLeft className="w-4 h-4" />
          <span>Back to Workers Directory</span>
        </button>

        <div className="flex items-center gap-2">
          <button
            onClick={() => setIsAssignRetrainingOpen(true)}
            className="flex items-center gap-1.5 rounded-lg border border-suraksha-amber/40 bg-amber-500/10 px-3.5 py-2 text-xs font-bold text-suraksha-amber hover:bg-amber-500/20 transition"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            <span>Assign Retraining Plan</span>
          </button>
        </div>
      </div>

      {/* Header Profile Dossier Card */}
      <div className="rounded-2xl border border-suraksha-border bg-suraksha-card p-6 shadow-card">
        <div className="flex flex-wrap items-start justify-between gap-4 border-b border-suraksha-border pb-6">
          <div className="flex items-center gap-4">
            <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-suraksha-surface border border-suraksha-border font-extrabold text-xl text-suraksha-amber shadow-subtle">
              {worker.name.split(' ').map((n) => n[0]).join('')}
            </div>
            <div>
              <div className="flex items-center gap-3">
                <h2 className="text-xl font-bold text-white tracking-wide">{worker.name}</h2>
                <StatusBadge status={worker.overallStatus} size="md" />
              </div>
              <p className="text-xs font-mono text-suraksha-subtext mt-1">
                Employee ID: <span className="text-white font-semibold">{worker.employeeId}</span> • Role:{' '}
                <span className="text-white font-semibold">{worker.role}</span>
              </p>
              <p className="text-xs text-suraksha-subtext mt-0.5">
                {worker.plant} ({worker.sector})
              </p>
            </div>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 text-xs">
            <div className="p-2.5 rounded-xl bg-suraksha-surface border border-suraksha-border">
              <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Safety Officer</span>
              <span className="font-bold text-white">{worker.safetyOfficer}</span>
            </div>
            <div className="p-2.5 rounded-xl bg-suraksha-surface border border-suraksha-border">
              <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Joined Date</span>
              <span className="font-bold text-white">{worker.joinedDate}</span>
            </div>
            <div className="p-2.5 rounded-xl bg-suraksha-surface border border-suraksha-border col-span-2 sm:col-span-1">
              <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Contact</span>
              <span className="font-bold text-white">{worker.phone}</span>
            </div>
          </div>
        </div>

        {/* Summary Metric Ribbon */}
        <div className="grid grid-cols-2 md:grid-cols-5 gap-3 pt-4 text-center">
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Modules Completed</p>
            <p className="text-lg font-bold text-white mt-0.5">{worker.modulesCompleted} / 3</p>
          </div>
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Latest AR Score</p>
            <p className="text-lg font-bold text-emerald-400 mt-0.5">{worker.latestScore}%</p>
          </div>
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Overall Competency</p>
            <p className="text-lg font-bold text-suraksha-amber mt-0.5">{worker.overallCompetency}</p>
          </div>
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Active Certificates</p>
            <p className="text-lg font-bold text-white mt-0.5">{worker.certificatesCount}</p>
          </div>
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60 col-span-2 md:col-span-1">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Retraining Status</p>
            <p className="text-sm font-bold text-white mt-1">
              <StatusBadge status={worker.retrainingStatus} size="sm" />
            </p>
          </div>
        </div>
      </div>

      {/* Main Grid Content Area */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column: Module Progress & Assessment Timeline (2 cols wide) */}
        <div className="lg:col-span-2 space-y-6">
          {/* Module Progress Section */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-white mb-4">
              Module Progress Breakdown
            </h4>

            <div className="space-y-4">
              {worker.moduleProgressList.map((m) => (
                <div
                  key={m.moduleId}
                  className="p-4 rounded-xl border border-suraksha-border bg-suraksha-surface/60 space-y-2"
                >
                  <div className="flex items-center justify-between text-xs">
                    <div>
                      <h5 className="font-bold text-white">{m.moduleName}</h5>
                      <p className="text-[11px] text-suraksha-subtext">{m.stage}</p>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="font-bold text-emerald-400">{m.score}% Score</span>
                      <StatusBadge status={m.status} size="sm" />
                    </div>
                  </div>

                  <div className="h-2 w-full bg-suraksha-card rounded-full overflow-hidden border border-suraksha-border">
                    <div
                      className="h-full bg-suraksha-blue rounded-full transition-all duration-500"
                      style={{ width: `${m.completionPercentage}%` }}
                    />
                  </div>

                  <div className="flex justify-between text-[10px] text-suraksha-subtext pt-1">
                    <span>Completion: {m.completionPercentage}%</span>
                    <span>Last Assessed: {m.lastUpdated}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Assessment History Section */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-white mb-4">
              AR Assessment History Logs
            </h4>

            <div className="space-y-3">
              {assessments.length > 0 ? (
                assessments.map((a) => (
                  <div
                    key={a.id}
                    onClick={() => setSelectedAssessment(a)}
                    className="p-3.5 rounded-xl border border-suraksha-border bg-suraksha-surface/60 hover:border-suraksha-borderLight hover:bg-suraksha-hover cursor-pointer transition flex flex-wrap items-center justify-between gap-3"
                  >
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-bold text-white">{a.scenarioName}</span>
                        <StatusBadge status={a.passFail} size="sm" />
                      </div>
                      <p className="text-[10px] text-suraksha-subtext mt-0.5">
                        Module: {a.moduleName} • Duration: {a.duration} • {a.dateTime}
                      </p>
                    </div>

                    <div className="flex items-center gap-4">
                      <div className="text-right">
                        <span className="text-xs font-bold text-emerald-400">{a.score}% Score</span>
                        <p className="text-[10px] text-suraksha-subtext">{a.correctActions} Correct / {a.wrongActions} Wrong</p>
                      </div>
                      <button className="p-1.5 rounded-lg border border-suraksha-border text-suraksha-amber hover:bg-suraksha-card">
                        <Eye className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                ))
              ) : (
                <div className="p-6 text-center text-xs text-suraksha-subtext bg-suraksha-surface/30 rounded-xl">
                  No historical AR assessment logs found for this worker profile.
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Right Column: Competency Radar, Weaknesses, Certificates, Retention */}
        <div className="space-y-6">
          {/* Competency Radar Profile Card */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-white mb-2">
              Competency Radar Profile
            </h4>
            <div className="h-56 w-full">
              <ResponsiveContainer width="100%" height="100%">
                <RadarChart cx="50%" cy="50%" outerRadius="70%" data={workerRadarData}>
                  <PolarGrid stroke="#1E3A5F" />
                  <PolarAngleAxis dataKey="dimension" stroke="#94A3B8" fontSize={9} />
                  <PolarRadiusAxis angle={30} domain={[0, 100]} stroke="#1E3A5F" fontSize={8} />
                  <Radar name="Worker" dataKey="score" stroke="#F59E0B" fill="#F59E0B" fillOpacity={0.4} />
                </RadarChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Identified Weak Areas */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-white mb-3">
              Identified Weak Protocols
            </h4>
            <div className="space-y-2">
              {worker.weakAreas.length > 0 ? (
                worker.weakAreas.map((area, i) => (
                  <div key={i} className="flex items-center gap-2 p-2.5 rounded-lg bg-rose-500/10 border border-rose-500/20 text-xs text-rose-300">
                    <AlertOctagon className="w-4 h-4 text-rose-400 shrink-0" />
                    <span>{area}</span>
                  </div>
                ))
              ) : (
                <div className="p-3 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-xs text-emerald-400 text-center font-medium">
                  ✓ No safety weaknesses currently flagged
                </div>
              )}
            </div>
          </div>

          {/* Retraining Plan */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <div className="flex items-center justify-between mb-3">
              <h4 className="text-sm font-bold uppercase tracking-wider text-white">Retraining Plan</h4>
              <button
                onClick={() => setIsAssignRetrainingOpen(true)}
                className="flex items-center gap-1.5 rounded-lg border border-amber-500/30 bg-amber-500/10 px-2.5 py-1 text-[11px] font-bold text-suraksha-amber hover:bg-amber-500/20 transition"
              >
                <RotateCcw className="w-3 h-3" />
                Assign
              </button>
            </div>

            {retrainingPlans.length > 0 ? (
              <div className="space-y-2">
                {retrainingPlans.map((plan) => (
                  <div
                    key={plan.id}
                    className="p-3 rounded-xl border border-suraksha-border bg-suraksha-surface/60 text-xs"
                  >
                    <div className="flex items-center justify-between">
                      <h6 className="font-bold text-white">{plan.moduleName}</h6>
                      <StatusBadge status={plan.status} size="sm" />
                    </div>
                    <p className="text-[11px] text-rose-300 mt-1.5">Weakness: {plan.weakArea}</p>
                    <p className="text-[11px] text-suraksha-subtext mt-0.5">Plan: {plan.recommendation}</p>
                    <div className="flex items-center justify-between text-[10px] text-suraksha-subtext mt-2 pt-2 border-t border-suraksha-border/40">
                      <span>
                        Initial <strong className="text-rose-400">{plan.initialScore}%</strong>
                        {plan.reassessmentScore !== undefined && (
                          <>
                            {' '}→ Re-assessed <strong className="text-emerald-400">{plan.reassessmentScore}%</strong>
                          </>
                        )}
                      </span>
                      {plan.reassessmentDate && <span>Re-eval by {plan.reassessmentDate}</span>}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="p-3 rounded-lg bg-suraksha-surface/50 border border-suraksha-border text-xs text-suraksha-subtext text-center">
                No active retraining order for this worker.
              </div>
            )}
          </div>

          {/* Certificates List */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-white mb-3">
              Compliance Certificates Registry
            </h4>
            <div className="space-y-2">
              {certificates.map((cert) => (
                <div
                  key={cert.id}
                  onClick={() => setSelectedCertificate(cert)}
                  className="p-3 rounded-xl border border-suraksha-border bg-suraksha-surface/60 hover:bg-suraksha-hover cursor-pointer transition text-xs"
                >
                  <div className="flex items-center justify-between">
                    <h6 className="font-bold text-white">{cert.moduleName}</h6>
                    <StatusBadge status={cert.status} size="sm" />
                  </div>
                  <p className="text-[10px] font-mono text-suraksha-subtext mt-1">{cert.certificateId}</p>
                  <div className="flex items-center justify-between mt-2 pt-2 border-t border-suraksha-border/40 text-[10px] text-suraksha-subtext">
                    <span>
                      Issue: <strong className="text-white">{cert.issueDate}</strong> · Expiry:{' '}
                      <strong className="text-white">{cert.expiryDate}</strong>
                    </span>
                    <span className="inline-flex items-center gap-1 font-bold text-suraksha-amber">
                      <Eye className="w-3 h-3" /> Demo Verify
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Ebbinghaus Retention Schedule */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-white mb-3">
              Ebbinghaus Retention Status
            </h4>
            <div className="grid grid-cols-3 gap-2 text-center text-xs">
              <div className="p-2 rounded-lg bg-suraksha-surface border border-suraksha-border">
                <span className="text-[10px] font-bold text-suraksha-subtext block">Day 1</span>
                <StatusBadge status={worker.retentionDay1} size="sm" className="mt-1" />
              </div>
              <div className="p-2 rounded-lg bg-suraksha-surface border border-suraksha-border">
                <span className="text-[10px] font-bold text-suraksha-subtext block">Day 7</span>
                <StatusBadge status={worker.retentionDay7} size="sm" className="mt-1" />
              </div>
              <div className="p-2 rounded-lg bg-suraksha-surface border border-suraksha-border">
                <span className="text-[10px] font-bold text-suraksha-subtext block">Day 30</span>
                <StatusBadge status={worker.retentionDay30} size="sm" className="mt-1" />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Inspection Modals */}
      <AssessmentDetailModal
        assessment={selectedAssessment}
        isOpen={!!selectedAssessment}
        onClose={() => setSelectedAssessment(null)}
      />
      <CertificateVerifyModal
        certificate={selectedCertificate}
        isOpen={!!selectedCertificate}
        onClose={() => setSelectedCertificate(null)}
      />
      <AssignRetrainingModal
        worker={worker}
        isOpen={isAssignRetrainingOpen}
        onClose={() => setIsAssignRetrainingOpen(false)}
        onAssign={() => {}}
      />
    </div>
  );
};
