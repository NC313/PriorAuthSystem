import { useState, useCallback } from 'react';
import { useSignalR } from './useSignalR';

export interface AppNotification {
  id: string;
  requestId: string;
  newStatus: string;
  message: string;
  timestamp: Date;
  isRead: boolean;
}

export function useNotifications() {
  const [notifications, setNotifications] = useState<AppNotification[]>([]);

  const onStatusUpdate = useCallback((requestId: string, newStatus: string) => {
    setNotifications(prev => [
      {
        id: crypto.randomUUID(),
        requestId,
        newStatus,
        message: `Auth ${requestId.slice(0, 8)}… changed to ${newStatus}`,
        timestamp: new Date(),
        isRead: false,
      },
      ...prev,
    ].slice(0, 50)); // keep latest 50
  }, []);

  const { isConnected } = useSignalR({ onStatusUpdate });

  const unreadCount = notifications.filter(n => !n.isRead).length;

  const markAllRead = useCallback(() => {
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
  }, []);

  const clearAll = useCallback(() => setNotifications([]), []);

  return { notifications, unreadCount, isConnected, markAllRead, clearAll };
}
