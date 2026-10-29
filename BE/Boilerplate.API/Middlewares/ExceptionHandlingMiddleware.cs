using Boilerplate.API.Common;

namespace Boilerplate.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request {TraceId} was cancelled by the client.", context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            await WriteErrorResponseAsync(context, exception);
        }
    }

    private async Task WriteErrorResponseAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, code, message) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "validation_error", exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "unauthorized", "Authentication is required."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "not_found", exception.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, "conflict", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "internal_error", "An unexpected error occurred.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception for request {TraceId}.", context.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(exception, "Request {TraceId} failed with {StatusCode}.", context.TraceIdentifier, statusCode);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        ApiResponse<object?> response = ApiResponse<object?>.Fail(
            message,
            [new ApiError(code, message)],
            context.TraceIdentifier);

        await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }
}
