import type { Call } from "./data";
import type { ActiveCallDto, ApiCallStatus, CallSession } from "../api/endpoints";

export type UiCallStatus = Call["status"];

export function apiToUiStatus(status: ApiCallStatus): UiCallStatus {
  switch (status) {
    case "Queued":
      return "queued";
    case "Ringing":
    case "Active":
      return "active-ai";
    case "Transferred":
      return "active-human";
    case "Completed":
      return "completed-ai";
    case "Missed":
      return "missed";
    case "Failed":
      return "abandoned";
    default:
      return "abandoned";
  }
}

export function sessionToUiCall(s: CallSession): Call {
  const wait = s.answeredAt
    ? Math.max(0, Math.round((new Date(s.answeredAt).getTime() - new Date(s.startedAt).getTime()) / 1000))
    : null;

  let resolution: Call["resolution"] = null;
  if (s.status === "Completed") resolution = "ai-resolved";
  else if (s.status === "Transferred" || (s.endedAt && wait && wait > 30)) resolution = "escalated";
  else if (s.status === "Missed" || s.status === "Failed") resolution = "unresolved";

  return {
    id: s.id,
    callerName: s.livekitRoomName,
    callerNumber: s.direction === "Outbound" ? "Outbound" : "Inbound",
    status: apiToUiStatus(s.status),
    duration: s.durationSeconds ?? null,
    agentId: null,
    agentName: null,
    agentType: s.status === "Transferred" ? "human" : s.status === "Active" ? "ai" : null,
    intent: "",
    confidence: 0,
    sentiment: "neutral",
    transcript: [],
    startTime: s.startedAt,
    endTime: s.endedAt ?? null,
    waitTime: wait ?? 0,
    resolution,
    csat: null,
    channel: "LiveKit",
    queue: "",
    skillGroup: "",
    escalationDelay: null,
    apiActions: [],
    recordingUrl: null,
  };
}

export function activeCallToUiCall(a: ActiveCallDto): Call {
  return sessionToUiCall({
    id: a.id,
    userId: "",
    callConfigurationId: null,
    livekitRoomName: a.livekitRoomName,
    status: a.status,
    direction: a.direction,
    startedAt: a.startedAt,
    answeredAt: a.answeredAt ?? null,
    endedAt: null,
    durationSeconds: a.durationSeconds ?? null,
    metadataJson: null,
    participantCount: a.participantCount,
    createdAt: a.createdAt,
  });
}
