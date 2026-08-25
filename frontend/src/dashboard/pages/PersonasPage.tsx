import { useState } from "react";
import { PageHeader, Panel, Btn, TextInput, TextArea, ErrorBox, Empty, Modal, Field, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { personasApi, actionsApi } from "../../api/endpoints";
import type { PersonaListItem, PersonaVersionDto, ActionDefinitionDto } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function PersonasPage() {
  const { data: personas, loading, error, refetch } = useApi(() => personasApi.list(), []);
  const [modal, setModal] = useState<"create" | "edit" | "version" | "actions" | null>(null);
  const [selected, setSelected] = useState<PersonaListItem | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [prompt, setPrompt] = useState("");
  const [configJson, setConfigJson] = useState("");
  const [busy, setBusy] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  if (!API_ENABLED) return <Empty>set VITE_API_URL to manage personas</Empty>;

  const openCreate = () => {
    setName(""); setDescription(""); setFormError(null);
    setModal("create");
  };
  const openEdit = (p: PersonaListItem) => {
    setSelected(p); setName(p.name); setDescription(p.description ?? ""); setFormError(null);
    setModal("edit");
  };
  const openVersion = (p: PersonaListItem) => {
    setSelected(p); setPrompt(""); setConfigJson(""); setFormError(null);
    setModal("version");
  };

  const submitCreate = async () => {
    setBusy(true); setFormError(null);
    try {
      await personasApi.create({ name, description: description || undefined });
      setModal(null);
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  const submitEdit = async () => {
    if (!selected) return;
    setBusy(true); setFormError(null);
    try {
      await personasApi.update(selected.id, { name, description: description || undefined });
      setModal(null);
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  const submitVersion = async () => {
    if (!selected) return;
    setBusy(true); setFormError(null);
    try {
      await personasApi.createVersion(selected.id, {
        systemPrompt: prompt,
        configurationJson: configJson.trim() || undefined,
      });
      setModal(null);
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  const toggleActive = async (p: PersonaListItem) => {
    await personasApi.update(p.id, { isActive: !p.isActive });
    void refetch();
  };

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="platform setup"
        title="AI personas"
        right={<Btn onClick={openCreate}>+ new persona</Btn>}
      />
      <ErrorBox error={error} />

      <Panel>
        {loading ? <Empty>loading…</Empty> : !personas || personas.length === 0 ? (
          <Empty>no personas yet — create your first AI persona</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Name</Th><Th>Description</Th><Th>Status</Th><Th>Updated</Th><Th /></tr></thead>
            <tbody>
              {(personas as PersonaListItem[]).map((p) => (
                <tr key={p.id} className="border-t border-line/50">
                  <Td>{p.name}</Td>
                  <Td>{p.description ?? "—"}</Td>
                  <Td><Badge label={p.isActive ? "active" : "inactive"} tone={p.isActive ? "mint" : "dim"} /></Td>
                  <Td mono>{new Date(p.updatedAt).toLocaleDateString()}</Td>
                  <Td>
                    <div className="flex gap-3">
                      <button onClick={() => openEdit(p)} className="font-mono text-[10px] uppercase text-dim hover:text-mint">edit</button>
                      <button onClick={() => openVersion(p)} className="font-mono text-[10px] uppercase text-dim hover:text-mint">versions</button>
                      <PersonaActionsCell personaId={p.id} onOpen={() => { setSelected(p); setModal("actions"); }} />
                      <button onClick={() => void toggleActive(p)} className="font-mono text-[10px] uppercase text-dim hover:text-amber">
                        {p.isActive ? "disable" : "enable"}
                      </button>
                    </div>
                  </Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Modal open={modal === "create"} title="New AI persona" onClose={() => setModal(null)}>
        <div className="space-y-4">
          <Field label="Name"><TextInput value={name} onChange={(e) => setName(e.target.value)} placeholder="Support agent" /></Field>
          <Field label="Description"><TextArea value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Handles billing and refunds…" /></Field>
          <ErrorBox error={formError} />
          <Btn onClick={submitCreate} disabled={busy || !name.trim()}>{busy ? "creating…" : "create persona"}</Btn>
        </div>
      </Modal>

      <Modal open={modal === "edit"} title={`Edit — ${selected?.name}`} onClose={() => setModal(null)}>
        <div className="space-y-4">
          <Field label="Name"><TextInput value={name} onChange={(e) => setName(e.target.value)} /></Field>
          <Field label="Description"><TextArea value={description} onChange={(e) => setDescription(e.target.value)} /></Field>
          <ErrorBox error={formError} />
          <Btn onClick={submitEdit} disabled={busy}>{busy ? "saving…" : "save changes"}</Btn>
        </div>
      </Modal>

      {modal === "version" && selected && (
        <VersionsPanel persona={selected} onClose={() => setModal(null)} prompt={prompt} setPrompt={setPrompt}
          configJson={configJson} setConfigJson={setConfigJson} busy={busy} formError={formError}
          onSubmit={submitVersion} />
      )}
      {modal === "actions" && selected && (
        <ActionsPanel personaId={selected.id} onClose={() => setModal(null)} />
      )}
    </div>
  );
}

function VersionsPanel({ persona, onClose, prompt, setPrompt, configJson, setConfigJson, busy, formError, onSubmit }: {
  persona: PersonaListItem; onClose: () => void;
  prompt: string; setPrompt: (v: string) => void;
  configJson: string; setConfigJson: (v: string) => void;
  busy: boolean; formError: string | null; onSubmit: () => void;
}) {
  const { data: versions, refetch } = useApi(() => personasApi.versions(persona.id), [persona.id]);

  return (
    <Modal open title={`Versions — ${persona.name}`} onClose={onClose}>
      <div className="space-y-5">
        <div className="space-y-2">
          {!versions || versions.length === 0 ? (
            <Empty>no versions yet</Empty>
          ) : (
            (versions as PersonaVersionDto[]).map((v) => (
              <div key={v.id} className="flex items-center justify-between border border-line bg-deep/60 px-3 py-2.5">
                <div>
                  <p className="font-display text-sm font-semibold text-mist">
                    v{v.versionNumber}
                    <span className="ml-2 font-mono text-[10px] font-normal text-dim">
                      {new Date(v.createdAt).toLocaleString()}
                    </span>
                  </p>
                  <p className="mt-0.5 max-h-8 overflow-hidden text-[11px] text-dim">{v.systemPrompt.slice(0, 90)}…</p>
                </div>
                {v.isPublished ? (
                  <Badge label="published" tone="mint" />
                ) : (
                  <Btn variant="amber" onClick={async () => { await personasApi.publishVersion(persona.id, v.id); void refetch(); }}>
                    publish
                  </Btn>
                )}
              </div>
            ))
          )}
        </div>

        <div className="space-y-3 border-t border-line pt-4">
          <p className="font-mono text-[10px] uppercase tracking-[0.16em] text-dim">draft new version</p>
          <Field label="System prompt"><TextArea value={prompt} onChange={(e) => setPrompt(e.target.value)} placeholder="You are a helpful support agent…" /></Field>
          <Field label="Configuration JSON (optional)"><TextArea value={configJson} onChange={(e) => setConfigJson(e.target.value)} placeholder='{"temperature": 0.7}' /></Field>
          <ErrorBox error={formError} />
          <Btn onClick={onSubmit} disabled={busy || !prompt.trim()}>{busy ? "saving…" : "create draft version"}</Btn>
        </div>
      </div>
    </Modal>
  );
}

function PersonaActionsCell({ personaId, onOpen }: { personaId: string; onOpen: () => void }) {
  const { data } = useApi(() => personasApi.actions(personaId), [personaId]);
  const count = data?.length ?? 0;
  return (
    <button onClick={onOpen} className="font-mono text-[10px] uppercase text-dim hover:text-mint">
      actions ({count})
    </button>
  );
}

function ActionsPanel({ personaId, onClose }: { personaId: string; onClose: () => void }) {
  const { data: linked, refetch } = useApi(() => personasApi.actions(personaId), [personaId]);
  const { data: all } = useApi(() => actionsApi.list(), []);
  const [error, setError] = useState<string | null>(null);

  const linkedIds = new Set((linked ?? []).map((a) => a.id));
  const available = (all ?? []).filter((a) => !linkedIds.has(a.id));

  return (
    <Modal open title="Persona actions" onClose={onClose}>
      <div className="space-y-5">
        <ErrorBox error={error} />
        <Panel title="linked actions">
          {!linked || linked.length === 0 ? <Empty>none linked</Empty> : (
            <ul className="space-y-1.5">
              {(linked as ActionDefinitionDto[]).map((a) => (
                <li key={a.id} className="flex items-center justify-between">
                  <span className="text-xs text-mist">{a.displayName ?? a.name} <span className="font-mono text-[9px] text-dim">({a.actionType})</span></span>
                  <button
                    onClick={async () => {
                      try { await personasApi.removeAction(personaId, a.id); void refetch(); }
                      catch (e) { setError(e instanceof Error ? e.message : "failed"); }
                    }}
                    className="font-mono text-[10px] uppercase text-coral"
                  >remove</button>
                </li>
              ))}
            </ul>
          )}
        </Panel>
        <Panel title="available actions">
          {available.length === 0 ? <Empty>all linked or none defined</Empty> : (
            <ul className="space-y-1.5">
              {available.map((a) => (
                <li key={a.id} className="flex items-center justify-between">
                  <span className="text-xs text-mist">{a.displayName ?? a.name} <span className="font-mono text-[9px] text-dim">({a.actionType})</span></span>
                  <button
                    onClick={async () => {
                      try { await personasApi.addAction(personaId, a.id); setError(null); void refetch(); }
                      catch (e) { setError(e instanceof Error ? e.message : "failed"); }
                    }}
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
