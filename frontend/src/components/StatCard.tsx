interface Props {
  label: string;
  value: string | number;
  borderColor: string;
  trend?: 'up' | 'down';
}

export default function StatCard({ label, value, borderColor, trend }: Props) {
  return (
    <div style={{
      background: 'var(--white)', borderRadius: 'var(--radius)',
      boxShadow: 'var(--shadow)', padding: '20px 24px',
      borderLeft: `4px solid ${borderColor}`,
      transition: 'var(--transition)',
      cursor: 'default',
    }}
    onMouseEnter={e => { e.currentTarget.style.boxShadow = 'var(--shadow-md)'; e.currentTarget.style.transform = 'translateY(-2px)'; }}
    onMouseLeave={e => { e.currentTarget.style.boxShadow = 'var(--shadow)'; e.currentTarget.style.transform = 'translateY(0)'; }}
    >
      <div style={{ fontSize: 28, fontWeight: 700, color: 'var(--gray-900)' }}>
        {value}
        {trend && (
          <span style={{ fontSize: 14, marginLeft: 8, color: trend === 'up' ? 'var(--green)' : 'var(--red)' }}>
            {trend === 'up' ? '\u2191' : '\u2193'}
          </span>
        )}
      </div>
      <div style={{ fontSize: 13, color: 'var(--gray-500)', marginTop: 4 }}>{label}</div>
    </div>
  );
}
