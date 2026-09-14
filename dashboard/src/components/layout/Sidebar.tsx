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
      className={`relative flex flex-col justify-between border-r border-suraksha-border bg-suraksha-dark transition-all duration-300 z-30 ${
        isCollapsed ? 'w-16' : 'w-64'
      }`}
    >
      {/* Brand Header */}
      <div className="p-4 border-b border-suraksha-border flex items-center justify-between">
        {!isCollapsed ? (
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-9 h-9 rounded-xl bg-suraksha-card border border-suraksha-border text-suraksha-amber shadow-subtle">
              <Shield className="w-5 h-5 fill-suraksha-amber/20" />
            </div>
            <div>
              <h1 className="text-sm font-extrabold tracking-wider text-white">SURAKSHAAR</h1>
              <p className="text-[9px] uppercase tracking-widest text-suraksha-amber font-semibold">
                Admin Portal
              </p>
            </div>
          </div>
        ) : (
          <div className="mx-auto flex items-center justify-center w-9 h-9 rounded-xl bg-suraksha-card border border-suraksha-border text-suraksha-amber">
            <Shield className="w-5 h-5 fill-suraksha-amber/20" />
          </div>
        )}

        <button
          onClick={onToggleCollapse}
          className="p-1 rounded-lg text-suraksha-subtext hover:bg-suraksha-hover hover:text-white transition"
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
              className={`relative w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-xs font-semibold transition-all duration-150 ${
                isActive
                  ? 'bg-suraksha-surface text-white shadow-subtle border border-suraksha-border'
                  : 'text-suraksha-subtext hover:bg-suraksha-hover hover:text-white'
              }`}
            >
              {isActive && (
                <span className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-5 bg-suraksha-amber rounded-r-full" />
              )}
              <Icon
                className={`w-4 h-4 shrink-0 ${
                  isActive ? 'text-suraksha-amber' : 'text-suraksha-subtext'
                }`}
              />
              {!isCollapsed && <span className="truncate">{item.label}</span>}
            </button>
          );
        })}
      </nav>

      {/* Bottom Profile & Actions */}
      <div className="p-3 border-t border-suraksha-border space-y-1">
        {!isCollapsed && (
          <div className="p-2.5 rounded-lg bg-suraksha-surface/60 border border-suraksha-border/60 mb-2 space-y-0.5">
            <p className="text-[10px] uppercase font-bold text-suraksha-subtext">Admin Profile</p>
            <h6 className="text-xs font-bold text-white truncate">Administrator</h6>
            <p className="text-[10px] text-suraksha-subtext truncate">Role: Administrator</p>
            <p className="text-[10px] text-suraksha-amber/90 truncate">Ministry of Labour &amp; Employment</p>
          </div>
        )}

        <button
          onClick={() => alert('Settings configuration modal')}
          title={isCollapsed ? 'Settings' : undefined}
          className="w-full flex items-center gap-3 px-3 py-2 rounded-lg text-xs font-semibold text-suraksha-subtext hover:bg-suraksha-hover hover:text-white transition"
        >
          <Settings className="w-4 h-4 text-suraksha-subtext shrink-0" />
          {!isCollapsed && <span>Settings</span>}
        </button>

        <button
          onClick={onLogout}
          title={isCollapsed ? 'Logout' : undefined}
          className="w-full flex items-center gap-3 px-3 py-2 rounded-lg text-xs font-semibold text-rose-400 hover:bg-rose-500/10 transition"
        >
          <LogOut className="w-4 h-4 shrink-0" />
          {!isCollapsed && <span>Logout</span>}
        </button>
      </div>
    </aside>
  );
};
