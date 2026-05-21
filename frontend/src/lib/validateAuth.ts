import type { CreateAccountForm, LoginForm } from "../types/auth";

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function validateLoginForm(form: LoginForm): string | null {
  const email = form.email.trim();

  if (!email) {
    return "Email is required.";
  }

  if (!EMAIL_PATTERN.test(email)) {
    return "Enter a valid email address.";
  }

  if (!form.password) {
    return "Password is required.";
  }

  return null;
}

export function validateCreateAccountForm(form: CreateAccountForm): string | null {
  const fullName = form.fullName.trim();
  const email = form.email.trim();

  if (!fullName) {
    return "Full name is required.";
  }

  if (fullName.length < 2) {
    return "Full name must be at least 2 characters.";
  }

  if (!email) {
    return "Email is required.";
  }

  if (!EMAIL_PATTERN.test(email)) {
    return "Enter a valid email address.";
  }

  if (!form.password) {
    return "Password is required.";
  }

  if (form.password.length < 8) {
    return "Password must be at least 8 characters.";
  }

  if (form.password !== form.confirmPassword) {
    return "Passwords do not match.";
  }

  return null;
}
