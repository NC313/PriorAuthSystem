import { useEffect, useRef, useState, useCallback } from 'react';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';

interface UseSignalROptions {
  onStatusUpdate?: (requestId: string, newStatus: string) => void;
}

export function useSignalR({ onStatusUpdate }: UseSignalROptions = {}) {
  const [isConnected, setIsConnected] = useState(false);
  const connectionRef = useRef<HubConnection | null>(null);
  const callbackRef = useRef(onStatusUpdate);
  callbackRef.current = onStatusUpdate;

  const connect = useCallback(async () => {
    if (connectionRef.current) return;

    const connection = new HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_URL}/hubs/priorauth`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    connection.on('ReceiveStatusUpdate', (requestId: string, newStatus: string) => {
      callbackRef.current?.(requestId, newStatus);
    });

    connection.onreconnected(() => setIsConnected(true));
    connection.onclose(() => setIsConnected(false));

    try {
      await connection.start();
      connectionRef.current = connection;
      setIsConnected(true);
    } catch {
      setIsConnected(false);
    }
  }, []);

  useEffect(() => {
    connect();
    return () => {
      connectionRef.current?.stop();
      connectionRef.current = null;
    };
  }, [connect]);

  return { isConnected };
}
