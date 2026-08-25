import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { authApi } from '../api/endpoints';
import type { UserDto } from '../api/endpoints';
import { bootstrapAuthToken, getStoredToken, storeToken } from '../api/auth';

interface AuthState {
  user: UserDto | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(true);

  const applyAuth = useCallback((token: string) => {
    storeToken(token);
  }, []);

  useEffect(() => {
    bootstrapAuthToken();
    const token = getStoredToken();
    if (!token) {
      setLoading(false);
      return;
    }
    authApi
      .me()
      .then(setUser)
      .catch(() => storeToken(null))
      .finally(() => setLoading(false));
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      const res = await authApi.login(email, password);
      applyAuth(res.accessToken);
      setUser(
        await authApi.me().catch(() => ({
          id: res.user.id,
          email: res.user.email,
          displayName: res.user.displayName,
          companyName: res.user.companyName ?? null,
          status: 'Active',
          isPartner: false,
          standardCredits: 0,
          premiumCredits: 0,
          createdAt: new Date().toISOString(),
        }))
      );
    },
    [applyAuth]
  );

  const register = useCallback(
    async (email: string, password: string, displayName: string) => {
      const res = await authApi.register({ email, password, displayName });
      applyAuth(res.accessToken);
      setUser(
        await authApi.me().catch(() => ({
          id: res.user.id,
          email: res.user.email,
          displayName: res.user.displayName,
          companyName: res.user.companyName ?? null,
          status: 'Active',
          isPartner: false,
          standardCredits: 0,
          premiumCredits: 0,
          createdAt: new Date().toISOString(),
        }))
      );
    },
    [applyAuth]
  );

  const logout = useCallback(() => {
    storeToken(null);
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>');
  return ctx;
}
