using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Boilerplate.API.Common;
using Boilerplate.API.Middlewares;
using Boilerplate.Application;
using Boilerplate.Application.Settings;
using Boilerplate.Infrastructure;
using Boilerplate.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;

namespace Boilerplate.API;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ConfigureLogging(builder.Logging);
        ConfigureServices(
            builder.Services,
            builder.Configuration,
            builder.Environment);

        WebApplication app = builder.Build();

        ConfigurePipeline(app);
        await ApplyDatabaseMigrationsAsync(app);

        await app.RunAsync();
    }

    private static void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole(options =>
            options.FormatterName = CustomLogFormatter.FormatterName);
        logging.AddConsoleFormatter<CustomLogFormatter, ConsoleFormatterOptions>();
    }

    private static void ConfigureServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        ConfigureValidationResponses(services);
        ConfigureCors(services, configuration);
        ConfigureDatabaseSettings(services, configuration);
        ConfigureJwtAuthentication(services, configuration);
        ConfigureSwagger(services, environment);

        services.AddAuthorization();
        services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database");
        services.AddApplication();
        services.AddInfrastructure();
    }

    private static void ConfigureSwagger(
        IServiceCollection services,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Boilerplate API",
                Version = "v1"
            });

            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter the JWT access token."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });
    }

    private static void ConfigureValidationResponses(IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                ApiError[] errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors.Select(error =>
                        new ApiError(
                            "validation_error",
                            string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "The supplied value is invalid."
                                : error.ErrorMessage,
                            entry.Key)))
                    .ToArray();

                var response = ApiResponse<object?>.Fail(
                    "The request is invalid.",
                    errors,
                    context.HttpContext.TraceIdentifier);

                return new BadRequestObjectResult(response);
            };
        });
    }

    private static void ConfigureCors(
        IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection corsSection = configuration.GetSection(CorsSettings.SectionName);
        CorsSettings corsSettings = corsSection.Get<CorsSettings>() ?? new CorsSettings();

        services
            .AddOptions<CorsSettings>()
            .Bind(corsSection);

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (corsSettings.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(corsSettings.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    policy.SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });
    }

    private static void ConfigureDatabaseSettings(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseSettings>()
            .Bind(configuration.GetRequiredSection(DatabaseSettings.SectionName))
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.ConnectionString),
                $"{DatabaseSettings.SectionName}:ConnectionString is required.")
            .ValidateOnStart();
    }

    private static void ConfigureJwtAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection jwtSection = configuration.GetRequiredSection(JwtSettings.SectionName);
        JwtSettings jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");

        if (Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SecretKey must contain at least 32 UTF-8 bytes. " +
                "Supply it through the Jwt__SecretKey environment variable or a secret store.");
        }

        services
            .AddOptions<JwtSettings>()
            .Bind(jwtSection)
            .Validate(
                settings => Encoding.UTF8.GetByteCount(settings.SecretKey) >= 32,
                "Jwt:SecretKey must contain at least 32 UTF-8 bytes.")
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.Issuer),
                "Jwt:Issuer is required.")
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.Audience),
                "Jwt:Audience is required.")
            .Validate(
                settings => settings.AccessTokenMinutes > 0 && settings.RefreshTokenDays > 0,
                "JWT token lifetimes must be greater than zero.")
            .ValidateOnStart();

        SymmetricSecurityKey signingKey = new(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        var response = ApiResponse<object?>.Fail(
                            "Authentication is required.",
                            [new ApiError("unauthorized", "A valid access token is required.")],
                            context.HttpContext.TraceIdentifier);

                        await context.Response.WriteAsJsonAsync(
                            response,
                            context.HttpContext.RequestAborted);
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;

                        var response = ApiResponse<object?>.Fail(
                            "Access is forbidden.",
                            [new ApiError("forbidden", "You do not have permission to access this resource.")],
                            context.HttpContext.TraceIdentifier);

                        await context.Response.WriteAsJsonAsync(
                            response,
                            context.HttpContext.RequestAborted);
                    }
                };
            });
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseCors();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Boilerplate API v1");
                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
            });
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }

    private static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
    {
        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        ILogger<Program> logger = scope.ServiceProvider
            .GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Applying pending database migrations.");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations are up to date.");
    }
}
