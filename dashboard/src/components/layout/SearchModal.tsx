import React, { useState, useEffect } from 'react';
import { Search, User, Award, ArrowRight } from 'lucide-react';
import { mockWorkers, mockAssessments } from '../../mockData';

interface SearchModalProps {
  isOpen: boolean;
  onClose: () => void;
  onNavigate: (screen: string, param?: string) => void;
}

export const SearchModal: React.FC<SearchModalProps> = ({ isOpen, onClose, onNavigate }) => {
  const [query, setQuery] = useState('');

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        if (isOpen) onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const filteredWorkers = mockWorkers.filter(
    (w) => w.name.toLowerCase().includes(query.toLowerCase()) || w.employeeId.toLowerCase().includes(query.toLowerCase())
  );

  const filteredAssessments = mockAssessments.filter(
    (a) => a.scenarioName.toLowerCase().includes(query.toLowerCase()) || a.moduleName.toLowerCase().includes(query.toLowerCase())
  );

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center pt-20 px-4">
      <div className="fixed inset-0 bg-black/80 backdrop-blur-sm" onClick={onClose} />
      <div className="relative w-full max-w-xl rounded-2xl border border-suraksha-border bg-suraksha-card shadow-elevated overflow-hidden z-10 animate-in fade-in zoom-in-95">
        <div className="flex items-center gap-3 border-b border-suraksha-border px-4 py-3 bg-suraksha-surface">
          <Search className="w-5 h-5 text-suraksha-amber" />
          <input
            type="text"
            autoFocus
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search workers, employee IDs, scenarios, certificates..."
            className="w-full bg-transparent text-sm text-white placeholder-suraksha-subtext focus:outline-none"
          />
          <kbd className="px-2 py-0.5 text-[10px] font-bold text-suraksha-subtext bg-suraksha-card border border-suraksha-border rounded">
            ESC
          </kbd>
        </div>

        <div className="p-4 max-h-96 overflow-y-auto space-y-4">
          {query.trim() === '' ? (
            <div className="text-xs text-suraksha-subtext space-y-2">
              <p className="uppercase font-bold tracking-wider text-[10px]">Quick Portal Navigation</p>
              <div className="grid grid-cols-2 gap-2">
                {[
                  { label: 'Overview Dashboard', screen: 'overview' },
                  { label: 'Workers Directory', screen: 'workers' },
                  { label: 'Assessment Logs', screen: 'assessments' },
                  { label: 'Module Analytics', screen: 'modules' },
                  { label: 'Competency Monitor', screen: 'competency' },
                  { label: 'Retraining Queue', screen: 'retraining' },
                  { label: 'Certificates Register', screen: 'certificates' },
                  { label: 'Retention Protocol', screen: 'retention' },
                ].map((item) => (
                  <button
                    key={item.screen}
                    onClick={() => {
                      onNavigate(item.screen);
                      onClose();
                    }}
                    className="flex items-center justify-between p-2.5 rounded-lg border border-suraksha-border bg-suraksha-surface/60 hover:bg-suraksha-hover hover:border-suraksha-borderLight text-left text-white transition"
                  >
                    <span>{item.label}</span>
                    <ArrowRight className="w-3.5 h-3.5 text-suraksha-subtext" />
                  </button>
                ))}
              </div>
            </div>
          ) : (
            <>
              {filteredWorkers.length > 0 && (
                <div>
                  <p className="text-[10px] uppercase font-bold text-suraksha-subtext mb-2">Matching Workers</p>
                  <div className="space-y-1">
                    {filteredWorkers.map((w) => (
                      <button
                        key={w.id}
                        onClick={() => {
                          onNavigate('worker-details', w.id);
                          onClose();
                        }}
                        className="w-full flex items-center justify-between p-2.5 rounded-lg hover:bg-suraksha-surface text-left transition"
                      >
                        <div className="flex items-center gap-2.5">
                          <User className="w-4 h-4 text-suraksha-amber" />
                          <div>
                            <span className="text-xs font-bold text-white">{w.name}</span>
                            <span className="text-[10px] text-suraksha-subtext ml-2">({w.employeeId})</span>
                          </div>
                        </div>
                        <span className="text-[10px] font-semibold text-suraksha-subtext">{w.sector}</span>
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {filteredAssessments.length > 0 && (
                <div>
                  <p className="text-[10px] uppercase font-bold text-suraksha-subtext mb-2">Matching AR Scenarios</p>
                  <div className="space-y-1">
                    {filteredAssessments.map((a) => (
                      <button
                        key={a.id}
                        onClick={() => {
                          onNavigate('assessments');
                          onClose();
                        }}
                        className="w-full flex items-center justify-between p-2.5 rounded-lg hover:bg-suraksha-surface text-left transition"
                      >
                        <div className="flex items-center gap-2.5">
                          <Award className="w-4 h-4 text-suraksha-blue" />
                          <span className="text-xs font-medium text-white">{a.scenarioName}</span>
                        </div>
                        <span className="text-[10px] font-bold text-emerald-400">{a.score}%</span>
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};
