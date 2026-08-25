import type { ReactNode } from 'react';

export function PageHeader({ kicker, title, right }: { kicker: string; title: string; right?: ReactNode }) {
  return (
    <div className="flex flex-wrap items-end justify-between gap-3">
      <div>
        <p className="kicker mb-1">// {kicker}</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">{title}</h1>
      </div>
      {right}
    </div>
  );
}

export function Panel({ title, children, actions }: { title?: string; children: ReactNode; actions?: ReactNode }) {
  return (
    <div className="border border-line bg-panel/40 p-5">
      {(title || actions) && (
        <div className="mb-4 flex items-center justify-between gap-3">
          {title && (
            <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">{title}</p>
          )}
          {actions}
        </div>
      )}
      {children}
    </div>
  );
}

export function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="block">
      <span className="mb-1.5 block font-mono text-[10px] uppercase tracking-[0.16em] text-dim">{label}</span>
      {children}
    </label>
  );
}

const inputCls =
  'w-full border border-line bg-deep px-3 py-2.5 font-mono text-xs text-mist placeholder:text-dim/60 focus:border-mint focus:outline-none';

export function TextInput(props: React.InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={`${inputCls} ${props.className ?? ''}`} />;
}

export function TextArea(props: React.TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea {...props} className={`${inputCls} min-h-[90px] resize-y ${props.className ?? ''}`} />;
}

export function Select(props: React.SelectHTMLAttributes<HTMLSelectElement>) {
  return <select {...props} className={`${inputCls} !py-2.5 ${props.className ?? ''}`} />;
}

export function Btn({
  variant = 'mint',
  ...props
}: React.ButtonHTMLAttributes<HTMLButtonElement> & { variant?: 'mint' | 'ghost' | 'amber' | 'coral' }) {
  const v = variant === 'mint' ? 'btn-mint' : variant === 'amber' ? 'btn-amber' : variant === 'coral' ? 'btn-coral' : 'btn-ghost';
  return <button {...props} className={`btn ${v} !px-4 !py-2.5 !text-[10px] disabled:opacity-40 ${props.className ?? ''}`} />;
}

export function ErrorBox({ error }: { error: string | null }) {
  if (!error) return null;
  return <p className="border border-coral/40 bg-coral/10 px-4 py-3 font-mono text-[11px] text-coral">{error}</p>;
}

export function Empty({ children = 'nothing here yet' }: { children?: ReactNode }) {
  return <p className="py-6 text-center font-mono text-[11px] text-dim">{children}</p>;
}

export function Th({ children }: { children?: ReactNode }) {
  return (
    <th className="pb-2 pr-4 text-left font-mono text-[9px] uppercase tracking-[0.14em] text-dim">
      {children}
    </th>
  );
}

export function Td({ children, mono }: { children: ReactNode; mono?: boolean }) {
  return (
    <td className={`py-2 pr-4 align-top text-xs text-mist/90 ${mono ? 'font-mono text-[11px]' : ''}`}>
      {children}
    </td>
  );
}

export function Modal({ open, title, onClose, children }: { open: boolean; title: string; onClose: () => void; children: ReactNode }) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/70 p-6" onClick={onClose}>
      <div
        className="mt-10 w-full max-w-xl border border-line bg-panel p-6"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between">
          <p className="font-display text-lg font-bold text-mist">{title}</p>
          <Btn variant="ghost" onClick={onClose}>✕ close</Btn>
        </div>
        {children}
      </div>
    </div>
  );
}
