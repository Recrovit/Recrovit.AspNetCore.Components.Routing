using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Recrovit.AspNetCore.Components.Routing.Configuration;
using Recrovit.AspNetCore.Components.Routing.Models;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Configuration;

public sealed class RecrovitRoutingOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaultState()
    {
        var options = new RecrovitRoutingOptions();

        Assert.Empty(options.RouteAssemblies);
        Assert.Empty(options.NotFoundPages);
        Assert.Null(options.DefaultLayout);
        Assert.Null(options.GetNotFoundPage(RecrovitRoutesKind.Host));
        Assert.Null(options.GetNotFoundPage(RecrovitRoutesKind.Client));
    }

    [Fact]
    public void AddRouteAssembly_ShouldAvoidDuplicateEntries()
    {
        var options = new RecrovitRoutingOptions();
        var assembly = typeof(StaticServerPage).Assembly;

        options.AddRouteAssembly(assembly);
        options.AddRouteAssembly(assembly);

        Assert.Single(options.RouteAssemblies);
        Assert.Same(assembly, options.RouteAssemblies[0]);
    }

    [Fact]
    public void SetNotFoundPageAndGetNotFoundPage_ShouldStorePerKind()
    {
        var options = new RecrovitRoutingOptions();

        options.SetNotFoundPage(RecrovitRoutesKind.Host, typeof(HostNotFoundPage));
        options.SetNotFoundPage(RecrovitRoutesKind.Client, typeof(ClientNotFoundPage));

        Assert.Equal(typeof(HostNotFoundPage), options.GetNotFoundPage(RecrovitRoutesKind.Host));
        Assert.Equal(typeof(ClientNotFoundPage), options.GetNotFoundPage(RecrovitRoutesKind.Client));
    }

    [Fact]
    public void FallbackDefinition_ShouldDefaultToStaticServerWithoutLayout()
    {
        var options = new RecrovitRoutingOptions();

        Assert.Equal(RecrovitRouteMode.StaticServer, options.FallbackDefinition.RouteMode);
        Assert.Null(options.FallbackDefinition.LayoutType);
    }

    [Fact]
    public void TopLevelRenderModeFactory_ShouldDefaultToRouteModeMapper()
    {
        var options = new RecrovitRoutingOptions();
        var expected = RecrovitRouteModeMapper.GetDefaultTopLevelRenderMode(RecrovitRouteMode.ClientOnly);

        var actual = options.TopLevelRenderModeFactory(RecrovitRouteMode.ClientOnly);

        var renderMode = Assert.IsType<InteractiveWebAssemblyRenderMode>(actual);
        var expectedRenderMode = Assert.IsType<InteractiveWebAssemblyRenderMode>(expected);
        Assert.Equal(expectedRenderMode.Prerender, renderMode.Prerender);
    }
}
