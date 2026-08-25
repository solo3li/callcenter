import { useState } from "react";
import { PageHeader, Panel, Btn, TextInput, TextArea, ErrorBox, Empty, Modal, Field, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { callConfigsApi, personasApi, workflowsApi, actionsApi } from "../../api/endpoints";
import type { CallConfigListItem, PersonaListItem, WorkflowListItem, ActionDefinitionDto } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function CallConfigsPage() {
  const { data: configs, loading, error, refetch } = useApi(() => callConfigsApi.list(), []);
  const [modal, setModal] = useState<"create" | "edit" | "actions" | null>(null);
  const [selected, setSelected] = useState<CallConfigListItem | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [personaId, setPersonaId] = useState("");
  const [workflowId, setWorkflowId] = useState("");
  const [configJson, setConfigJson] = useState("{}");
  const [busy, setBusy] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  if (!API_ENABLED) return <Empty>set VITE_API_URL to manage call configurations</Empty>;

  const openEdit = (c: CallConfigListItem) => {
    setSelected(c);
    setName(c.name);
    setDescription(c.description ?? "");
    setPersonaId(c.personaId ?? "");
    setWorkflowId(c.workflowId ?? "");
    setConfigJson(c.configJson ?? "{}");
    setFormError(null);
    setModal("edit");
  };

  const submit = async () => {
    setBusy(true); setFormError(null);
    const payload = {
      name,
      description: description || undefined,
      personaId: personaId || undefined,
      workflowId: workflowId || undefined,
      configJson: configJson.trim() ? configJson : undefined,
    };
    try {
      if (modal === "create") await callConfigsApi.create(payload as { name: string });
      else if (selected) await callConfigsApi.update(selected.id, payload);
      setModal(null);
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="platform setup"
        title="Call configurations"
        right={<Btn onClick={() => { setName(""); setDescription(""); setPersonaId(""); setWorkflowId(""); setConfigJson("{}"); setFormError(null); setSelected(null); setModal("create"); }}>+ new configuration</Btn>}
      />
      <ErrorBox error={error} />

      <Panel>
        {loading ? <Empty>loading…</Empty> : !configs || configs.length === 0 ? (
          <Empty>no configurations — create one to put a persona on a phone line</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Name</Th><Th>Persona</Th><Th>Workflow</Th><Th>Actions</Th><Th>Status</Th><Th /></tr></thead>
            <tbody>
              {(configs as CallConfigListItem[]).map((c) => (
                <tr key={c.id} className="border-t border-line/50">
                  <Td>{c.name}<br /><span className="font-mono text-[9px] text-dim">{c.description}</span></Td>
                  <Td>{c.personaName ?? "—"}</Td>
                  <Td>{c.workflowName ?? "—"}</Td>
                  <Td mono>{c.actionCount}</Td>
                  <Td><Badge label={c.isActive ? "live" : "idle"} tone={c.isActive ? "mint" : "dim"} /></Td>
                  <Td>
                    <div className="flex gap-3">
                      <button onClick={() => openEdit(c)} className="font-mono text-[10px] uppercase text-dim hover:text-mint">edit</button>
                      {!c.isActive && (
                        <button
                          onClick={async () => { await callConfigsApi.activate(c.id); void refetch(); }}
                          className="font-mono text-[10px] uppercase text-mint hover:text-amber"
                        >activate</button>
                      )}
                      <button
                        onClick={() => { setSelected(c); setModal("actions"); }}
                        className="font-mono text-[10px] uppercase text-dim hover:text-mint"
                      >actions</button>
                      <button
                        onClick={async () => { await callConfigsApi.del(c.id); void refetch(); }}
                        className="font-mono text-[10px] uppercase text-dim hover:text-coral"
                      >delete</button>
                    </div>
                  </Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Modal open={modal === "create" || modal === "edit"} title={modal === "create" ? "New call configuration" : `Edit — ${selected?.name}`} onClose={() => setModal(null)}>
        <div className="space-y-4">
          <Field label="Name"><TextInput value={name} onChange={(e) => setName(e.target.value)} placeholder="Main support line" /></Field>
          <Field label="Description"><TextInput value={description} onChange={(e) => setDescription(e.target.value)} /></Field>
          <div className="grid grid-cols-2 gap-4">
            <Field label="AI persona"><PersonaSelect value={personaId} onChange={setPersonaId} /></Field>
            <Field label="Workflow"><WorkflowSelect value={workflowId} onChange={setWorkflowId} /></Field>
          </div>
          <Field label="Config JSON (optional)"><TextArea value={configJson} onChange={(e) => setConfigJson(e.target.value)} /></Field>
          <ErrorBox error={formError} />
          <Btn onClick={submit} disabled={busy || !name.trim()}>{busy ? "saving…" : modal === "create" ? "create" : "save"}</Btn>
        </div>
      </Modal>

      {modal === "actions" && selected && <ConfigActionsPanel configId={selected.id} onClose={() => setModal(null)} />}
    </div>
  );
}

function PersonaSelect({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const { data } = useApi(() => personasApi.list(), []);
  return (
    <select value={value} onChange={(e) => onChange(e.target.value)} className="w-full border border-line bg-deep px-3 py-2.5 font-mono text-xs text-mist focus:border-mint focus:outline-none">
      <option value="">— none —</option>
      {(data as PersonaListItem[] | null)?.filter((p) => p.isActive).map((p) => (
        <option key={p.id} value={p.id}>{p.name}</option>
      ))}
    </select>
  );
}

function WorkflowSelect({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const { data } = useApi(() => workflowsApi.list(), []);
  return (
    <select value={value} onChange={(e) => onChange(e.target.value)} className="w-full border border-line bg-deep px-3 py-2.5 font-mono text-xs text-mist focus:border-mint focus:outline-none">
      <option value="">— none —</option>
      {(data as WorkflowListItem[] | null)?.filter((w) => w.isActive).map((w) => (
        <option key={w.id} value={w.id}>{w.name}</option>
      ))}
    </select>
  );
}

function ConfigActionsPanel({ configId, onClose }: { configId: string; onClose: () => void }) {
  const { data: linked, refetch } = useApi(() => callConfigsApi.getActions(configId), [configId]);
  const { data: all } = useApi(() => actionsApi.list(), []);
  const [error, setError] = useState<string | null>(null);

  const linkedIds = new Set((linked ?? []).map((a) => a.id));
  const available = (all ?? []).filter((a) => !linkedIds.has(a.id));

  const save = async (ids: string[]) => {
    try {
      await callConfigsApi.setActions(configId, ids);
      setError(null);
      void refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : "failed");
    }
  };

  return (
    <Modal open title="Configuration actions" onClose={onClose}>
      <div className="space-y-5">
        <ErrorBox error={error} />
        <p className="font-mono text-[10px] leading-relaxed text-dim">
          Actions execute mid-call (refunds, CRM writes, bookings). Linked actions are exposed to the AI agent on this line.
        </p>
        <Panel title={`linked (${linked?.length ?? 0})`}>
          {!linked || linked.length === 0 ? <Empty>none linked</Empty> : (
            <ul className="space-y-1.5">
              {(linked as ActionDefinitionDto[]).map((a) => (
                <li key={a.id} className="flex items-center justify-between">
                  <span className="text-xs text-mist">{a.displayName ?? a.name} <span className="font-mono text-[9px] text-dim">({a.actionType})</span></span>
                  <button
                    onClick={() => void save((linked ?? []).map((x) => x.id).filter((id) => id !== a.id))}
                    className="font-mono text-[10px] uppercase text-coral"
                  >remove</button>
                </li>
              ))}
            </ul>
          )}
        </Panel>
        <Panel title="add action">
          {available.length === 0 ? <Empty>all linked or none defined</Empty> : (
            <ul className="space-y-1.5">
              {available.map((a) => (
                <li key={a.id} className="flex items-center justify-between">
                  <span className="text-xs text-mist">{a.displayName ?? a.name} <span className="font-mono text-[9px] text-dim">({a.actionType})</span></span>
                  <button
                    onClick={() => void save([...(linked ?? []).map((x) => x.id), a.id])}
                    className="font-mono text-[10px] uppercase text-mint"
                  >link</button>
                </li>
              ))}
            </ul>
          )}
        </Panel>
      </div>
    </Modal>
  );
}
