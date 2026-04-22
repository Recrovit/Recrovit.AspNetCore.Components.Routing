using Microsoft.AspNetCore.Components;
using Recrovit.AspNetCore.Components.Routing.Models;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Models;

public sealed class PublicContractsTests
{
    [Fact]
    public void RecrovitRouteMode_ShouldExposeStablePublicMembers()
    {
        Assert.Equal(
            ["StaticServer", "InteractiveServer", "InteractiveWebAssembly", "InteractiveAuto", "ClientOnly"],
            Enum.GetNames<RecrovitRouteMode>());
    }

    [Fact]
    public void RecrovitRoutesKind_ShouldExposeStablePublicMembers()
    {
        Assert.Equal(
            ["Host", "Client"],
            Enum.GetNames<RecrovitRoutesKind>());
    }

    [Fact]
    public void RecrovitPageRouteDefinition_ShouldUseValueEqualityAndPreserveProperties()
    {
        var left = new RecrovitPageRouteDefinition(RecrovitRouteMode.ClientOnly, typeof(DefaultProbeLayout));
        var right = new RecrovitPageRouteDefinition(RecrovitRouteMode.ClientOnly, typeof(DefaultProbeLayout));

        Assert.Equal(left, right);
        Assert.Equal(RecrovitRouteMode.ClientOnly, left.RouteMode);
        Assert.Equal(typeof(DefaultProbeLayout), left.LayoutType);
    }

    [Fact]
    public void RecrovitLayoutResolverContext_ShouldUseValueEqualityAndPreserveProperties()
    {
        var routeData = new RouteData(typeof(StaticServerPage), new Dictionary<string, object?>());
        var definition = new RecrovitPageRouteDefinition(RecrovitRouteMode.StaticServer, typeof(DefaultProbeLayout));
        var left = new RecrovitLayoutResolverContext(typeof(StaticServerPage), definition, typeof(OverrideProbeLayout), routeData);
        var right = new RecrovitLayoutResolverContext(typeof(StaticServerPage), definition, typeof(OverrideProbeLayout), routeData);

        Assert.Equal(left, right);
        Assert.Equal(typeof(StaticServerPage), left.PageType);
        Assert.Equal(definition, left.Definition);
        Assert.Equal(typeof(OverrideProbeLayout), left.DefaultLayout);
        Assert.Same(routeData, left.RouteData);
    }
}
