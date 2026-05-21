import { Link } from "react-router-dom";
import type { ReactNode } from "react";

type AuthLayoutProps = {
  title: string;
  subtitle: string;
  children: ReactNode;
  footer: ReactNode;
};

export function AuthLayout({ title, subtitle, children, footer }: AuthLayoutProps) {
  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-brand">
          <p className="eyebrow">Sparkle ERP Support</p>
          <h1>{title}</h1>
          <p className="auth-subtitle">{subtitle}</p>
        </div>

        {children}

        <div className="auth-footer">{footer}</div>
      </div>

      <p className="auth-note">
        Accounts are stored in SQL Server. Seed users share password <strong>Sparkle@123</strong>.
      </p>
    </div>
  );
}

export function AuthLink({ to, children }: { to: string; children: ReactNode }) {
  return (
    <Link className="auth-link" to={to}>
      {children}
    </Link>
  );
}
