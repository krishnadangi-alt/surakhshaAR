import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import StatusBadge from '../components/StatusBadge';
import { getWorkerDetail } from '../services/api';
import { competencyLabel, formatDate } from '../utils/format';
import type { WorkerDetail } from '../types';

const STAGES = [
  { key: 'learn', label: 'Training', icon: '\u{1F4D6}' },
  { key: 'practice', label: 'Practice', icon: '\u2699' },
  { key: 'assess', label: 'Assessment', icon: '\u{1F4CB}' },
  { key: 'retrain', label: 'Retraining', icon: '\u{1F504}' },
  { key: 'reassess', label: 'Reassess', icon: '\u2705}' },
  { key: 'certify', label: 'Certification', icon: '\u{1F393}' },
];

function getStageIndex(stage: string): number {
  return STAGES.findIndex((s) => s.key === stage);
}

export default function WorkerDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [worker, setWorker] = useState<WorkerDetail | null>(null);
  const [notFound, setNotFound] = useState(false);

  const workerId = Number(id);

  useEffect(() => {
    if (Number.isNaN(workerId)) {
      setNotFound(true);
      return;
    }
    getWorkerDetail(workerId).then((d) => {
      if (d) setWorker(d);
      else setNotFound(true);
    });
  }, [workerId]);

  const latestAssessment = worker?.assessments[0] ?? null;
  const currentStage = worker?.progress[0]?.stage ?? 'learn';
  const stageIndex = getStageIndex(currentStage);

  const allWeaknesses = useMemo(() => {
    const map = new Map<string, number>();
    worker?.assessments.forEach((a) => {
      a.weaknesses.forEach((w) => map.set(w, (map.get(w) ?? 0) + 1));
    });
    return Array.from(map.entries()).sort((a, b) => b[1] - a[1]);
  }, [worker]);

  const criticalErrors = useMemo(() => {
    return worker?.assessments.filter((a) => !a.passed && a.weaknesses.length >= 2).length ?? 0;
  }, [worker]);

  const certificateEligible = latestAssessment?.passed ?? false;
  const hasCertificate = (worker?.certificates.length ?? 0) > 0;

  if (notFound) {
    return (
      <div className="placeholder-page">
        <div className="icon">{'\u26A0'}</div>
        <h2>Worker Not Found</h2>
        <p className="text-sm text-muted mb-4">No worker exists with ID {id}.</p>
        <button className="btn" onClick={() => navigate('/workers')}>Back to Workers</button>
      </div>
    );
  }

  if (!worker) {
    return <div className="text-sm text-muted">Loading worker details...</div>;
  }

  return (
    <div className="page-enter">
      <div className="flex-between mb-4">
        <div>
          <button className="btn btn-sm" onClick={() => navigate('/workers')} style={{ marginBottom: 8 }}>
            {'\u2190'} Back
          </button>
          <h1 style={{ fontSize: '1.3rem', fontWeight: 700 }}>{worker.name}</h1>
          <div className="text-sm text-secondary">
            {worker.employee_id} &middot; {worker.role}
          </div>
        </div>
        <StatusBadge status={latestAssessment ? (latestAssessment.passed ? 'passed' : 'failed') : 'in_training'} />
      </div>

      <div className="card mb-5 animate-in">
        <div className="card-header">Training Progression</div>
        <div className="card-body">
          <div className="progression">
            {STAGES.map((stage, i) => {
              const isActive = i <= stageIndex;
              const isCurrent = i === stageIndex;
              return (
                <div key={stage.key} style={{ display: 'flex', alignItems: 'center' }}>
                  <div className={`progression-step${isActive ? ' active' : ''}${isCurrent ? ' current' : ''}`}>
                    <div className="step-icon">{stage.icon}</div>
                    <div className="step-label">{stage.label}</div>
                  </div>
                  {i < STAGES.length - 1 && (
                    <div className={`progression-connector${i < stageIndex ? ' active' : ''}`} />
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </div>

      <div className="card mb-5 animate-in delay-1">
        <div className="card-header">Overview</div>
        <div className="card-body">
          <div className="detail-grid">
            <div className="detail-item">
              <label>Latest Score</label>
              <div className="value">{latestAssessment ? `${latestAssessment.score}%` : '—'}</div>
            </div>
            <div className="detail-item">
              <label>Competency</label>
              <div className="value">{latestAssessment ? competencyLabel(latestAssessment.score) : '—'}</div>
            </div>
            <div className="detail-item">
              <label>Attempts</label>
              <div className="value">{worker.assessments.length}</div>
            </div>
            <div className="detail-item">
              <label>Certified Modules</label>
              <div className="value">{worker.certified_modules.length > 0 ? worker.certified_modules.join(', ') : '—'}</div>
            </div>
          </div>
        </div>
      </div>


      <div className="grid-2">
        <div className="card animate-in delay-2">
          <div className="card-header">Competency — Weak Areas</div>
          <div className="card-body">
            {allWeaknesses.length > 0 ? (
              <ul className="weakness-list">
                {allWeaknesses.map(([area, count]) => (
                  <li key={area}>{area} <span className="text-muted text-sm">({count}x)</span></li>
                ))}
              </ul>
            ) : (
              <div className="text-sm text-muted">No significant weaknesses recorded.</div>
            )}
          </div>
        </div>

        <div className="card animate-in delay-3">
          <div className="card-header">Assessment & Certification</div>
          <div className="card-body">
            <div className="detail-item mb-4">
              <label>Critical Errors</label>
              <div className="value" style={{ color: criticalErrors > 0 ? 'var(--c-danger)' : 'var(--c-success)' }}>
                {criticalErrors}
              </div>
            </div>
            <div className="detail-item mb-4">
              <label>Retraining Required</label>
              <div className="value">
                {latestAssessment && !latestAssessment.passed ? (
                  <span style={{ color: 'var(--c-warning)' }}>Yes — {allWeaknesses[0]?.[0] ?? 'General'} focus</span>
                ) : (
                  <span style={{ color: 'var(--c-success)' }}>No</span>
                )}
              </div>
            </div>
            <div className="detail-item">
              <label>Certificate</label>
              <div className="value">
                {hasCertificate ? (
                  <span style={{ color: 'var(--c-success)' }}>Issued</span>
                ) : certificateEligible ? (
                  <span style={{ color: 'var(--c-info)' }}>Eligible</span>
                ) : (
                  <span style={{ color: 'var(--c-danger)' }}>Not Eligible</span>
                )}
              </div>
            </div>
            {worker.certificates.length > 0 && (
              <div className="mt-4">
                <label style={{ fontSize: '0.7rem', color: 'var(--c-text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  Issued Certificates
                </label>
                {worker.certificates.map((c) => (
                  <div key={c.id} className="text-sm" style={{ marginTop: 4 }}>
                    <strong>{c.certificate_number}</strong> — valid until{' '}
                    {new Date(c.valid_until).toLocaleDateString('en-IN')}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="card mt-4 animate-in delay-4">
        <div className="card-header">Assessment History</div>
        <div className="card-body" style={{ padding: 0 }}>
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Attempt</th>
                  <th>Module</th>
                  <th>Score</th>
                  <th>Result</th>
                  <th>Weaknesses</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {worker.assessments.map((a) => (
                  <tr key={a.id}>
                    <td>#{a.attempt_number}</td>
                    <td>Module {a.module_id}</td>
                    <td>{a.score}%</td>
                    <td><StatusBadge status={a.passed ? 'passed' : 'failed'} /></td>
                    <td className="text-secondary">{a.weaknesses.length > 0 ? a.weaknesses.join(', ') : '—'}</td>
                    <td className="text-secondary">{formatDate(a.created_at)}</td>
                  </tr>
                ))}
                {worker.assessments.length === 0 && (
                  <tr>
                    <td colSpan={6} style={{ textAlign: 'center', padding: 24 }} className="text-muted">
                      No assessments recorded.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}

