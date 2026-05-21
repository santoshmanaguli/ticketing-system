import type { CreateAccountForm, LoginForm } from "../types/auth";

export type AuthUserResponse = {
  id: number;
  fullName: string;
  email: string;
  role: string;
};

export type AuthResponse = {
  token: string;
  expiresAt: string;
  user: AuthUserResponse;
};

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5001/api";

async function authRequest<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...init?.headers
    },
    ...init
  });

  if (!response.ok) {
    let message = `Request failed with ${response.status}`;

    try {
      const error = await response.json();
      message = error.message ?? message;
    } catch {
      // Keep the generic HTTP message if the response is not JSON.
    }

    throw new Error(message);
  }

  return response.json() as Promise<T>;
}

export function registerAccount(form: CreateAccountForm): Promise<AuthResponse> {
  return authRequest<AuthResponse>("/auth/register", {
    method: "POST",
    body: JSON.stringify({
      fullName: form.fullName.trim(),
      email: form.email.trim(),
      password: form.password
    })
  });
}

export function loginAccount(form: LoginForm): Promise<AuthResponse> {
  return authRequest<AuthResponse>("/auth/login", {
    method: "POST",
    body: JSON.stringify({
      email: form.email.trim(),
      password: form.password
    })
  });
}

export function getCurrentUser(token: string): Promise<AuthUserResponse> {
  return authRequest<AuthUserResponse>("/auth/me", {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
}
