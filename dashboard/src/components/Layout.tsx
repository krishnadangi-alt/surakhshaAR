import { type ReactNode } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const NAV = [
  { to: '/', label: 'Dashboard', icon: '\u{1F4CA}' },
  { to: '/workers', label: 'Workers', icon: '\u{1F465}' },
  { to: '/analytics', label: 'Analytics', icon: '\u{1F4C8}' },
  { to: '/certificates', label: 'Certificates', icon: '\u{1F393}' },
  { to: '/settings', label: 'Settings', icon: '\u2699\uFE0F' },
];

export default function Layout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth();
  const loc = useLocation();

  const currentTitle =
    loc.pathname.startsWith('/workers/') && loc.pathname !== '/workers'
      ? 'Worker Detail'
      : NAV.find((n) => n.to === loc.pathname)?.label ?? 'Dashboard';

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <h1>SURAKSHAAR</h1>
          <div className="brand-sub">Admin Portal</div>
        </div>

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
          <br />&amp; Competency Management
        </div>
      </aside>

      <div className="main-content">
        <header className="topbar">
          <div className="topbar-title">{currentTitle}</div>
          <div className="topbar-right">
            <span className="user-badge">
              <span>{user?.name}</span>
              <span className="user-sep">|</span>
              <span className="user-role">{user?.role}</span>
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
