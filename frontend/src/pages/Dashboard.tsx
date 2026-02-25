import { useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { formatDistanceToNow, format, isPast } from 'date-fns';
import { getStats, getPendingAuths } from '../api/priorAuth';
import { useSignalR } from '../hooks/useSignalR';
import PageHeader from '../components/PageHeader';
import StatCard from '../components/StatCard';
import StatusBadge from '../components/StatusBadge';
import DataTable, { type Column } from '../components/DataTable';
import ActionButton from '../components/ActionButton';
import type { PriorAuthSummaryDto, PriorAuthStatus } from '../types';

export default function Dashboard() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  useSignalR({
    onStatusUpdate: () => {
      queryClient.invalidateQueries({ queryKey: ['stats'] });
      queryClient.invalidateQueries({ queryKey: ['pendingAuths'] });
    },
  });

  const { data: stats, isLoading: statsLoading } = useQuery({ queryKey: ['stats'], queryFn: getStats });
  const { data: pending, isLoading: pendingLoading } = useQuery({ queryKey: ['pendingAuths'], queryFn: getPendingAuths });

  const columns: Column<PriorAuthSummaryDto>[] = [
    { key: 'patientName', header: 'Patient' },
    { key: 'providerName', header: 'Provider' },
    { key: 'payerName', header: 'Payer' },
    { key: 'cptCode', header: 'Procedure (CPT)', width: '110px' },
    { key: 'icdCode', header: 'Diagnosis (ICD-10)', width: '130px' },
    { key: 'submittedAt', header: 'Submitted', render: (r) => formatDistanceToNow(new Date(r.submittedAt), { addSuffix: true }) },
    { key: 'requiredResponseBy', header: 'Response Due', render: (r) => {
      const overdue = isPast(new Date(r.requiredResponseBy));
      return (
        <span style={{ color: overdue ? 'var(--red)' : 'inherit', fontWeight: overdue ? 600 : 400 }}>
          {overdue && '\u26a0 '}{format(new Date(r.requiredResponseBy), 'MMM d, yyyy')}
        </span>
      );
    }},
    { key: 'status', header: 'Status', render: (r) => <StatusBadge status={r.status as PriorAuthStatus} /> },
    { key: 'action', header: '', width: '80px', render: (r) => (
      <ActionButton variant="ghost" onClick={(e) => { e.stopPropagation(); navigate(`/app/auth/${r.id}`); }}>View</ActionButton>
    )},
  ];

  const denialBreakdown = stats?.denialReasonBreakdown ?? {};
  const totalDenials = Object.values(denialBreakdown).reduce((a, b) => a + b, 0);

  const barColors = ['var(--red)', 'var(--amber)', 'var(--blue)', 'var(--purple)', 'var(--orange)', 'var(--green)'];

  return (
    <div>
      <PageHeader title="Dashboard" subtitle="Real-time overview of prior authorization activity" />

      {/* Stats Row */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: 16, marginBottom: 24 }}>
        <StatCard label="Pending" value={statsLoading ? '...' : stats?.pending ?? 0} borderColor="var(--blue)" />
        <StatCard label="Approved" value={statsLoading ? '...' : stats?.approved ?? 0} borderColor="var(--green)" />
        <StatCard label="Denied" value={statsLoading ? '...' : stats?.denied ?? 0} borderColor="var(--red)" />
        <StatCard label="Under Review" value={statsLoading ? '...' : stats?.underReview ?? 0} borderColor="var(--amber)" />
        <StatCard label="Avg Response Time" value={statsLoading ? '...' : `${stats?.avgResponseDays ?? 0} days`} borderColor="var(--purple)" />
      </div>

      {/* Denial Reasons Chart */}
      {totalDenials > 0 && (
        <div style={{
          background: 'var(--white)', borderRadius: 'var(--radius)',
          boxShadow: 'var(--shadow)', padding: 24, marginBottom: 24,
        }}>
          <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 16 }}>Denial Reason Breakdown</h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            {Object.entries(denialBreakdown).map(([reason, count], i) => (
              <div key={reason} style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                <span style={{ fontSize: 12, color: 'var(--gray-600)', width: 180, flexShrink: 0 }}>{reason}</span>
                <div style={{ flex: 1, height: 24, background: 'var(--gray-100)', borderRadius: 4, overflow: 'hidden' }}>
                  <div style={{
                    height: '100%', width: `${(count / totalDenials) * 100}%`,
                    background: barColors[i % barColors.length], borderRadius: 4,
                    transition: 'width 0.5s ease',
                  }} />
                </div>
                <span style={{ fontSize: 12, fontWeight: 600, width: 32, textAlign: 'right' }}>{count}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Pending Auths Table */}
      <div>
        <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 12 }}>Pending Authorizations</h3>
        <DataTable
          columns={columns}
          data={pending ?? []}
          loading={pendingLoading}
          onRowClick={(r) => navigate(`/app/auth/${r.id}`)}
          emptyMessage="No pending authorizations"
        />
      </div>
    </div>
  );
}
