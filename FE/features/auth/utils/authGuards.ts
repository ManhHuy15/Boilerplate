import { ACCOUNT_ROLES } from "@/features/auth/constants/auth";
import type { Account, AuthSession } from "@/features/auth/types/auth";
import { isRecord } from "@/shared/lib/guards";

export function isAccount(value: unknown): value is Account {
  return (
    isRecord(value) &&
    typeof value.id === "number" &&
    typeof value.username === "string" &&
    (value.role === ACCOUNT_ROLES.user || value.role === ACCOUNT_ROLES.admin)
  );
}

export function isAuthSession(value: unknown): value is AuthSession {
  return (
    isRecord(value) &&
    typeof value.accessToken === "string" &&
    typeof value.accessTokenExpiresAtUtc === "string" &&
    isAccount(value.account)
  );
}

export function isNull(value: unknown): value is null {
  return value === null;
}
