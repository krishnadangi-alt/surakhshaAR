import React from 'react';
import {
  LayoutDashboard,
  Users,
  Award,
  BarChart3,
  ShieldCheck,
  RotateCcw,
  FileCheck,
  BrainCircuit,
  FileSpreadsheet,
  ChevronLeft,
  ChevronRight,
  LogOut,
  Shield,
  Settings,
} from 'lucide-react';

interface SidebarProps {
  currentScreen: string;
  onNavigate: (screen: string) => void;
  isCollapsed: boolean;
  onToggleCollapse: () => void;
  onLogout: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({
  currentScreen,
  onNavigate,
  isCollapsed,
  onToggleCollapse,
  onLogout,
}) => {
  const navItems = [
    { id: 'overview', label: 'Overview', icon: LayoutDashboard },
    { id: 'workers', label: 'Workers', icon: Users },
    { id: 'assessments', label: 'Assessments', icon: Award },
    { id: 'modules', label: 'Module Analytics', icon: BarChart3 },
    { id: 'competency', label: 'Competency', icon: ShieldCheck },
    { id: 'retraining', label: 'Retraining', icon: RotateCcw },
    { id: 'certificates', label: 'Certificates', icon: FileCheck },
    { id: 'retention', label: 'Retention', icon: BrainCircuit },
    { id: 'reports', label: 'Reports & Export', icon: FileSpreadsheet },
  ];

  return (
    <aside
      className={`relative flex flex-col justify-between border-r border-slate-800 bg-[#0F172A] text-slate-200 transition-all duration-300 z-30 ${
        isCollapsed ? 'w-16' : 'w-64'
      }`}
    >
      {/* Brand Header */}
      <div className="p-4 border-b border-slate-800 flex items-center justify-between">
        {!isCollapsed ? (
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-10 h-10 rounded-xl bg-slate-800 border border-slate-700 text-[#F59E0B] shadow-sm">
              <Shield className="w-5 h-5 fill-[#F59E0B]/20" />
            </div>
            <div>
              <h1 className="text-base font-extrabold tracking-wider text-white">SURAKSHAAR</h1>
              <p className="text-[10px] uppercase tracking-widest text-[#F59E0B] font-bold">
                Admin Portal
              </p>
            </div>
          </div>
        ) : (
          <div className="mx-auto flex items-center justify-center w-10 h-10 rounded-xl bg-slate-800 border border-slate-700 text-[#F59E0B]">
            <Shield className="w-5 h-5 fill-[#F59E0B]/20" />
          </div>
        )}

        <button
          onClick={onToggleCollapse}
          className="p-1.5 rounded-lg text-slate-400 hover:bg-slate-800 hover:text-white transition"
          title={isCollapsed ? 'Expand Sidebar' : 'Collapse Sidebar'}
        >
          {isCollapsed ? <ChevronRight className="w-4 h-4" /> : <ChevronLeft className="w-4 h-4" />}
        </button>
      </div>

      {/* Navigation Links */}
      <nav className="flex-1 overflow-y-auto px-2 py-4 space-y-1">
        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive = currentScreen === item.id || (currentScreen === 'worker-details' && item.id === 'workers');

          return (
            <button
              key={item.id}
              onClick={() => onNavigate(item.id)}
              title={isCollapsed ? item.label : undefined}
              className={`relative w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-[13.5px] font-semibold transition-all duration-150 ${
                isActive
                  ? 'bg-slate-800 text-white shadow-sm border border-slate-700'
                  : 'text-slate-400 hover:bg-slate-800/60 hover:text-white'
              }`}
            >
              {isActive && (
                <span className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-5 bg-[#F59E0B] rounded-r-full" />
              )}
              <Icon
                className={`w-4.5 h-4.5 shrink-0 ${
                  isActive ? 'text-[#F59E0B]' : 'text-slate-400'
                }`}
              />
              {!isCollapsed && <span className="truncate">{item.label}</span>}
            </button>
          );
        })}
      </nav>

      {/* Bottom Profile & Actions */}
      <div className="p-3 border-t border-slate-800 space-y-1">
        {!isCollapsed && (
          <div className="p-3 rounded-xl bg-slate-800/80 border border-slate-700/80 mb-2 space-y-0.5">
            <p className="text-[11px] uppercase font-bold text-slate-400">Admin Profile</p>
            <h6 className="text-[13.5px] font-bold text-white truncate">Administrator</h6>
            <p className="text-[11px] text-slate-400 truncate">Role: Administrator</p>
            <p className="text-[11px] text-[#F59E0B] font-semibold truncate">Ministry of Labour &amp; Employment</p>
          </div>
        )}

        <button
          onClick={() => alert('Settings configuration modal')}
          title={isCollapsed ? 'Settings' : undefined}
          className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-[13.5px] font-semibold text-slate-400 hover:bg-slate-800 hover:text-white transition"
        >
          <Settings className="w-4.5 h-4.5 text-slate-400 shrink-0" />
          {!isCollapsed && <span>Settings</span>}
        </button>

        <button
          onClick={onLogout}
          title={isCollapsed ? 'Logout' : undefined}
          className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-[13.5px] font-semibold text-rose-400 hover:bg-rose-500/10 transition"
        >
          <LogOut className="w-4.5 h-4.5 shrink-0" />
          {!isCollapsed && <span>Logout</span>}
        </button>
      </div>
    </aside>
  );
};
