using System.Reflection;
using System.Text.Json.Serialization;
using Boilerplate.API;
using Boilerplate.API.Common;
using Boilerplate.Application;
using Boilerplate.Application.Services.AuthServices.DTOs;
using Boilerplate.Domain;
using Boilerplate.Infrastructure;
using Boilerplate.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Boilerplate.Tests.Architecture;

public sealed class ArchitectureConventionsTests
{
    private static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Boilerplate.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Boilerplate.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_Must_Have_Zero_Project_Or_Framework_Dependencies()
    {
        string[] referencedAssemblies = DomainAssembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(referencedAssemblies, name => name.StartsWith("Boilerplate.", StringComparison.Ordinal));
        Assert.DoesNotContain(referencedAssemblies, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(referencedAssemblies, name => name.StartsWith("Npgsql", StringComparison.Ordinal));
    }

    [Fact]
    public void Application_Must_Only_Depend_On_Domain_Within_Solution()
    {
        string[] referencedSolutionProjects = ApplicationAssembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .Where(name => name.StartsWith("Boilerplate.", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(referencedSolutionProjects);
        Assert.Equal("Boilerplate.Domain", referencedSolutionProjects[0]);
    }

    [Fact]
    public void Infrastructure_Must_Not_Reference_Api()
    {
        string[] referencedAssemblies = InfrastructureAssembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("Boilerplate.API", referencedAssemblies);
    }

    [Fact]
    public void Concrete_Classes_Across_All_Layers_Must_Be_Sealed()
    {
        Assembly[] assemblies = [DomainAssembly, ApplicationAssembly, InfrastructureAssembly, ApiAssembly];

        string[] unsealedClasses = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                !type.IsSealed &&
                type.Namespace is not null &&
                type.Namespace.StartsWith("Boilerplate.", StringComparison.Ordinal) &&
                !type.Namespace.Contains(".Migrations", StringComparison.Ordinal) &&
                !type.Name.StartsWith('<'))
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        Assert.Empty(unsealedClasses);
    }

    [Fact]
    public void DependencyInjected_Classes_Must_Use_Explicit_Constructor_And_Private_Readonly_Underscore_Fields()
    {
        Assembly[] assemblies = [ApplicationAssembly, InfrastructureAssembly, ApiAssembly];

        Type[] diClasses = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                !type.IsNested &&
                !type.Name.StartsWith('<') &&
                type.Namespace is not null &&
                (type.Namespace.Contains(".Services", StringComparison.Ordinal) ||
                 type.Namespace.Contains(".Security", StringComparison.Ordinal) ||
                 type.Namespace.Contains(".Controllers", StringComparison.Ordinal) ||
                 type.Namespace.Contains(".Middlewares", StringComparison.Ordinal) ||
                 type.Namespace.Contains(".BackgroundServices", StringComparison.Ordinal)) &&
                !type.Namespace.Contains(".DTOs", StringComparison.Ordinal) &&
                !type.Name.EndsWith("Response", StringComparison.Ordinal) &&
                !type.Name.EndsWith("Request", StringComparison.Ordinal) &&
                !type.Name.EndsWith("Result", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(diClasses);

        foreach (Type type in diClasses)
        {
            FieldInfo[] instanceFields = type.GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in instanceFields)
            {
                Assert.True(
                    field.IsPrivate && field.IsInitOnly && field.Name.StartsWith('_') && char.IsLower(field.Name[1]),
                    $"Field '{type.FullName}.{field.Name}' must be 'private readonly _camelCase' (no primary constructor captured fields).");
            }
        }
    }

    [Fact]
    public void Controllers_Must_Inherit_BaseController_And_Never_Depend_On_Infrastructure_Or_DbContext()
    {
        Type[] controllers = ApiAssembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                typeof(ControllerBase).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(controllers);

        foreach (Type controller in controllers)
        {
            Assert.True(
                typeof(BaseController).IsAssignableFrom(controller),
                $"Controller {controller.FullName} must inherit from BaseController.");

            ConstructorInfo[] constructors = controller.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            foreach (ConstructorInfo constructor in constructors)
            {
                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    string parameterNamespace = parameter.ParameterType.Namespace ?? string.Empty;
                    Assert.False(
                        parameterNamespace.StartsWith("Boilerplate.Infrastructure", StringComparison.Ordinal),
                        $"Controller {controller.FullName} must not inject Infrastructure type {parameter.ParameterType.FullName}.");
                    Assert.False(
                        parameter.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal),
                        $"Controller {controller.FullName} must remain thin and never inject a DbContext directly.");
                }
            }
        }
    }

    [Fact]
    public void Controller_Actions_Must_Never_Expose_Domain_Entities_Directly()
    {
        Type[] controllers = ApiAssembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                typeof(BaseController).IsAssignableFrom(type))
            .ToArray();

        foreach (Type controller in controllers)
        {
            MethodInfo[] actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (MethodInfo action in actions)
            {
                AssertNoDomainEntityInTypeGraph(action.ReturnType, $"{controller.Name}.{action.Name} return type");
                foreach (ParameterInfo parameter in action.GetParameters())
                {
                    AssertNoDomainEntityInTypeGraph(parameter.ParameterType, $"{controller.Name}.{action.Name} parameter {parameter.Name}");
                }
            }
        }
    }

    [Fact]
    public void AuthResponse_Must_Never_Serialize_Raw_RefreshToken_In_Json()
    {
        PropertyInfo? refreshTokenProp = typeof(AuthResponse).GetProperty(nameof(AuthResponse.RefreshToken));
        PropertyInfo? refreshExpiresProp = typeof(AuthResponse).GetProperty(nameof(AuthResponse.RefreshTokenExpiresAtUtc));

        Assert.NotNull(refreshTokenProp);
        Assert.NotNull(refreshExpiresProp);
        Assert.NotNull(refreshTokenProp.GetCustomAttribute<JsonIgnoreAttribute>());
        Assert.NotNull(refreshExpiresProp.GetCustomAttribute<JsonIgnoreAttribute>());
    }

    [Fact]
    public void EfCore_Runtime_Model_Must_Have_No_Pending_Model_Changes_Against_AutoGenerated_Snapshot()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=boilerplate;Username=postgres;Password=postgres")
            .Options;

        using var dbContext = new ApplicationDbContext(options);

        Assert.False(
            dbContext.Database.HasPendingModelChanges(),
            "EF Core runtime model has pending changes that do not match ApplicationDbContextModelSnapshot. " +
            "Never edit entities/configurations or hand-edit auto-generated migration files without running 'dotnet ef migrations add'.");
    }

    private static void AssertNoDomainEntityInTypeGraph(Type type, string context)
    {
        Assert.False(
            typeof(BaseEntity).IsAssignableFrom(type),
            $"{context} exposes domain entity {type.FullName}.");

        if (type.IsGenericType)
        {
            foreach (Type genericArgument in type.GetGenericArguments())
            {
                AssertNoDomainEntityInTypeGraph(genericArgument, context);
            }
        }
    }
}
