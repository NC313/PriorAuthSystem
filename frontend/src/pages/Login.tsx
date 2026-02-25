import { useNavigate } from 'react-router-dom';
import type { DemoRole, DemoUser } from '../types';

const profiles: { icon: string; user: DemoUser; description: string }[] = [
  { icon: '\ud83c\udfe5', user: { role: 'Admin', name: 'Dr. Sarah Chen', userId: 'admin-001' }, description: 'Full system access \u2014 manage all authorizations, users, and reports' },
  { icon: '\ud83d\udc68\u200d\u2695\ufe0f', user: { role: 'Reviewer', name: 'Marcus Williams', userId: 'reviewer-001' }, description: 'Review and action pending prior authorization requests' },
  { icon: '\ud83e\ude7a', user: { role: 'Provider', name: 'Dr. James Okafor', userId: 'provider-001' }, description: 'Submit and track prior authorization requests for your patients' },
];

const roleBadgeColors: Record<DemoRole, string> = {
  Admin: 'var(--red)',
  Reviewer: 'var(--amber)',
  Provider: 'var(--green)',
};

export default function Login() {
  const navigate = useNavigate();

  const selectRole = (profile: typeof profiles[0]) => {
    localStorage.setItem('demo_role', profile.user.role);
    localStorage.setItem('demo_user', JSON.stringify(profile.user));
    navigate('/app');
  };

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      {/* Left Panel */}
      <div style={{
        flex: 1, background: 'var(--navy)', color: 'var(--white)',
        display: 'flex', flexDirection: 'column', justifyContent: 'center',
        alignItems: 'center', padding: 48, position: 'relative',
      }}>
        <div style={{ maxWidth: 400, textAlign: 'center' }}>
          <div style={{ fontSize: 48, marginBottom: 16 }}>{'\u2695'}</div>
          <h1 style={{ fontSize: 32, fontWeight: 800, marginBottom: 8 }}>PriorAuth Manager</h1>
          <p style={{ fontSize: 16, color: 'var(--gray-400)', lineHeight: 1.6 }}>
            FHIR R4 Compliant Clinical Decision Support Platform
          </p>
        </div>
        <div style={{
          position: 'absolute', bottom: 32, fontSize: 11, color: 'var(--gray-400)',
          display: 'flex', alignItems: 'center', gap: 6,
        }}>
          <span>{'\ud83d\udd12'}</span> HIPAA Compliant | SOC 2 Type II
        </div>
      </div>

      {/* Right Panel */}
      <div style={{
        flex: 1, background: 'var(--white)',
        display: 'flex', flexDirection: 'column', justifyContent: 'center',
        alignItems: 'center', padding: 48,
      }}>
        <div style={{ maxWidth: 420, width: '100%' }}>
          <h2 style={{ fontSize: 24, fontWeight: 700, marginBottom: 4 }}>Select Demo Profile</h2>
          <p style={{ fontSize: 14, color: 'var(--gray-500)', marginBottom: 32 }}>
            Choose a role to explore the platform
          </p>

          <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            {profiles.map(profile => (
              <button
                key={profile.user.role}
                onClick={() => selectRole(profile)}
                style={{
                  display: 'flex', alignItems: 'center', gap: 16,
                  padding: 20, borderRadius: 'var(--radius-lg)',
                  border: '2px solid var(--gray-200)', background: 'var(--white)',
                  textAlign: 'left', transition: 'var(--transition)',
                  cursor: 'pointer', width: '100%',
                }}
                onMouseEnter={e => { e.currentTarget.style.borderColor = 'var(--green)'; e.currentTarget.style.boxShadow = 'var(--shadow-md)'; }}
                onMouseLeave={e => { e.currentTarget.style.borderColor = 'var(--gray-200)'; e.currentTarget.style.boxShadow = 'none'; }}
              >
                <div style={{
                  width: 48, height: 48, borderRadius: '50%',
                  background: 'var(--gray-100)',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  fontSize: 24, flexShrink: 0,
                }}>{profile.icon}</div>
                <div style={{ flex: 1 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <span style={{ fontWeight: 600, fontSize: 15 }}>{profile.user.name}</span>
                    <span style={{
                      fontSize: 10, fontWeight: 700, padding: '2px 8px',
                      borderRadius: 999, color: 'var(--white)',
                      background: roleBadgeColors[profile.user.role],
                    }}>{profile.user.role}</span>
                  </div>
                  <div style={{ fontSize: 12, color: 'var(--gray-500)', marginTop: 4 }}>{profile.description}</div>
                </div>
              </button>
            ))}
          </div>

          <p style={{
            fontSize: 11, color: 'var(--gray-400)', textAlign: 'center', marginTop: 32,
          }}>
            This is a portfolio demonstration. No real patient data is used.
          </p>
        </div>
      </div>
    </div>
  );
}
