interface DonutItem {
  label: string;
  value: number;
  color: string;
}

interface Props {
  data: DonutItem[];
}

export default function DonutChart({ data }: Props) {
  const total = data.reduce((sum, d) => sum + d.value, 0);
  if (total === 0) {
    return <div className="text-sm text-muted">No data available</div>;
  }

  let cumulative = 0;
  const segments = data.flatMap((d) => {
    const start = cumulative;
    cumulative += (d.value / total) * 360;
    return [`${d.color} ${start}deg ${cumulative}deg`];
  });

  const gradient = `conic-gradient(${segments.join(', ')})`;

  return (
    <div className="donut-wrap">
      <div className="donut" style={{ background: gradient }} />
      <div className="donut-legend">
        {data.map((d) => (
          <div className="donut-legend-item" key={d.label}>
            <span className="donut-legend-dot" style={{ background: d.color }} />
            <span>{d.label}: <strong>{d.value}</strong></span>
          </div>
        ))}
      </div>
    </div>
  );
}
