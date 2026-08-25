import api from './client';

export interface CallSession {
  id: string;
  livekitRoomName: string;
  status: string;
  direction: string;
  startedAt: string;
  endedAt?: string;
  answeredAt?: string;
  durationSeconds?: number;
  metadataJson?: string;
}

export interface CallDetail extends CallSession {
  participants: CallParticipant[];
  transfers: CallTransfer[];
  recordings: CallRecording[];
}

export interface CallParticipant {
  id: string;
  participantType: string;
  livekitIdentity: string;
  displayName?: string;
  joinedAt: string;
  leftAt?: string;
}

export interface CallTransfer {
  id: string;
  status: string;
  reason?: string;
  toHumanAgentName: string;
  requestedAt: string;
  acceptedAt?: string;
}

export interface CallRecording {
  id: string;
  objectKey: string;
  durationSeconds?: number;
  sizeBytes?: number;
  status: string;
  createdAt: string;
}

export interface Agent {
  id: string;
  name: string;
  status: string;
  isActive: boolean;
  maxConcurrentCalls: number;
}

export interface TodayStats {
  total: number;
  active: number;
  answered: number;
  transferred: number;
  missed: number;
  avgDurationSeconds: number;
  agentsOnline: number;
  hourly: { hour: string; count: number }[];
}

export interface QueueStats {
  activeCount: number;
  agentsOnline: number;
  activeCalls: { id: string; roomName: string; status: string; durationSeconds: number }[];
  agents: { id: string; name: string; status: string }[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    displayName: string;
    companyName?: string;
  };
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/api/auth/login', { email, password }),
  register: (data: { email: string; password: string; displayName: string }) =>
    api.post<AuthResponse>('/api/auth/register', data),
  me: () => api.get<AuthResponse['user']>('/api/auth/me'),
};

export const statsApi = {
  today: () => api.get<TodayStats>('/api/stats/today'),
  queue: () => api.get<QueueStats>('/api/stats/queue'),
  agents: () => api.get<Agent[]>('/api/stats/agents'),
};

export const callsApi = {
  list: (params?: { status?: string; page?: number; limit?: number }) => {
    const qs = new URLSearchParams();
    if (params?.status) qs.set('status', params.status);
    if (params?.page) qs.set('page', String(params.page));
    if (params?.limit) qs.set('limit', String(params.limit));
    return api.get<{ items: CallSession[]; total: number; page: number; pages: number }>(
      `/api/calls?${qs}`
    );
  },
  get: (id: string) => api.get<CallDetail>(`/api/calls/${id}`),
  active: () => api.get<CallSession[]>('/api/calls/active'),
  end: (id: string) => api.post(`/api/calls/${id}/end`),
};

export const agentsApi = {
  list: () => api.get<Agent[]>('/api/human-agents'),
};