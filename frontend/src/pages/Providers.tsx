import { useQuery } from '@tanstack/react-query';
import { getAllProviders } from '../api/providers';
import PageHeader from '../components/PageHeader';
import DataTable, { type Column } from '../components/DataTable';
import ActionButton from '../components/ActionButton';
import { useToast } from '../components/Toast';
import type { ProviderDto } from '../types';

export default function Providers() {
  const { showToast } = useToast();
  const { data: providers, isLoading } = useQuery({ queryKey: ['providers'], queryFn: getAllProviders });

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
        }}>{'\ud83d\udccb'}</button>
      </div>
    )},
    { key: 'fullName', header: 'Name', render: (r) => <span style={{ fontWeight: 500 }}>{r.fullName}</span> },
    { key: 'specialty', header: 'Specialty' },
    { key: 'email', header: 'Email' },
    { key: 'actions', header: '', width: '100px', render: () => (
      <ActionButton variant="ghost" style={{ padding: '4px 10px', fontSize: 11 }}>View Auths</ActionButton>
    )},
  ];

  return (
    <div>
      <PageHeader
        title="Provider Directory"
        action={<span style={{ fontSize: 13, color: 'var(--gray-500)', background: 'var(--gray-100)', padding: '4px 12px', borderRadius: 999 }}>{(providers ?? []).length} providers</span>}
      />
      <DataTable columns={columns} data={providers ?? []} loading={isLoading} emptyMessage="No providers found" />
    </div>
  );
}
