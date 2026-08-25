const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000';

const TOKEN_KEY = 'tandem_access_token';
let authToken: string | null = null;
let refreshInFlight: Promise<boolean> | null = null;

export function getStoredToken(): string | null {
  if (authToken) return authToken;
  try {
    return localStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

export function setAuthToken(token: string | null) {
  authToken = token;
  try {
    if (token) localStorage.setItem(TOKEN_KEY, token);
    else localStorage.removeItem(TOKEN_KEY);
  } catch {
    // storage unavailable (private mode etc.) — session-only auth
  }
}

export function bootstrapAuthToken() {
  authToken = getStoredToken();
}

function isAuthPath(path: string): boolean {
  return (
    path.startsWith('/api/auth/login') ||
    path.startsWith('/api/auth/register') ||
    path.startsWith('/api/auth/refresh') ||
    path.startsWith('/api/auth/agent-login')
  );
}

async function tryRefreshToken(): Promise<boolean> {
  const current = getStoredToken();
  if (!current) return false;

  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      try {
        const res = await fetch(`${API_BASE}/api/auth/refresh`, {
          method: 'POST',
          headers: { Authorization: `Bearer ${current}` },
        });
        if (!res.ok) return false;
        const data = await res.json();
        if (!data?.accessToken) return false;
        setAuthToken(data.accessToken);
        return true;
      } catch {
        return false;
      } finally {
        setTimeout(() => { refreshInFlight = null; }, 0);
      }
    })();
  }
  return refreshInFlight;
}

async function extractError(res: Response): Promise<string> {
  const body = await res.json().catch(() => null);
  if (!body) return `HTTP ${res.status}`;

  // ASP.NET ValidationProblemDetails: { title, errors: { Field: [msg, ...] } }
  if (body.errors && typeof body.errors === 'object') {
    const msgs = Object.values(body.errors as Record<string, string[]>).flat();
    if (msgs.length) return msgs.join(' ');
  }
  return body.error || body.title || body.message || `HTTP ${res.status}`;
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const doFetch = async (): Promise<Response> => {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options?.headers as Record<string, string>),
    };
    const token = getStoredToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    return fetch(`${API_BASE}${path}`, { ...options, headers });
  };

  let res = await doFetch();

  // Transparent access-token rotation: one refresh + one retry per request.
  if (res.status === 401 && !isAuthPath(path)) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      res = await doFetch();
    } else {
      setAuthToken(null);
    }
  }

  if (!res.ok) {
    throw new Error(await extractError(res));
  }
  return res.json();
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'POST', body: body ? JSON.stringify(body) : undefined }),
  patch: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'PATCH', body: body ? JSON.stringify(body) : undefined }),
  del: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
};

export default api;
