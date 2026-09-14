import React, { useState } from 'react';
import { ChevronLeft, ChevronRight, ArrowUpDown } from 'lucide-react';

export interface Column<T> {
  key: string;
  header: string;
  render?: (item: T) => React.ReactNode;
  sortable?: boolean;
  align?: 'left' | 'center' | 'right';
  width?: string;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  onRowClick?: (item: T) => void;
  pageSize?: number;
  emptyMessage?: string;
  className?: string;
}

export function DataTable<T extends Record<string, any>>({
  columns,
  data,
  onRowClick,
  pageSize = 10,
  emptyMessage = 'No records found matching criteria.',
  className = '',
}: DataTableProps<T>) {
  const [currentPage, setCurrentPage] = useState(1);
  const [sortColumn, setSortColumn] = useState<string | null>(null);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  const handleSort = (key: string) => {
    if (sortColumn === key) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortColumn(key);
      setSortDirection('asc');
    }
  };

  const sortedData = React.useMemo(() => {
    if (!sortColumn) return data;
    return [...data].sort((a, b) => {
      const aVal = a[sortColumn];
      const bVal = b[sortColumn];
      if (aVal < bVal) return sortDirection === 'asc' ? -1 : 1;
      if (aVal > bVal) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }, [data, sortColumn, sortDirection]);

  const totalPages = Math.ceil(sortedData.length / pageSize) || 1;
  const paginatedData = sortedData.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  return (
    <div className={`rounded-xl border border-suraksha-border bg-suraksha-card overflow-hidden shadow-card ${className}`}>
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-suraksha-border bg-suraksha-surface/80 text-xs uppercase font-bold tracking-wider text-suraksha-subtext">
              {columns.map((col) => (
                <th
                  key={col.key}
                  onClick={() => col.sortable && handleSort(col.key)}
                  style={{ width: col.width }}
                  className={`px-4 py-3.5 ${col.align === 'right' ? 'text-right' : col.align === 'center' ? 'text-center' : 'text-left'} ${
                    col.sortable ? 'cursor-pointer select-none hover:text-white' : ''
                  }`}
                >
                  <div className={`inline-flex items-center gap-1 ${col.align === 'right' ? 'justify-end' : col.align === 'center' ? 'justify-center' : 'justify-start'}`}>
                    {col.header}
                    {col.sortable && <ArrowUpDown className="w-3 h-3 text-suraksha-subtext/60" />}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-suraksha-border/40 text-xs">
            {paginatedData.length > 0 ? (
              paginatedData.map((item, index) => (
                <tr
                  key={item.id || index}
                  onClick={() => onRowClick && onRowClick(item)}
                  className={`transition-colors duration-150 ${
                    onRowClick ? 'cursor-pointer hover:bg-suraksha-surface/90 hover:border-l-2 hover:border-l-suraksha-amber' : 'hover:bg-suraksha-surface/40'
                  }`}
                >
                  {columns.map((col) => (
                    <td
                      key={col.key}
                      className={`px-4 py-3.5 text-suraksha-text ${
                        col.align === 'right' ? 'text-right' : col.align === 'center' ? 'text-center' : 'text-left'
                      }`}
                    >
                      {col.render ? col.render(item) : item[col.key]}
                    </td>
                  ))}
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={columns.length} className="px-4 py-12 text-center text-suraksha-subtext">
                  <div className="flex flex-col items-center justify-center gap-2">
                    <span className="text-sm font-medium">{emptyMessage}</span>
                  </div>
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination Footer */}
      <div className="flex items-center justify-between px-4 py-3 border-t border-suraksha-border bg-suraksha-surface/50 text-xs text-suraksha-subtext">
        <div>
          Showing <span className="font-semibold text-white">{data.length > 0 ? (currentPage - 1) * pageSize + 1 : 0}</span> to{' '}
          <span className="font-semibold text-white">{Math.min(currentPage * pageSize, data.length)}</span> of{' '}
          <span className="font-semibold text-white">{data.length}</span> entries
        </div>

        <div className="flex items-center gap-1.5">
          <button
            disabled={currentPage === 1}
            onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            className="p-1.5 rounded-lg border border-suraksha-border text-suraksha-subtext hover:bg-suraksha-hover hover:text-white disabled:opacity-40 disabled:hover:bg-transparent"
          >
            <ChevronLeft className="w-4 h-4" />
          </button>
          <span className="px-2 font-medium text-white">
            Page {currentPage} of {totalPages}
          </span>
          <button
            disabled={currentPage === totalPages}
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            className="p-1.5 rounded-lg border border-suraksha-border text-suraksha-subtext hover:bg-suraksha-hover hover:text-white disabled:opacity-40 disabled:hover:bg-transparent"
          >
            <ChevronRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
}
