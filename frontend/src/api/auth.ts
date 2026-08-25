import { setAuthToken } from './client';

const TOKEN_KEY = 'tandem_access_token';

export function getStoredToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

export function storeToken(token: string | null) {
  try {
    if (token) localStorage.setItem(TOKEN_KEY, token);
    else localStorage.removeItem(TOKEN_KEY);
  } catch {
    // storage unavailable (private mode etc.) — session-only auth
  }
  setAuthToken(token);
}

export function bootstrapAuthToken() {
  setAuthToken(getStoredToken());
}
