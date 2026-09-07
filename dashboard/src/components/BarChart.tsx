interface BarData {
  label: string;
  value: number;
  max?: number;
  tone?: 'default' | 'accent' | 'success';
}

interface Props {
  data: BarData[];
}

export default function BarChart({ data }: Props) {
  const max = Math.max(...data.map((d) => d.max ?? d.value), 1);

  return (
    <div className="bar-chart">
      {data.map((d, i) => (
        <div className={`bar-row animate-in delay-${Math.min(i + 1, 8)}`} key={d.label}>
          <span className="bar-label">{d.label}</span>
          <div className="bar-track">
            <div
              className={`bar-fill ${d.tone ?? ''}`}
              style={{ width: `${Math.max((d.value / max) * 100, 0)}%` }}
            />
          </div>
          <span className="bar-value">{d.value}</span>
        </div>
      ))}
    </div>
  );
}
