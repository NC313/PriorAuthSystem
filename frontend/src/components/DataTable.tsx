import type { ReactNode } from 'react';

export interface Column<T> {
  key: string;
  header: string;
  render?: (row: T) => ReactNode;
  width?: string;
}

interface Props<T> {
  columns: Column<T>[];
  data: T[];
  onRowClick?: (row: T) => void;
  loading?: boolean;
  emptyMessage?: string;
}

export default function DataTable<T>({ columns, data, onRowClick, loading, emptyMessage = 'No data found' }: Props<T>) {
  if (loading) {
    return (
      <div style={{ background: 'var(--white)', borderRadius: 'var(--radius)', boxShadow: 'var(--shadow)', overflow: 'hidden' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr>
              {columns.map(col => (
                <th key={col.key} style={{
                  padding: '12px 16px', textAlign: 'left', fontSize: 12,
                  fontWeight: 600, color: 'var(--gray-500)', textTransform: 'uppercase',
                  letterSpacing: '0.05em', background: 'var(--gray-50)',
                  borderBottom: '2px solid var(--gray-200)', position: 'sticky', top: 0,
                  width: col.width,
                }}>{col.header}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {Array.from({ length: 5 }).map((_, i) => (
              <tr key={i}>
                {columns.map(col => (
                  <td key={col.key} style={{ padding: '12px 16px' }}>
                    <div style={{
                      height: 16, background: 'var(--gray-200)', borderRadius: 4,
                      animation: 'pulse 1.5s ease-in-out infinite',
                      width: `${60 + Math.random() * 30}%`,
                    }} />
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
        <style>{`@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }`}</style>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div style={{
        background: 'var(--white)', borderRadius: 'var(--radius)', boxShadow: 'var(--shadow)',
        padding: '48px 24px', textAlign: 'center', color: 'var(--gray-400)',
      }}>
        <div style={{ fontSize: 32, marginBottom: 8 }}>&#128196;</div>
        <div>{emptyMessage}</div>
      </div>
    );
  }

  return (
    <div style={{ background: 'var(--white)', borderRadius: 'var(--radius)', boxShadow: 'var(--shadow)', overflow: 'auto' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr>
            {columns.map(col => (
              <th key={col.key} style={{
                padding: '12px 16px', textAlign: 'left', fontSize: 12,
                fontWeight: 600, color: 'var(--gray-500)', textTransform: 'uppercase',
                letterSpacing: '0.05em', background: 'var(--gray-50)',
                borderBottom: '2px solid var(--gray-200)', position: 'sticky', top: 0,
                width: col.width,
              }}>{col.header}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row, i) => (
            <tr
              key={i}
              onClick={() => onRowClick?.(row)}
              style={{
                background: i % 2 === 0 ? 'var(--white)' : 'var(--gray-50)',
                cursor: onRowClick ? 'pointer' : 'default',
                transition: 'var(--transition)',
              }}
              onMouseEnter={e => { if (onRowClick) e.currentTarget.style.background = 'var(--blue-light)'; }}
              onMouseLeave={e => { e.currentTarget.style.background = i % 2 === 0 ? 'var(--white)' : 'var(--gray-50)'; }}
            >
              {columns.map(col => (
                <td key={col.key} style={{ padding: '12px 16px', borderBottom: '1px solid var(--gray-100)' }}>
                  {col.render ? col.render(row) : String((row as Record<string, unknown>)[col.key] ?? '')}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
