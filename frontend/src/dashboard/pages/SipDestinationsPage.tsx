import { useState } from "react";
import { PageHeader, Panel, Btn, TextInput, Field, ErrorBox, Empty, Modal, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { sipDestinationsApi, personasApi, personaRoutingApi } from "../../api/endpoints";
import type { SipDestinationItem } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function SipDestinationsPage() {
  const { data: destinations, loading, error, refetch } = useApi(
    () => sipDestinationsApi.list(), []);
  const { data: personas } = useApi(() => personasApi.list(), []);
  const { data: routing, refetch: refetchRouting } = useApi(
    () => personaRoutingApi.getDefault(), []);

  const [modal, setModal] = useState(false);
  const [name, setName] = useState("");
  const [callTo, setCallTo] = useState("");
  const [description, setDescription] = useState("");
  const [busy, setBusy] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  if (!API_ENABLED) return <Empty>set VITE_API_URL to manage SIP destinations</Empty>;

  const create = async () => {
    setBusy(true); setFormError(null);
    try {
      await sipDestinationsApi.create({ name, callTo, description: description || undefined });
      setName(""); setCallTo(""); setDescription("");
      setModal(false);
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    } finally { setBusy(false); }
  };

  const toggle = async (d: SipDestinationItem) => {
    try {
      await sipDestinationsApi.update(d.id, { isEnabled: !d.isEnabled });
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    }
  };

  const remove = async (id: string) => {
    try {
      await sipDestinationsApi.remove(id);
      void refetch();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    }
  };

  const setDefaultPersona = async (personaId: string | null) => {
    try {
      await personaRoutingApi.setDefault(personaId);
      void refetchRouting();
    } catch (e) {
      setFormError(e instanceof Error ? e.message : "failed");
    }
  };

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="platform setup"
        title="SIP Destinations"
        right={<Btn onClick={() => { setFormError(null); setModal(true); }}>+ new destination</Btn>}
      />
      <ErrorBox error={error ?? formError} />

      <Panel>
        <div className="p-4 border-b border-line/50">
          <Field label="Default AI Persona (inbound calls)">
            <select
              className="bg-transparent border border-line rounded px-2 py-1"
              value={routing?.defaultPersonaId ?? ""}
              onChange={(e) => setDefaultPersona(e.target.value || null)}
            >
              <option value="">— none (calls will be dropped) —</option>
              {(personas ?? []).map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </Field>
          <p className="text-xs opacity-60 mt-2">
            Inbound SIP calls are answered by this persona. Transfer targets below are announced to the AI by name only.
          </p>
        </div>

        {loading ? <Empty>loading…</Empty> : !destinations || destinations.length === 0 ? (
          <Empty>no destinations yet — add logical targets like Support or Sales</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Name</Th><Th>Target</Th><Th>Status</Th><Th /></tr></thead>
            <tbody>
              {(destinations as SipDestinationItem[]).map((d) => (
                <tr key={d.id} className="border-t border-line/50">
                  <Td>{d.name}</Td>
                  <Td mono>{d.callTo}</Td>
                  <Td>
                    <Badge label={d.isEnabled ? "Enabled" : "Disabled"}
                      tone={d.isEnabled ? "mint" : "coral"} />
                  </Td>
                  <Td>
                    <div className="flex gap-2 justify-end">
                      <Btn onClick={() => toggle(d)}>{d.isEnabled ? "Disable" : "Enable"}</Btn>
                      <Btn onClick={() => remove(d.id)}>Delete</Btn>
                    </div>
                  </Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      {modal && (
        <Modal open title="New SIP destination" onClose={() => setModal(false)}>
          <div className="space-y-3">
            <Field label="Name (spoken by the AI)">
              <TextInput value={name} onChange={(e) => setName(e.target.value)}
                placeholder="Support" />
            </Field>
            <Field label="PBX target (never shown to the AI)">
              <TextInput value={callTo} onChange={(e) => setCallTo(e.target.value)}
                placeholder="e.g. support-queue or 7001" />
            </Field>
            <Field label="Description (optional)">
              <TextInput value={description} onChange={(e) => setDescription(e.target.value)}
                placeholder="Support ring group on Issabel" />
            </Field>
            <ErrorBox error={formError} />
            <div className="flex gap-2 justify-end">
              <Btn onClick={() => setModal(false)}>Cancel</Btn>
              <Btn disabled={busy || !name || !callTo} onClick={create}>Create</Btn>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
