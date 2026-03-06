import { useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { formatDistanceToNow, format, isPast } from 'date-fns';
import { getStats, getPendingAuths } from '../api/priorAuth';
import { useSignalR } from '../hooks/useSignalR';
import PageHeader from '../components/PageHeader';
import StatCard from '../components/StatCard';
import StatusBadge from '../components/StatusBadge';
import DataTable, { type Column } from '../components/DataTable';
import ActionButton from '../components/ActionButton';
import type { PriorAuthSummaryDto, PriorAuthStatus } from '../types';

// --- Donut Chart (pure SVG) ---
interface DonutSlice { label: string; value: number; color: string; }

function DonutChart({ slices, size = 160 }: { slices: DonutSlice[]; size?: number }) {
  const total = slices.reduce((s, sl) => s + sl.value, 0);
  if (total === 0) return (
    <div style={{ width: size, height: size, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <span style={{ fontSize: 12, color: 'var(--gray-400)' }}>No data</span>
    </div>
  );

  const r = 54;
  const cx = size / 2;
  const cy = size / 2;
  const stroke = 28;
  const circumference = 2 * Math.PI * r;

  let offset = 0;
  const segments = slices.map(sl => {
    const pct = sl.value / total;
    const dash = pct * circumference;
    const seg = { ...sl, dash, gap: circumference - dash, offset };
    offset += dash;
    return seg;
  });

  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} style={{ transform: 'rotate(-90deg)' }}>
      {/* Background ring */}
      <circle cx={cx} cy={cy} r={r} fill="none" stroke="var(--gray-100)" strokeWidth={stroke} />
      {segments.map((seg, i) => (
        <circle
          key={i}
          cx={cx} cy={cy} r={r}
          fill="none"
          stroke={seg.color}
          strokeWidth={stroke}
          strokeDasharray={`${seg.dash} ${seg.gap}`}
          strokeDashoffset={-seg.offset}
        />
      ))}
    </svg>
  );
}

// --- Bar Chart (pure CSS) ---
interface BarItem { label: string; value: number; color: string; }

function HorizontalBarChart({ items }: { items: BarItem[] }) {
  const max = Math.max(...items.map(i => i.value), 1);
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
      {items.map(item => (
        <div key={item.label} style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <span style={{ fontSize: 12, color: 'var(--gray-600)', width: 200, flexShrink: 0 }}>{item.label}</span>
          <div style={{ flex: 1, height: 22, background: 'var(--gray-100)', borderRadius: 4, overflow: 'hidden' }}>
            <div style={{
              height: '100%',
              width: `${(item.value / max) * 100}%`,
              background: item.color,
              borderRadius: 4,
              transition: 'width 0.6s ease',
              minWidth: item.value > 0 ? 4 : 0,
            }} />
          </div>
          <span style={{ fontSize: 12, fontWeight: 600, width: 24, textAlign: 'right', color: 'var(--gray-700)' }}>{item.value}</span>
        </div>
      ))}
    </div>
  );
}

