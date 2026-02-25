import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { submitPriorAuth } from '../api/priorAuth';
import { getAllPatients } from '../api/patients';
import { getAllProviders } from '../api/providers';
import { getAllPayers } from '../api/payers';
import { useDemoUser } from '../hooks/useDemoUser';
import { useToast } from '../components/Toast';
import PageHeader from '../components/PageHeader';
import ActionButton from '../components/ActionButton';

const commonCodes = [
  { condition: 'Lumbar MRI', icd: 'M54.5', cpt: '72148', notes: 'Requires 6wks conservative tx' },
  { condition: 'Cervical MRI', icd: 'M54.2', cpt: '72141', notes: 'Document neuro symptoms' },
  { condition: 'PT (20 visits)', icd: 'M47.816', cpt: '97110', notes: 'Include functional limitations' },
  { condition: 'Hip Replacement', icd: 'M16.11', cpt: '27130', notes: 'Conservative tx failed' },
  { condition: 'Knee Arthroscopy', icd: 'M23.201', cpt: '29881', notes: 'Document instability' },
];

export default function Submit() {
  const navigate = useNavigate();
  const { user } = useDemoUser();
  const { showToast } = useToast();

  const { data: patients } = useQuery({ queryKey: ['patients'], queryFn: getAllPatients });
  const { data: providers } = useQuery({ queryKey: ['providers'], queryFn: getAllProviders });
  const { data: payers } = useQuery({ queryKey: ['payers'], queryFn: getAllPayers });

  const [patientId, setPatientId] = useState('');
  const [providerId, setProviderId] = useState('');
  const [payerId, setPayerId] = useState('');
  const [icdCode, setIcdCode] = useState('');
  const [cptCode, setCptCode] = useState('');
  const [requiredResponseBy, setRequiredResponseBy] = useState('');
  const [justification, setJustification] = useState('');
  const [loading, setLoading] = useState(false);
  const [resultId, setResultId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const today = new Date().toISOString().split('T')[0];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const newId = await submitPriorAuth({
        patientId,
        providerId,
        payerId,
        icdCode: icdCode.split(' ')[0] ?? icdCode,
        icdDescription: icdCode.includes(' ') ? icdCode.split(' ').slice(1).join(' ').replace(/^[\u2014\u2013-]\s*/, '') : '',
        cptCode: cptCode.split(' ')[0] ?? cptCode,
        cptDescription: cptCode.includes(' ') ? cptCode.split(' ').slice(1).join(' ').replace(/^[\u2014\u2013-]\s*/, '') : '',
        cptRequiresPriorAuth: true,
        clinicalNotes: justification,
        clinicalDocumentedBy: user?.name ?? 'Unknown',
        clinicalSupportingDocumentPath: '',
        requiredResponseBy: new Date(requiredResponseBy).toISOString(),
      });
      setResultId(newId);
      showToast('Authorization submitted successfully', 'success');
    } catch {
      setError('Failed to submit authorization. Please check all fields and try again.');
      showToast('Submission failed', 'error');
    } finally {
      setLoading(false);
    }
  };

  if (resultId) {
    return (
      <div>
        <PageHeader title="Submit Authorization" />
        <div style={{
          background: 'var(--green-light)', border: '2px solid var(--green)',
          borderRadius: 'var(--radius-lg)', padding: 32, textAlign: 'center', maxWidth: 500, margin: '0 auto',
        }}>
          <div style={{ fontSize: 48, marginBottom: 16 }}>{'\u2705'}</div>
          <h2 style={{ fontSize: 20, fontWeight: 700, marginBottom: 8, color: 'var(--green)' }}>Authorization Submitted</h2>
          <p style={{ fontSize: 13, color: 'var(--gray-600)', marginBottom: 4 }}>Request ID:</p>
          <code style={{ fontSize: 12, background: 'var(--white)', padding: '4px 12px', borderRadius: 4 }}>{resultId}</code>
          <div style={{ display: 'flex', gap: 12, justifyContent: 'center', marginTop: 24 }}>
            <ActionButton variant="primary" onClick={() => navigate(`/app/auth/${resultId}`)}>View Authorization</ActionButton>
            <ActionButton variant="ghost" onClick={() => { setResultId(null); setPatientId(''); setProviderId(''); setPayerId(''); setIcdCode(''); setCptCode(''); setRequiredResponseBy(''); setJustification(''); }}>Submit Another</ActionButton>
          </div>
        </div>
      </div>
    );
  }

  const inputStyle = { width: '100%', padding: '8px 12px', borderRadius: 'var(--radius)', border: '1px solid var(--gray-200)', fontSize: 13, marginTop: 4 };
  const labelStyle = { fontSize: 12, fontWeight: 600 as const, color: 'var(--gray-600)', display: 'block' as const, marginBottom: 2 };

  return (
    <div>
      <PageHeader title="Submit Authorization" subtitle="Create a new prior authorization request" />
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 380px', gap: 24, alignItems: 'start' }}>
        {/* Form */}
        <form onSubmit={handleSubmit} style={{
          background: 'var(--white)', borderRadius: 'var(--radius)',
          boxShadow: 'var(--shadow)', padding: 24,
        }}>
          <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 20 }}>New Prior Authorization Request</h3>

          {error && (
            <div style={{ background: 'var(--red-light)', color: 'var(--red)', padding: 12, borderRadius: 'var(--radius)', marginBottom: 16, fontSize: 13 }}>{error}</div>
          )}

          <div style={{ fontSize: 11, fontWeight: 700, color: 'var(--gray-400)', letterSpacing: '0.1em', marginBottom: 12 }}>PATIENT & PROVIDER</div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 20 }}>
            <div>
              <label style={labelStyle}>Patient</label>
              <select value={patientId} onChange={e => setPatientId(e.target.value)} required style={inputStyle}>
                <option value="">Select patient...</option>
                {(patients ?? []).map(p => <option key={p.id} value={p.id}>{p.fullName} ({p.memberId})</option>)}
              </select>
            </div>
            <div>
              <label style={labelStyle}>Provider</label>
              <select value={providerId} onChange={e => setProviderId(e.target.value)} required style={inputStyle}>
                <option value="">Select provider...</option>
                {(providers ?? []).map(p => <option key={p.id} value={p.id}>{p.fullName} \u2014 {p.specialty}</option>)}
              </select>
            </div>
          </div>
          <div style={{ marginBottom: 20 }}>
            <label style={labelStyle}>Payer</label>
            <select value={payerId} onChange={e => setPayerId(e.target.value)} required style={inputStyle}>
              <option value="">Select payer...</option>
              {(payers ?? []).map(p => <option key={p.id} value={p.id}>{p.name} ({p.payerId})</option>)}
            </select>
          </div>

          <div style={{ fontSize: 11, fontWeight: 700, color: 'var(--gray-400)', letterSpacing: '0.1em', marginBottom: 12 }}>CLINICAL INFORMATION</div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 16 }}>
            <div>
              <label style={labelStyle}>ICD-10 Code</label>
              <input value={icdCode} onChange={e => setIcdCode(e.target.value)} required placeholder="e.g. M54.5 \u2014 Cervicalgia" style={inputStyle} />
            </div>
            <div>
              <label style={labelStyle}>CPT Code</label>
              <input value={cptCode} onChange={e => setCptCode(e.target.value)} required placeholder="e.g. 72141 \u2014 MRI Cervical Spine" style={inputStyle} />
            </div>
          </div>
          <div style={{ marginBottom: 20 }}>
            <label style={labelStyle}>Required Response By</label>
            <input type="date" value={requiredResponseBy} onChange={e => setRequiredResponseBy(e.target.value)} required min={today} style={inputStyle} />
          </div>

          <div style={{ fontSize: 11, fontWeight: 700, color: 'var(--gray-400)', letterSpacing: '0.1em', marginBottom: 12 }}>JUSTIFICATION</div>
          <div style={{ marginBottom: 24 }}>
            <label style={labelStyle}>Clinical Justification</label>
            <textarea value={justification} onChange={e => setJustification(e.target.value)} required rows={6} placeholder="Provide detailed clinical reasoning including diagnosis history, conservative treatments attempted, and medical necessity..." style={{ ...inputStyle, resize: 'vertical' as const }} />
          </div>

          <ActionButton variant="primary" type="submit" fullWidth loading={loading}>Submit Authorization Request</ActionButton>
        </form>

        {/* Reference Card */}
        <div style={{
          background: 'var(--white)', borderRadius: 'var(--radius)',
          boxShadow: 'var(--shadow)', padding: 20,
        }}>
          <h4 style={{ fontSize: 14, fontWeight: 600, marginBottom: 12 }}>Common ICD-10 / CPT Combinations</h4>
          <table style={{ width: '100%', fontSize: 11, borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--gray-200)' }}>
                <th style={{ textAlign: 'left', padding: '6px 4px', color: 'var(--gray-500)' }}>Condition</th>
                <th style={{ textAlign: 'left', padding: '6px 4px', color: 'var(--gray-500)' }}>ICD-10</th>
                <th style={{ textAlign: 'left', padding: '6px 4px', color: 'var(--gray-500)' }}>CPT</th>
                <th style={{ textAlign: 'left', padding: '6px 4px', color: 'var(--gray-500)' }}>Notes</th>
              </tr>
            </thead>
            <tbody>
              {commonCodes.map(c => (
                <tr key={c.cpt} style={{ borderBottom: '1px solid var(--gray-100)' }}>
                  <td style={{ padding: '6px 4px', fontWeight: 500 }}>{c.condition}</td>
                  <td style={{ padding: '6px 4px', fontFamily: 'monospace' }}>{c.icd}</td>
                  <td style={{ padding: '6px 4px', fontFamily: 'monospace' }}>{c.cpt}</td>
                  <td style={{ padding: '6px 4px', color: 'var(--gray-500)' }}>{c.notes}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
