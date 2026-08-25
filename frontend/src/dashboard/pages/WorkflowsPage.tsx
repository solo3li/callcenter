import { useState } from "react";
import { PageHeader, Panel, Btn, TextInput, TextArea, ErrorBox, Empty, Modal, Field, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { workflowsApi } from "../../api/endpoints";
import type { WorkflowListItem, WorkflowVersionDto } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function WorkflowsPage() {
  const { data: workflows, loading, error, refetch } = useApi(() => workflowsApi.list(), []);
  const [modal, setModal] = useState<"create" | "versions" | null>(null);
  const [selected, setSelected] = useState<WorkflowListItem | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [busy, setBusy] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  if (!API_ENABLED) return <Empty>set VITE_API_URL to manage workflows</Empty>;

  const submitCreate = async () => {
    setBusy(true); setFormError(null);
    try {
      await workflowsApi.create({ name, description: description || undefined });
      setModal(null); void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="platform setup"
        title="Workflows"
        right={<Btn onClick={() => { setName(""); setDescription(""); setFormError(null); setModal("create"); }}>+ new workflow</Btn>}
      />
      <ErrorBox error={error} />

      <Panel>
        {loading ? <Empty>loading…</Empty> : !workflows || workflows.length === 0 ? (
          <Empty>no workflows yet</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Name</Th><Th>Description</Th><Th>Versions</Th><Th>Status</Th><Th /></tr></thead>
            <tbody>
              {(workflows as WorkflowListItem[]).map((w) => (
                <tr key={w.id} className="border-t border-line/50">
                  <Td>{w.name}</Td>
                  <Td>{w.description ?? "—"}</Td>
                  <Td mono>{w.versionCount}</Td>
                  <Td><Badge label={w.isActive ? "active" : "inactive"} tone={w.isActive ? "mint" : "dim"} /></Td>
                  <Td>
                    <div className="flex gap-3">
                      <button
                        onClick={() => { setSelected(w); setFormError(null); setModal("versions"); }}
                        className="font-mono text-[10px] uppercase text-dim hover:text-mint"
                      >versions</button>
                      <button
                        onClick={async () => { await workflowsApi.update(w.id, { isActive: !w.isActive }); void refetch(); }}
                        className="font-mono text-[10px] uppercase text-dim hover:text-amber"
                      >{w.isActive ? "disable" : "enable"}</button>
                    </div>
                  </Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Modal open={modal === "create"} title="New workflow" onClose={() => setModal(null)}>
        <div className="space-y-4">
          <Field label="Name"><TextInput value={name} onChange={(e) => setName(e.target.value)} placeholder="Refund escalation flow" /></Field>
          <Field label="Description"><TextArea value={description} onChange={(e) => setDescription(e.target.value)} /></Field>
          <ErrorBox error={formError} />
          <Btn onClick={submitCreate} disabled={busy || !name.trim()}>{busy ? "creating…" : "create workflow"}</Btn>
        </div>
      </Modal>

      {modal === "versions" && selected && (
        <WorkflowVersionsPanel workflow={selected} onClose={() => setModal(null)} />
      )}
    </div>
  );
}

function WorkflowVersionsPanel({ workflow, onClose }: { workflow: WorkflowListItem; onClose: () => void }) {
  const { data: versions, refetch } = useApi(() => workflowsApi.versions(workflow.id), [workflow.id]);
  const [definitionJson, setDefinitionJson] = useState("{\n  \"steps\": []\n}");
  const [busy, setBusy] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const createVersion = async () => {
    setBusy(true); setFormError(null);
    try {
      await workflowsApi.createVersion(workflow.id, definitionJson);
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "invalid JSON or failed");
    } finally { setBusy(false); }
  };

  return (
    <Modal open title={`Versions — ${workflow.name}`} onClose={onClose}>
      <div className="space-y-5">
        {!versions || versions.length === 0 ? <Empty>no versions yet</Empty> : (
          (versions as WorkflowVersionDto[]).map((v) => (
            <div key={v.id} className="border border-line bg-deep/60 px-3 py-2.5">
              <div className="flex items-center justify-between">
                <button onClick={() => setExpandedId(expandedId === v.id ? null : v.id)} className="font-display text-sm font-semibold text-mist">
                  v{v.versionNumber}
                  <span className="ml-2 font-mono text-[10px] font-normal text-dim">{new Date(v.createdAt).toLocaleString()}</span>
                  <span className="ml-2 font-mono text-[9px] uppercase tracking-[0.1em] text-dim">
                    {expandedId === v.id ? "▲ hide" : "▼ view json"}
                  </span>
                </button>
                {v.isPublished ? <Badge label="published" tone="mint" /> : (
                  <Btn variant="amber" onClick={async () => { await workflowsApi.publishVersion(v.id); void refetch(); }}>publish</Btn>
                )}
              </div>
              {expandedId === v.id && (
                <pre className="mt-2 max-h-40 overflow-auto border border-line bg-ink p-2 font-mono text-[10px] leading-relaxed text-dim">
                  {JSON.stringify(JSON.parse(v.definitionJson), null, 2)}
                </pre>
              )}
            </div>
          ))
        )}

        <div className="space-y-3 border-t border-line pt-4">
          <p className="font-mono text-[10px] uppercase tracking-[0.16em] text-dim">draft new version</p>
          <Field label="Definition JSON"><TextArea value={definitionJson} onChange={(e) => setDefinitionJson(e.target.value)} /></Field>
          <ErrorBox error={formError} />
          <Btn onClick={createVersion} disabled={busy}>{busy ? "saving…" : "create draft version"}</Btn>
        </div>
      </div>
    </Modal>
  );
}
