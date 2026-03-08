import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { formatDistanceToNow } from 'date-fns';
import type { AppNotification } from '../hooks/useNotifications';

const statusColor: Record<string, string> = {
  Approved: 'var(--green)',
  AppealApproved: 'var(--green)',
  Denied: 'var(--red)',
  AppealDenied: 'var(--red)',
  Submitted: 'var(--blue)',
  UnderReview: 'var(--amber)',
  AdditionalInfoRequested: 'var(--amber)',
  Appealed: 'var(--purple)',
  Expired: 'var(--gray-400)',
};

interface Props {
  notifications: AppNotification[];
  unreadCount: number;
  onMarkAllRead: () => void;
  onClearAll: () => void;
}

export default function NotificationBell({ notifications, unreadCount, onMarkAllRead, onClearAll }: Props) {
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  // Close on outside click
  useEffect(() => {
    if (!open) return;
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [open]);

  const handleOpen = () => {
    setOpen(prev => !prev);
  };

  const handleItemClick = (requestId: string) => {
    setOpen(false);
    navigate(`/app/auth/${requestId}`);
  };

  return (
    <div ref={containerRef} style={{ position: 'relative' }}>
      {/* Bell button */}
      <button
        onClick={handleOpen}
        style={{
          position: 'relative', background: 'none', border: 'none',
          cursor: 'pointer', padding: 4, lineHeight: 1,
          color: open ? 'var(--navy)' : 'var(--gray-400)',
        }}
        aria-label={`Notifications${unreadCount > 0 ? `, ${unreadCount} unread` : ''}`}
      >
        <span style={{ fontSize: 20 }}>🔔</span>
        {unreadCount > 0 && (
          <span style={{
            position: 'absolute', top: 0, right: 0,
            background: 'var(--red)', color: 'var(--white)',
            fontSize: 10, fontWeight: 700, lineHeight: 1,
            padding: '2px 5px', borderRadius: 999,
            minWidth: 16, textAlign: 'center',
          }}>
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {/* Dropdown */}
      {open && (
        <div style={{
          position: 'absolute', top: 'calc(100% + 8px)', right: 0,
          width: 360, maxHeight: 480,
          background: 'var(--white)', borderRadius: 'var(--radius)',
          boxShadow: '0 8px 32px rgba(0,0,0,0.15)', border: '1px solid var(--gray-200)',
          display: 'flex', flexDirection: 'column', zIndex: 200,
          overflow: 'hidden',
        }}>
          {/* Header */}
          <div style={{
            display: 'flex', alignItems: 'center', justifyContent: 'space-between',
            padding: '12px 16px', borderBottom: '1px solid var(--gray-100)',
            flexShrink: 0,
          }}>
            <span style={{ fontSize: 14, fontWeight: 700, color: 'var(--gray-900)' }}>
              Notifications
              {unreadCount > 0 && (
                <span style={{
                  marginLeft: 8, fontSize: 11, fontWeight: 600,
                  background: 'var(--red)', color: 'var(--white)',
                  padding: '2px 7px', borderRadius: 999,
                }}>{unreadCount}</span>
              )}
            </span>
            <div style={{ display: 'flex', gap: 8 }}>
              {unreadCount > 0 && (
                <button onClick={onMarkAllRead} style={{
                  fontSize: 11, color: 'var(--blue)', background: 'none',
                  border: 'none', cursor: 'pointer', padding: 0,
                }}>Mark all read</button>
              )}
              {notifications.length > 0 && (
                <button onClick={onClearAll} style={{
                  fontSize: 11, color: 'var(--gray-400)', background: 'none',
                  border: 'none', cursor: 'pointer', padding: 0,
                }}>Clear all</button>
              )}
            </div>
          </div>

          {/* List */}
          <div style={{ overflowY: 'auto', flex: 1 }}>
            {notifications.length === 0 ? (
              <div style={{
                padding: 32, textAlign: 'center',
                color: 'var(--gray-400)', fontSize: 13,
              }}>
                No notifications yet
              </div>
            ) : (
              notifications.map(n => (
                <button
                  key={n.id}
                  onClick={() => handleItemClick(n.requestId)}
                  style={{
                    display: 'flex', alignItems: 'flex-start', gap: 12,
                    width: '100%', padding: '12px 16px', textAlign: 'left',
                    background: n.isRead ? 'var(--white)' : 'var(--gray-50)',
                    border: 'none', borderBottom: '1px solid var(--gray-100)',
                    cursor: 'pointer', transition: 'background 0.15s',
                  }}
                  onMouseEnter={e => (e.currentTarget.style.background = 'var(--gray-100)')}
                  onMouseLeave={e => (e.currentTarget.style.background = n.isRead ? 'var(--white)' : 'var(--gray-50)')}
                >
                  {/* Unread dot */}
                  <div style={{
                    width: 8, height: 8, borderRadius: '50%', flexShrink: 0, marginTop: 4,
                    background: n.isRead ? 'transparent' : 'var(--blue)',
                  }} />
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 2 }}>
                      <span style={{
                        fontSize: 11, fontWeight: 700, padding: '2px 8px', borderRadius: 999,
                        background: `${statusColor[n.newStatus] ?? 'var(--gray-400)'}22`,
                        color: statusColor[n.newStatus] ?? 'var(--gray-600)',
                      }}>{n.newStatus}</span>
                    </div>
                    <div style={{ fontSize: 12, color: 'var(--gray-700)', marginBottom: 2 }}>
                      {n.message}
                    </div>
                    <div style={{ fontSize: 11, color: 'var(--gray-400)' }}>
                      {formatDistanceToNow(n.timestamp, { addSuffix: true })}
                    </div>
                  </div>
                </button>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
