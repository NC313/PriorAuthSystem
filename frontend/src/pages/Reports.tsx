import { useQuery } from '@tanstack/react-query';
import { getStats, getAllAuths } from '../api/priorAuth';
import PageHeader from '../components/PageHeader';
import StatCard from '../components/StatCard';
import type { PriorAuthSummaryDto, PriorAuthStatus } from '../types';

const statusColors: Partial<Record<PriorAuthStatus, string>> = {
  Submitted: 'var(--blue)',
  UnderReview: 'var(--amber)',
  Approved: 'var(--green)',
  Denied: 'var(--red)',
  AdditionalInfoRequested: 'var(--orange)',
  Appealed: 'var(--purple)',
  AppealApproved: '#10b981',
  AppealDenied: '#f43f5e',
  Expired: 'var(--gray-400)',
  Draft: 'var(--gray-300)',
};

export default function Reports() {
  const { data: stats, isLoading: statsLoading } = useQuery({ queryKey: ['stats'], queryFn: getStats });
  const { data: allAuths } = useQuery({ queryKey: ['allAuths'], queryFn: getAllAuths });

  // Status breakdown for donut chart
  const statusBreakdown: Record<string, number> = {};
  (allAuths ?? []).forEach((a: PriorAuthSummaryDto) => {
    statusBreakdown[a.status] = (statusBreakdown[a.status] ?? 0) + 1;
  });
  const totalAuths = allAuths?.length ?? 0;

  // Build conic gradient
  let gradientParts: string[] = [];
  let cumPct = 0;
  Object.entries(statusBreakdown).forEach(([status, count]) => {
    const pct = totalAuths > 0 ? (count / totalAuths) * 100 : 0;
    const color = statusColors[status as PriorAuthStatus] ?? 'var(--gray-300)';
    gradientParts.push(`${color} ${cumPct}% ${cumPct + pct}%`);
    cumPct += pct;
  });
  const gradient = gradientParts.length > 0 ? `conic-gradient(${gradientParts.join(', ')})` : 'var(--gray-200)';

  // Monthly trend (last 6 months)
  const monthCounts: { label: string; count: number }[] = [];
  const now = new Date();
  for (let i = 5; i >= 0; i--) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
    const label = d.toLocaleString('default', { month: 'short' });
    const count = (allAuths ?? []).filter((a: PriorAuthSummaryDto) => {
      const s = new Date(a.submittedAt);
      return s.getMonth() === d.getMonth() && s.getFullYear() === d.getFullYear();
    }).length;
    monthCounts.push({ label, count });
  }
  const maxCount = Math.max(...monthCounts.map(m => m.count), 1);

  // Denial analysis
  const denialBreakdown = stats?.denialReasonBreakdown ?? {};
  const totalDenials = Object.values(denialBreakdown).reduce((a: number, b: number) => a + b, 0);

  return (
    <div>
      <PageHeader title="Analytics & Reports" subtitle="Comprehensive authorization analytics" />

      {/* Stats Row */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16, marginBottom: 24 }}>
        <StatCard label="Pending" value={statsLoading ? '...' : stats?.pending ?? 0} borderColor="var(--blue)" />
        <StatCard label="Approved" value={statsLoading ? '...' : stats?.approved ?? 0} borderColor="var(--green)" />
        <StatCard label="Denied" value={statsLoading ? '...' : stats?.denied ?? 0} borderColor="var(--red)" />
        <StatCard label="Avg Response" value={statsLoading ? '...' : `${stats?.avgResponseDays ?? 0} days`} borderColor="var(--purple)" />
      </div>

      {/* Charts Row */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, marginBottom: 24 }}>
        {/* Donut */}
        <div style={{ background: 'var(--white)', borderRadius: 'var(--radius)', boxShadow: 'var(--shadow)', padding: 24 }}>
          <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 16 }}>Authorization Volume by Status</h3>
          <div style={{ display: 'flex', alignItems: 'center', gap: 32 }}>
            <div style={{
              width: 160, height: 160, borderRadius: '50%', background: gradient,
              position: 'relative', flexShrink: 0,
            }}>
              <div style={{
                position: 'absolute', inset: 30, borderRadius: '50%',
                background: 'var(--white)', display: 'flex', alignItems: 'center', justifyContent: 'center',
                flexDirection: 'column',
              }}>
                <div style={{ fontSize: 24, fontWeight: 700 }}>{totalAuths}</div>
                <div style={{ fontSize: 10, color: 'var(--gray-500)' }}>Total</div>
              </div>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
              {Object.entries(statusBreakdown).map(([status, count]) => (
                <div key={status} style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 12 }}>
                  <span style={{ width: 10, height: 10, borderRadius: 2, background: statusColors[status as PriorAuthStatus] ?? 'var(--gray-300)' }} />
                  <span style={{ color: 'var(--gray-600)', flex: 1 }}>{status}</span>
                  <span style={{ fontWeight: 600 }}>{count}</span>
                  <span style={{ color: 'var(--gray-400)', width: 36, textAlign: 'right' }}>{totalAuths > 0 ? Math.round((count / totalAuths) * 100) : 0}%</span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Bar Chart */}
        <div style={{ background: 'var(--white)', borderRadius: 'var(--radius)', boxShadow: 'var(--shadow)', padding: 24 }}>
          <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 16 }}>Monthly Submission Trend</h3>
          <div style={{ display: 'flex', alignItems: 'flex-end', gap: 12, height: 180, paddingTop: 16 }}>
            {monthCounts.map(m => (
              <div key={m.label} style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <span style={{ fontSize: 12, fontWeight: 600, marginBottom: 4 }}>{m.count}</span>
                <div style={{
                  width: '100%', borderRadius: '4px 4px 0 0',
                  background: 'var(--blue)', height: `${(m.count / maxCount) * 140}px`,
                  minHeight: m.count > 0 ? 8 : 0, transition: 'height 0.3s ease',
                }} />
                <span style={{ fontSize: 11, color: 'var(--gray-500)', marginTop: 8 }}>{m.label}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Denial Analysis */}
      {totalDenials > 0 && (
        <div style={{ background: 'var(--white)', borderRadius: 'var(--radius)', boxShadow: 'var(--shadow)', padding: 24, marginBottom: 24 }}>
          <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 16 }}>Denial Analysis</h3>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--gray-200)' }}>
                <th style={{ textAlign: 'left', padding: '8px', color: 'var(--gray-500)', fontSize: 12 }}>Denial Reason</th>
                <th style={{ textAlign: 'right', padding: '8px', color: 'var(--gray-500)', fontSize: 12 }}>Count</th>
                <th style={{ textAlign: 'right', padding: '8px', color: 'var(--gray-500)', fontSize: 12 }}>% of Total</th>
              </tr>
            </thead>
            <tbody>
              {Object.entries(denialBreakdown).map(([reason, count]) => (
                <tr key={reason} style={{ borderBottom: '1px solid var(--gray-100)' }}>
                  <td style={{ padding: 8 }}>{reason}</td>
                  <td style={{ padding: 8, textAlign: 'right', fontWeight: 600 }}>{count}</td>
                  <td style={{ padding: 8, textAlign: 'right' }}>{Math.round((count / totalDenials) * 100)}%</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div style={{ fontSize: 12, color: 'var(--gray-400)', textAlign: 'center', padding: 16 }}>
        This report reflects live data from the authorization management system. Data refreshes every 30 seconds.
      </div>
    </div>
  );
}
