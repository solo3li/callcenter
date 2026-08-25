import { PageHeader, Panel, ErrorBox, Empty, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { licensesApi, partnersApi } from "../../api/endpoints";
import type { LicenseDto, PartnerDto } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function BusinessPage() {
  const { data: licenses, error: licError } = useApi(() => licensesApi.list(), []);
  const { data: partners, error: partError } = useApi(() => partnersApi.list(), []);

  if (!API_ENABLED) return <Empty>set VITE_API_URL to view licenses & partners</Empty>;

  return (
    <div className="space-y-6">
      <PageHeader kicker="business" title="Licenses & partners" />

      <Panel title={`licenses (${licenses?.length ?? 0})`}>
        <ErrorBox error={licError} />
        {!licenses || licenses.length === 0 ? (
          <Empty>no licenses issued</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Id</Th><Th>Status</Th><Th>Starts</Th><Th>Ends</Th><Th>Limits</Th></tr></thead>
            <tbody>
              {(licenses as LicenseDto[]).map((l) => (
                <tr key={l.id} className="border-t border-line/50">
                  <Td mono>{l.id.slice(0, 8)}…</Td>
                  <Td><Badge label={l.status} tone={l.status === "Active" ? "mint" : "dim"} /></Td>
                  <Td mono>{new Date(l.startsAt).toLocaleDateString()}</Td>
                  <Td mono>{l.endsAt ? new Date(l.endsAt).toLocaleDateString() : "—"}</Td>
                  <Td mono>{l.limitsJson ? l.limitsJson.slice(0, 60) : "—"}</Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Panel title={`partners (${partners?.length ?? 0})`}>
        <ErrorBox error={partError} />
        {!partners || partners.length === 0 ? (
          <Empty>no partner accounts</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Name</Th><Th>Id</Th><Th>Raw</Th></tr></thead>
            <tbody>
              {(partners as PartnerDto[]).map((p) => (
                <tr key={String(p.id)} className="border-t border-line/50">
                  <Td>{p.name ?? "—"}</Td>
                  <Td mono>{String(p.id).slice(0, 8)}…</Td>
                  <Td mono>{JSON.stringify(p).slice(0, 70)}…</Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>
    </div>
  );
}
