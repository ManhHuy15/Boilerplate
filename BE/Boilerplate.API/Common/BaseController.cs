using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.API.Common;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(
        T data,
        string message = "Success") =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse<T>> Created<T>(
        string? location,
        T data,
        string message = "Created successfully") =>
        base.Created(location, ApiResponse<T>.Ok(data, message));
}
