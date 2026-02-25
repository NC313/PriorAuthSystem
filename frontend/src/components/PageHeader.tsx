import type { ReactNode } from 'react';

interface Props {
  title: string;
  subtitle?: string;
  action?: ReactNode;
}

export default function PageHeader({ title, subtitle, action }: Props) {
  return (
    <div style={{
      display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start',
      marginBottom: 24,
    }}>
      <div>
        <h1 style={{ fontSize: 24, fontWeight: 700, color: 'var(--gray-900)' }}>{title}</h1>
        {subtitle && <p style={{ fontSize: 14, color: 'var(--gray-500)', marginTop: 4 }}>{subtitle}</p>}
      </div>
      {action && <div>{action}</div>}
    </div>
  );
}
