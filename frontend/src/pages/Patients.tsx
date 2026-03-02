import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import { getAllPatients, createPatient } from '../api/patients';
import { getPriorAuthsByPatient } from '../api/priorAuth';
import PageHeader from '../components/PageHeader';
import StatusBadge from '../components/StatusBadge';
import ActionButton from '../components/ActionButton';
import Modal from '../components/Modal';
import { useToast } from '../components/Toast';
import { useDemoUser } from '../hooks/useDemoUser';
import type { PatientDto, PriorAuthSummaryDto, PriorAuthStatus } from '../types';

export default function Patients() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const { user } = useDemoUser();
  const [search, setSearch] = useState('');
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [showAddModal, setShowAddModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    firstName: '', lastName: '', dateOfBirth: '',
    memberId: '', insurancePlanId: '', phone: '', email: '',
  });

  const { data: patients, isLoading } = useQuery({ queryKey: ['patients'], queryFn: getAllPatients });

  const filtered = (patients ?? []).filter(p =>
    !search || p.fullName.toLowerCase().includes(search.toLowerCase()) || p.memberId.toLowerCase().includes(search.toLowerCase())
  );

  const handleSubmit = async () => {
    if (!form.firstName || !form.lastName || !form.dateOfBirth || !form.memberId || !form.phone) {
      showToast('First name, last name, date of birth, member ID, and phone are required', 'error');
      return;
    }
    setSubmitting(true);
    try {
      await createPatient(form);
      await queryClient.invalidateQueries({ queryKey: ['patients'] });
      showToast(`Patient ${form.firstName} ${form.lastName} added`, 'success');
      setShowAddModal(false);
      setForm({ firstName: '', lastName: '', dateOfBirth: '', memberId: '', insurancePlanId: '', phone: '', email: '' });
    } catch {
      showToast('Failed to create patient', 'error');
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
        title="Patient Management"
        action={
          <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
            <span style={{ fontSize: 13, color: 'var(--gray-500)', background: 'var(--gray-100)', padding: '4px 12px', borderRadius: 999 }}>{filtered.length} patients</span>
            {user?.role === 'Admin' && (
              <ActionButton variant="primary" onClick={() => setShowAddModal(true)}>+ Add Patient</ActionButton>
            )}
          </div>
        }
      />

      <div style={{ marginBottom: 20 }}>
        <input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="🔍 Search by name or member ID..."
          style={{
            width: '100%', padding: '10px 16px', borderRadius: 'var(--radius)',
            border: '1px solid var(--gray-200)', fontSize: 14, background: 'var(--white)',
          }}
        />
      </div>

      {isLoading ? (
        <div style={{ textAlign: 'center', padding: 48, color: 'var(--gray-400)' }}>Loading patients...</div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
          {filtered.map(p => (
            <PatientCard key={p.id} patient={p} expanded={expandedId === p.id} onToggle={() => setExpandedId(expandedId === p.id ? null : p.id)} navigate={navigate} />
          ))}
        </div>
      )}

      {showAddModal && (
        <Modal
          title="Add Patient"
          onClose={() => setShowAddModal(false)}
          footer={
            <>
              <ActionButton variant="ghost" onClick={() => setShowAddModal(false)}>Cancel</ActionButton>
              <ActionButton variant="primary" onClick={handleSubmit} disabled={submitting}>
                {submitting ? 'Saving...' : 'Save Patient'}
              </ActionButton>
            </>
          }
        >
          {field('First Name *', 'firstName')}
          {field('Last Name *', 'lastName')}
          {field('Date of Birth *', 'dateOfBirth', 'date')}
          {field('Member ID *', 'memberId')}
          {field('Insurance Plan ID', 'insurancePlanId')}
          {field('Phone *', 'phone', 'tel')}
          {field('Email', 'email', 'email')}
        </Modal>
      )}
    </div>
  );
}

function PatientCard({ patient: p, expanded, onToggle, navigate }: { patient: PatientDto; expanded: boolean; onToggle: () => void; navigate: ReturnType<typeof useNavigate> }) {
  const { data: auths } = useQuery({
    queryKey: ['patientAuths', p.id],
    queryFn: () => getPriorAuthsByPatient(p.id),
    enabled: expanded,
  });

  const initials = `${p.firstName[0]}${p.lastName[0]}`;

  return (
    <div style={{
      background: 'var(--white)', borderRadius: 'var(--radius)',
      boxShadow: 'var(--shadow)', overflow: 'hidden',
    }}>
      <div style={{ padding: 20, display: 'flex', gap: 16, alignItems: 'center' }}>
        <div style={{
          width: 44, height: 44, borderRadius: '50%', background: 'var(--navy)',
          color: 'var(--white)', display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 16, fontWeight: 700, flexShrink: 0,
        }}>{initials}</div>
        <div style={{ flex: 1 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <span style={{ fontWeight: 600, fontSize: 15 }}>{p.fullName}</span>
            <code style={{ fontSize: 11, background: 'var(--gray-100)', padding: '2px 8px', borderRadius: 4 }}>{p.memberId}</code>
          </div>
          <div style={{ fontSize: 12, color: 'var(--gray-500)', marginTop: 4 }}>
            DOB: {format(new Date(p.dateOfBirth), 'MMM d, yyyy')}
          </div>
        </div>
        <ActionButton variant="ghost" onClick={onToggle}>
          {expanded ? 'Hide' : 'View Authorizations'}
        </ActionButton>
      </div>

      {expanded && (
        <div style={{ borderTop: '1px solid var(--gray-200)', padding: 16, background: 'var(--gray-50)' }}>
          {!auths ? (
            <div style={{ textAlign: 'center', color: 'var(--gray-400)', padding: 12 }}>Loading...</div>
          ) : auths.length === 0 ? (
            <div style={{ textAlign: 'center', color: 'var(--gray-400)', padding: 12 }}>No authorizations found</div>
          ) : (
            <table style={{ width: '100%', fontSize: 12, borderCollapse: 'collapse' }}>
              <thead>
                <tr>
                  <th style={{ textAlign: 'left', padding: '6px 8px', color: 'var(--gray-500)' }}>Status</th>
                  <th style={{ textAlign: 'left', padding: '6px 8px', color: 'var(--gray-500)' }}>CPT</th>
                  <th style={{ textAlign: 'left', padding: '6px 8px', color: 'var(--gray-500)' }}>Submitted</th>
                  <th style={{ textAlign: 'right', padding: '6px 8px', color: 'var(--gray-500)' }}></th>
                </tr>
              </thead>
              <tbody>
                {auths.map((a: PriorAuthSummaryDto) => (
                  <tr key={a.id} style={{ borderTop: '1px solid var(--gray-200)' }}>
                    <td style={{ padding: '8px' }}><StatusBadge status={a.status as PriorAuthStatus} /></td>
                    <td style={{ padding: '8px', fontFamily: 'monospace' }}>{a.cptCode}</td>
                    <td style={{ padding: '8px' }}>{format(new Date(a.submittedAt), 'MMM d, yyyy')}</td>
                    <td style={{ padding: '8px', textAlign: 'right' }}>
                      <ActionButton variant="ghost" onClick={() => navigate(`/app/auth/${a.id}`)} style={{ padding: '2px 8px', fontSize: 11 }}>View</ActionButton>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  );
}
