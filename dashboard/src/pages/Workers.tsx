import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import StatusBadge from '../components/StatusBadge';
import { getWorkerList } from '../services/api';
import { competencyLabel, deriveWorkerStatus, formatDate } from '../utils/format';
import type { Worker } from '../types';

export default function Workers() {
  const navigate = useNavigate();
  const [workers, setWorkers] = useState<Worker[]>([]);
  const [search, setSearch] = useState('');
  const [moduleFilter, setModuleFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  useEffect(() => {
    getWorkerList().then((d) => setWorkers(d.workers));
  }, []);

  const enriched = useMemo(() => {
    return workers.map((w) => {
      const prog = w.progress[0];
      const stage = prog?.stage ?? '';
      const scoreMap: Record<string, number> = {
        certify: 85, retain: 90, assess: 70, diagnose: 50, retrain: 55, practice: 65, learn: 40,
      };
      const score = scoreMap[stage] ?? 60;
      const status = deriveWorkerStatus(stage, score);
      return { ...w, _score: score, _status: status, _module: prog?.module_name ?? '—' };
    });
  }, [workers]);

  const filtered = useMemo(() => {
    return enriched.filter((w) => {
      const q = search.toLowerCase();
      const matchSearch =
        !q ||
        w.name.toLowerCase().includes(q) ||
        w.employee_id.toLowerCase().includes(q) ||
        w.role.toLowerCase().includes(q);
      const matchModule = !moduleFilter || w._module === moduleFilter;
      const matchStatus = !statusFilter || w._status === statusFilter;
      return matchSearch && matchModule && matchStatus;
    });
  }, [enriched, search, moduleFilter, statusFilter]);

  const modules = useMemo(() => {
    const set = new Set<string>();
    workers.forEach((w) => w.progress.forEach((p) => set.add(p.module_name)));
    return Array.from(set).sort();
  }, [workers]);

  return (
    <div className="page-enter">
      <div className="card animate-in">
        <div className="card-header">
          <span>Worker Directory</span>
          <span className="text-sm text-muted">{filtered.length} worker{filtered.length !== 1 ? 's' : ''}</span>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="filter-row">
            <input
              className="form-input"
              type="text"
              placeholder="Search name, ID, role..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              style={{ minWidth: 220 }}
            />
            <select
              className="form-input"
              value={moduleFilter}
              onChange={(e) => setModuleFilter(e.target.value)}
            >
              <option value="">All Modules</option>
              {modules.map((m) => (
                <option key={m} value={m}>{m}</option>
              ))}
            </select>
            <select
              className="form-input"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            >
              <option value="">All Statuses</option>
              <option value="certified">Certified</option>
              <option value="passed">Passed</option>
              <option value="in_training">In Training</option>
              <option value="retraining">Retraining</option>
              <option value="failed">Failed</option>
            </select>
          </div>

          {/* Table */}
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Worker</th>
                  <th>Employee ID</th>
                  <th>Module</th>
                  <th>Score</th>
                  <th>Competency</th>
                  <th>Status</th>
                  <th>Last Activity</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((w) => (
                  <tr
                    key={w.id}
                    className="clickable"
                    onClick={() => navigate(`/workers/${w.id}`)}
                  >
                    <td>{w.name}</td>
                    <td>{w.employee_id}</td>
                    <td>{w._module}</td>
                    <td>{w._score}%</td>
                    <td className="text-secondary">{competencyLabel(w._score)}</td>
                    <td><StatusBadge status={w._status} /></td>
                    <td className="text-secondary">
                      {w.progress[0] ? formatDate(w.progress[0].last_updated) : '—'}
                    </td>
                  </tr>
                ))}
                {filtered.length === 0 && (
                  <tr>
                    <td colSpan={7} style={{ textAlign: 'center', padding: 32 }} className="text-muted">
                      No workers match the current filters.
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
