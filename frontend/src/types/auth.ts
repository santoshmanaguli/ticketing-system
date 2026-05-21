export type AuthUser = {
  id: number;
  fullName: string;
  email: string;
  role: string;
};

export type AuthSession = {
  user: AuthUser;
  token: string;
  expiresAt: string;
};

export type LoginForm = {
  email: string;
  password: string;
};

export type CreateAccountForm = {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
};
