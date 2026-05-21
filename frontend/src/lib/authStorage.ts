import type { AuthSession } from "../types/auth";

const SESSION_KEY = "sparkle.session";

export function toSession(response: {
  token: string;
  expiresAt: string;
  user: { id: number; fullName: string; email: string; role: string };
}): AuthSession {
  return {
    token: response.token,
    expiresAt: response.expiresAt,
    user: {
      id: response.user.id,
      fullName: response.user.fullName,
      email: response.user.email,
      role: response.user.role
    }
  };
}

export function loadSession(): AuthSession | null {
  try {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) {
      return null;
    }

    const session = JSON.parse(raw) as AuthSession;
    if (!session.token || !session.user) {
      return null;
    }

    if (session.expiresAt && new Date(session.expiresAt).getTime() <= Date.now()) {
      clearSession();
      return null;
    }

    return session;
  } catch {
    return null;
  }
}

export function saveSession(session: AuthSession) {
  localStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

export function clearSession() {
  localStorage.removeItem(SESSION_KEY);
}

export function getAuthToken(): string | null {
  return loadSession()?.token ?? null;
}
