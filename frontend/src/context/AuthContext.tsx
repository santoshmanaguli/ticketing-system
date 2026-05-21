import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode
} from "react";
import { loginAccount, registerAccount } from "../api/auth";
import { clearSession, loadSession, saveSession, toSession } from "../lib/authStorage";
import type { AuthSession, CreateAccountForm, LoginForm } from "../types/auth";

type AuthContextValue = {
  session: AuthSession | null;
  isAuthenticated: boolean;
  login: (form: LoginForm) => Promise<void>;
  register: (form: CreateAccountForm) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(() => loadSession());

  const login = useCallback(async (form: LoginForm) => {
    const response = await loginAccount(form);
    const nextSession = toSession(response);
    saveSession(nextSession);
    setSession(nextSession);
  }, []);

  const register = useCallback(async (form: CreateAccountForm) => {
    await registerAccount(form);
  }, []);

  const logout = useCallback(() => {
    clearSession();
    setSession(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isAuthenticated: session !== null,
      login,
      register,
      logout
    }),
    [session, login, register, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within AuthProvider.");
  }

  return context;
}
