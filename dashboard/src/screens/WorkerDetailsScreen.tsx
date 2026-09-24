import React, { useState, useEffect } from 'react';
import { fetchWorkerDetail, fetchDashboardAssessments } from '../services/api';
import type { DashboardWorkerDetail } from '../services/api';
import { StatusBadge } from '../components/common/StatusBadge';
import { AssessmentDetailModal } from '../components/modals/AssessmentDetailModal';
import { CertificateVerifyModal } from '../components/modals/CertificateVerifyModal';
import { AssignRetrainingModal } from '../components/modals/AssignRetrainingModal';
import type { Assessment, Certificate, Worker, RetrainingRecord } from '../types';
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
  const [liveDetail, setLiveDetail] = useState<DashboardWorkerDetail | null>(null);
  const [liveAssessments, setLiveAssessments] = useState<Assessment[]>([]);

  useEffect(() => {
    const numericId = workerId.startsWith('w-') ? parseInt(workerId.replace('w-', ''), 10) : parseInt(workerId, 10);
    if (!isNaN(numericId)) {
      fetchWorkerDetail(numericId).then((detail) => {
        if (detail) setLiveDetail(detail);
      });
    }
    fetchDashboardAssessments().then((asmts) => {
      if (asmts && asmts.length > 0) {
        setLiveAssessments(
          asmts.filter(
            (a) =>
              a.workerId === workerId ||
              a.workerId === `w-${numericId}` ||
              a.employeeId === workerId ||
              (liveDetail && a.employeeId === liveDetail.employee_id)
          )
        );
      }
    });
  }, [workerId, liveDetail?.employee_id]);

  const worker: Worker | null = liveDetail
    ? {
        id: `w-${liveDetail.id}`,
        employeeId: liveDetail.employee_id,
        name: liveDetail.name,
        sector: 'Dhanbad Region-1',
        plant: liveDetail.employee_id.startsWith('GUEST') ? 'SurakshaAR AR Testing Hub' : 'Jharia Deep Shaft Mine #4',
        role: liveDetail.role,
        email: `${liveDetail.name.toLowerCase().replace(/[^a-z0-9]/g, '.')}@mining.jh.gov.in`,
        phone: '+91 98765 43210',
        joinedDate: '2026-01-15',
        safetyOfficer: 'Inspector R. K. Soren',
        overallStatus: liveDetail.certificates && liveDetail.certificates.length > 0 ? 'Certified' : 'In Training',
        modulesCompleted: liveDetail.certificates ? liveDetail.certificates.length : 0,
        latestScore:
          liveAssessments.length > 0
            ? liveAssessments[0].score
            : liveDetail.assessments.length > 0
            ? Math.round(liveDetail.assessments[0].score)
            : 85,
        overallCompetency:
          (liveAssessments.length > 0 ? liveAssessments[0].score : 85) >= 80 ? 'Competent' : 'Needs Retraining',
        lastAssessmentDate: liveAssessments.length > 0 ? liveAssessments[0].dateTime : '2026-09-16',
        certificatesCount: liveDetail.certificates ? liveDetail.certificates.length : 0,
        retrainingStatus: 'Completed',
        moduleProgressList: liveDetail.progress.map((p) => ({
          moduleId: p.module_code,
          moduleName: p.module_name,
          stage: p.stage,
          status: p.status === 'completed' ? 'Completed' : 'In Progress',
          completionPercentage: p.status === 'completed' ? 100 : 50,
          score: liveAssessments.length > 0 ? liveAssessments[0].score : 85,
          lastUpdated: p.last_updated || '2026-09-16',
        })),
        weakAreas: liveAssessments.length > 0 && liveAssessments[0].wrongActions > 0 ? ['PASS Extinguisher Technique'] : [],
        retentionDay1: 'Completed',
        retentionDay7: 'Scheduled',
        retentionDay30: 'Scheduled',
      }
    : null;

  if (!worker) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <button
            onClick={onBack}
            className="flex items-center gap-2 rounded-lg border border-suraksha-border bg-suraksha-card px-3.5 py-2 text-xs font-semibold text-suraksha-text hover:bg-suraksha-hover transition"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Back to Workers Directory</span>
          </button>
        </div>
        <div className="p-12 text-center rounded-xl border border-suraksha-border bg-suraksha-card text-suraksha-subtext text-sm">
          Loading worker dossier from backend...
        </div>
      </div>
    );
  }

  const assessments = liveAssessments;
  const certificates: Certificate[] =
    liveDetail?.certificates && liveDetail.certificates.length > 0
      ? liveDetail.certificates.map((c) => ({
          id: `cert-${c.id}`,
          certificateId: c.certificate_number,
          workerId: `w-${c.worker_id}`,
          workerName: worker.name,
          employeeId: worker.employeeId,
          sector: 'Dhanbad Region-1',
          moduleId: `m-${c.module_id}`,
          moduleName: 'Fire & Explosion Response',
          resultGrade: worker.latestScore >= 85 ? 'Grade A (Exemplary)' : 'Grade B (Competent)',
          issueDate: c.issued_at ? c.issued_at.slice(0, 10) : '2026-09-16',
          expiryDate: c.valid_until ? c.valid_until.slice(0, 10) : '2027-09-16',
          status: 'Active' as const,
          verificationCode: `SHA256:${c.certificate_number.slice(-8)}`,
          issuerDepartment: 'Directorate General of Mines Safety (DGMS)',
        }))
      : [];

  const retrainingPlans: RetrainingRecord[] = liveAssessments
    .filter((a) => a.passFail === 'Fail' || a.criticalErrors > 0)
    .map((a, idx) => ({
      id: `rp-${idx}`,
      workerId: worker.id,
      workerName: worker.name,
      employeeId: worker.employeeId,
      sector: worker.sector,
      moduleId: a.moduleId,
      moduleName: a.moduleName,
      weakArea: a.criticalErrorDetails || 'SOP Compliance / Extinguisher Technique',
      recommendation: 'Targeted AR SOP retraining recommended',
      status: 'Assigned',
      assignedDate: a.dateTime.slice(0, 10),
      initialScore: a.score,
    }));

  const [selectedAssessment, setSelectedAssessment] = useState<Assessment | null>(null);
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null);
  const [isAssignRetrainingOpen, setIsAssignRetrainingOpen] = useState(false);

  // Radar data for this worker (aligned to the 6 competency dimensions)
  const latestProfile = liveDetail?.competency_profile?.[0]?.competencies;
  const workerRadarData = latestProfile && Object.keys(latestProfile).length > 0
    ? Object.entries(latestProfile).map(([dim, val]: [string, any]) => ({
        dimension: dim,
        score: val.score,
      }))
    : [
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
          className="flex items-center gap-2 rounded-lg border border-suraksha-border bg-suraksha-card px-3.5 py-2 text-xs font-semibold text-suraksha-text hover:bg-suraksha-hover transition"
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
                <h2 className="text-xl font-bold text-suraksha-heading tracking-wide">{worker.name}</h2>
                <StatusBadge status={worker.overallStatus} size="md" />
              </div>
              <p className="text-xs font-mono text-suraksha-subtext mt-1 font-medium">
                Employee ID: <span className="text-suraksha-heading font-bold">{worker.employeeId}</span> • Role:{' '}
                <span className="text-suraksha-heading font-bold">{worker.role}</span>
              </p>
              <p className="text-xs text-suraksha-subtext mt-0.5 font-medium">
                {worker.plant} ({worker.sector})
              </p>
            </div>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 text-xs">
            <div className="p-2.5 rounded-xl bg-suraksha-surface border border-suraksha-border">
              <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Safety Officer</span>
              <span className="font-bold text-suraksha-heading">{worker.safetyOfficer}</span>
            </div>
            <div className="p-2.5 rounded-xl bg-suraksha-surface border border-suraksha-border">
              <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Joined Date</span>
              <span className="font-bold text-suraksha-heading">{worker.joinedDate}</span>
            </div>
            <div className="p-2.5 rounded-xl bg-suraksha-surface border border-suraksha-border col-span-2 sm:col-span-1">
              <span className="text-[10px] text-suraksha-subtext uppercase font-bold block">Contact</span>
              <span className="font-bold text-suraksha-heading">{worker.phone}</span>
            </div>
          </div>
        </div>

        {/* Summary Metric Ribbon */}
        <div className="grid grid-cols-2 md:grid-cols-5 gap-3 pt-4 text-center">
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Modules Completed</p>
            <p className="text-lg font-bold text-suraksha-heading mt-0.5">{worker.modulesCompleted} / 3</p>
          </div>
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Latest AR Score</p>
            <p className="text-lg font-bold text-emerald-700 mt-0.5">{worker.latestScore}%</p>
          </div>
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Overall Competency</p>
            <p className="text-lg font-bold text-suraksha-amber mt-0.5">{worker.overallCompetency}</p>
          </div>
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Active Certificates</p>
            <p className="text-lg font-bold text-suraksha-heading mt-0.5">{worker.certificatesCount}</p>
          </div>
          <div className="p-3 rounded-xl bg-suraksha-surface/50 border border-suraksha-border/60 col-span-2 md:col-span-1">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Retraining Status</p>
            <p className="text-sm font-bold text-suraksha-heading mt-1">
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
            <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading mb-4">
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
                      <h5 className="font-bold text-suraksha-heading">{m.moduleName}</h5>
                      <p className="text-[11px] text-suraksha-subtext font-medium">{m.stage}</p>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="font-bold text-emerald-700">{m.score}% Score</span>
                      <StatusBadge status={m.status} size="sm" />
                    </div>
                  </div>

                  <div className="h-2 w-full bg-suraksha-card rounded-full overflow-hidden border border-suraksha-border">
                    <div
                      className="h-full bg-suraksha-blue rounded-full transition-all duration-500"
                      style={{ width: `${m.completionPercentage}%` }}
                    />
                  </div>

                  <div className="flex justify-between text-[10px] text-suraksha-subtext font-medium pt-1">
                    <span>Completion: {m.completionPercentage}%</span>
                    <span>Last Assessed: {m.lastUpdated}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Assessment History Section */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading mb-4">
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
                        <span className="text-xs font-bold text-suraksha-heading">{a.scenarioName}</span>
                        <StatusBadge status={a.passFail} size="sm" />
                      </div>
                      <p className="text-[10px] text-suraksha-subtext mt-0.5 font-medium">
                        Module: {a.moduleName} • Duration: {a.duration} • {a.dateTime}
                      </p>
                    </div>

                    <div className="flex items-center gap-4">
                      <div className="text-right">
                        <span className="text-xs font-bold text-emerald-700">{a.score}% Score</span>
                        <p className="text-[10px] text-suraksha-subtext font-medium">{a.correctActions} Correct / {a.wrongActions} Wrong</p>
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
            <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading mb-2">
              Competency Radar Profile
            </h4>
            <div className="h-56 w-full">
              <ResponsiveContainer width="100%" height="100%">
                <RadarChart cx="50%" cy="50%" outerRadius="70%" data={workerRadarData}>
                  <PolarGrid stroke="#CBD5E1" />
                  <PolarAngleAxis dataKey="dimension" stroke="#475569" fontSize={9} />
                  <PolarRadiusAxis angle={30} domain={[0, 100]} stroke="#CBD5E1" fontSize={8} />
                  <Radar name="Worker" dataKey="score" stroke="#D97706" fill="#D97706" fillOpacity={0.4} />
                </RadarChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Identified Weak Areas */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading mb-3">
              Identified Weak Protocols
            </h4>
            <div className="space-y-2">
              {worker.weakAreas.length > 0 ? (
                worker.weakAreas.map((area, i) => (
                  <div key={i} className="flex items-center gap-2 p-2.5 rounded-lg bg-rose-50 border border-rose-200 text-xs font-semibold text-rose-800">
                    <AlertOctagon className="w-4 h-4 text-rose-600 shrink-0" />
                    <span>{area}</span>
                  </div>
                ))
              ) : (
                <div className="p-3 rounded-lg bg-emerald-50 border border-emerald-200 text-xs text-emerald-700 text-center font-bold">
                  ✓ No safety weaknesses currently flagged
                </div>
              )}
            </div>
          </div>

          {/* Retraining Plan */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <div className="flex items-center justify-between mb-3">
              <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading">Retraining Plan</h4>
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
                      <h6 className="font-bold text-suraksha-heading">{plan.moduleName}</h6>
                      <StatusBadge status={plan.status} size="sm" />
                    </div>
                    <p className="text-[11px] text-rose-700 font-semibold mt-1.5">Weakness: {plan.weakArea}</p>
                    <p className="text-[11px] text-suraksha-subtext font-medium mt-0.5">Plan: {plan.recommendation}</p>
                    <div className="flex items-center justify-between text-[10px] text-suraksha-subtext font-medium mt-2 pt-2 border-t border-suraksha-border/40">
                      <span>
                        Initial <strong className="text-rose-700 font-bold">{plan.initialScore}%</strong>
                        {plan.reassessmentScore !== undefined && (
                          <>
                            {' '}→ Re-assessed <strong className="text-emerald-700 font-bold">{plan.reassessmentScore}%</strong>
                          </>
                        )}
                      </span>
                      {plan.reassessmentDate && <span>Re-eval by {plan.reassessmentDate}</span>}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="p-3 rounded-lg bg-suraksha-surface/50 border border-suraksha-border text-xs text-suraksha-subtext font-medium text-center">
                No active retraining order for this worker.
              </div>
            )}
          </div>

          {/* Certificates List */}
          <div className="rounded-xl border border-suraksha-border bg-suraksha-card p-5 shadow-card">
            <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading mb-3">
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
                    <h6 className="font-bold text-suraksha-heading">{cert.moduleName}</h6>
                    <StatusBadge status={cert.status} size="sm" />
                  </div>
                  <p className="text-[10px] font-mono text-suraksha-subtext font-medium mt-1">{cert.certificateId}</p>
                  <div className="flex items-center justify-between mt-2 pt-2 border-t border-suraksha-border/40 text-[10px] text-suraksha-subtext font-medium">
                    <span>
                      Issue: <strong className="text-suraksha-heading font-bold">{cert.issueDate}</strong> · Expiry:{' '}
                      <strong className="text-suraksha-heading font-bold">{cert.expiryDate}</strong>
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
            <h4 className="text-sm font-bold uppercase tracking-wider text-suraksha-heading mb-3">
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
