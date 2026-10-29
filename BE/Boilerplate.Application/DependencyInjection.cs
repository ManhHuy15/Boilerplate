using Boilerplate.Application.Services.AuthServices;
using Boilerplate.Application.Services.RefreshTokenServices;
using Microsoft.Extensions.DependencyInjection;

namespace Boilerplate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }
}
