import type { ApiError, ApiResponse } from "@/shared/types/api";
import { isNullableString, isRecord } from "@/shared/lib/guards";
import { HTTP_HEADERS, MEDIA_TYPES } from "@/shared/constants/http";

type DataValidator<T> = (value: unknown) => value is T;
type UnauthorizedHandler = (rejectedAccessToken: string) => Promise<string | null>;

interface RequestOptions extends Omit<RequestInit, "headers"> {
  accessToken?: string;
  headers?: Record<string, string>;
  onUnauthorized?: UnauthorizedHandler;
}

export class ApiClientError extends Error {
  public readonly status: number;
  public readonly errors: ApiError[];
  public readonly traceId: string | null;

  public constructor(
    message: string,
    status: number,
    errors: ApiError[] = [],
    traceId: string | null = null,
  ) {
    super(message);
    this.name = "ApiClientError";
    this.status = status;
    this.errors = errors;
    this.traceId = traceId;
  }
}

function isApiError(value: unknown): value is ApiError {
  return (
    isRecord(value) &&
    typeof value.code === "string" &&
    typeof value.message === "string" &&
    isNullableString(value.field)
  );
}

function parseEnvelope<T>(
  value: unknown,
  validateData: DataValidator<T>,
): ApiResponse<T> | null {
  if (
    !isRecord(value) ||
    typeof value.success !== "boolean" ||
    typeof value.message !== "string" ||
    !isNullableString(value.traceId)
  ) {
    return null;
  }

  const errors = value.errors;
  if (errors !== null && (!Array.isArray(errors) || !errors.every(isApiError))) {
    return null;
  }

  if (value.data !== null && !validateData(value.data)) {
    return null;
  }

  return {
    success: value.success,
    message: value.message,
    data: value.data,
    errors,
    traceId: value.traceId,
  };
}

async function readJson(response: Response): Promise<unknown> {
  const contentType = response.headers.get(HTTP_HEADERS.contentType);
  if (!contentType?.includes(MEDIA_TYPES.json)) {
    return null;
  }

  try {
    return await response.json();
  } catch {
    return null;
  }
}

export async function apiRequest<T>(
  path: string,
  validateData: DataValidator<T>,
  options: RequestOptions = {},
): Promise<ApiResponse<T>> {
  const {
    accessToken,
    headers: customHeaders,
    onUnauthorized,
    ...requestInit
  } = options;
  const headers = new Headers(customHeaders);

  if (requestInit.body !== undefined && !headers.has(HTTP_HEADERS.contentType)) {
    headers.set(HTTP_HEADERS.contentType, MEDIA_TYPES.json);
  }

  if (accessToken) {
    headers.set(HTTP_HEADERS.authorization, `Bearer ${accessToken}`);
  }

  let response: Response;
  try {
    response = await fetch(path, {
      ...requestInit,
      headers,
      credentials: "include",
      cache: "no-store",
    });
  } catch {
    throw new ApiClientError(
      "Không thể kết nối tới máy chủ. Vui lòng kiểm tra Backend đang chạy.",
      0,
    );
  }

  if (response.status === 401 && accessToken && onUnauthorized) {
    const refreshedAccessToken = await onUnauthorized(accessToken);

    if (refreshedAccessToken) {
      return apiRequest(path, validateData, {
        ...requestInit,
        accessToken: refreshedAccessToken,
        headers: customHeaders,
      });
    }
  }

  const payload = await readJson(response);
  const envelope = parseEnvelope(payload, validateData);

  if (!envelope) {
    throw new ApiClientError(
      response.ok
        ? "Máy chủ trả về dữ liệu không đúng định dạng."
        : `Yêu cầu thất bại (${response.status}).`,
      response.status,
    );
  }

  if (!response.ok || !envelope.success) {
    throw new ApiClientError(
      envelope.message || "Yêu cầu không thành công.",
      response.status,
      envelope.errors ?? [],
      envelope.traceId,
    );
  }

  return envelope;
}
