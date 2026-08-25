import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Badge from "../components/Badge";
import { CALLS, type Call } from "../data";
import { callsApi, recordingsApi, transfersApi } from "../../api/endpoints";
import type { CallDetail as ApiCallDetail } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

function fmtDuration(s: number | null | undefined): string {
  if (s == null) return "—";
  const m = Math.floor(s / 60);
  return `${m}m ${s % 60}s`;
}

function fmtTime(iso: string | null | undefined): string {
  if (!iso) return "—";
  return new Date(iso).toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" });
}

const UI_BADGE: Record<string, { label: string; tone: "mint" | "amber" | "coral" | "dim" }> = {
  "active-ai": { label: "AI — live now", tone: "mint" },
  "active-human": { label: "Human — live now", tone: "amber" },
  queued: { label: "Waiting in queue", tone: "coral" },
  "completed-ai": { label: "Completed", tone: "mint" },
  "completed-escalated": { label: "Escalated to human", tone: "amber" },
  missed: { label: "Missed", tone: "coral" },
  abandoned: { label: "Failed", tone: "dim" },
};

const API_BADGE: Record<string, { label: string; tone: "mint" | "amber" | "coral" | "dim" }> = {
  Queued: { label: "Queued", tone: "coral" },
  Ringing: { label: "Ringing", tone: "amber" },
  Active: { label: "Active — live now", tone: "mint" },
  Transferred: { label: "Transferred to human", tone: "amber" },
  Completed: { label: "Completed", tone: "mint" },
  Missed: { label: "Missed", tone: "coral" },
  Failed: { label: "Failed", tone: "coral" },
};

export default function CallDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const shouldFetchApi = API_ENABLED && id && !CALLS.some((c) => c.id === id);
  const { data: apiCall, error, loading } = useApi(
    () => (shouldFetchApi ? callsApi.get(id!) : Promise.resolve(null)),
    [id]
  );

  const mockCall: Call | undefined = !shouldFetchApi ? CALLS.find((c) => c.id === id) : undefined;

  if (!loading && !error && !mockCall && !apiCall) {
    return (
      <div className="space-y-6">
        <button onClick={() => navigate(-1)} className="flex items-center gap-2 font-mono text-[11px] uppercase tracking-[0.14em] text-dim transition-colors hover:text-mist">
          <svg width="12" height="10" viewBox="0 0 12 10" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M11 5H1M4 1 .5 5 4 9" /></svg>
          Back
        </button>
        <p className="font-display text-2xl text-mist">Call not found</p>
      </div>
    );
  }

  if (mockCall) {
    return <MockDetailView call={mockCall} onBack={() => navigate("/dashboard/history")} />;
  }

  if (apiCall) {
    return <ApiDetailView call={apiCall} onBack={() => navigate("/dashboard/history")} />;
  }

  return (
    <div className="space-y-6">
      <button onClick={() => navigate(-1)} className="font-mono text-[11px] uppercase tracking-[0.14em] text-dim hover:text-mist">
        ← Back
      </button>
      <p className="font-display text-xl text-mist">{loading ? "Loading call…" : error ?? "—"}</p>
    </div>
  );
}

function MockDetailView({ call, onBack }: { call: Call; onBack: () => void }) {
  const sb = UI_BADGE[call.status];
  return (
    <div className="space-y-6">
      <BackButton onClick={onBack} />
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="kicker mb-1">// call detail</p>
          <h1 className="font-display text-2xl font-bold tracking-tight text-mist">{call.callerName}</h1>
          <p className="mt-1 font-mono text-sm text-dim">{call.callerNumber}</p>
        </div>
        <Badge label={sb?.label ?? call.status} tone={sb?.tone ?? "dim"} />
      </div>
      <InfoGrid
        cells={[
          { label: "Agent", value: call.agentName ? `${call.agentName}${call.agentType ? ` (${call.agentType})` : ""}` : "—" },
          { label: "Duration", value: fmtDuration(call.duration) },
          { label: "Wait time", value: `${call.waitTime}s` },
          { label: "Started", value: fmtTime(call.startTime) },
        ]}
      />
      {call.transcript.length > 0 && (
        <Section title="transcript">
          <div className="space-y-2.5">
            {call.transcript.map((t, i) => (
              <div key={i} className="flex gap-3">
                <span className={`w-14 shrink-0 pt-0.5 font-mono text-[9px] uppercase tracking-[0.1em] ${t.role === "caller" ? "text-dim" : t.role === "human" ? "text-amber" : "text-mint"}`}>
                  {t.role}
                </span>
                <p className="text-sm leading-relaxed text-mist/90">{t.text}</p>
              </div>
            ))}
          </div>
        </Section>
      )}
      {call.apiActions.length > 0 && (
        <Section title="api actions">
          <ul className="space-y-1.5">
            {call.apiActions.map((a, i) => (
              <li key={i} className="font-mono text-[11px] text-dim">
                [{a.timestamp}] <span className="text-mist/90">{a.action}</span>
              </li>
            ))}
          </ul>
        </Section>
      )}
    </div>
  );
}

