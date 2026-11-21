import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { ACCOUNT_ROLES, AUTH_CONSTRAINTS } from "@/features/auth/constants/auth";
import { isAccount, isAuthSession, isNull } from "@/features/auth/utils/authGuards";
import { ApiClientError, apiRequest } from "@/shared/lib/apiClient";
import { isNullableString, isRecord } from "@/shared/lib/guards";

describe("Runtime boundary guards (High-Trust Data Validation)", () => {
  it("validates primitive and record boundaries strictly", () => {
    assert.equal(isRecord({ key: "value" }), true);
    assert.equal(isRecord(null), false);
    assert.equal(isRecord(["array"]), false);

    assert.equal(isNullableString("text"), true);
    assert.equal(isNullableString(null), true);
    assert.equal(isNullableString(123), false);
    assert.equal(isNull(null), true);
    assert.equal(isNull(undefined), false);
  });

  it("accepts only valid Account and AuthSession contracts and rejects malformed payloads", () => {
    const validAccount = {
      id: 1,
      username: "minhanh",
      role: ACCOUNT_ROLES.user,
    };

    assert.equal(isAccount(validAccount), true);
    assert.equal(isAccount({ ...validAccount, role: "SuperAdmin" }), false);
    assert.equal(isAccount({ ...validAccount, id: "1" }), false);

    const validSession = {
      accessToken: "jwt-token",
      accessTokenExpiresAtUtc: "2026-11-21T10:00:00Z",
      account: validAccount,
    };

    assert.equal(isAuthSession(validSession), true);
    assert.equal(
      isAuthSession({
        ...validSession,
        accessTokenExpiresAtUtc: 12345,
      }),
      false,
    );
  });

  it("keeps FE auth constraints synchronized with BE DataAnnotations", () => {
    assert.equal(AUTH_CONSTRAINTS.username.minLength, 3);
    assert.equal(AUTH_CONSTRAINTS.username.maxLength, 100);
    assert.equal(AUTH_CONSTRAINTS.password.minLength, 8);
    assert.equal(AUTH_CONSTRAINTS.password.maxLength, 128);
  });
});

describe("apiRequest envelope & 401 token refresh orchestration", () => {
  it("retries automatically with refreshed token when 401 is encountered", async () => {
    const originalFetch = globalThis.fetch;
    const seenTokens: string[] = [];

    globalThis.fetch = (async (_input: RequestInfo | URL, init?: RequestInit) => {
      const headers = new Headers(init?.headers);
      const auth = headers.get("Authorization") ?? "";
      seenTokens.push(auth);

      if (auth === "Bearer expired-token") {
        return new Response(
          JSON.stringify({
            success: false,
            message: "Authentication is required.",
            data: null,
            errors: [{ code: "unauthorized", message: "Expired", field: null }],
            traceId: "trace-401",
          }),
          {
            status: 401,
            headers: { "Content-Type": "application/json" },
          },
        );
      }

      return new Response(
        JSON.stringify({
          success: true,
          message: "Success",
          data: { id: 7, username: "alice", role: ACCOUNT_ROLES.admin },
          errors: null,
          traceId: "trace-200",
        }),
        {
          status: 200,
          headers: { "Content-Type": "application/json" },
        },
      );
    }) as typeof fetch;

    try {
      const result = await apiRequest("/api/auth/me", isAccount, {
        accessToken: "expired-token",
        onUnauthorized: async (rejectedToken) => {
          assert.equal(rejectedToken, "expired-token");
          return "fresh-token";
        },
      });

      assert.equal(result.success, true);
      assert.equal(result.data?.username, "alice");
      assert.deepEqual(seenTokens, ["Bearer expired-token", "Bearer fresh-token"]);
    } finally {
      globalThis.fetch = originalFetch;
    }
  });

  it("throws ApiClientError when server returns payload violating the runtime validator", async () => {
    const originalFetch = globalThis.fetch;

    globalThis.fetch = (async () =>
      new Response(
        JSON.stringify({
          success: true,
          message: "Success",
          data: { id: "wrong-type", username: 123 },
          errors: null,
          traceId: "trace-bad-shape",
        }),
        {
          status: 200,
          headers: { "Content-Type": "application/json" },
        },
      )) as typeof fetch;

    try {
      await assert.rejects(
        () => apiRequest("/api/auth/me", isAccount),
        (error: unknown) =>
          error instanceof ApiClientError &&
          error.status === 200 &&
          error.message.includes("không đúng định dạng"),
      );
    } finally {
      globalThis.fetch = originalFetch;
    }
  });
});
