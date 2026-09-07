interface Props {
  label: string;
  value: string | number;
  tone?: 'default' | 'success' | 'warning' | 'danger';
}

export default function KpiCard({ label, value, tone = 'default' }: Props) {
  const cls = tone === 'default' ? '' : tone;

  return (
    <div className="kpi animate-in">
      <div className="kpi-label">{label}</div>
      <div className={`kpi-value ${cls}`}>{value}</div>
    </div>
  );
}
