namespace Boilerplate.API.Common;

/// <summary>
/// A machine-readable error returned to an API consumer.
/// </summary>
/// <param name="Code">Stable code used by clients to identify the error.</param>
/// <param name="Message">Safe, human-readable error message.</param>
/// <param name="Field">Related request field, when applicable.</param>
public sealed record ApiError(
    string Code,
    string Message,
    string? Field = null);

/// <summary>
/// Standard API response containing a payload.
/// The HTTP status code remains on the HTTP response and is not duplicated here.
/// </summary>
public sealed class ApiResponse<T>
{
    private ApiResponse()
    {
    }

    public bool Success { get; private init; }

    public string Message { get; private init; } = string.Empty;

    public T? Data { get; private init; }

    public IReadOnlyList<ApiError>? Errors { get; private init; }

    public string? TraceId { get; private init; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Fail(
        string message,
        IEnumerable<ApiError> errors,
        string? traceId = null,
        T? data = default) =>
        new()
        {
            Success = false,
            Message = message,
            Data = data,
            Errors = errors.ToArray(),
            TraceId = traceId
        };
}
