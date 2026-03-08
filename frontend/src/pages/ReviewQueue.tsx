import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import { format, isPast, differenceInHours } from 'date-fns';
import { getPagedPendingAuths, approvePriorAuth, denyPriorAuth, requestAdditionalInfo } from '../api/priorAuth';
import { useDemoUser } from '../hooks/useDemoUser';
import { useToast } from '../components/Toast';
import PageHeader from '../components/PageHeader';
import StatusBadge from '../components/StatusBadge';
import DataTable, { type Column } from '../components/DataTable';
import ActionButton from '../components/ActionButton';
import Modal from '../components/Modal';
import type { PriorAuthSummaryDto, PriorAuthStatus, DenialReason } from '../types';

type ModalType = 'approve' | 'deny' | 'info' | null;

const PAGE_SIZE = 20;

const denialReasons: { value: DenialReason; label: string }[] = [
  { value: 'NotMedicallyNecessary', label: 'Not Medically Necessary' },
  { value: 'ServiceNotCovered', label: 'Service Not Covered' },
  { value: 'RequiresAlternativeTreatment', label: 'Requires Alternative Treatment' },
  { value: 'InsufficientDocumentation', label: 'Insufficient Documentation' },
  { value: 'OutOfNetwork', label: 'Out of Network' },
  { value: 'DuplicateRequest', label: 'Duplicate Request' },
  { value: 'EligibilityIssue', label: 'Eligibility Issue' },
  { value: 'Other', label: 'Other' },
];

function getPriority(r: PriorAuthSummaryDto) {
  const due = new Date(r.requiredResponseBy);
  if (isPast(due)) return { label: 'High', color: 'var(--red)', bg: 'var(--red-light)' };
  if (differenceInHours(due, new Date()) <= 24) return { label: 'Medium', color: 'var(--amber)', bg: 'var(--amber-light)' };
  return { label: 'Normal', color: 'var(--green)', bg: 'var(--green-light)' };
}

