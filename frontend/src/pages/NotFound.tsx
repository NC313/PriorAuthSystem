import { useNavigate } from 'react-router-dom';

export default function NotFound() {
  const navigate = useNavigate();

  return (
    <div style={{
      display: 'flex', flexDirection: 'column', alignItems: 'center',
      justifyContent: 'center', minHeight: '70vh', textAlign: 'center',
      padding: 24,
    }}>
      <div style={{
        fontSize: 120, fontWeight: 800, color: 'var(--navy)',
        lineHeight: 1, marginBottom: 8, letterSpacing: '-4px',
      }}>
        404
      </div>
      <div style={{ fontSize: 14, color: 'var(--gray-400)', marginBottom: 24 }}>
        &#9866;&#9866;&#9866;
      </div>
      <h1 style={{
        fontSize: 24, fontWeight: 700, color: 'var(--gray-900)', marginBottom: 8,
      }}>
        Authorization Not Found
      </h1>
      <p style={{
        fontSize: 14, color: 'var(--gray-500)', marginBottom: 32, maxWidth: 400,
      }}>
        The requested resource could not be located in our system.
      </p>
      <button
        onClick={() => navigate('/app')}
        style={{
          padding: '12px 28px', borderRadius: 'var(--radius)',
          background: 'var(--navy)', color: 'var(--white)',
          border: 'none', fontSize: 14, fontWeight: 600,
          cursor: 'pointer', transition: 'var(--transition)',
        }}
      >
        Back to Dashboard
      </button>
    </div>
  );
}
