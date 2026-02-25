import type { PriorAuthStatus } from '../types';

const statusConfig: Record<PriorAuthStatus, { bg: string; color: string; dot: string }> = {
  Draft: { bg: 'var(--gray-100)', color: 'var(--gray-600)', dot: 'var(--gray-400)' },
  Submitted: { bg: 'var(--blue-light)', color: 'var(--blue)', dot: 'var(--blue)' },
  UnderReview: { bg: 'var(--amber-light)', color: 'var(--amber)', dot: 'var(--amber)' },
  Approved: { bg: 'var(--green-light)', color: 'var(--green)', dot: 'var(--green)' },
  Denied: { bg: 'var(--red-light)', color: 'var(--red)', dot: 'var(--red)' },
  AdditionalInfoRequested: { bg: 'var(--orange-light)', color: 'var(--orange)', dot: 'var(--orange)' },
  Appealed: { bg: 'var(--purple-light)', color: 'var(--purple)', dot: 'var(--purple)' },
  AppealApproved: { bg: 'var(--green-light)', color: 'var(--green)', dot: 'var(--green)' },
  AppealDenied: { bg: 'var(--red-light)', color: 'var(--red)', dot: 'var(--red)' },
  Expired: { bg: 'var(--gray-100)', color: 'var(--gray-500)', dot: 'var(--gray-400)' },
};

const statusLabels: Record<PriorAuthStatus, string> = {
  Draft: 'Draft',
  Submitted: 'Submitted',
  UnderReview: 'Under Review',
  Approved: 'Approved',
  Denied: 'Denied',
  AdditionalInfoRequested: 'Info Requested',
  Appealed: 'Appealed',
  AppealApproved: 'Appeal Approved',
  AppealDenied: 'Appeal Denied',
  Expired: 'Expired',
};

interface Props {
  status: PriorAuthStatus;
  large?: boolean;
}

export default function StatusBadge({ status, large }: Props) {
  const config = statusConfig[status] ?? statusConfig.Draft;
  return (
    <span style={{
      display: 'inline-flex',
      alignItems: 'center',
      gap: 6,
      padding: large ? '6px 14px' : '3px 10px',
      borderRadius: 999,
      fontSize: large ? 14 : 12,
      fontWeight: 600,
      background: config.bg,
      color: config.color,
      whiteSpace: 'nowrap',
    }}>
      <span style={{
        width: large ? 8 : 6,
        height: large ? 8 : 6,
        borderRadius: '50%',
        background: config.dot,
      }} />
      {statusLabels[status] ?? status}
    </span>
  );
}
