import { useNavigate } from "react-router-dom";
import { PageHeader, Panel, ErrorBox, Empty, Th, Td } from "../components/ui";
import Badge from "../components/Badge";
import { statsApi } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";
import { apiToUiStatus } from "../statusMap";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function QueuePage() {
  const navigate = useNavigate();
  const { data, error, loading, refetch } = useApi(
    () => (API_ENABLED ? statsApi.queue() : Promise.resolve(null)),
    []
  );

  return (
    <div className="space-y-6">
      <PageHeader
        kicker="queue monitor"
        title="Live queue"
        right={
          <button onClick={() => void refetch()} className="btn btn-ghost !px-4 !py-2.5 !text-[10px]">
            ↻ refresh
          </button>
        }
      />

      <ErrorBox error={error} />

      <div className="grid gap-4 sm:grid-cols-2">
        <Panel>
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Active calls</p>
          <p className="mt-1 font-display text-4xl font-bold tabular-nums text-mint">
            {data?.activeCount ?? 0}
          </p>
        </Panel>
        <Panel>
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Agents online</p>
          <p className="mt-1 font-display text-4xl font-bold tabular-nums text-amber">
            {data?.agentsOnline ?? 0}
          </p>
        </Panel>
      </div>

      <Panel title="active calls">
        {loading ? (
          <Empty>loading…</Empty>
        ) : !data || data.activeCalls.length === 0 ? (
          <Empty>no active calls right now</Empty>
        ) : (
          <table className="w-full">
            <thead>
              <tr><Th>Room</Th><Th>Status</Th><Th>Duration</Th><Th /></tr>
            </thead>
            <tbody>
              {data.activeCalls.map((c) => (
                <tr key={c.id} className="border-t border-line/50">
                  <Td mono>{c.roomName}</Td>
                  <Td>
                    <Badge label={apiToUiStatus(c.status)} tone={c.status === "Transferred" ? "amber" : "mint"} />
                  </Td>
                  <Td>{c.durationSeconds}s</Td>
                  <Td>
                    <button
                      onClick={() => navigate(`/dashboard/call/${c.id}`)}
                      className="font-mono text-[10px] uppercase tracking-[0.12em] text-mint hover:text-mist"
                    >
                      inspect →
                    </button>
                  </Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Panel title="agents">
        {!data || data.agents.length === 0 ? (
          <Empty>no registered agents</Empty>
        ) : (
          <table className="w-full">
            <thead>
              <tr><Th>Name</Th><Th>Status</Th></tr>
            </thead>
            <tbody>
              {data.agents.map((a) => (
                <tr key={a.id} className="border-t border-line/50">
                  <Td>{a.name}</Td>
                  <Td>
                    <Badge label={a.status} tone={a.status === "Available" ? "mint" : "dim"} />
                  </Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>
    </div>
  );
}
