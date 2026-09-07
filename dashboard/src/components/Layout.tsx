import { type ReactNode, useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const NAV = [
  { to: '/', label: 'Overview', icon: '\u{1F4CA}' },
  { to: '/workers', label: 'Workers', icon: '\u{1F465}' },
  { to: '/analytics', label: 'Analytics', icon: '\u{1F4C8}' },
  { to: '/certificates', label: 'Certifications', icon: '\u{1F393}' },
  { to: '/settings', label: 'Settings', icon: '\u2699\u{FE0F}' },
];

function getGreeting(): string {
  const h = new Date().getHours();
  if (h < 12) return 'Good morning';
  if (h < 17) return 'Good afternoon';
  return 'Good evening';
}

export default function Layout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth();
  const loc = useLocation();
  const [collapsed, setCollapsed] = useState(false);

  const currentTitle = NAV.find((n) => n.to === loc.pathname)?.label ?? 'SURAKSHAAR';

  return (
    <div className="app-shell">
      <aside className={`sidebar${collapsed ? ' collapsed' : ''}`}>
        <div className="sidebar-brand">
          <h1>SURAKSHAAR</h1>
          <div className="brand-sub">Safety Portal</div>
        </div>

        <button
          className="sidebar-collapse-btn"
          onClick={() => setCollapsed(!collapsed)}
          title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {collapsed ? '\u2192' : '\u2190'} {collapsed ? '' : 'Collapse'}
        </button>

        <nav className="sidebar-nav">
          {NAV.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/'}
              className={({ isActive }) => (isActive ? 'active' : '')}
            >
              <span className="nav-icon">{item.icon}</span>
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          Industrial Safety Training
          <br />& Competency Management
          <br />Government of India
        </div>
      </aside>

      <div className="main-content">
        <header className="topbar">
          <div className="topbar-left">
            <div>
              <div className="topbar-greeting">{getGreeting()}, {user?.name?.split(' ')[0] ?? 'Admin'}</div>
              <div className="topbar-subtitle">Here's the current safety training overview across your workforce.</div>
            </div>
            <div className="topbar-status" style={{ marginLeft: 16 }}>
              <span className="status-dot" />
              <span>System operational</span>
            </div>
          </div>

          <div className="topbar-right">
            <button className="topbar-icon-btn" title="Notifications">
              {'\u{1F514}'}
              <span className="notification-dot" />
            </button>
            <span className="user-badge">
              <span>{user?.name}</span>
              <span style={{ color: 'var(--c-text-muted)' }}>|</span>
              <span style={{ color: 'var(--c-text-secondary)' }}>{user?.role}</span>
            </span>
            <button className="btn btn-sm" onClick={logout} title="Sign out">
              Logout
            </button>
          </div>
        </header>
        <main className="page-body">{children}</main>
      </div>
    </div>
  );
}
