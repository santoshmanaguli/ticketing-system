import { FormEvent, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AuthLayout, AuthLink } from "../components/AuthLayout";
import { useAuth } from "../context/AuthContext";
import { validateCreateAccountForm } from "../lib/validateAuth";
import type { CreateAccountForm } from "../types/auth";

const emptyForm: CreateAccountForm = {
  fullName: "",
  email: "",
  password: "",
  confirmPassword: ""
};

export default function CreateAccountPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState<CreateAccountForm>(emptyForm);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const validationError = validateCreateAccountForm(form);
    if (validationError) {
      setError(validationError);
      return;
    }

    try {
      setSubmitting(true);
      await register(form);
      navigate("/login", {
        replace: true,
        state: { message: "Account created. Sign in with your email and password." }
      });
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Unable to create account.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <AuthLayout
      title="Create account"
      subtitle="Register to work on Sparkle ERP support tickets."
      footer={
        <>
          Already registered? <AuthLink to="/login">Sign in</AuthLink>
        </>
      }
    >
      <form className="auth-form" onSubmit={handleSubmit}>
        {error && <div className="alert auth-alert">{error}</div>}

        <label>
          Full name
          <input
            autoComplete="name"
            value={form.fullName}
            onChange={(event) => setForm({ ...form, fullName: event.target.value })}
            placeholder="Aarav Mehta"
          />
        </label>

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
            autoComplete="new-password"
            type="password"
            value={form.password}
            onChange={(event) => setForm({ ...form, password: event.target.value })}
            placeholder="At least 8 characters"
          />
        </label>

        <label>
          Confirm password
          <input
            autoComplete="new-password"
            type="password"
            value={form.confirmPassword}
            onChange={(event) => setForm({ ...form, confirmPassword: event.target.value })}
            placeholder="Re-enter your password"
          />
        </label>

        <button className="primary-button auth-submit" type="submit" disabled={submitting}>
          {submitting ? "Creating account..." : "Create account"}
        </button>
      </form>
    </AuthLayout>
  );
}
