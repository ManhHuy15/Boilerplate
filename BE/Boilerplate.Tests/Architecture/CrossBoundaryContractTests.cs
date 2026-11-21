using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Boilerplate.API.Common;
using Boilerplate.Application.Services.AuthServices.DTOs;
using Boilerplate.Domain.Enums;
using Xunit;

namespace Boilerplate.Tests.Architecture;

/// <summary>
/// Prevents Cross-Boundary Drift between Backend C# DTOs and Frontend TypeScript runtime guards.
/// If a developer or AI Agent modifies a DTO property name, enum value, or validation limit in BE
/// without updating the corresponding contract in FE, these tests fail immediately.
/// </summary>
public sealed class CrossBoundaryContractTests
{
    private static readonly JsonSerializerOptions ApiJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void ApiResponse_And_ApiError_Json_Shape_Matches_Frontend_parseEnvelope_Guard()
    {
        var response = ApiResponse<AccountResponse>.Fail(
            "Validation failed",
            [new ApiError("validation_error", "Invalid username", "username")],
            "trace-123");

        string json = JsonSerializer.Serialize(response, ApiJsonOptions);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        // FE shared/types/api.ts & apiClient.ts parseEnvelope expects exact camelCase keys:
        Assert.Equal(JsonValueKind.False, root.GetProperty("success").ValueKind);
        Assert.Equal(JsonValueKind.String, root.GetProperty("message").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
        Assert.Equal(JsonValueKind.String, root.GetProperty("traceId").ValueKind);

        JsonElement errors = root.GetProperty("errors");
        Assert.Equal(JsonValueKind.Array, errors.ValueKind);
        JsonElement firstError = errors[0];
        Assert.Equal("validation_error", firstError.GetProperty("code").GetString());
        Assert.Equal("Invalid username", firstError.GetProperty("message").GetString());
        Assert.Equal("username", firstError.GetProperty("field").GetString());
    }

    [Fact]
    public void AuthResponse_And_AccountResponse_Json_Shape_Matches_Frontend_authGuards()
    {
        var authResponse = new AuthResponse
        {
            AccessToken = "jwt-access-token",
            AccessTokenExpiresAtUtc = new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc),
            RefreshToken = "secret-raw-refresh-token-must-never-appear-in-json",
            RefreshTokenExpiresAtUtc = new DateTime(2026, 11, 28, 0, 0, 0, DateTimeKind.Utc),
            Account = new AccountResponse(42, "minhanh", AccountRole.Admin)
        };

        string json = JsonSerializer.Serialize(authResponse, ApiJsonOptions);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        // FE features/auth/utils/authGuards.ts -> isAuthSession & isAccount
        Assert.Equal("jwt-access-token", root.GetProperty("accessToken").GetString());
        Assert.Equal(JsonValueKind.String, root.GetProperty("accessTokenExpiresAtUtc").ValueKind);

        // Raw refresh token must be omitted from JSON
        Assert.False(root.TryGetProperty("refreshToken", out _));
        Assert.False(root.TryGetProperty("refreshTokenExpiresAtUtc", out _));

        JsonElement account = root.GetProperty("account");
        Assert.Equal(42, account.GetProperty("id").GetInt32());
        Assert.Equal("minhanh", account.GetProperty("username").GetString());
        Assert.Equal("Admin", account.GetProperty("role").GetString());
    }

    [Fact]
    public void AccountRole_Enum_Names_Match_Frontend_ACCOUNT_ROLES_Constants()
    {
        string[] enumNames = Enum.GetNames<AccountRole>();

        // FE features/auth/constants/auth.ts: ACCOUNT_ROLES = { user: "User", admin: "Admin" }
        Assert.Equal(["User", "Admin"], enumNames);
    }

    [Fact]
    public void Register_And_Login_DataAnnotations_Match_Frontend_AUTH_CONSTRAINTS()
    {
        StringLengthAttribute? registerUserLen = typeof(RegisterRequest)
            .GetProperty(nameof(RegisterRequest.Username))?
            .GetCustomAttribute<StringLengthAttribute>();
        StringLengthAttribute? registerPassLen = typeof(RegisterRequest)
            .GetProperty(nameof(RegisterRequest.Password))?
            .GetCustomAttribute<StringLengthAttribute>();

        Assert.NotNull(registerUserLen);
        Assert.NotNull(registerPassLen);

        // FE features/auth/constants/auth.ts: AUTH_CONSTRAINTS
        Assert.Equal(3, registerUserLen.MinimumLength);
        Assert.Equal(100, registerUserLen.MaximumLength);
        Assert.Equal(8, registerPassLen.MinimumLength);
        Assert.Equal(128, registerPassLen.MaximumLength);
    }
}
