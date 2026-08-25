import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { getStoredToken } from '../api/client';
import type { QueueUpdateEvent } from '../api/endpoints';

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export function useLiveHub(enabled: boolean) {
  const [connected, setConnected] = useState(false);
  const [queue, setQueue] = useState<QueueUpdateEvent | null>(null);
  const [tick, setTick] = useState(0);
  const connRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!enabled) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE}/hubs/call`, {
        accessTokenFactory: () => getStoredToken() ?? '',
      })
      .withAutomaticReconnect([0, 3000, 5000, 10000])
      .build();

    connection.on('QueueUpdate', (payload: QueueUpdateEvent) => {
      setQueue(payload);
      setTick((t) => t + 1);
    });

    connection.onreconnected(() => setConnected(true));
    connection.onclose(() => setConnected(false));

    connection
      .start()
      .then(() => setConnected(true))
      .catch(() => setConnected(false));

    connRef.current = connection;

    return () => {
      void connection.stop();
      connRef.current = null;
    };
  }, [enabled]);

  return { connected, queue, tick, connection: connRef.current };
}
