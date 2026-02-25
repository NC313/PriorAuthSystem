import { useQuery } from '@tanstack/react-query';
import { getAllPayers } from '../api/payers';
import { getAllAuths } from '../api/priorAuth';
import PageHeader from '../components/PageHeader';
import type { PayerDto, PriorAuthSummaryDto } from '../types';

export default function Payers() {
  const { data: payers, isLoading } = useQuery({ queryKey: ['payers'], queryFn: getAllPayers });
  const { data: allAuths } = useQuery({ queryKey: ['allAuths'], queryFn: getAllAuths });

  if (isLoading) {
    return <div style={{ textAlign: 'center', padding: 48, color: 'var(--gray-400)' }}>Loading payers...</div>;
  }

  const getPayerStats = (payerName: string) => {
    const auths = (allAuths ?? []).filter((a: PriorAuthSummaryDto) => a.payerName === payerName);
    const approved = auths.filter(a => a.status === 'Approved' || a.status === 'AppealApproved').length;
    const total = auths.length;
    const rate = total > 0 ? Math.round((approved / total) * 100) : 0;
    return { total, approvalRate: rate };
  };

  const slaColor = (days: number) => {
    if (days <= 3) return 'var(--green)';
    if (days <= 5) return 'var(--amber)';
    return 'var(--red)';
  };

  return (
    <div>
      <PageHeader title="Payer Management" />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
        {(payers ?? []).map((p: PayerDto) => {
          const stats = getPayerStats(p.name);
          return (
            <div key={p.id} style={{
              background: 'var(--white)', borderRadius: 'var(--radius)',
              boxShadow: 'var(--shadow)', padding: 24,
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 16 }}>
                <h3 style={{ fontSize: 18, fontWeight: 600 }}>{p.name}</h3>
                <code style={{ fontSize: 11, background: 'var(--gray-100)', padding: '3px 10px', borderRadius: 4 }}>{p.payerId}</code>
              </div>
              <div style={{ marginBottom: 16 }}>
                <div style={{ fontSize: 12, color: 'var(--gray-500)', marginBottom: 4 }}>Standard Response SLA</div>
                <span style={{ fontSize: 20, fontWeight: 700, color: slaColor(p.standardResponseDays) }}>{p.standardResponseDays} days</span>
              </div>
              <div style={{ fontSize: 12, color: 'var(--gray-500)', marginBottom: 12 }}>
                <div>{p.email}</div>
                <div>{p.phone}</div>
              </div>
              <div style={{
                borderTop: '1px solid var(--gray-200)', paddingTop: 12,
                display: 'flex', gap: 24,
              }}>
                <div>
                  <div style={{ fontSize: 20, fontWeight: 700 }}>{stats.total}</div>
                  <div style={{ fontSize: 11, color: 'var(--gray-500)' }}>Total Auths</div>
                </div>
                <div>
                  <div style={{ fontSize: 20, fontWeight: 700, color: 'var(--green)' }}>{stats.approvalRate}%</div>
                  <div style={{ fontSize: 11, color: 'var(--gray-500)' }}>Approval Rate</div>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
