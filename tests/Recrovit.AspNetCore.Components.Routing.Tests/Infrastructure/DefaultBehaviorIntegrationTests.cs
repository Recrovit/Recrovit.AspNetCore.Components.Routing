using Microsoft.Extensions.DependencyInjection;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Models;
using Recrovit.AspNetCore.Components.Routing.Resolution;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Infrastructure;

public sealed class DefaultBehaviorIntegrationTests
{
    [Theory]
    [InlineData("/wasm-probe")]
    [InlineData("/optional")]
    [InlineData("/optional/entity")]
    public void ConfiguredPage_ShouldOverrideAttributeForEachRoute(string path)
    {
        var services = new ServiceCollection();
        services.AddTestRouting(options =>
            options.ConfigurePage<InteractiveWebAssemblyPage>(RecrovitRouteMode.ClientOnly, typeof(OverrideProbeLayout)));

        using var provider = services.BuildServiceProvider();
        var expected = new RecrovitPageRouteDefinition(RecrovitRouteMode.ClientOnly, typeof(OverrideProbeLayout));

        Assert.Equal(expected, provider.GetRequiredService<IRecrovitPageRouteDefinitionResolver>()
            .GetDefinition(typeof(InteractiveWebAssemblyPage)));
        Assert.Equal(expected, provider.GetRequiredService<RecrovitRouteModeResolver>().Resolve(path));
    }

    [Fact]
    public void DefaultPageRouteDefinitionResolver_ShouldUseAttributeOrFallback()
    {
        var services = new ServiceCollection();
        services.AddTestRouting(options => options.FallbackDefinition = new(RecrovitRouteMode.ClientOnly, typeof(OverrideProbeLayout)));

        using var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<IRecrovitPageRouteDefinitionResolver>();

        var attributed = resolver.GetDefinition(typeof(InteractiveWebAssemblyPage));
        var fallback = resolver.GetDefinition(typeof(FallbackRoutePage));

        Assert.Equal(RecrovitRouteMode.InteractiveWebAssembly, attributed.RouteMode);
        Assert.Equal(RecrovitRouteMode.ClientOnly, fallback.RouteMode);
        Assert.Equal(typeof(OverrideProbeLayout), fallback.LayoutType);
    }

    [Fact]
    public void DefaultLayoutResolver_ShouldUseDefinitionLayoutOrDefaultLayout()
    {
        var services = new ServiceCollection();
        services.AddTestRouting();

        using var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<IRecrovitLayoutResolver>();

        var definitionLayout = resolver.ResolveLayout(new(
            typeof(StaticServerPage),
            new(RecrovitRouteMode.StaticServer, typeof(OverrideProbeLayout)),
            typeof(DefaultProbeLayout),
            new(typeof(StaticServerPage), new Dictionary<string, object?>())));

        var defaultLayout = resolver.ResolveLayout(new(
            typeof(StaticServerPage),
            new(RecrovitRouteMode.StaticServer, null),
            typeof(DefaultProbeLayout),
            new(typeof(StaticServerPage), new Dictionary<string, object?>())));

        Assert.Equal(typeof(OverrideProbeLayout), definitionLayout);
        Assert.Equal(typeof(DefaultProbeLayout), defaultLayout);
    }

    [Fact]
    public void DefaultReloadPolicy_ShouldRequireReloadOnlyWhenModesDiffer()
    {
        var services = new ServiceCollection();
        services.AddTestRouting();

        using var provider = services.BuildServiceProvider();
        var policy = provider.GetRequiredService<IRecrovitReloadPolicy>();

        Assert.False(policy.RequiresFullReload(RecrovitRouteMode.StaticServer, RecrovitRouteMode.StaticServer));
        Assert.True(policy.RequiresFullReload(RecrovitRouteMode.StaticServer, RecrovitRouteMode.InteractiveServer));
    }
}
