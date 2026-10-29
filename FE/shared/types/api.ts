export interface ApiError {
  code: string;
  message: string;
  field: string | null;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: ApiError[] | null;
  traceId: string | null;
}