function ApiDetailView({ call, onBack }: { call: ApiCallDetail; onBack: () => void }) {
  const badge = API_BADGE[call.status] ?? { label: call.status, tone: "dim" as const };
  const humanParticipant = call.participants.find((p) => p.participantType === "Human");
  const waitSeconds =
    call.answeredAt
      ? Math.max(0, Math.round((new Date(call.answeredAt).getTime() - new Date(call.startedAt).getTime()) / 1000))
      : null;
  const [actionMsg, setActionMsg] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [transferReason, setTransferReason] = useState("");
  const [transferMsg, setTransferMsg] = useState<string | null>(null);

  const isActive = ["Queued", "Ringing", "Active", "Transferred"].includes(call.status);

  const forceEnd = async () => {
    setBusy(true);
    setActionMsg(null);
    try {
      await callsApi.end(call.id);
      setActionMsg("Call ended. Refresh to see updated status.");
    } catch (e) {
      setActionMsg(e instanceof Error ? e.message : "Failed to end call");
    } finally {
      setBusy(false);
    }
  };

  const initiateTransfer = async () => {
    setBusy(true);
    setTransferMsg(null);
    try {
      await transfersApi.initiate(call.id, transferReason);
      setTransferMsg("Transfer requested — routing to next available agent. Refresh to see it in the list below.");
      setTransferReason("");
    } catch (e) {
      setTransferMsg(e instanceof Error ? e.message : "Transfer failed");
    } finally {
      setBusy(false);
    }
  };

  const downloadRecording = async (recordingId: string) => {
    setActionMsg(null);
    try {
      const res = await recordingsApi.downloadUrl(call.id, recordingId);
      window.open(res.url, "_blank");
    } catch (e) {
      setActionMsg(e instanceof Error ? `Download link failed: ${e.message}` : "Download failed");
    }
  };

  let metadataPretty: string | null = null;
  if (call.metadataJson) {
    try {
      metadataPretty = JSON.stringify(JSON.parse(call.metadataJson), null, 2);
    } catch {
      metadataPretty = call.metadataJson;
    }
  }

  return (
    <div className="space-y-6">
      <BackButton onClick={onBack} />

      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="kicker mb-1">// call detail</p>
          <h1 className="font-display text-2xl font-bold tracking-tight text-mist">{call.livekitRoomName}</h1>
          <p className="mt-1 font-mono text-sm text-dim">
            {call.direction} · {call.status}
            {call.callConfigurationName ? ` · ${call.callConfigurationName}` : ""}
          </p>
        </div>
        <Badge label={badge.label} tone={badge.tone} />
      </div>

      {isActive && (
        <div className="space-y-3 border border-amber/40 bg-amber/10 px-4 py-3">
          <p className="font-mono text-[11px] uppercase tracking-[0.14em] text-amber">
            supervisor control
          </p>
          <div className="flex flex-wrap items-center gap-3">
            <button
              onClick={forceEnd}
              disabled={busy}
              className="btn btn-coral !px-4 !py-2 !text-[10px] disabled:opacity-40"
            >
              {busy ? "ending…" : "■ force end call"}
            </button>
            <input
              value={transferReason}
              onChange={(e) => setTransferReason(e.target.value)}
              placeholder="transfer reason (optional)…"
              className="min-w-[220px] flex-1 border border-line bg-deep px-3 py-2 font-mono text-xs text-mist placeholder:text-dim/60 focus:border-mint focus:outline-none"
            />
            <button
              onClick={initiateTransfer}
              disabled={busy}
              className="btn btn-mint !px-4 !py-2 !text-[10px] disabled:opacity-40"
            >
              {busy ? "routing…" : "→ transfer to next agent"}
            </button>
          </div>
          {(actionMsg || transferMsg) && (
            <p className="font-mono text-[10px] leading-relaxed text-dim">
              {actionMsg ?? transferMsg}
            </p>
          )}
        </div>
      )}

      <InfoGrid
        cells={[
          { label: "Human agent", value: humanParticipant?.displayName ?? call.handoff?.toHumanAgentName ?? call.transfers[0]?.toHumanAgentName ?? "—" },
          { label: "Duration", value: fmtDuration(call.durationSeconds) },
          { label: "Wait to answer", value: waitSeconds != null ? `${waitSeconds}s` : "—" },
          { label: "Participants", value: String(call.participants.length) },
        ]}
      />

      <InfoGrid
        cells={[
          { label: "Started", value: fmtTime(call.startedAt) },
          { label: "Answered", value: fmtTime(call.answeredAt ?? null) },
          { label: "Ended", value: fmtTime(call.endedAt ?? null) },
          { label: "Created", value: new Date(call.createdAt).toLocaleString() },
        ]}
      />

      <Section title={`participants (${call.participants.length})`}>
        {call.participants.length === 0 ? (
          <Empty>No participants recorded.</Empty>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="border-b border-line">
                <Th>Type</Th><Th>Identity</Th><Th>Display name</Th><Th>Joined</Th><Th>Left</Th>
              </tr>
            </thead>
            <tbody>
              {call.participants.map((p) => (
                <tr key={p.id} className="border-b border-line/50 last:border-0">
                  <Td>{p.participantType}</Td>
                  <Td mono>{p.livekitIdentity}</Td>
                  <Td>{p.displayName ?? "—"}</Td>
                  <Td>{fmtTime(p.joinedAt)}</Td>
                  <Td>{fmtTime(p.leftAt ?? null)}</Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Section>

      <Section title={`transfers (${call.transfers.length})`}>
        {call.transfers.length === 0 ? (
          <Empty>No transfers — handled end-to-end by AI.</Empty>
        ) : (
          <ul className="space-y-2">
            {call.transfers.map((t) => (
              <li key={t.id} className="border border-line bg-deep/60 px-3 py-2.5">
                <div className="flex items-center justify-between gap-3">
                  <span className="font-display text-sm font-semibold text-mist">
                    → {t.toHumanAgentName ?? "Unknown agent"}
                  </span>
                  <Badge
                    label={t.status}
                    tone={t.status === "Accepted" || t.status === "Completed" ? "mint" : t.status === "Requested" ? "amber" : "coral"}
                  />
                </div>
                <p className="mt-1 font-mono text-[10px] text-dim">
                  requested {new Date(t.requestedAt).toLocaleString()}
                  {t.acceptedAt ? ` · accepted ${fmtTime(t.acceptedAt)}` : ""}
                  {t.reason ? ` · ${t.reason}` : ""}
                  {t.failureReason ? ` · failed: ${t.failureReason}` : ""}
                </p>
              </li>
            ))}
          </ul>
        )}
      </Section>

      <Section title={`recordings (${call.recordings.length})`}>
        {call.recordings.length === 0 ? (
          <Empty>No recordings for this call.</Empty>
        ) : (
          <ul className="space-y-2">
            {call.recordings.map((r) => (
              <li key={r.id} className="flex items-center justify-between border border-line bg-deep/60 px-3 py-2.5">
                <div>
                  <p className="font-mono text-[11px] text-mist">{r.objectKey}</p>
                  <p className="font-mono text-[9px] uppercase tracking-[0.12em] text-dim">
                    {r.storageProvider} · {fmtDuration(r.durationSeconds)} · {r.sizeBytes ? `${Math.round(r.sizeBytes / 1024)} KB` : "?"}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  {r.status === "Available" && (
                    <button
                      onClick={() => void downloadRecording(r.id)}
                      className="font-mono text-[10px] uppercase tracking-[0.12em] text-mint hover:text-mist"
                    >
                      ↓ download
                    </button>
                  )}
                  <Badge label={r.status} tone={r.status === "Available" ? "mint" : "amber"} />
                </div>
              </li>
            ))}
          </ul>
        )}
      </Section>

      {call.handoff && (
        <Section title="ai handoff context">
          <p className="text-sm leading-relaxed text-mist/90">{call.handoff.summary ?? "No summary."}</p>
          {call.handoff.contextDataJson && (
            <pre className="mt-3 overflow-x-auto border border-line bg-deep p-3 font-mono text-[10px] leading-relaxed text-dim">
              {safeJson(call.handoff.contextDataJson)}
            </pre>
          )}
        </Section>
      )}

      {metadataPretty && (
        <Section title="metadata">
          <pre className="overflow-x-auto border border-line bg-deep p-3 font-mono text-[10px] leading-relaxed text-dim">
            {metadataPretty}
          </pre>
        </Section>
      )}
    </div>
  );
}

function safeJson(raw: string): string {
  try {
    return JSON.stringify(JSON.parse(raw), null, 2);
  } catch {
    return raw;
  }
}

function BackButton({ onClick }: { onClick: () => void }) {
  return (
    <button onClick={onClick} className="flex items-center gap-2 font-mono text-[11px] uppercase tracking-[0.14em] text-dim transition-colors hover:text-mist">
      <svg width="12" height="10" viewBox="0 0 12 10" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M11 5H1M4 1 .5 5 4 9" /></svg>
      Back to history
    </button>
  );
}

function InfoGrid({ cells }: { cells: { label: string; value: string }[] }) {
  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {cells.map((c) => (
        <div key={c.label} className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">{c.label}</p>
          <p className="mt-1 truncate font-display text-lg font-bold tabular-nums text-mist">{c.value}</p>
        </div>
      ))}
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="border border-line bg-panel/40 p-5">
      <p className="mb-4 font-mono text-[10px] uppercase tracking-[0.18em] text-dim">{title}</p>
      {children}
    </div>
  );
}

function Empty({ children }: { children: React.ReactNode }) {
  return <p className="font-mono text-[11px] text-dim">{children}</p>;
}

function Th({ children }: { children: React.ReactNode }) {
  return <th className="pb-2 pr-4 text-left font-mono text-[9px] uppercase tracking-[0.14em] text-dim">{children}</th>;
}

function Td({ children, mono }: { children: React.ReactNode; mono?: boolean }) {
  return (
    <td className={`py-2 pr-4 text-xs ${mono ? "font-mono text-[11px]" : ""} text-mist/90`}>{children}</td>
  );
}
