interface Props {
  status: string;
}

const STATUS_MAP: Record<string, { label: string; cls: string }> = {
  passed: { label: 'Passed', cls: 'badge-passed' },
  failed: { label: 'Failed', cls: 'badge-failed' },
  retraining: { label: 'Retraining', cls: 'badge-retraining' },
  'in_training': { label: 'In Training', cls: 'badge-in-training' },
  certified: { label: 'Certified', cls: 'badge-certified' },
};

export default function StatusBadge({ status }: Props) {
  const entry = STATUS_MAP[status] ?? { label: status, cls: 'badge-in-training' };
  return <span className={`badge ${entry.cls}`}>{entry.label}</span>;
}
