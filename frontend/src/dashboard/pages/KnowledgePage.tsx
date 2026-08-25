import { useState } from "react";
import { PageHeader, Panel, Btn, TextInput, TextArea, Field, ErrorBox, Empty, Modal, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { knowledgeApi } from "../../api/endpoints";
import type { KnowledgeBaseListItem, KnowledgeDocumentDto, SearchResultItem } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function KnowledgePage() {
  const { data: kbs, loading, error, refetch } = useApi(() => knowledgeApi.kbs(), []);
  const [selectedKb, setSelectedKb] = useState<KnowledgeBaseListItem | null>(null);
  const [createModal, setCreateModal] = useState(false);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [busy, setBusy] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  if (!API_ENABLED) return <Empty>set VITE_API_URL to manage knowledge bases</Empty>;

  const createKb = async () => {
    setBusy(true); setFormError(null);
    try {
      await knowledgeApi.createKb({ name, description: description || undefined });
      setCreateModal(false); setName(""); setDescription("");
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="platform setup"
        title="Knowledge bases"
        right={<Btn onClick={() => { setFormError(null); setCreateModal(true); }}>+ new knowledge base</Btn>}
      />
      <ErrorBox error={error} />

      <Panel>
        {loading ? <Empty>loading…</Empty> : !kbs || kbs.length === 0 ? (
          <Empty>no knowledge bases — upload documents so your AI personas can answer from them (RAG)</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Name</Th><Th>Description</Th><Th>Docs</Th><Th>Status</Th><Th /></tr></thead>
            <tbody>
              {(kbs as KnowledgeBaseListItem[]).map((kb) => (
                <tr key={kb.id} className="border-t border-line/50">
                  <Td>{kb.name}</Td>
                  <Td>{kb.description ?? "—"}</Td>
                  <Td mono>{kb.documentCount}</Td>
                  <Td><Badge label={kb.isActive ? "active" : "inactive"} tone={kb.isActive ? "mint" : "dim"} /></Td>
                  <Td>
                    <div className="flex gap-3">
                      <button onClick={() => setSelectedKb(kb)} className="font-mono text-[10px] uppercase text-dim hover:text-mint">documents</button>
                      <button
                        onClick={async () => { await knowledgeApi.deleteKb(kb.id); void refetch(); }}
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

      <Modal open={createModal} title="New knowledge base" onClose={() => setCreateModal(false)}>
        <div className="space-y-4">
          <Field label="Name"><TextInput value={name} onChange={(e) => setName(e.target.value)} placeholder="Product FAQ" /></Field>
          <Field label="Description"><TextInput value={description} onChange={(e) => setDescription(e.target.value)} /></Field>
          <ErrorBox error={formError} />
          <Btn onClick={createKb} disabled={busy || !name.trim()}>{busy ? "creating…" : "create"}</Btn>
        </div>
      </Modal>

      {selectedKb && (
        <KbDetailPanel kb={selectedKb} onClose={() => setSelectedKb(null)} />
      )}
    </div>
  );
}

function KbDetailPanel({ kb, onClose }: { kb: KnowledgeBaseListItem; onClose: () => void }) {
  const { data: docs, refetch } = useApi(() => knowledgeApi.documents(kb.id), [kb.id]);
  const [docName, setDocName] = useState("");
  const [content, setContent] = useState("");
  const [searchQuery, setSearchQuery] = useState("");
  const [results, setResults] = useState<SearchResultItem[] | null>(null);
  const [searching, setSearching] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const upload = async () => {
    setBusy(true); setError(null);
    try {
      await knowledgeApi.uploadDocument(kb.id, {
        name: docName,
        sourceUri: `inline://${docName}`,
        contentType: "text/plain",
        content,
      });
      setDocName(""); setContent("");
      void refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  const search = async () => {
    setSearching(true); setError(null);
    try {
      const r = await knowledgeApi.search(kb.id, searchQuery);
      setResults(r);
    } catch (e) {
      setError(e instanceof Error ? e.message : "search failed (embeddings may not be configured)");
    } finally { setSearching(false); }
  };

  return (
    <Modal open title={`Knowledge base — ${kb.name}`} onClose={onClose}>
      <div className="space-y-5">
        <ErrorBox error={error} />

        <Panel title="semantic search test">
          <div className="flex items-end gap-3">
            <div className="flex-1">
              <TextInput
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="ask a question…"
              />
            </div>
            <Btn onClick={search} disabled={searching || !searchQuery.trim()}>
              {searching ? "…" : "search"}
            </Btn>
          </div>
          {results && (
            <ul className="mt-4 space-y-2">
              {results.length === 0 ? <Empty>no matches</Empty> : results.map((r) => (
                <li key={r.chunkId} className="border border-line bg-deep/60 px-3 py-2">
                  <p className="font-mono text-[9px] uppercase tracking-[0.12em] text-dim">
                    {r.documentName} · score {typeof r.score === "number" ? r.score.toFixed(3) : r.score}
                  </p>
                  <p className="mt-1 text-xs leading-relaxed text-mist/90">{r.content}</p>
                </li>
              ))}
            </ul>
          )}
        </Panel>

        <Panel title={`documents (${docs?.length ?? 0})`}>
          {!docs || docs.length === 0 ? <Empty>no documents uploaded</Empty> : (
            <table className="w-full">
              <thead><tr><Th>Name</Th><Th>Chunks</Th><Th>Status</Th><Th /></tr></thead>
              <tbody>
                {(docs as KnowledgeDocumentDto[]).map((d) => (
                  <tr key={d.id} className="border-t border-line/50">
                    <Td>{d.name}</Td>
                    <Td mono>{d.chunkCount}</Td>
                    <Td><Badge label={d.status} tone={d.status === "ready" ? "mint" : "amber"} /></Td>
                    <Td>
                      <button
                        onClick={async () => { await knowledgeApi.deleteDocument(d.id); void refetch(); }}
                        className="font-mono text-[10px] uppercase text-dim hover:text-coral"
                      >delete</button>
                    </Td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </Panel>

        <Panel title="upload document">
          <div className="space-y-3">
            <Field label="Name"><TextInput value={docName} onChange={(e) => setDocName(e.target.value)} placeholder="refund-policy.txt" /></Field>
            <Field label="Content"><TextArea value={content} onChange={(e) => setContent(e.target.value)} placeholder="paste document text here — it will be chunked automatically" /></Field>
            <Btn onClick={upload} disabled={busy || !docName.trim() || !content.trim()}>
              {busy ? "uploading…" : "upload & chunk"}
            </Btn>
          </div>
        </Panel>
      </div>
    </Modal>
  );
}
