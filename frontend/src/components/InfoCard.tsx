import type { ReactNode } from 'react';

interface FieldRow {
  label: string;
  value: ReactNode;
}

interface Props {
  title: string;
  fields: FieldRow[];
}

export default function InfoCard({ title, fields }: Props) {
  return (
    <div style={{
      background: 'var(--white)', borderRadius: 'var(--radius)',
      boxShadow: 'var(--shadow)', overflow: 'hidden',
    }}>
      <div style={{
        background: 'var(--navy)', color: 'var(--white)',
        padding: '12px 16px', fontSize: 14, fontWeight: 600,
      }}>{title}</div>
      <div style={{ padding: 16 }}>
        {fields.map((f, i) => (
          <div key={i} style={{
            display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start',
            padding: '8px 0',
            borderBottom: i < fields.length - 1 ? '1px solid var(--gray-100)' : 'none',
          }}>
            <span style={{ fontSize: 12, color: 'var(--gray-500)', fontWeight: 500, minWidth: 100 }}>{f.label}</span>
            <span style={{ fontSize: 13, color: 'var(--gray-900)', textAlign: 'right', flex: 1 }}>{f.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
