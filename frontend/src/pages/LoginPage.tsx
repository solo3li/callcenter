import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export default function LoginPage() {
  const { login, register, user, loading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation() as { state?: { from?: string } };

  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (!loading && user) {
    navigate(location.state?.from ?? '/dashboard', { replace: true });
  }

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      if (mode === 'login') await login(email, password);
      else await register(email, password, displayName || email.split('@')[0]);
      navigate(location.state?.from ?? '/dashboard', { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Authentication failed');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-ink">
      <div className="bg-grid" aria-hidden="true" />
      <div className="bg-tint" aria-hidden="true" />

      <div className="relative z-10 w-full max-w-md border border-line bg-panel/60 p-8">
        <div className="mb-8 flex items-center gap-2.5">
          <svg width="28" height="28" viewBox="0 0 32 32" aria-hidden="true">
            <circle cx="12" cy="16" r="8.5" fill="none" stroke="var(--color-mint)" strokeWidth="2.6" />
            <circle cx="20.5" cy="16" r="8.5" fill="none" stroke="var(--color-amber)" strokeWidth="2.6" opacity="0.85" />
          </svg>
          <span className="font-display text-xl font-bold tracking-tight text-mist">Tandem</span>
        </div>

        <p className="kicker mb-1">// operator access</p>
        <h1 className="mb-6 font-display text-2xl font-bold tracking-tight text-mist">
          {mode === 'login' ? 'Sign in to dashboard' : 'Create your account'}
        </h1>

        <form onSubmit={submit} className="space-y-4">
          {mode === 'register' && (
            <div>
              <label className="mb-1.5 block font-mono text-[10px] uppercase tracking-[0.16em] text-dim">
                Display name
              </label>
              <input
                type="text"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                placeholder="Jane Doe"
                className="w-full border border-line bg-deep px-4 py-2.5 font-mono text-xs text-mist placeholder:text-dim/60 focus:border-mint focus:outline-none"
              />
            </div>
          )}

          <div>
            <label className="mb-1.5 block font-mono text-[10px] uppercase tracking-[0.16em] text-dim">
              Email
            </label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@company.com"
              autoComplete="email"
              className="w-full border border-line bg-deep px-4 py-2.5 font-mono text-xs text-mist placeholder:text-dim/60 focus:border-mint focus:outline-none"
            />
          </div>

          <div>
            <label className="mb-1.5 block font-mono text-[10px] uppercase tracking-[0.16em] text-dim">
              Password
            </label>
            <input
              type="password"
              required
              minLength={6}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              className="w-full border border-line bg-deep px-4 py-2.5 font-mono text-xs text-mist placeholder:text-dim/60 focus:border-mint focus:outline-none"
            />
          </div>

          {error && (
            <p className="border border-coral/40 bg-coral/10 px-3 py-2 font-mono text-[11px] text-coral">
              {error}
            </p>
          )}

          <button type="submit" disabled={submitting} className="btn btn-mint w-full !py-3 disabled:opacity-50">
            {submitting ? 'Working…' : mode === 'login' ? 'Sign in' : 'Create account'}
          </button>
        </form>

        <button
          onClick={() => {
            setMode(mode === 'login' ? 'register' : 'login');
            setError(null);
          }}
          className="mt-4 w-full font-mono text-[10px] uppercase tracking-[0.14em] text-dim transition-colors hover:text-mint"
        >
          {mode === 'login' ? 'no account? register →' : '← back to sign in'}
        </button>

        <Link
          to="/"
          className="mt-6 flex items-center gap-2 font-mono text-[10px] uppercase tracking-[0.14em] text-dim transition-colors hover:text-mist"
        >
          <svg width="10" height="10" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.8">
            <path d="M11 1H1m3 3-3-3 3-3" />
          </svg>
          back to site
        </Link>
      </div>
    </div>
  );
}
