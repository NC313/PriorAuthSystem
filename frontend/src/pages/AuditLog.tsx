import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { format } from 'date-fns';
import { getAuditLog } from '../api/priorAuth';
import PageHeader from '../components/PageHeader';
import DataTable, { type Column } from '../components/DataTable';
import type { AuditEntry } from '../types';

type TimeFilter = '24h' | '7d' | '30d' | 'all';

export default function AuditLog() {
  const navigate = useNavigate();
  const [roleFilter, setRoleFilter] = useState('All');
  const [actionFilter, setActionFilter] = useState('All');
  const [timeFilter, setTimeFilter] = useState<TimeFilter>('all');

  const { data: entries, isLoading } = useQuery({ queryKey: ['auditLog'], queryFn: getAuditLog });

  const filtered = (entries ?? []).filter(e => {
    if (roleFilter !== 'All' && !e.performedBy.toLowerCase().includes(roleFilter.toLowerCase())) return false;
    if (actionFilter !== 'All' && e.action !== actionFilter) return false;
    if (timeFilter !== 'all') {
      const now = Date.now();
      const ts = new Date(e.timestamp).getTime();
      const hours = timeFilter === '24h' ? 24 : timeFilter === '7d' ? 168 : 720;
      if (now - ts > hours * 3600 * 1000) return false;
    }
    return true;
  });

  const actionColor = (action: string) => {
    if (action.includes('Status')) return 'var(--blue)';
    if (action.includes('Error')) return 'var(--red)';
    return 'var(--gray-500)';
  };

  const actions = [...new Set((entries ?? []).map(e => e.action))];

  const selectStyle = { padding: '6px 12px', borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', fontSize: 12, background: 'var(--white)' };

  const columns: Column<AuditEntry>[] = [
    { key: 'timestamp', header: 'Timestamp', width: '180px', render: (r) => (
      <span style={{ fontSize: 12 }}>{format(new Date(r.timestamp), 'MMM d, yyyy h:mm:ss a')}</span>
    )},
    { key: 'performedBy', header: 'User', render: (r) => <span style={{ fontWeight: 500 }}>{r.performedBy}</span> },
    { key: 'action', header: 'Action', render: (r) => (
      <span style={{ color: actionColor(r.action), fontWeight: 500, fontSize: 12 }}>{r.action}</span>
    )},
    { key: 'requestId', header: 'Request ID', width: '120px', render: (r) => r.requestId ? (
      <code onClick={(e) => { e.stopPropagation(); navigate(`/app/auth/${r.requestId}`); }} style={{ fontSize: 11, cursor: 'pointer', color: 'var(--blue)' }}>
        {r.requestId.slice(0, 8)}...
      </code>
    ) : <span style={{ color: 'var(--gray-400)' }}>-</span> },
    { key: 'details', header: 'Details', render: (r) => <span style={{ fontSize: 12, color: 'var(--gray-600)' }}>{r.details}</span> },
  ];

  return (
    <div>
      <PageHeader title="Audit Log" subtitle="Complete record of all system actions" />

      <div style={{ display: 'flex', gap: 12, marginBottom: 20 }}>
        <select value={roleFilter} onChange={e => setRoleFilter(e.target.value)} style={selectStyle}>
          <option value="All">All Roles</option>
          <option value="admin">Admin</option>
          <option value="reviewer">Reviewer</option>
          <option value="provider">Provider</option>
        </select>
        <select value={actionFilter} onChange={e => setActionFilter(e.target.value)} style={selectStyle}>
          <option value="All">All Actions</option>
          {actions.map(a => <option key={a} value={a}>{a}</option>)}
        </select>
        <select value={timeFilter} onChange={e => setTimeFilter(e.target.value as TimeFilter)} style={selectStyle}>
          <option value="24h">Last 24 hours</option>
          <option value="7d">Last 7 days</option>
          <option value="30d">Last 30 days</option>
          <option value="all">All time</option>
        </select>
      </div>

      <DataTable columns={columns} data={filtered} loading={isLoading} emptyMessage="No audit entries found" />
    </div>
  );
}