export default function ReviewQueue() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useDemoUser();
  const { showToast } = useToast();
  const [modal, setModal] = useState<ModalType>(null);
  const [selectedId, setSelectedId] = useState('');
  const [notes, setNotes] = useState('');
  const [denialReason, setDenialReason] = useState<DenialReason>('NotMedicallyNecessary');
  const [loading, setLoading] = useState(false);

  // Filter state
  const [searchInput, setSearchInput] = useState('');
  const [search, setSearch] = useState(''); // debounced
  const [priorityFilter, setPriorityFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);

  // Debounce search: wait 400 ms after the user stops typing
  useEffect(() => {
    const timer = setTimeout(() => {
      setSearch(searchInput);
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [searchInput]);

  // Reset to page 1 when filters change
  useEffect(() => { setPage(1); }, [statusFilter, priorityFilter]);

  const { data, isLoading } = useQuery({
    queryKey: ['pendingAuths', 'paged', page, search, statusFilter, priorityFilter],
    queryFn: () => getPagedPendingAuths({
      page,
      pageSize: PAGE_SIZE,
      search: search || undefined,
      status: statusFilter || undefined,
      priority: priorityFilter || undefined,
    }),
    placeholderData: keepPreviousData,
  });

  const openModal = (type: ModalType, id: string) => {
    setModal(type);
    setSelectedId(id);
    setNotes('');
  };

  const handleAction = async () => {
    setLoading(true);
    try {
      if (modal === 'approve') {
        await approvePriorAuth(selectedId, user?.userId ?? '', notes);
        showToast('Authorization approved successfully', 'success');
      } else if (modal === 'deny') {
        await denyPriorAuth(selectedId, user?.userId ?? '', denialReason, notes);
        showToast('Authorization denied', 'success');
      } else if (modal === 'info') {
        await requestAdditionalInfo(selectedId, user?.userId ?? '', notes);
        showToast('Additional information requested', 'success');
      }
      queryClient.invalidateQueries({ queryKey: ['pendingAuths'] });
      queryClient.invalidateQueries({ queryKey: ['stats'] });
      setModal(null);
    } catch {
      showToast('Action failed. Please try again.', 'error');
    } finally {
      setLoading(false);
    }
  };

  const clearFilters = () => {
    setSearchInput('');
    setSearch('');
    setStatusFilter('');
    setPriorityFilter('');
    setPage(1);
  };

  const canReview = user?.role === 'Reviewer' || user?.role === 'Admin';
  const hasFilters = searchInput || statusFilter || priorityFilter;

  const columns: Column<PriorAuthSummaryDto>[] = [
    { key: 'priority', header: 'Priority', width: '90px', render: (r) => {
      const p = getPriority(r);
      return <span style={{ fontSize: 11, fontWeight: 600, padding: '3px 10px', borderRadius: 999, background: p.bg, color: p.color }}>{p.label}</span>;
    }},
    { key: 'patientName', header: 'Patient' },
    { key: 'cptCode', header: 'CPT', width: '80px' },
    { key: 'icdCode', header: 'ICD-10', width: '90px' },
    { key: 'payerName', header: 'Payer' },
    { key: 'requiredResponseBy', header: 'Due', width: '110px', render: (r) => format(new Date(r.requiredResponseBy), 'MMM d, yyyy') },
    { key: 'status', header: 'Status', render: (r) => <StatusBadge status={r.status as PriorAuthStatus} /> },
    { key: 'actions', header: 'Actions', width: '280px', render: (r) => {
      if (!canReview) return null;
      return (
        <div style={{ display: 'flex', gap: 6 }} onClick={e => e.stopPropagation()}>
          <ActionButton variant="primary" onClick={() => openModal('approve', r.id)} style={{ padding: '4px 10px', fontSize: 11 }}>Approve</ActionButton>
          <ActionButton variant="danger" onClick={() => openModal('deny', r.id)} style={{ padding: '4px 10px', fontSize: 11 }}>Deny</ActionButton>
          <ActionButton variant="warning" onClick={() => openModal('info', r.id)} style={{ padding: '4px 10px', fontSize: 11 }}>Request Info</ActionButton>
        </div>
      );
    }},
  ];

  const totalPages = data?.totalPages ?? 1;
  const totalCount = data?.totalCount ?? 0;

  return (
    <div>
      <PageHeader title="Review Queue" subtitle="Authorizations requiring clinical review" />

      {/* Search & Filter Bar */}
      <div style={{ display: 'flex', gap: 12, marginBottom: 16, flexWrap: 'wrap' }}>
        <input
          type="text"
          placeholder="Search patient, payer, CPT, ICD..."
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          style={{
            flex: 1, minWidth: 220, padding: '8px 12px', borderRadius: 'var(--radius)',
            border: '1px solid var(--gray-200)', fontSize: 13, outline: 'none',
          }}
        />
        <select
          value={statusFilter}
          onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '8px 12px', borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', fontSize: 13 }}
        >
          <option value="">All Statuses</option>
          <option value="Submitted">Submitted</option>
          <option value="UnderReview">Under Review</option>
        </select>
        <select
          value={priorityFilter}
          onChange={e => setPriorityFilter(e.target.value)}
          style={{ padding: '8px 12px', borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', fontSize: 13 }}
        >
          <option value="">All Priorities</option>
          <option value="High">High</option>
          <option value="Medium">Medium</option>
          <option value="Normal">Normal</option>
        </select>
        {hasFilters && (
          <button
            onClick={clearFilters}
            style={{ padding: '8px 12px', borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', fontSize: 13, cursor: 'pointer', background: 'var(--gray-50)', color: 'var(--gray-600)' }}
          >
            Clear
          </button>
        )}
        <span style={{ display: 'flex', alignItems: 'center', fontSize: 12, color: 'var(--gray-400)' }}>
          {totalCount} result{totalCount !== 1 ? 's' : ''}
        </span>
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        loading={isLoading}
        onRowClick={(r) => navigate(`/app/auth/${r.id}`)}
        emptyMessage="No authorizations match your filters"
      />

      {/* Pagination Controls */}
      {totalPages > 1 && (
        <div style={{
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          marginTop: 16, padding: '12px 0',
        }}>
          <span style={{ fontSize: 13, color: 'var(--gray-500)' }}>
            Page {page} of {totalPages} &mdash; {totalCount} total
          </span>
          <div style={{ display: 'flex', gap: 8 }}>
            <ActionButton
              variant="ghost"
              onClick={() => setPage(p => p - 1)}
              disabled={!data?.hasPreviousPage}
              style={{ padding: '6px 14px', fontSize: 13 }}
            >
              &larr; Prev
            </ActionButton>
            {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
              // Show pages around current page
              let p: number;
              if (totalPages <= 7) {
                p = i + 1;
              } else if (page <= 4) {
                p = i + 1;
              } else if (page >= totalPages - 3) {
                p = totalPages - 6 + i;
              } else {
                p = page - 3 + i;
              }
              return (
                <button
                  key={p}
                  onClick={() => setPage(p)}
                  style={{
                    padding: '6px 12px', borderRadius: 'var(--radius)', fontSize: 13,
                    border: '1px solid var(--gray-200)', cursor: 'pointer',
                    background: p === page ? 'var(--navy)' : 'var(--white)',
                    color: p === page ? 'var(--white)' : 'var(--gray-700)',
                    fontWeight: p === page ? 600 : 400,
                  }}
                >
                  {p}
                </button>
              );
            })}
            <ActionButton
              variant="ghost"
              onClick={() => setPage(p => p + 1)}
              disabled={!data?.hasNextPage}
              style={{ padding: '6px 14px', fontSize: 13 }}
            >
              Next &rarr;
            </ActionButton>
          </div>
        </div>
      )}

      {/* Approve Modal */}
      {modal === 'approve' && (
        <Modal title="Approve Authorization" onClose={() => setModal(null)} footer={
          <ActionButton variant="primary" loading={loading} onClick={handleAction}>Confirm Approval</ActionButton>
        }>
          <div style={{ marginBottom: 12 }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Reviewer ID</label>
            <input value={user?.userId ?? ''} disabled style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, background: 'var(--gray-50)' }} />
          </div>
          <div>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Notes</label>
            <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={4} style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, resize: 'vertical' }} />
          </div>
        </Modal>
      )}

      {/* Deny Modal */}
      {modal === 'deny' && (
        <Modal title="Deny Authorization" onClose={() => setModal(null)} footer={
          <ActionButton variant="danger" loading={loading} onClick={handleAction}>Confirm Denial</ActionButton>
        }>
          <div style={{ marginBottom: 12 }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Reviewer ID</label>
            <input value={user?.userId ?? ''} disabled style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, background: 'var(--gray-50)' }} />
          </div>
          <div style={{ marginBottom: 12 }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Denial Reason</label>
            <select value={denialReason} onChange={e => setDenialReason(e.target.value as DenialReason)} style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4 }}>
              {denialReasons.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}
            </select>
          </div>
          <div>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Notes</label>
            <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={4} style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, resize: 'vertical' }} />
          </div>
        </Modal>
      )}

      {/* Request Info Modal */}
      {modal === 'info' && (
        <Modal title="Request Additional Information" onClose={() => setModal(null)} footer={
          <ActionButton variant="warning" loading={loading} onClick={handleAction}>Send Request</ActionButton>
        }>
          <div style={{ marginBottom: 12 }}>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Requested By</label>
            <input value={user?.userId ?? ''} disabled style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, background: 'var(--gray-50)' }} />
          </div>
          <div>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Details of Information Needed</label>
            <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={4} placeholder="Describe what additional information is needed..." style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, resize: 'vertical' }} />
          </div>
        </Modal>
      )}
    </div>
  );
}
