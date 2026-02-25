import { NavLink, Outlet, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useDemoUser } from '../hooks/useDemoUser';
import { useSignalR } from '../hooks/useSignalR';
import { getStats } from '../api/priorAuth';

const navSections = [
  {
    label: 'CLINICAL',
    items: [
      { icon: '\ud83d\udccb', label: 'Dashboard', to: '/app', end: true },
      { icon: '\u2705', label: 'Review Queue', to: '/app/review-queue', badge: true },
      { icon: '\u2795', label: 'Submit Auth', to: '/app/submit' },
    ],
  },
  {
    label: 'MANAGEMENT',
    items: [
      { icon: '\ud83d\udc65', label: 'Patients', to: '/app/patients' },
      { icon: '\ud83c\udfe5', label: 'Providers', to: '/app/providers' },
      { icon: '\ud83d\udcb3', label: 'Payers', to: '/app/payers' },
    ],
  },
  {
    label: 'ANALYTICS',
    items: [
      { icon: '\ud83d\udcca', label: 'Reports', to: '/app/reports' },
      { icon: '\ud83d\udd0d', label: 'Audit Log', to: '/app/audit' },
      { icon: '\ud83d\udd2c', label: 'FHIR Explorer', to: '/app/fhir' },
    ],
  },
];

const pageTitles: Record<string, string> = {
  '/app': 'Dashboard',
  '/app/review-queue': 'Review Queue',
  '/app/submit': 'Submit Authorization',
  '/app/patients': 'Patients',
  '/app/providers': 'Providers',
  '/app/payers': 'Payers',
  '/app/reports': 'Reports',
  '/app/audit': 'Audit Log',
  '/app/fhir': 'FHIR Explorer',
};

export default function AppShell() {
  const { user, logout } = useDemoUser();
  const location = useLocation();
  const { isConnected } = useSignalR();
  const { data: stats } = useQuery({ queryKey: ['stats'], queryFn: getStats });

  const pendingCount = stats ? stats.pending : 0;
  const initials = user?.name?.split(' ').map(n => n[0]).join('').slice(0, 2) ?? '?';
  const pageTitle = pageTitles[location.pathname] ?? 'Authorization Detail';

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      {/* Sidebar */}
      <aside style={{
        width: 260, background: 'var(--navy)', position: 'fixed', top: 0, bottom: 0,
        display: 'flex', flexDirection: 'column', color: 'var(--white)', zIndex: 100,
      }}>
        <div style={{ padding: '20px 20px 16px' }}>
          <div style={{ fontSize: 18, fontWeight: 700, display: 'flex', alignItems: 'center', gap: 8 }}>
            <span>⚕</span> PriorAuth Manager
          </div>
          <div style={{ fontSize: 11, color: 'var(--gray-400)', marginTop: 4 }}>Clinical Decision Support</div>
        </div>
        <div style={{ height: 1, background: 'rgba(255,255,255,0.1)', margin: '0 16px' }} />
        <nav style={{ flex: 1, padding: '12px 0', overflowY: 'auto' }}>
          {navSections.map(section => (
            <div key={section.label}>
              <div style={{
                fontSize: 10, fontWeight: 700, color: 'var(--gray-400)',
                padding: '16px 20px 6px', letterSpacing: '0.1em',
              }}>{section.label}</div>
              {section.items.map(item => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.end}
                  style={({ isActive }) => ({
                    display: 'flex', alignItems: 'center', gap: 10,
                    padding: '10px 20px', fontSize: 13, fontWeight: 500,
                    color: isActive ? 'var(--white)' : 'var(--gray-400)',
                    background: isActive ? 'var(--navy-light)' : 'transparent',
                    borderLeft: isActive ? '3px solid var(--green)' : '3px solid transparent',
                    transition: 'var(--transition)',
                    textDecoration: 'none',
                  })}
                >
                  <span>{item.icon}</span>
                  <span style={{ flex: 1 }}>{item.label}</span>
                  {item.badge && pendingCount > 0 && (
                    <span style={{
                      background: 'var(--red)', color: 'var(--white)', fontSize: 10,
                      fontWeight: 700, padding: '2px 7px', borderRadius: 999,
                    }}>{pendingCount}</span>
                  )}
                </NavLink>
              ))}
            </div>
          ))}
        </nav>
        <div style={{
          padding: 16, borderTop: '1px solid rgba(255,255,255,0.1)',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 10 }}>
            <div style={{
              width: 36, height: 36, borderRadius: '50%', background: 'var(--green)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontSize: 13, fontWeight: 700, color: 'var(--white)',
            }}>{initials}</div>
            <div>
              <div style={{ fontSize: 13, fontWeight: 600 }}>{user?.name}</div>
              <span style={{
                fontSize: 10, fontWeight: 600, padding: '2px 8px',
                borderRadius: 999, background: 'var(--navy-hover)', color: 'var(--green)',
              }}>{user?.role}</span>
            </div>
          </div>
          <button onClick={logout} style={{
            width: '100%', padding: '8px 0', borderRadius: 'var(--radius)',
            border: '1px solid rgba(255,255,255,0.2)', background: 'transparent',
            color: 'var(--gray-400)', fontSize: 12, fontWeight: 500,
          }}>Switch Role</button>
        </div>
      </aside>

      {/* Main area */}
      <div style={{ marginLeft: 260, flex: 1, display: 'flex', flexDirection: 'column' }}>
        {/* Top bar */}
        <header style={{
          height: 60, background: 'var(--white)', borderBottom: '1px solid var(--gray-200)',
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          padding: '0 24px', position: 'sticky', top: 0, zIndex: 50,
        }}>
          <div style={{ fontSize: 15, fontWeight: 600, color: 'var(--gray-900)' }}>{pageTitle}</div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <div style={{
              display: 'flex', alignItems: 'center', gap: 6, fontSize: 12,
              color: isConnected ? 'var(--green)' : 'var(--gray-400)',
              padding: '4px 12px', borderRadius: 999,
              background: isConnected ? 'var(--green-light)' : 'var(--gray-100)',
            }}>
              <span style={{
                width: 8, height: 8, borderRadius: '50%',
                background: isConnected ? 'var(--green)' : 'var(--gray-400)',
                animation: isConnected ? 'pulse-dot 2s ease-in-out infinite' : 'none',
              }} />
              {isConnected ? 'Live Updates' : 'Connecting...'}
            </div>
            <span style={{ fontSize: 18, color: 'var(--gray-400)', cursor: 'pointer' }}>{'\ud83d\udd14'}</span>
            <div style={{
              width: 32, height: 32, borderRadius: '50%', background: 'var(--navy)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontSize: 12, fontWeight: 700, color: 'var(--white)',
            }}>{initials}</div>
          </div>
        </header>

        {/* Content */}
        <main style={{ flex: 1, padding: 24, overflowY: 'auto' }}>
          <Outlet />
        </main>
      </div>

      <style>{`@keyframes pulse-dot { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }`}</style>
    </div>
  );
}
