export const HTTP_METHODS = {
  get: "GET",
  post: "POST",
  put: "PUT",
  patch: "PATCH",
  delete: "DELETE",
} as const;

export const HTTP_HEADERS = {
  authorization: "Authorization",
  contentType: "Content-Type",
} as const;

export const MEDIA_TYPES = {
  json: "application/json",
} as const;
