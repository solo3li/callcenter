import { useState } from "react";
import { PageHeader, Panel, ErrorBox, Empty, Th, Td } from "../components/ui";
import { usageApi } from "../../api/endpoints";
import type { UsageRecordDto, UsageSummaryDto } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function UsagePage() {
  const [metricFilter, setMetricFilter] = useState("");
  const { data: summary, error: sumErr } = useApi(() => usageApi.summary(), []);
  const { data: records, loading, error } = useApi(
    () => usageApi.records(metricFilter ? { metricType: metricFilter } : undefined),
    [metricFilter]
  );

  if (!API_ENABLED) return <Empty>set VITE_API_URL to view usage</Empty>;

  return (
    <div className="space-y-6">
      <PageHeader kicker="business" title="Usage & metering" />

      <ErrorBox error={sumErr ?? error} />

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {(summary as UsageSummaryDto[] | null)?.map((s) => (
          <Panel key={s.metricType}>
            <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">{s.metricType}</p>
            <p className="mt-1 font-display text-3xl font-bold tabular-nums text-mint">
              {Number(s.totalQuantity).toLocaleString(undefined, { maximumFractionDigits: 2 })}
              <span className="ml-1.5 text-xs font-normal text-dim">{s.unit}</span>
            </p>
            <p className="mt-0.5 font-mono text-[9px] text-dim">{s.count} events</p>
          </Panel>
        ))}
        {!summary || summary.length === 0 ? (
          <Panel><Empty>no usage recorded yet</Empty></Panel>
        ) : null}
      </div>

      <Panel
        title="usage events"
        actions={
          <input
            value={metricFilter}
            onChange={(e) => setMetricFilter(e.target.value)}
            placeholder="filter by metric…"
            className="border border-line bg-deep px-3 py-1.5 font-mono text-[10px] text-mist placeholder:text-dim/60 focus:border-mint focus:outline-none"
          />
        }
      >
        {loading ? <Empty>loading…</Empty> : !records || records.length === 0 ? (
          <Empty>no usage events</Empty>
        ) : (
          <table className="w-full">
            <thead><tr><Th>Metric</Th><Th>Qty</Th><Th>Unit</Th><Th>Occurred</Th><Th>Call</Th></tr></thead>
            <tbody>
              {(records as UsageRecordDto[]).slice(0, 50).map((r) => (
                <tr key={r.id} className="border-t border-line/50">
                  <Td mono>{r.metricType}</Td>
                  <Td mono>{r.quantity}</Td>
                  <Td>{r.unit}</Td>
                  <Td mono>{new Date(r.occurredAt).toLocaleString()}</Td>
                  <Td mono>{r.callSessionId?.slice(0, 8) ?? "—"}</Td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>
    </div>
  );
}
