import api from './client';

export interface CallSession {
  id: string;
  userId: string;
  callConfigurationId?: string | null;
  livekitRoomName: string;
  status: ApiCallStatus;
  direction: 'Inbound' | 'Outbound';
  startedAt: string;
  answeredAt?: string | null;
  endedAt?: string | null;
  durationSeconds?: number | null;
  metadataJson?: string | null;
  participantCount?: number;
  createdAt: string;
}

export type ApiCallStatus =
  | 'Queued'
  | 'Ringing'
  | 'Active'
  | 'Transferred'
  | 'Completed'
  | 'Missed'
  | 'Failed'
  | string;

export interface CallParticipantDto {
  id: string;
  humanAgentId?: string | null;
  participantType: string;
  livekitIdentity: string;
  livekitParticipantSid?: string | null;
  displayName?: string | null;
  joinedAt: string;
  leftAt?: string | null;
  createdAt: string;
}

export interface CallTransferDto {
  id: string;
  callSessionId: string;
  fromParticipantId?: string | null;
  toHumanAgentId?: string | null;
  toHumanAgentName?: string | null;
  status: string;
  reason?: string | null;
  failureReason?: string | null;
  requestedAt: string;
  acceptedAt?: string | null;
  completedAt?: string | null;
  failedAt?: string | null;
}

export interface CallRecordingDto {
  id: string;
  storageProvider: string;
  objectKey: string;
  contentType?: string | null;
  durationSeconds?: number | null;
  sizeBytes?: number | null;
  status: string;
  createdAt: string;
  completedAt?: string | null;
}

export interface CallHandoffDto {
  id: string;
  callTransferId?: string | null;
  toHumanAgentName?: string | null;
  reason?: string | null;
  summary?: string | null;
  contextDataJson?: string | null;
  status: string;
  createdAt: string;
}

export interface CallDetail extends CallSession {
  callConfigurationName?: string | null;
  livekitRoomSid?: string | null;
  participants: CallParticipantDto[];
  transfers: CallTransferDto[];
  recordings: CallRecordingDto[];
  handoff?: CallHandoffDto | null;
}

export interface ActiveCallDto {
  id: string;
  livekitRoomName: string;
  status: ApiCallStatus;
  direction: 'Inbound' | 'Outbound';
  startedAt: string;
  answeredAt?: string | null;
  durationSeconds?: number | null;
  participantCount: number;
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
  totalCalls: number;
  activeCalls: number;
  answeredCalls: number;
  transferredCalls: number;
  missedCalls: number;
  avgDurationSeconds: number;
  agentsOnline: number;
  hourly: { hour: string; count: number }[];
}

export interface HourlyDataPoint {
  hour: string;
  count: number;
}

export interface PeriodStats {
  from: string;
  to: string;
  totalCalls: number;
  completedCalls: number;
  avgDurationSeconds: number;
  hourly: HourlyDataPoint[];
}

export interface IntentStats {
  intent: string;
  count: number;
  percentage: number;
}

export interface AgentStatsDto {
  agentId: string;
  name: string;
  status: string;
  totalCalls: number;
  avgDurationSeconds: number;
  lastActiveAt?: string | null;
}

export interface PersonaListItem {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface QueueStats {
  activeCount: number;
  agentsOnline: number;
  activeCalls: ActiveCallDto[];
  agents: { id: string; name: string; status: string }[];
}

export interface QueueUpdateEvent {
  activeCount: number;
  agentsOnline: number;
  activeCalls: {
    id: string;
    roomName: string;
    status: ApiCallStatus;
    startTime: string;
    durationSeconds: number;
  }[];
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
    companyName?: string | null;
  };
}

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  companyName?: string | null;
  status: string;
  isPartner: boolean;
  standardCredits: number;
  premiumCredits: number;
  createdAt: string;
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/api/auth/login', { email, password }),
  register: (data: { email: string; password: string; displayName: string }) =>
    api.post<AuthResponse>('/api/auth/register', data),
  me: () => api.get<UserDto>('/api/auth/me'),
};

export const statsApi = {
  today: () => api.get<TodayStats>('/api/stats/today'),
  queue: () => api.get<QueueStats>('/api/stats/queue'),
  agents: () => api.get<AgentStatsDto[]>('/api/stats/agents'),
  hourly: (date?: string) =>
    api.get<HourlyDataPoint[]>(`/api/stats/hourly${date ? `?date=${encodeURIComponent(date)}` : ''}`),
  period: (from: string, to: string) =>
    api.get<PeriodStats>(`/api/stats/period?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`),
  intents: (from?: string, to?: string) => {
    const qs = new URLSearchParams();
    if (from) qs.set('from', from);
    if (to) qs.set('to', to);
    const query = qs.toString();
    return api.get<IntentStats[]>(`/api/stats/intents${query ? `?${query}` : ''}`);
  },
};

export const callsApi = {
  list: (params?: { status?: string; from?: string; page?: number; limit?: number }) => {
    const qs = new URLSearchParams();
    if (params?.status) qs.set('status', params.status);
    if (params?.from) qs.set('from', params.from);
    if (params?.page) qs.set('page', String(params.page));
    if (params?.limit) qs.set('limit', String(params.limit));
    return api.get<{ items: CallSession[]; totalCount: number; page: number; limit: number }>(
      `/api/calls?${qs}`
    );
  },
  get: (id: string) => api.get<CallDetail>(`/api/calls/${id}`),
  active: () => api.get<ActiveCallDto[]>('/api/calls/active'),
  end: (id: string) => api.post(`/api/calls/${id}/end`),
};

export const humanAgentsApi = {
  list: () => api.get<Agent[]>('/api/human-agents'),
};

export const personasApi = {
  list: () => api.get<PersonaListItem[]>('/api/personas'),
};
