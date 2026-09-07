/**
 * Simple authentication context for the admin portal.
 *
 * For the hackathon prototype this uses a lightweight token stored in
 * sessionStorage. The username/password is checked against a small set
 * of demo credentials. Replace with real backend auth when available.
 */

import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';

interface AuthState {
  isAuthenticated: boolean;
  user: { name: string; role: string } | null;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => void;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

// Demo credentials — replace with real backend auth in production.
const DEMO_USERS: Record<string, { password: string; name: string; role: string }> = {
  admin: { password: 'admin123', name: 'Admin Officer', role: 'Administrator' },
  trainer: { password: 'trainer123', name: 'Lead Trainer', role: 'Trainer' },
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<{ name: string; role: string } | null>(() => {
    const stored = sessionStorage.getItem('surakshaar_user');
    return stored ? JSON.parse(stored) : null;
  });

  const login = useCallback(async (username: string, password: string): Promise<boolean> => {
    // Simulate network delay
    await new Promise((r) => setTimeout(r, 400));
    const entry = DEMO_USERS[username.trim().toLowerCase()];
    if (entry && entry.password === password) {
      const u = { name: entry.name, role: entry.role };
      sessionStorage.setItem('surakshaar_user', JSON.stringify(u));
      setUser(u);
      return true;
    }
    return false;
  }, []);

  const logout = useCallback(() => {
    sessionStorage.removeItem('surakshaar_user');
    setUser(null);
  }, []);

  const value = useMemo<AuthState>(
    () => ({ isAuthenticated: !!user, user, login, logout }),
    [user, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
