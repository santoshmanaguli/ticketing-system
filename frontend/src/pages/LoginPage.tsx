import { FormEvent, useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { AuthLayout, AuthLink } from "../components/AuthLayout";
import { useAuth } from "../context/AuthContext";
import { validateLoginForm } from "../lib/validateAuth";
import type { LoginForm } from "../types/auth";

const emptyForm: LoginForm = {
  email: "",
  password: ""
};

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [form, setForm] = useState<LoginForm>(emptyForm);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const redirectTo = (location.state as { from?: string; message?: string } | null)?.from ?? "/";

  useEffect(() => {
    const message = (location.state as { message?: string } | null)?.message;
    if (message) {
      setSuccess(message);
    }
  }, [location.state]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const validationError = validateLoginForm(form);
    if (validationError) {
      setError(validationError);
      return;
    }

    try {
      setSubmitting(true);
      await login(form);
      navigate(redirectTo, { replace: true });
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Unable to sign in.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <AuthLayout
      title="Sign in"
      subtitle="Access the Sparkle support ticketing workspace."
      footer={
        <>
          New here? <AuthLink to="/create-account">Create an account</AuthLink>
        </>
      }
    >
      <form className="auth-form" onSubmit={handleSubmit}>
        {success && <div className="alert auth-success">{success}</div>}
        {error && <div className="alert auth-alert">{error}</div>}

        <label>
          Email
          <input
            autoComplete="email"
            type="email"
            value={form.email}
            onChange={(event) => setForm({ ...form, email: event.target.value })}
            placeholder="you@company.com"
          />
        </label>

        <label>
          Password
          <input
            autoComplete="current-password"
            type="password"
            value={form.password}
            onChange={(event) => setForm({ ...form, password: event.target.value })}
            placeholder="Enter your password"
          />
        </label>

        <button className="primary-button auth-submit" type="submit" disabled={submitting}>
          {submitting ? "Signing in..." : "Sign in"}
        </button>
      </form>
    </AuthLayout>
  );
}
