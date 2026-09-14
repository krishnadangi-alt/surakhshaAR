import React, { useState } from 'react';
import type { Worker } from '../../types';
import { Modal } from '../common/Modal';
import { ShieldAlert, Calendar, BookOpen, Check } from 'lucide-react';

interface AssignRetrainingModalProps {
  worker: Worker | null;
  isOpen: boolean;
  onClose: () => void;
  onAssign: (data: { workerId: string; moduleId: string; weakArea: string; date: string }) => void;
}

export const AssignRetrainingModal: React.FC<AssignRetrainingModalProps> = ({
  worker,
  isOpen,
  onClose,
  onAssign,
}) => {
  const [selectedModule, setSelectedModule] = useState('m-gas');
  const [weakArea, setWeakArea] = useState('SCBA Cylinder Pressure Zeroing & Methane Calibration');
  const [targetDate, setTargetDate] = useState('2026-09-20');
  const [isSuccess, setIsSuccess] = useState(false);

  if (!worker) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onAssign({
      workerId: worker.id,
      moduleId: selectedModule,
      weakArea,
      date: targetDate,
    });
    setIsSuccess(true);
    setTimeout(() => {
      setIsSuccess(false);
      onClose();
    }, 1200);
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Assign Retraining & Mandatory Re-evaluation"
      subtitle={`Target Worker: ${worker.name} (${worker.employeeId})`}
      maxWidth="md"
    >
      {isSuccess ? (
        <div className="flex flex-col items-center justify-center py-8 text-center">
          <div className="rounded-full bg-emerald-500/10 p-4 text-emerald-400 mb-3 border border-emerald-500/30">
            <Check className="w-8 h-8" />
          </div>
          <h4 className="text-base font-bold text-white mb-1">Retraining Order Logged</h4>
          <p className="text-xs text-suraksha-subtext">
            Mandatory retraining session scheduled and notification dispatched to {worker.name}.
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="p-3.5 rounded-xl bg-suraksha-surface border border-suraksha-border flex items-start gap-3">
            <ShieldAlert className="w-5 h-5 text-suraksha-amber shrink-0 mt-0.5" />
            <div className="text-xs text-suraksha-subtext">
              Assigning retraining overrides current compliance status to <strong className="text-white">Retraining Assigned</strong>. The worker will be re-assessed on AR simulation equipment upon completion.
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-suraksha-subtext mb-1.5">
              Select Safety Module
            </label>
            <div className="relative">
              <BookOpen className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-suraksha-subtext" />
              <select
                value={selectedModule}
                onChange={(e) => setSelectedModule(e.target.value)}
                className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface pl-9 pr-4 py-2 text-xs font-semibold text-white focus:border-suraksha-blue focus:outline-none"
              >
                <option value="m-fire">Fire Safety — Extinguisher & Evacuation Drills</option>
                <option value="m-gas">Gas Safety — SCBA & Methane Isolation</option>
                <option value="m-mach">Machinery Safety — LOTO & Interlocks</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-suraksha-subtext mb-1.5">
              Identified Weak Protocols
            </label>
            <input
              type="text"
              value={weakArea}
              onChange={(e) => setWeakArea(e.target.value)}
              className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface px-3 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
            />
          </div>

          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-suraksha-subtext mb-1.5">
              Target Re-assessment Date
            </label>
            <div className="relative">
              <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-suraksha-subtext" />
              <input
                type="date"
                value={targetDate}
                onChange={(e) => setTargetDate(e.target.value)}
                className="w-full rounded-lg border border-suraksha-border bg-suraksha-surface pl-9 pr-4 py-2 text-xs font-medium text-white focus:border-suraksha-blue focus:outline-none"
              />
            </div>
          </div>

          <div className="flex items-center justify-end gap-2 pt-4 border-t border-suraksha-border">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg border border-suraksha-border px-4 py-2 text-xs font-semibold text-suraksha-subtext hover:bg-suraksha-hover transition"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="rounded-lg bg-suraksha-blue px-4 py-2 text-xs font-semibold text-white hover:bg-suraksha-blueHover transition shadow-subtle"
            >
              Confirm Retraining Order
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
};
