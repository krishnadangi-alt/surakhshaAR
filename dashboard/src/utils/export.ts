/**
 * Client-side CSV export helper (mock-data phase).
 * Generates a real downloadable .csv file from the supplied rows.
 */
export function downloadCsv(
  filename: string,
  headers: (string | number)[],
  rows: (string | number | boolean | null | undefined)[][],
): void {
  const escapeCell = (value: string | number | boolean | null | undefined): string => {
    const str = value === null || value === undefined ? '' : String(value);
    if (str.includes(',') || str.includes('"') || str.includes('\n')) {
      return `"${str.replace(/"/g, '""')}"`;
    }
    return str;
  };

  const lines = [headers.map(escapeCell).join(','), ...rows.map((r) => r.map(escapeCell).join(','))];
  const blob = new Blob([`\uFEFF${lines.join('\r\n')}`], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = filename.endsWith('.csv') ? filename : `${filename}.csv`;
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);
  URL.revokeObjectURL(url);
}

export function timestampForExport(): string {
  return new Date().toISOString().slice(0, 10);
}