export default function Dashboard() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  useSignalR({
    onStatusUpdate: () => {
      queryClient.invalidateQueries({ queryKey: ['stats'] });
      queryClient.invalidateQueries({ queryKey: ['pendingAuths'] });
    },
  });

  const { data: stats, isLoading: statsLoading } = useQuery({ queryKey: ['stats'], queryFn: getStats });
  const { data: pending, isLoading: pendingLoading } = useQuery({ queryKey: ['pendingAuths'], queryFn: getPendingAuths });

  const columns: Column<PriorAuthSummaryDto>[] = [
    { key: 'patientName', header: 'Patient' },
    { key: 'providerName', header: 'Provider' },
    { key: 'payerName', header: 'Payer' },
    { key: 'cptCode', header: 'Procedure (CPT)', width: '110px' },
    { key: 'icdCode', header: 'Diagnosis (ICD-10)', width: '130px' },
    { key: 'submittedAt', header: 'Submitted', render: (r) => formatDistanceToNow(new Date(r.submittedAt), { addSuffix: true }) },
    { key: 'requiredResponseBy', header: 'Response Due', render: (r) => {
      const overdue = isPast(new Date(r.requiredResponseBy));
      return (
        <span style={{ color: overdue ? 'var(--red)' : 'inherit', fontWeight: overdue ? 600 : 400 }}>
          {overdue && '\u26a0 '}{format(new Date(r.requiredResponseBy), 'MMM d, yyyy')}
        </span>
      );
    }},
    { key: 'status', header: 'Status', render: (r) => <StatusBadge status={r.status as PriorAuthStatus} /> },
    { key: 'action', header: '', width: '80px', render: (r) => (
      <ActionButton variant="ghost" onClick={(e) => { e.stopPropagation(); navigate(`/app/auth/${r.id}`); }}>View</ActionButton>
    )},
  ];

  // Status distribution donut
  const statusSlices: DonutSlice[] = [
    { label: 'Pending',      value: stats?.pending ?? 0,     color: 'var(--blue)' },
    { label: 'Approved',     value: stats?.approved ?? 0,    color: 'var(--green)' },
    { label: 'Denied',       value: stats?.denied ?? 0,      color: 'var(--red)' },
    { label: 'Under Review', value: stats?.underReview ?? 0, color: 'var(--amber)' },
  ];

  // Denial reason bars
  const denialBreakdown = stats?.denialReasonBreakdown ?? {};
  const denialColors = ['var(--red)', 'var(--amber)', 'var(--blue)', 'var(--purple)', 'var(--orange)', 'var(--green)'];
  const denialItems: BarItem[] = Object.entries(denialBreakdown).map(([label, value], i) => ({
    label, value, color: denialColors[i % denialColors.length],
  }));

  const cardStyle = {
    background: 'var(--white)', borderRadius: 'var(--radius)',
    boxShadow: 'var(--shadow)', padding: 24,
  };

  return (
    <div>
      <PageHeader title="Dashboard" subtitle="Real-time overview of prior authorization activity" />

      {/* Stats Row */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: 16, marginBottom: 24 }}>
        <StatCard label="Pending"           value={statsLoading ? '...' : stats?.pending ?? 0}                      borderColor="var(--blue)" />
        <StatCard label="Approved"          value={statsLoading ? '...' : stats?.approved ?? 0}                     borderColor="var(--green)" />
        <StatCard label="Denied"            value={statsLoading ? '...' : stats?.denied ?? 0}                       borderColor="var(--red)" />
        <StatCard label="Under Review"      value={statsLoading ? '...' : stats?.underReview ?? 0}                  borderColor="var(--amber)" />
        <StatCard label="Avg Response Time" value={statsLoading ? '...' : `${stats?.avgResponseDays ?? 0} days`}    borderColor="var(--purple)" />
      </div>

      {/* Charts Row */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 24 }}>

        {/* Status Distribution Donut */}
        <div style={cardStyle}>
          <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 20 }}>Status Distribution</h3>
          {statsLoading ? (
            <div style={{ height: 160, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--gray-400)', fontSize: 13 }}>Loading...</div>
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', gap: 32 }}>
              <div style={{ flexShrink: 0 }}>
                <DonutChart slices={statusSlices} size={160} />
              </div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                {statusSlices.map(sl => (
                  <div key={sl.label} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <div style={{ width: 10, height: 10, borderRadius: '50%', background: sl.color, flexShrink: 0 }} />
                    <span style={{ fontSize: 13, color: 'var(--gray-600)' }}>{sl.label}</span>
                    <span style={{ fontSize: 13, fontWeight: 700, marginLeft: 'auto', paddingLeft: 16 }}>{sl.value}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Denial Reason Breakdown */}
        <div style={cardStyle}>
          <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 20 }}>Denial Reason Breakdown</h3>
          {statsLoading ? (
            <div style={{ height: 160, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--gray-400)', fontSize: 13 }}>Loading...</div>
          ) : denialItems.length === 0 ? (
            <div style={{ height: 160, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--gray-400)', fontSize: 13 }}>No denials yet</div>
          ) : (
            <HorizontalBarChart items={denialItems} />
          )}
        </div>

      </div>

      {/* Pending Auths Table */}
      <div>
        <h3 style={{ fontSize: 16, fontWeight: 600, marginBottom: 12 }}>Pending Authorizations</h3>
        <DataTable
          columns={columns}
          data={pending ?? []}
          loading={pendingLoading}
          onRowClick={(r) => navigate(`/app/auth/${r.id}`)}
          emptyMessage="No pending authorizations"
        />
      </div>
    </div>
  );
}
