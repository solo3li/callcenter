import { useState } from "react";
import { PageHeader, Panel, Btn, TextInput, Field, ErrorBox, Empty, Modal, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { agentsAdminApi } from "../../api/endpoints";
import type { HumanAgentAdminDto, AccessKeyListItem } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;
const AGENT_STATUSES = ["Available", "Break", "NotReady", "Offline", "InCall"];

export default function AgentsAdminPage() {
  const { data: agents, loading, error, refetch } = useApi(() => agentsAdminApi.list(), []);
  const [createModal, setCreateModal] = useState(false);
  const [keysFor, setKeysFor] = useState<HumanAgentAdminDto | null>(null);
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [maxCalls, setMaxCalls] = useState("1");
  const [keyName, setKeyName] = useState("login key");
  const [issuedKey, setIssuedKey] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  if (!API_ENABLED) return <Empty>set VITE_API_URL to manage human agents</Empty>;

  const createAgent = async () => {
    setBusy(true); setFormError(null);
    try {
      await agentsAdminApi.create({ name, email: email || undefined, maxConcurrentCalls: Number(maxCalls) || 1 });
      setCreateModal(false); setName(""); setEmail(""); setMaxCalls("1");
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  const setStatus = async (id: string, status: string) => {
    await agentsAdminApi.setStatus(id, status);
    void refetch();
  };

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="business"
        title="Human agents"
        right={<Btn onClick={() => { setFormError(null); setCreateModal(true); }}>+ onboard agent</Btn>}
      />
      <ErrorBox error={error} />

      <Panel>
        {loading ? <Empty>loading…</Empty> : !agents || agents.length === 0 ? (
          <Empty>no human agents — onboard one and issue them an access key for the mobile app</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Name</Th><Th>Email</Th><Th>Status</Th><Th>Max calls</Th><Th /></tr></thead>
            <tbody>
              {(agents as HumanAgentAdminDto[]).map((a) => (
                <tr key={a.id} className="border-t border-line/50">
                  <Td>{a.name}</Td>
                  <Td mono>{a.email ?? "—"}</Td>
                  <Td><Badge label={String(a.status)} tone={Number(a.status) === 1 || a.status === "Available" ? "mint" : Number(a.status) === 0 || a.status === "Offline" ? "dim" : "amber"} /></Td>
                  <Td mono>{a.maxConcurrentCalls}</Td>
                  <Td>
                    <div className="flex flex-wrap items-center gap-2">
                      <select
                        value=""
                        onChange={(e) => e.target.value && void setStatus(a.id, e.target.value)}
                        className="border border-line bg-deep px-2 py-1 font-mono text-[10px] text-mist focus:border-mint focus:outline-none"
                      >
                        <option value="">set status…</option>
                        {AGENT_STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
                      </select>
                      <button
                        onClick={() => { setKeysFor(a); setIssuedKey(null); setKeyName(`${a.name} login key`); }}
                        className="font-mono text-[10px] uppercase text-dim hover:text-mint"
                      >access keys</button>
                    </div>
                  </Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Modal open={createModal} title="Onboard human agent" onClose={() => setCreateModal(false)}>
        <div className="space-y-4">
          <Field label="Name"><TextInput value={name} onChange={(e) => setName(e.target.value)} placeholder="Marco Rossi" /></Field>
          <Field label="Email (optional)"><TextInput value={email} onChange={(e) => setEmail(e.target.value)} placeholder="marco@company.com" /></Field>
          <Field label="Max concurrent calls"><TextInput value={maxCalls} onChange={(e) => setMaxCalls(e.target.value)} inputMode="numeric" /></Field>
          <ErrorBox error={formError} />
          <Btn onClick={createAgent} disabled={busy || !name.trim()}>{busy ? "creating…" : "create agent"}</Btn>
        </div>
      </Modal>

      {keysFor && (
        <AccessKeysPanel agent={keysFor} keyName={keyName} setKeyName={setKeyName}
          issuedKey={issuedKey} setIssuedKey={setIssuedKey} onClose={() => setKeysFor(null)} />
      )}
    </div>
  );
}

function AccessKeysPanel({ agent, keyName, setKeyName, issuedKey, setIssuedKey, onClose }: {
  agent: HumanAgentAdminDto; keyName: string; setKeyName: (v: string) => void;
  issuedKey: string | null; setIssuedKey: (v: string | null) => void; onClose: () => void;
}) {
  const { data: keys, refetch } = useApi(() => agentsAdminApi.accessKeys(agent.id), [agent.id]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const issue = async () => {
    setBusy(true); setError(null);
    try {
      const res = await agentsAdminApi.issueKey(agent.id, { name: keyName });
      setIssuedKey(res.rawKey);
      void refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  return (
    <Modal open title={`Access keys — ${agent.name}`} onClose={onClose}>
      <div className="space-y-5">
        {issuedKey && (
          <div className="space-y-1.5">
            <p className="font-mono text-[11px] text-amber">⚠ give this to the agent — shown only once.</p>
            <pre className="overflow-x-auto border border-mint/40 bg-deep p-3 font-mono text-xs text-mint">{issuedKey}</pre>
            <p className="font-mono text-[9px] text-dim">they paste it into the mobile app login screen.</p>
          </div>
        )}

        <div className="flex items-end gap-3">
          <div className="flex-1"><Field label="Key name"><TextInput value={keyName} onChange={(e) => setKeyName(e.target.value)} /></Field></div>
          <Btn onClick={issue} disabled={busy || !keyName.trim()}>{busy ? "issuing…" : "issue key"}</Btn>
        </div>

        <ErrorBox error={error} />

        <Panel title={`existing keys (${keys?.length ?? 0})`}>
          {!keys || keys.length === 0 ? <Empty>none issued yet</Empty> : (
            <table className="w-full">
              <thead><tr><Th>Name</Th><Th>Prefix</Th><Th>Status</Th><Th>Last used</Th><Th /></tr></thead>
              <tbody>
                {(keys as AccessKeyListItem[]).map((k) => (
                  <tr key={k.id} className="border-t border-line/50">
                    <Td>{k.name}</Td>
                    <Td mono>{k.keyPrefix}…</Td>
                    <Td><Badge label={k.status} tone={k.status === "Active" ? "mint" : "dim"} /></Td>
                    <Td mono>{k.lastUsedAt ? new Date(k.lastUsedAt).toLocaleDateString() : "never"}</Td>
                    <Td>
                      {k.status === "Active" && (
                        <button
                          onClick={async () => { await agentsAdminApi.revokeKey(agent.id, k.id); void refetch(); }}
                          className="font-mono text-[10px] uppercase text-dim hover:text-coral"
                        >revoke</button>
                      )}
                    </Td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </Panel>
      </div>
    </Modal>
  );
}
