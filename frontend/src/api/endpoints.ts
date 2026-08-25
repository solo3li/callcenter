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
  activeCalls: {
    id: string;
    roomName: string;
    callerId: string;
    status: ApiCallStatus;
    startTime: string;
    durationSeconds: number;
  }[];
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

// -------------------------- EXTENDED PLATFORM API --------------------------

export interface ActionDefinitionDto {
  id: string;
  name: string;
  displayName?: string | null;
  description?: string | null;
  actionType: string;
  isSystem: boolean;
  inputSchemaJson?: string | null;
  outputSchemaJson?: string | null;
  configurationJson?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface PersonaVersionDto {
  id: string;
  personaId: string;
  versionNumber: number;
  systemPrompt: string;
  configurationJson?: string | null;
  isPublished: boolean;
  createdAt: string;
}

export const personasApi = {
  list: () => api.get<PersonaListItem[]>('/api/personas'),
  get: (id: string) => api.get<PersonaListItem>(`/api/personas/${id}`),
  create: (data: { name: string; description?: string }) =>
    api.post<PersonaListItem>('/api/personas', data),
  update: (id: string, data: { name?: string; description?: string; isActive?: boolean }) =>
    api.patch<PersonaListItem>(`/api/personas/${id}`, data),
  del: (id: string) => api.del<{ message?: string }>(`/api/personas/${id}`),
  actions: (personaId: string) => api.get<ActionDefinitionDto[]>(`/api/personas/${personaId}/actions`),
  addAction: (personaId: string, actionDefinitionId: string) =>
    api.post<null>(`/api/personas/${personaId}/actions/${actionDefinitionId}`),
  removeAction: (personaId: string, actionDefinitionId: string) =>
    api.del<{ message?: string }>(`/api/personas/${personaId}/actions/${actionDefinitionId}`),
  versions: (personaId: string) => api.get<PersonaVersionDto[]>(`/api/personas/${personaId}/versions`),
  createVersion: (personaId: string, data: { systemPrompt: string; configurationJson?: string }) =>
    api.post<PersonaVersionDto>(`/api/personas/${personaId}/versions`, data),
  getVersion: (personaId: string, versionId: string) =>
    api.get<PersonaVersionDto>(`/api/personas/${personaId}/versions/${versionId}`),
  publishVersion: (personaId: string, versionId: string) =>
    api.post<PersonaVersionDto>(`/api/personas/${personaId}/versions/${versionId}/publish`),
};

export interface WorkflowListItem {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  versionCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface WorkflowVersionDto {
  id: string;
  workflowId: string;
  versionNumber: number;
  definitionJson: string;
  isPublished: boolean;
  createdAt: string;
}

export const workflowsApi = {
  list: () => api.get<WorkflowListItem[]>('/api/workflows'),
  get: (id: string) => api.get<WorkflowListItem>(`/api/workflows/${id}`),
  create: (data: { name: string; description?: string }) =>
    api.post<WorkflowListItem>('/api/workflows', data),
  update: (id: string, data: { name?: string; description?: string; isActive?: boolean }) =>
    api.put<WorkflowListItem>(`/api/workflows/${id}`, data),
  del: (id: string) => api.del<{ message?: string }>(`/api/workflows/${id}`),
  versions: (workflowId: string) => api.get<WorkflowVersionDto[]>(`/api/workflows/${workflowId}/versions`),
  createVersion: (workflowId: string, definitionJson: string) =>
    api.post<WorkflowVersionDto>(`/api/workflows/${workflowId}/versions`, { definitionJson }),
  publishVersion: (versionId: string) =>
    api.post<WorkflowVersionDto>(`/api/workflow-versions/${versionId}/publish`),
};

export interface CallConfigListItem {
  id: string;
  name: string;
  description?: string | null;
  personaId?: string | null;
  personaName?: string | null;
  workflowId?: string | null;
  workflowName?: string | null;
  isActive: boolean;
  configJson?: string | null;
  actionCount: number;
  createdAt: string;
  updatedAt: string;
}

export const callConfigsApi = {
  list: () => api.get<CallConfigListItem[]>('/api/call-configurations'),
  get: (id: string) => api.get<CallConfigListItem>(`/api/call-configurations/${id}`),
  create: (data: { name: string; description?: string; personaId?: string; workflowId?: string; configJson?: string }) =>
    api.post<CallConfigListItem>('/api/call-configurations', data),
  update: (id: string, data: { name?: string; description?: string; personaId?: string | null; workflowId?: string | null; configJson?: string; isActive?: boolean }) =>
    api.patch<CallConfigListItem>(`/api/call-configurations/${id}`, data),
  del: (id: string) => api.del<void>(`/api/call-configurations/${id}`),
  activate: (id: string) => api.post<CallConfigListItem>(`/api/call-configurations/${id}/activate`),
  getActions: (id: string) => api.get<ActionDefinitionDto[]>(`/api/call-configurations/${id}/actions`),
  setActions: (id: string, actionDefinitionIds: string[]) =>
    api.put<unknown>(`/api/call-configurations/${id}/actions`, { actionDefinitionIds }),
};

export const actionsApi = {
  list: (type?: string) => api.get<ActionDefinitionDto[]>(`/api/actions${type ? `?type=${encodeURIComponent(type)}` : ''}`),
};

export interface KnowledgeBaseListItem {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  documentCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface KnowledgeDocumentDto {
  id: string;
  knowledgeBaseId: string;
  name: string;
  sourceUri: string;
  contentType: string;
  metadataJson?: string | null;
  status: string;
  chunkCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface SearchResultItem {
  chunkId: string;
  documentId: string;
  documentName: string;
  content: string;
  score: number;
}

export const knowledgeApi = {
  kbs: () => api.get<KnowledgeBaseListItem[]>('/api/knowledge-bases'),
  createKb: (data: { name: string; description?: string }) =>
    api.post<KnowledgeBaseListItem>('/api/knowledge-bases', data),
  updateKb: (id: string, data: { name?: string; description?: string; isActive?: boolean }) =>
    api.put<KnowledgeBaseListItem>(`/api/knowledge-bases/${id}`, data),
  deleteKb: (id: string) => api.del<{ message?: string }>(`/api/knowledge-bases/${id}`),
  documents: (kbId: string) => api.get<KnowledgeDocumentDto[]>(`/api/knowledge-bases/${kbId}/documents`),
  uploadDocument: (kbId: string, data: { name: string; sourceUri: string; contentType: string; content: string; metadataJson?: string }) =>
    api.post<KnowledgeDocumentDto>(`/api/knowledge-bases/${kbId}/documents`, data),
  deleteDocument: (docId: string) => api.del<{ message?: string }>(`/api/knowledge-documents/${docId}`),
  search: (kbId: string, query: string, topK = 5) =>
    api.post<SearchResultItem[]>(`/api/knowledge-bases/${kbId}/search`, { query, topK }),
};

export interface UsageRecordDto {
  id: string;
  userId: string;
  partnerId?: string | null;
  licenseId?: string | null;
  callSessionId?: string | null;
  idempotencyKey: string;
  metricType: string;
  quantity: number;
  unit: string;
  occurredAt: string;
  metadataJson?: string | null;
}

export interface UsageSummaryDto {
  metricType: string;
  totalQuantity: number;
  unit: string;
  count: number;
}

export const usageApi = {
  records: (params?: { metricType?: string; from?: string; to?: string }) => {
    const qs = new URLSearchParams();
    if (params?.metricType) qs.set('metricType', params.metricType);
    if (params?.from) qs.set('from', params.from);
    if (params?.to) qs.set('to', params.to);
    const query = qs.toString();
    return api.get<UsageRecordDto[]>(`/api/usage${query ? `?${query}` : ''}`);
  },
  summary: () => api.get<UsageSummaryDto[]>('/api/usage/summary'),
};

export interface ApiKeyListItem {
  id: string;
  name: string;
  keyPrefix: string;
  status: string;
  scopes: string[];
  lastUsedAt?: string | null;
  expiresAt?: string | null;
  createdAt: string;
}

export interface CreateApiKeyResponse {
  id: string;
  name: string;
  rawKey: string;
  keyPrefix: string;
  createdAt: string;
}

export const apiKeysApi = {
  list: () => api.get<ApiKeyListItem[]>('/api/api-keys'),
  create: (data: { name: string; scopes?: string[]; expiresAt?: string }) =>
    api.post<CreateApiKeyResponse>('/api/api-keys', data),
  revoke: (id: string) => api.del<{ message: string }>(`/api/api-keys/${id}`),
};

export interface HumanAgentAdminDto {
  id: string;
  name: string;
  email?: string | null;
  status: string;
  isActive: boolean;
  maxConcurrentCalls: number;
  createdAt: string;
  updatedAt: string;
}

export interface AccessKeyListItem {
  id: string;
  name: string;
  keyPrefix: string;
  status: string;
  expiresAt?: string | null;
  lastUsedAt?: string | null;
  revokedAt?: string | null;
  createdAt: string;
}

export interface CreateAccessKeyResponse {
  id: string;
  name: string;
  rawKey: string;
  keyPrefix: string;
  expiresAt?: string | null;
  createdAt: string;
}

export const agentsAdminApi = {
  list: () => api.get<HumanAgentAdminDto[]>('/api/human-agents'),
  create: (data: { name: string; email?: string; maxConcurrentCalls?: number }) =>
    api.post<HumanAgentAdminDto>('/api/human-agents', data),
  setStatus: (id: string, status: string) =>
    api.patch<HumanAgentAdminDto>(`/api/human-agents/${id}/status`, { status }),
  accessKeys: (agentId: string) => api.get<AccessKeyListItem[]>(`/api/human-agents/${agentId}/access-keys`),
  issueKey: (agentId: string, data: { name: string; expiresAt?: string }) =>
    api.post<CreateAccessKeyResponse>(`/api/human-agents/${agentId}/access-keys`, data),
  revokeKey: (agentId: string, keyId: string) =>
    api.del<void>(`/api/human-agents/${agentId}/access-keys/${keyId}`),
};

export const recordingsApi = {
  downloadUrl: (callSessionId: string, recordingId: string) =>
    api.get<{ url: string }>(`/api/calls/${callSessionId}/recordings/${recordingId}/download`),
};

export interface LicenseDto {
  id: string;
  userId: string;
  partnerId?: string | null;
  partnerPlanId?: string | null;
  status: string;
  startsAt: string;
  endsAt?: string | null;
  limitsJson?: string | null;
  metadataJson?: string | null;
  createdAt: string;
}

export const licensesApi = {
  list: () => api.get<LicenseDto[]>('/api/licenses'),
};

export interface PartnerDto {
  id: string;
  name?: string;
  [key: string]: unknown;
}

export const partnersApi = {
  list: () => api.get<PartnerDto[]>('/api/partners'),
  stats: (partnerId: string) => api.get<Record<string, unknown>>(`/api/partners/${partnerId}/stats`),
};
