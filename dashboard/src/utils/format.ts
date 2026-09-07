/**
 * Format an ISO date string into a human-readable relative / absolute label.
 */
export function formatDate(iso: string): string {
  const d = new Date(iso);
  const now = new Date();
  const diffMs = now.getTime() - d.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  const diffHr = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHr / 24);

  if (diffMin < 1) return 'Just now';
  if (diffMin < 60) return `${diffMin}m ago`;
  if (diffHr < 24) return `${diffHr}h ago`;
  if (diffDay === 1) return 'Yesterday';
  if (diffDay < 7) return `${diffDay} days ago`;

  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
}

/**
 * Derive a worker's overall status from their latest assessment / progress.
 */
export function deriveWorkerStatus(stage: string, latestScore: number | null): string {
  if (stage === 'certify' || stage === 'retain') return 'certified';
  if (stage === 'retrain') return 'retraining';
  if (stage === 'diagnose') return latestScore !== null && latestScore < 60 ? 'failed' : 'in_training';
  if (latestScore !== null) {
    return latestScore >= 60 ? 'passed' : 'failed';
  }
  return 'in_training';
}

/**
 * Derive competency label from a score.
 */
export function competencyLabel(score: number): string {
  if (score >= 85) return 'Strong';
  if (score >= 70) return 'Good';
  if (score >= 60) return 'Fair';
  return 'Weak';
}
