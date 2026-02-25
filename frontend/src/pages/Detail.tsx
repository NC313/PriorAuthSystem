import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { getPriorAuthById, approvePriorAuth, denyPriorAuth, requestAdditionalInfo, appealPriorAuth } from '../api/priorAuth';
import { useDemoUser } from '../hooks/useDemoUser';
import { useToast } from '../components/Toast';
import StatusBadge from '../components/StatusBadge';
import InfoCard from '../components/InfoCard';
import ActionButton from '../components/ActionButton';
import Modal from '../components/Modal';
import type { PriorAuthStatus, DenialReason } from '../types';

export default function Detail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useDemoUser();
  const { showToast } = useToast();
  const [modal, setModal] = useState<string | null>(null);
  const [notes, setNotes] = useState('');
  const [denialReason, setDenialReason] = useState<DenialReason>('MedicallyUnnecessary');
  const [justification, setJustification] = useState('');
  const [loading, setLoading] = useState(false);

  const { data: auth, isLoading } = useQuery({
    queryKey: ['priorAuth', id],
    queryFn: () => getPriorAuthById(id!),
    enabled: !!id,
  });

  const handleAction = async () => {
    if (!id) return;
    setLoading(true);
    try {
      if (modal === 'approve') {
        await approvePriorAuth(id, user?.userId ?? '', notes);
        showToast('Authorization approved', 'success');
      } else if (modal === 'deny') {
        await denyPriorAuth(id, user?.userId ?? '', denialReason, notes);
        showToast('Authorization denied', 'success');
      } else if (modal === 'info') {
        await requestAdditionalInfo(id, user?.userId ?? '', notes);
        showToast('Additional info requested', 'success');
      } else if (modal === 'appeal') {
        await appealPriorAuth(id, user?.userId ?? '', justification);
        showToast('Appeal submitted', 'success');
      }
      queryClient.invalidateQueries({ queryKey: ['priorAuth', id] });
      queryClient.invalidateQueries({ queryKey: ['stats'] });
      queryClient.invalidateQueries({ queryKey: ['pendingAuths'] });
      setModal(null);
    } catch {
      showToast('Action failed', 'error');
    } finally {
      setLoading(false);
    }
  };

  if (isLoading || !auth) {
    return <div style={{ textAlign: 'center', padding: 48, color: 'var(--gray-400)' }}>Loading...</div>;
  }

  const status = auth.status as PriorAuthStatus;
  const role = user?.role;
  const canReview = (status === 'Submitted' || status === 'UnderReview') && (role === 'Reviewer' || role === 'Admin');
  const canAppeal = status === 'Denied' && role === 'Provider';
  const canResubmit = status === 'AdditionalInfoRequested' && role === 'Provider';
  const noActions = !canReview && !canAppeal && !canResubmit;

  const denialReasons: DenialReason[] = ['MedicallyUnnecessary', 'NotCovered', 'RequiresStepTherapy', 'InsufficientDocumentation', 'OutOfNetwork', 'DuplicateRequest'];

  return (
    <div style={{ paddingBottom: noActions ? 0 : 80 }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 24 }}>
        <ActionButton variant="ghost" onClick={() => navigate(-1)}>{'\u2190'} Back</ActionButton>
        <div style={{ flex: 1 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <h1 style={{ fontSize: 22, fontWeight: 700 }}>Prior Authorization</h1>
            <code style={{ fontSize: 12, background: 'var(--gray-100)', padding: '4px 8px', borderRadius: 4 }}>{auth.id}</code>
          </div>
        </div>
        <StatusBadge status={status} large />
        <span style={{ fontSize: 12, color: 'var(--gray-400)' }}>
          Updated {auth.statusTransitions?.length > 0 ? format(new Date(auth.statusTransitions[auth.statusTransitions.length - 1].transitionedAt), 'MMM d, yyyy h:mm a') : 'N/A'}
        </span>
      </div>

      {/* Info Cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 16, marginBottom: 24 }}>
        <InfoCard title="Patient" fields={[
          { label: 'Name', value: auth.patient?.fullName ?? `${auth.patient?.firstName} ${auth.patient?.lastName}` },
          { label: 'DOB', value: auth.patient?.dateOfBirth ? format(new Date(auth.patient.dateOfBirth), 'MMM d, yyyy') : 'N/A' },
          { label: 'Member ID', value: auth.patient?.memberId ?? 'N/A' },
          { label: 'Phone', value: auth.patient?.phone ?? 'N/A' },
          { label: 'Email', value: auth.patient?.email ?? 'N/A' },
        ]} />
        <InfoCard title="Provider" fields={[
          { label: 'Name', value: auth.provider?.fullName ?? `Dr. ${auth.provider?.firstName} ${auth.provider?.lastName}` },
          { label: 'NPI', value: auth.provider?.npi ?? 'N/A' },
          { label: 'Specialty', value: auth.provider?.specialty ?? 'N/A' },
          { label: 'Email', value: auth.provider?.email ?? 'N/A' },
        ]} />
        <InfoCard title="Authorization" fields={[
          { label: 'ICD-10', value: `${auth.icdCode} \u2014 ${auth.icdDescription}` },
          { label: 'CPT', value: `${auth.cptCode} \u2014 ${auth.cptDescription}` },
          { label: 'Response Due', value: auth.requiredResponseBy ? format(new Date(auth.requiredResponseBy), 'MMM d, yyyy') : 'N/A' },
          { label: 'Payer', value: auth.payer?.name ?? auth.payer?.payerId ?? 'N/A' },
        ]} />
      </div>

      {/* Clinical Justification */}
      {auth.clinicalJustification && (
        <div style={{
          background: 'var(--gray-50)', border: '1px solid var(--gray-200)',
          borderRadius: 'var(--radius)', padding: 16, marginBottom: 24,
        }}>
          <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-500)', marginBottom: 8 }}>Clinical Justification</div>
          <div style={{ fontSize: 13, color: 'var(--gray-700)', lineHeight: 1.6 }}>{auth.clinicalJustification}</div>
        </div>
      )}

      {/* Status Timeline */}
      <div style={{
        background: 'var(--white)', borderRadius: 'var(--radius)',
        boxShadow: 'var(--shadow)', padding: 24,
      }}>
        <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 16 }}>Status Timeline</h3>
        <div style={{ position: 'relative', paddingLeft: 28 }}>
          {(auth.statusTransitions ?? []).map((t, i) => {
            const isLast = i === auth.statusTransitions.length - 1;
            return (
              <div key={i} style={{ position: 'relative', paddingBottom: isLast ? 0 : 24 }}>
                {!isLast && (
                  <div style={{
                    position: 'absolute', left: -20, top: 14, width: 2, bottom: 0,
                    background: 'var(--gray-200)',
                  }} />
                )}
                <div style={{
                  position: 'absolute', left: -26, top: 2, width: 14, height: 14,
                  borderRadius: '50%',
                  background: isLast ? 'var(--green)' : 'var(--gray-300)',
                  border: `2px solid ${isLast ? 'var(--green)' : 'var(--gray-300)'}`,
                  boxShadow: isLast ? 'var(--shadow-md)' : 'none',
                }} />
                <div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <StatusBadge status={t.toStatus} />
                    <span style={{ fontSize: 12, color: 'var(--gray-400)' }}>
                      {format(new Date(t.transitionedAt), 'MMM d, yyyy h:mm a')}
                    </span>
                  </div>
                  <div style={{ fontSize: 12, color: 'var(--gray-500)', marginTop: 4 }}>
                    By: {t.transitionedBy}
                  </div>
                  {t.notes && (
                    <div style={{ fontSize: 12, color: 'var(--gray-400)', fontStyle: 'italic', marginTop: 2 }}>{t.notes}</div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Action Panel */}
      {!noActions && (
        <div style={{
          position: 'fixed', bottom: 0, left: 260, right: 0,
          background: 'var(--white)', borderTop: '1px solid var(--gray-200)',
          boxShadow: '0 -4px 12px rgba(0,0,0,0.05)', padding: '12px 24px',
          display: 'flex', gap: 12, justifyContent: 'flex-end', zIndex: 50,
        }}>
          {canReview && (
            <>
              <ActionButton variant="primary" onClick={() => setModal('approve')}>Approve</ActionButton>
              <ActionButton variant="danger" onClick={() => setModal('deny')}>Deny</ActionButton>
              <ActionButton variant="warning" onClick={() => setModal('info')}>Request Additional Info</ActionButton>
            </>
          )}
          {canAppeal && (
            <ActionButton variant="secondary" onClick={() => setModal('appeal')} style={{ background: 'var(--purple)', borderColor: 'var(--purple)' }}>Submit Appeal</ActionButton>
          )}
          {canResubmit && (
            <ActionButton variant="secondary" onClick={() => setModal('resubmit')} style={{ background: 'var(--blue)', borderColor: 'var(--blue)' }}>Resubmit</ActionButton>
          )}
        </div>
      )}

      {noActions && (
        <div style={{
          background: 'var(--gray-50)', border: '1px solid var(--gray-200)',
          borderRadius: 'var(--radius)', padding: 16, marginTop: 24, textAlign: 'center',
          color: 'var(--gray-500)', fontSize: 13,
        }}>No actions available for this authorization.</div>
      )}

      {/* Modals */}
      {modal === 'approve' && (
        <Modal title="Approve Authorization" onClose={() => setModal(null)} footer={
          <ActionButton variant="primary" loading={loading} onClick={handleAction}>Confirm Approval</ActionButton>
        }>
          <div>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Notes</label>
            <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={4} style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, resize: 'vertical' }} />
          </div>
        </Modal>
      )}
      {modal === 'deny' && (
        <Modal title="Deny Authorization" onClose={() => setModal(null)} footer={
          <ActionButton variant="danger" loading={loading} onClick={handleAction}>Confirm Denial</ActionButton>
        }>
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
      {modal === 'info' && (
        <Modal title="Request Additional Information" onClose={() => setModal(null)} footer={
          <ActionButton variant="warning" loading={loading} onClick={handleAction}>Send Request</ActionButton>
        }>
          <div>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Details</label>
            <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={4} placeholder="Describe what information is needed..." style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, resize: 'vertical' }} />
          </div>
        </Modal>
      )}
      {modal === 'appeal' && (
        <Modal title="Submit Appeal" onClose={() => setModal(null)} footer={
          <ActionButton variant="secondary" loading={loading} onClick={handleAction} style={{ background: 'var(--purple)', borderColor: 'var(--purple)' }}>Submit Appeal</ActionButton>
        }>
          <div>
            <label style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>Clinical Justification for Appeal</label>
            <textarea value={justification} onChange={e => setJustification(e.target.value)} rows={6} placeholder="Provide updated clinical reasoning..." style={{ width: '100%', padding: 8, borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', marginTop: 4, resize: 'vertical' }} />
          </div>
        </Modal>
      )}
    </div>
  );
}
