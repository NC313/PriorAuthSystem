import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { getAllPayers, createPayer } from '../api/payers';
import { getAllAuths } from '../api/priorAuth';
import PageHeader from '../components/PageHeader';
import ActionButton from '../components/ActionButton';
import Modal from '../components/Modal';
import { useToast } from '../components/Toast';
import { useDemoUser } from '../hooks/useDemoUser';
import type { PayerDto, PriorAuthSummaryDto } from '../types';

export default function Payers() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { user } = useDemoUser();
  const { data: payers, isLoading } = useQuery({ queryKey: ['payers'], queryFn: getAllPayers });
  const { data: allAuths } = useQuery({ queryKey: ['allAuths'], queryFn: getAllAuths });
  const [showAddModal, setShowAddModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    payerName: '', payerId: '', standardResponseDays: '',
    phone: '', email: '',
  });

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

  const handleSubmit = async () => {
    const days = parseInt(form.standardResponseDays, 10);
    if (!form.payerName || !form.payerId || !days || !form.phone) {
      showToast('Payer name, payer ID, response days, and phone are required', 'error');
      return;
    }
    setSubmitting(true);
    try {
      await createPayer({ ...form, standardResponseDays: days });
      await queryClient.invalidateQueries({ queryKey: ['payers'] });
      showToast(`Payer ${form.payerName} added`, 'success');
      setShowAddModal(false);
      setForm({ payerName: '', payerId: '', standardResponseDays: '', phone: '', email: '' });
    } catch {
      showToast('Failed to create payer', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const field = (label: string, key: keyof typeof form, type = 'text') => (
    <div style={{ marginBottom: 14 }}>
      <label style={{ display: 'block', fontSize: 12, color: 'var(--gray-500)', marginBottom: 4 }}>{label}</label>
      <input
        type={type}
        value={form[key]}
        onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))}
        style={{
          width: '100%', padding: '8px 12px', borderRadius: 'var(--radius)',
          border: '1px solid var(--gray-200)', fontSize: 14, boxSizing: 'border-box',
        }}
      />
    </div>
  );

  return (
    <div>
      <PageHeader
        title="Payer Management"
        action={user?.role === 'Admin' ? (
          <ActionButton variant="primary" onClick={() => setShowAddModal(true)}>+ Add Payer</ActionButton>
        ) : undefined}
      />
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

      {showAddModal && (
        <Modal
          title="Add Payer"
          onClose={() => setShowAddModal(false)}
          footer={
            <>
              <ActionButton variant="ghost" onClick={() => setShowAddModal(false)}>Cancel</ActionButton>
              <ActionButton variant="primary" onClick={handleSubmit} disabled={submitting}>
                {submitting ? 'Saving...' : 'Save Payer'}
              </ActionButton>
            </>
          }
        >
          {field('Payer Name *', 'payerName')}
          {field('Payer ID *', 'payerId')}
          {field('Standard Response Days *', 'standardResponseDays', 'number')}
          {field('Phone *', 'phone', 'tel')}
          {field('Email', 'email', 'email')}
        </Modal>
      )}
    </div>
  );
}
