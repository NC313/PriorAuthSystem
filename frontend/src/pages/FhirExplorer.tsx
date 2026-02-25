import { useState } from 'react';
import { getFhirPatient, getFhirClaim } from '../api/priorAuth';
import PageHeader from '../components/PageHeader';

type Tab = 'Patient' | 'Claim';

function syntaxHighlight(json: string): string {
  return json.replace(
    /("(\\u[\da-fA-F]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+-]?\d+)?)/g,
    (match) => {
      if (/^"/.test(match)) {
        if (/:$/.test(match)) {
          return `<span style="color: #94A3B8">${match}</span>`;
        }
        return `<span style="color: var(--green)">${match}</span>`;
      }
      if (/true|false/.test(match)) {
        return `<span style="color: var(--red)">${match}</span>`;
      }
      if (/null/.test(match)) {
        return `<span style="color: var(--red)">${match}</span>`;
      }
      return `<span style="color: var(--amber)">${match}</span>`;
    }
  );
}

function PatientSummary({ data }: { data: Record<string, unknown> }) {
  const name = Array.isArray(data.name) && data.name.length > 0
    ? [data.name[0].given?.join(' '), data.name[0].family].filter(Boolean).join(' ')
    : 'N/A';
  const gender = (data.gender as string) ?? 'N/A';
  const birthDate = (data.birthDate as string) ?? 'N/A';
  const identifiers = Array.isArray(data.identifier)
    ? data.identifier.map((id: Record<string, unknown>) => `${id.system}: ${id.value}`).join(', ')
    : 'N/A';

  const fields = [
    { label: 'Name', value: name },
    { label: 'Gender', value: gender },
    { label: 'Birth Date', value: birthDate },
    { label: 'Identifiers', value: identifiers },
  ];

  return (
    <div style={{
      background: 'var(--gray-50)', borderRadius: 'var(--radius)', padding: 16, marginTop: 12,
      border: '1px solid var(--gray-200)',
    }}>
      <div style={{ fontSize: 13, fontWeight: 600, marginBottom: 10, color: 'var(--gray-900)' }}>
        Patient Summary
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
        {fields.map(f => (
          <div key={f.label}>
            <span style={{ fontSize: 11, color: 'var(--gray-500)', fontWeight: 600 }}>{f.label}</span>
            <div style={{ fontSize: 13, color: 'var(--gray-900)' }}>{f.value}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

function ClaimSummary({ data }: { data: Record<string, unknown> }) {
  const status = (data.status as string) ?? 'N/A';
  const use = (data.use as string) ?? 'N/A';
  const created = (data.created as string) ?? 'N/A';
  const patient = data.patient && typeof data.patient === 'object'
    ? ((data.patient as Record<string, unknown>).display as string) ?? ((data.patient as Record<string, unknown>).reference as string) ?? 'N/A'
    : 'N/A';
  const provider = data.provider && typeof data.provider === 'object'
    ? ((data.provider as Record<string, unknown>).display as string) ?? ((data.provider as Record<string, unknown>).reference as string) ?? 'N/A'
    : 'N/A';
  const insurer = data.insurer && typeof data.insurer === 'object'
    ? ((data.insurer as Record<string, unknown>).display as string) ?? ((data.insurer as Record<string, unknown>).reference as string) ?? 'N/A'
    : 'N/A';

  const fields = [
    { label: 'Status', value: status },
    { label: 'Use', value: use },
    { label: 'Created', value: created },
    { label: 'Patient', value: patient },
    { label: 'Provider', value: provider },
    { label: 'Insurer', value: insurer },
  ];

  return (
    <div style={{
      background: 'var(--gray-50)', borderRadius: 'var(--radius)', padding: 16, marginTop: 12,
      border: '1px solid var(--gray-200)',
    }}>
      <div style={{ fontSize: 13, fontWeight: 600, marginBottom: 10, color: 'var(--gray-900)' }}>
        Claim Summary
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 8 }}>
        {fields.map(f => (
          <div key={f.label}>
            <span style={{ fontSize: 11, color: 'var(--gray-500)', fontWeight: 600 }}>{f.label}</span>
            <div style={{ fontSize: 13, color: 'var(--gray-900)' }}>{f.value}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default function FhirExplorer() {
  const [activeTab, setActiveTab] = useState<Tab>('Patient');
  const [patientId, setPatientId] = useState('');
  const [claimId, setClaimId] = useState('');
  const [response, setResponse] = useState<Record<string, unknown> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [copied, setCopied] = useState(false);

  const fetchResource = async () => {
    const id = activeTab === 'Patient' ? patientId : claimId;
    if (!id.trim()) {
      setError('Please enter a resource ID.');
      return;
    }
    setLoading(true);
    setError('');
    setResponse(null);
    try {
      const data = activeTab === 'Patient'
        ? await getFhirPatient(id.trim())
        : await getFhirClaim(id.trim());
      setResponse(data as Record<string, unknown>);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Failed to fetch resource';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  const copyJson = () => {
    if (!response) return;
    navigator.clipboard.writeText(JSON.stringify(response, null, 2));
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const tabStyle = (tab: Tab) => ({
    padding: '10px 24px',
    fontSize: 13,
    fontWeight: 600 as const,
    border: 'none',
    borderBottom: activeTab === tab ? '2px solid var(--green)' : '2px solid transparent',
    background: 'transparent',
    color: activeTab === tab ? 'var(--navy)' : 'var(--gray-400)',
    cursor: 'pointer' as const,
    transition: 'var(--transition)',
  });

  const formattedJson = response ? JSON.stringify(response, null, 2) : '';

  return (
    <div>
      <PageHeader title="FHIR R4 Explorer" subtitle="Query FHIR-compliant resources from this system" />

      {/* Info Banner */}
      <div style={{
        background: 'var(--blue-light)', border: '1px solid var(--blue)',
        borderRadius: 'var(--radius)', padding: '14px 20px', marginBottom: 24,
        display: 'flex', alignItems: 'flex-start', gap: 10,
      }}>
        <span style={{ fontSize: 18, flexShrink: 0 }}>&#9432;</span>
        <p style={{ fontSize: 13, color: 'var(--navy)', lineHeight: 1.6, margin: 0 }}>
          This system implements HL7 FHIR R4 as required by the CMS Interoperability and Prior Authorization
          Final Rule (CMS-0057-F), effective January 1, 2026.
        </p>
      </div>

      {/* Tabs */}
      <div style={{ borderBottom: '1px solid var(--gray-200)', marginBottom: 24, display: 'flex' }}>
        <button onClick={() => { setActiveTab('Patient'); setResponse(null); setError(''); }} style={tabStyle('Patient')}>
          Patient Resource
        </button>
        <button onClick={() => { setActiveTab('Claim'); setResponse(null); setError(''); }} style={tabStyle('Claim')}>
          Claim Resource
        </button>
      </div>

      {/* Tab Content */}
      <div style={{
        background: 'var(--white)', borderRadius: 'var(--radius)',
        boxShadow: 'var(--shadow)', padding: 24,
      }}>
        <p style={{ fontSize: 13, color: 'var(--gray-600)', marginBottom: 16 }}>
          {activeTab === 'Patient'
            ? 'The FHIR Patient resource represents demographic and administrative information about a person receiving healthcare services. This resource is the foundation for identifying individuals within the system.'
            : 'The FHIR Claim resource represents a request for adjudication of products and/or services. In the prior authorization context, it maps to a PriorAuthorizationRequest with associated clinical and administrative data.'}
        </p>

        {/* ID Input */}
        <div style={{ marginBottom: 16 }}>
          <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--gray-600)', marginBottom: 6 }}>
            {activeTab === 'Patient' ? 'Patient ID' : 'Prior Authorization ID'}
            <span style={{ fontWeight: 400, color: 'var(--gray-400)', marginLeft: 8 }}>
              e.g. {activeTab === 'Patient' ? '3fa85f64-5717-4562-b3fc-2c963f66afa6' : 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'}
            </span>
          </label>
          <div style={{ display: 'flex', gap: 8 }}>
            <input
              type="text"
              value={activeTab === 'Patient' ? patientId : claimId}
              onChange={e => activeTab === 'Patient' ? setPatientId(e.target.value) : setClaimId(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && fetchResource()}
              placeholder={`Enter ${activeTab === 'Patient' ? 'Patient' : 'Authorization'} ID...`}
              style={{
                flex: 1, padding: '10px 14px', borderRadius: 'var(--radius)',
                border: '1px solid var(--gray-200)', fontSize: 13,
                fontFamily: 'monospace', outline: 'none',
              }}
            />
            <button
              onClick={fetchResource}
              disabled={loading}
              style={{
                padding: '10px 20px', borderRadius: 'var(--radius)',
                background: loading ? 'var(--gray-300)' : 'var(--navy)',
                color: 'var(--white)', border: 'none', fontSize: 13,
                fontWeight: 600, cursor: loading ? 'not-allowed' : 'pointer',
                transition: 'var(--transition)',
              }}
            >
              {loading ? 'Fetching...' : 'Fetch Resource'}
            </button>
          </div>
        </div>

        {/* Error */}
        {error && (
          <div style={{
            background: 'var(--red-light)', border: '1px solid var(--red)',
            borderRadius: 'var(--radius)', padding: '10px 14px', marginBottom: 16,
            fontSize: 13, color: 'var(--red)',
          }}>
            {error}
          </div>
        )}

        {/* Response Pane */}
        {response && (
          <>
            <div style={{ position: 'relative' }}>
              <div style={{
                display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                marginBottom: 8,
              }}>
                <span style={{ fontSize: 12, fontWeight: 600, color: 'var(--gray-600)' }}>
                  Response &mdash; {activeTab}/{activeTab === 'Patient' ? patientId : claimId}
                </span>
                <button
                  onClick={copyJson}
                  style={{
                    padding: '4px 12px', borderRadius: 'var(--radius)',
                    border: '1px solid var(--gray-200)', background: 'var(--white)',
                    fontSize: 11, fontWeight: 600, cursor: 'pointer',
                    color: copied ? 'var(--green)' : 'var(--gray-600)',
                    transition: 'var(--transition)',
                  }}
                >
                  {copied ? 'Copied!' : 'Copy JSON'}
                </button>
              </div>
              <div style={{
                background: 'var(--navy)', borderRadius: 'var(--radius)',
                padding: 20, maxHeight: 420, overflowY: 'auto',
              }}>
                <pre
                  style={{
                    fontFamily: "'Fira Code', 'Consolas', monospace",
                    fontSize: 12, lineHeight: 1.6, margin: 0,
                    color: 'var(--gray-300)', whiteSpace: 'pre-wrap', wordBreak: 'break-word',
                  }}
                  dangerouslySetInnerHTML={{ __html: syntaxHighlight(formattedJson) }}
                />
              </div>
            </div>

            {/* Human-Readable Summary */}
            {activeTab === 'Patient' ? (
              <PatientSummary data={response} />
            ) : (
              <ClaimSummary data={response} />
            )}
          </>
        )}
      </div>
    </div>
  );
}
