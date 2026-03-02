import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { getAllProviders, createProvider } from '../api/providers';
import PageHeader from '../components/PageHeader';
import DataTable, { type Column } from '../components/DataTable';
import ActionButton from '../components/ActionButton';
import Modal from '../components/Modal';
import { useToast } from '../components/Toast';
import { useDemoUser } from '../hooks/useDemoUser';
import type { ProviderDto } from '../types';

export default function Providers() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { user } = useDemoUser();
  const { data: providers, isLoading } = useQuery({ queryKey: ['providers'], queryFn: getAllProviders });
  const [showAddModal, setShowAddModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    firstName: '', lastName: '', npi: '', specialty: '',
    organizationName: '', phone: '', email: '',
  });

  const copyNpi = (npi: string) => {
    navigator.clipboard.writeText(npi);
    showToast(`NPI ${npi} copied to clipboard`, 'success');
  };

  const columns: Column<ProviderDto>[] = [
    { key: 'npi', header: 'NPI', width: '140px', render: (r) => (
      <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
        <code style={{ fontSize: 12 }}>{r.npi}</code>
        <button onClick={(e) => { e.stopPropagation(); copyNpi(r.npi); }} style={{
          background: 'none', border: 'none', fontSize: 12, color: 'var(--gray-400)',
          cursor: 'pointer', padding: 2,
        }}>{'📋'}</button>
      </div>
    )},
    { key: 'fullName', header: 'Name', render: (r) => <span style={{ fontWeight: 500 }}>{r.fullName}</span> },
    { key: 'specialty', header: 'Specialty' },
    { key: 'email', header: 'Email' },
    { key: 'actions', header: '', width: '100px', render: () => (
      <ActionButton variant="ghost" style={{ padding: '4px 10px', fontSize: 11 }}>View Auths</ActionButton>
    )},
  ];

  const handleSubmit = async () => {
    if (!form.firstName || !form.lastName || !form.npi || !form.specialty || !form.phone) {
      showToast('First name, last name, NPI, specialty, and phone are required', 'error');
      return;
    }
    setSubmitting(true);
    try {
      await createProvider(form);
      await queryClient.invalidateQueries({ queryKey: ['providers'] });
      showToast(`Provider Dr. ${form.firstName} ${form.lastName} added`, 'success');
      setShowAddModal(false);
      setForm({ firstName: '', lastName: '', npi: '', specialty: '', organizationName: '', phone: '', email: '' });
    } catch {
      showToast('Failed to create provider', 'error');
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
        title="Provider Directory"
        action={
          <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
            <span style={{ fontSize: 13, color: 'var(--gray-500)', background: 'var(--gray-100)', padding: '4px 12px', borderRadius: 999 }}>{(providers ?? []).length} providers</span>
            {user?.role === 'Admin' && (
              <ActionButton variant="primary" onClick={() => setShowAddModal(true)}>+ Add Provider</ActionButton>
            )}
          </div>
        }
      />
      <DataTable columns={columns} data={providers ?? []} loading={isLoading} emptyMessage="No providers found" />

      {showAddModal && (
        <Modal
          title="Add Provider"
          onClose={() => setShowAddModal(false)}
          footer={
            <>
              <ActionButton variant="ghost" onClick={() => setShowAddModal(false)}>Cancel</ActionButton>
              <ActionButton variant="primary" onClick={handleSubmit} disabled={submitting}>
                {submitting ? 'Saving...' : 'Save Provider'}
              </ActionButton>
            </>
          }
        >
          {field('First Name *', 'firstName')}
          {field('Last Name *', 'lastName')}
          {field('NPI (10 digits) *', 'npi')}
          {field('Specialty *', 'specialty')}
          {field('Organization Name', 'organizationName')}
          {field('Phone *', 'phone', 'tel')}
          {field('Email', 'email', 'email')}
        </Modal>
      )}
    </div>
  );
}
