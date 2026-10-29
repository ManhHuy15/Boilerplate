using System.Security.Claims;
using Boilerplate.API.Common;
using Boilerplate.Application.Services.AuthServices;
using Boilerplate.Application.Services.AuthServices.DTOs;
using Boilerplate.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Boilerplate.API.Controllers;

[Route("api/[controller]")]
public sealed class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IAuthService authService,
        IOptions<JwtSettings> jwtOptions)
    {
        _authService = authService;
        _jwtSettings = jwtOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<ApiResponse<AuthResponse>>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        AuthResponse response = await _authService.RegisterAsync(request, cancellationToken);
        WriteRefreshTokenCookie(response);

        return Created(null, response, "Account created successfully.");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        AuthResponse response = await _authService.LoginAsync(request, cancellationToken);
        WriteRefreshTokenCookie(response);

        return Success(response, "Signed in successfully.");
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshAsync(
        CancellationToken cancellationToken)
    {
        string refreshToken = ReadRefreshTokenCookie();
        AuthResponse response = await _authService.RefreshTokenAsync(
            refreshToken,
            cancellationToken);
        WriteRefreshTokenCookie(response);

        return Success(response, "Token refreshed successfully.");
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> LogoutAsync(
        CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(
                _jwtSettings.RefreshCookieName,
                out string? refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        }

        DeleteRefreshTokenCookie();
        return Success<object?>(null, "Signed out successfully.");
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<ApiResponse<AccountResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> GetMeAsync(
        CancellationToken cancellationToken)
    {
        string? username = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new UnauthorizedAccessException("Access token does not contain a valid username.");
        }

        AccountResponse account = await _authService.GetAccountAsync(
            username,
            cancellationToken);
        return Success(account);
    }

    private string ReadRefreshTokenCookie()
    {
        if (!Request.Cookies.TryGetValue(
                _jwtSettings.RefreshCookieName,
                out string? refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token is missing.");
        }

        return refreshToken;
    }

    private void WriteRefreshTokenCookie(AuthResponse response)
    {
        Response.Cookies.Append(
            _jwtSettings.RefreshCookieName,
            response.RefreshToken,
            CreateRefreshCookieOptions(response.RefreshTokenExpiresAtUtc));
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            _jwtSettings.RefreshCookieName,
            CreateRefreshCookieOptions(DateTimeOffset.UnixEpoch));
    }

    private CookieOptions CreateRefreshCookieOptions(DateTimeOffset expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _jwtSettings.RefreshCookieSecure,
            SameSite = ParseSameSiteMode(_jwtSettings.RefreshCookieSameSite),
            Path = "/api/auth",
            Expires = expiresAtUtc,
            IsEssential = true
        };
    }

    private static SameSiteMode ParseSameSiteMode(string value)
    {
        return Enum.TryParse(value, true, out SameSiteMode sameSiteMode)
            ? sameSiteMode
            : SameSiteMode.Strict;
    }
}
