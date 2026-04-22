using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Configuration;
using Recrovit.AspNetCore.Components.Routing.Resolution;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Configuration;

public sealed class RecrovitRoutingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRecrovitComponentRouting_ShouldThrowWhenConfigureIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddRecrovitComponentRouting(null!));
    }

    [Fact]
    public void AddRecrovitComponentRouting_ShouldRequireAtLeastOneRouteAssembly()
    {
        var services = new ServiceCollection();
        services.AddRecrovitComponentRouting(_ => { });

        using var serviceProvider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => serviceProvider.GetRequiredService<IOptions<RecrovitRoutingOptions>>().Value);
    }

    [Fact]
    public void AddRecrovitComponentRouting_ShouldRegisterDefaultServices()
    {
        var services = new ServiceCollection();
        services.AddRecrovitComponentRouting(options => options.AddRouteAssembly(typeof(StaticServerPage).Assembly));

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredService<IRecrovitPageRouteDefinitionResolver>());
        Assert.NotNull(serviceProvider.GetRequiredService<IRecrovitLayoutResolver>());
        Assert.NotNull(serviceProvider.GetRequiredService<IRecrovitReloadPolicy>());
        Assert.NotNull(serviceProvider.GetRequiredService<RecrovitRouteModeResolver>());
    }

    [Fact]
    public void AddRecrovitComponentRouting_ShouldPreserveCustomRegistrations()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRecrovitPageRouteDefinitionResolver, CustomPageRouteDefinitionResolver>();
        services.AddSingleton<IRecrovitLayoutResolver, CustomLayoutResolver>();
        services.AddSingleton<IRecrovitReloadPolicy, CustomReloadPolicy>();

        services.AddRecrovitComponentRouting(options => options.AddRouteAssembly(typeof(StaticServerPage).Assembly));

        using var serviceProvider = services.BuildServiceProvider();

        Assert.IsType<CustomPageRouteDefinitionResolver>(serviceProvider.GetRequiredService<IRecrovitPageRouteDefinitionResolver>());
        Assert.IsType<CustomLayoutResolver>(serviceProvider.GetRequiredService<IRecrovitLayoutResolver>());
        Assert.IsType<CustomReloadPolicy>(serviceProvider.GetRequiredService<IRecrovitReloadPolicy>());
    }

    [Fact]
    public void AddRecrovitComponentRouting_ShouldBuildRouteResolverFromConfiguredAssemblies()
    {
        var services = new ServiceCollection();
        services.AddRecrovitComponentRouting(options => options.AddRouteAssembly(typeof(StaticServerPage).Assembly));

        using var serviceProvider = services.BuildServiceProvider();
        var resolver = serviceProvider.GetRequiredService<RecrovitRouteModeResolver>();

        var definition = resolver.Resolve("/client-only-probe");

        Assert.Equal(global::Recrovit.AspNetCore.Components.Routing.Models.RecrovitRouteMode.ClientOnly, definition.RouteMode);
    }
}
