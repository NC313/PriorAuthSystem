import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { format, isPast, differenceInHours } from 'date-fns';
import { getPendingAuths, approvePriorAuth, denyPriorAuth, requestAdditionalInfo } from '../api/priorAuth';
import { useDemoUser } from '../hooks/useDemoUser';
import { useToast } from '../components/Toast';
import PageHeader from '../components/PageHeader';
import StatusBadge from '../components/StatusBadge';
import DataTable, { type Column } from '../components/DataTable';
import ActionButton from '../components/ActionButton';
import Modal from '../components/Modal';
import type { PriorAuthSummaryDto, PriorAuthStatus, DenialReason } from '../types';

type ModalType = 'approve' | 'deny' | 'info' | null;

export default function ReviewQueue() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useDemoUser();
  const { showToast } = useToast();
  const [modal, setModal] = useState<ModalType>(null);
  const [selectedId, setSelectedId] = useState('');
  const [notes, setNotes] = useState('');
  const [denialReason, setDenialReason] = useState<DenialReason>('MedicallyUnnecessary');
  const [loading, setLoading] = useState(false);

  const { data: pending, isLoading } = useQuery({ queryKey: ['pendingAuths'], queryFn: getPendingAuths });

  const reviewable = (pending ?? []).filter(a =>
    a.status === 'Submitted' || a.status === 'UnderReview'
  );

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

  const getPriority = (r: PriorAuthSummaryDto) => {
    const due = new Date(r.requiredResponseBy);
    if (isPast(due)) return { label: 'High', color: 'var(--red)', bg: 'var(--red-light)' };
    if (differenceInHours(due, new Date()) <= 24) return { label: 'Medium', color: 'var(--amber)', bg: 'var(--amber-light)' };
    return { label: 'Normal', color: 'var(--green)', bg: 'var(--green-light)' };
  };

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
    { key: 'actions', header: 'Actions', width: '280px', render: (r) => (
      <div style={{ display: 'flex', gap: 6 }} onClick={e => e.stopPropagation()}>
        <ActionButton variant="primary" onClick={() => openModal('approve', r.id)} style={{ padding: '4px 10px', fontSize: 11 }}>Approve</ActionButton>
        <ActionButton variant="danger" onClick={() => openModal('deny', r.id)} style={{ padding: '4px 10px', fontSize: 11 }}>Deny</ActionButton>
        <ActionButton variant="warning" onClick={() => openModal('info', r.id)} style={{ padding: '4px 10px', fontSize: 11 }}>Request Info</ActionButton>
      </div>
    )},
  ];

  const denialReasons: DenialReason[] = ['MedicallyUnnecessary', 'NotCovered', 'RequiresStepTherapy', 'InsufficientDocumentation', 'OutOfNetwork', 'DuplicateRequest'];

  return (
    <div>
      <PageHeader title="Review Queue" subtitle="Authorizations requiring clinical review" />

      <DataTable
        columns={columns}
        data={reviewable}
        loading={isLoading}
        onRowClick={(r) => navigate(`/app/auth/${r.id}`)}
        emptyMessage="No authorizations pending review"
      />

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
              {denialReasons.map(r => <option key={r} value={r}>{r}</option>)}
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
