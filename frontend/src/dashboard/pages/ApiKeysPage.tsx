import { useState } from "react";
import { PageHeader, Panel, Btn, TextInput, Field, ErrorBox, Empty, Modal, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { apiKeysApi } from "../../api/endpoints";
import type { ApiKeyListItem, CreateApiKeyResponse } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function ApiKeysPage() {
  const { data: keys, loading, error, refetch } = useApi(() => apiKeysApi.list(), []);
  const [modal, setModal] = useState(false);
  const [name, setName] = useState("");
  const [scopes, setScopes] = useState("calls:read,calls:write");
  const [created, setCreated] = useState<CreateApiKeyResponse | null>(null);
  const [busy, setBusy] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  if (!API_ENABLED) return <Empty>set VITE_API_URL to manage API keys</Empty>;

  const create = async () => {
    setBusy(true); setFormError(null);
    try {
      const res = await apiKeysApi.create({
        name,
        scopes: scopes.split(",").map((s) => s.trim()).filter(Boolean),
      });
      setCreated(res);
      setName("");
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="business"
        title="API keys"
        right={<Btn onClick={() => { setCreated(null); setModal(true); }}>+ new key</Btn>}
      />
      <ErrorBox error={error} />

      <Panel>
        {loading ? <Empty>loading…</Empty> : !keys || keys.length === 0 ? (
          <Empty>no API keys — create one to call the platform API programmatically</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Name</Th><Th>Prefix</Th><Th>Scopes</Th><Th>Status</Th><Th>Last used</Th><Th /></tr></thead>
            <tbody>
              {(keys as ApiKeyListItem[]).map((k) => (
                <tr key={k.id} className="border-t border-line/50">
                  <Td>{k.name}</Td>
                  <Td mono>{k.keyPrefix}…</Td>
                  <Td mono>{k.scopes.join(", ") || "—"}</Td>
                  <Td><Badge label={k.status} tone={k.status === "Active" ? "mint" : "coral"} /></Td>
                  <Td mono>{k.lastUsedAt ? new Date(k.lastUsedAt).toLocaleDateString() : "never"}</Td>
                  <Td>
                    {k.status === "Active" && (
                      <button
                        onClick={async () => { await apiKeysApi.revoke(k.id); void refetch(); }}
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

      <Modal open={modal} title="Create API key" onClose={() => { setModal(false); setCreated(null); }}>
        {created ? (
          <div className="space-y-4">
            <p className="font-mono text-[11px] leading-relaxed text-amber">
              ⚠ copy this key now — it is shown only once.
            </p>
            <pre className="overflow-x-auto border border-mint/40 bg-deep p-3 font-mono text-xs text-mint">{created.rawKey}</pre>
            <p className="font-mono text-[10px] text-dim">prefix: {created.keyPrefix}</p>
          </div>
        ) : (
          <div className="space-y-4">
            <Field label="Name"><TextInput value={name} onChange={(e) => setName(e.target.value)} placeholder="production integration" /></Field>
            <Field label="Scopes (comma-separated)"><TextInput value={scopes} onChange={(e) => setScopes(e.target.value)} /></Field>
            <ErrorBox error={formError} />
            <Btn onClick={create} disabled={busy || !name.trim()}>{busy ? "creating…" : "create key"}</Btn>
          </div>
        )}
      </Modal>
    </div>
  );
}
