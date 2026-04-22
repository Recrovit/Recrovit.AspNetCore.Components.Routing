using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Recrovit.AspNetCore.Components.Routing.Configuration;
using Recrovit.AspNetCore.Components.Routing.Models;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Configuration;

public sealed class RecrovitRouteModeMapperTests
{
    [Fact]
    public void CreateDefaultFallbackDefinition_ShouldReturnStaticServerWithoutLayout()
    {
        var definition = RecrovitRouteModeMapper.CreateDefaultFallbackDefinition();

        Assert.Equal(RecrovitRouteMode.StaticServer, definition.RouteMode);
        Assert.Null(definition.LayoutType);
    }

    [Theory]
    [InlineData(RecrovitRouteMode.StaticServer)]
    [InlineData(RecrovitRouteMode.InteractiveServer)]
    [InlineData(RecrovitRouteMode.InteractiveWebAssembly)]
    [InlineData(RecrovitRouteMode.InteractiveAuto)]
    [InlineData(RecrovitRouteMode.ClientOnly)]
    public void CreateDefinition_ShouldPreserveRouteModeAndLayout(RecrovitRouteMode routeMode)
    {
        var definition = RecrovitRouteModeMapper.CreateDefinition(routeMode, typeof(LayoutComponentBase));

        Assert.Equal(routeMode, definition.RouteMode);
        Assert.Equal(typeof(LayoutComponentBase), definition.LayoutType);
    }

    [Fact]
    public void CreateDefinition_ShouldFallbackForUnknownRouteMode()
    {
        var definition = RecrovitRouteModeMapper.CreateDefinition((RecrovitRouteMode)999, typeof(LayoutComponentBase));

        Assert.Equal(RecrovitRouteMode.StaticServer, definition.RouteMode);
        Assert.Null(definition.LayoutType);
    }

    [Theory]
    [InlineData(RecrovitRouteMode.StaticServer, null)]
    [InlineData(RecrovitRouteMode.InteractiveServer, typeof(InteractiveServerRenderMode))]
    [InlineData(RecrovitRouteMode.InteractiveWebAssembly, typeof(InteractiveWebAssemblyRenderMode))]
    [InlineData(RecrovitRouteMode.InteractiveAuto, typeof(InteractiveAutoRenderMode))]
    [InlineData(RecrovitRouteMode.ClientOnly, typeof(InteractiveWebAssemblyRenderMode))]
    public void GetDefaultTopLevelRenderMode_ShouldMapEachSupportedRouteMode(
        RecrovitRouteMode routeMode,
        Type? expectedType)
    {
        var renderMode = RecrovitRouteModeMapper.GetDefaultTopLevelRenderMode(routeMode);

        if (expectedType is null)
        {
            Assert.Null(renderMode);
            return;
        }

        Assert.IsType(expectedType, renderMode);
    }

    [Fact]
    public void GetDefaultTopLevelRenderMode_ShouldReturnClientOnlyWebAssemblyWithoutPrerender()
    {
        var renderMode = RecrovitRouteModeMapper.GetDefaultTopLevelRenderMode(RecrovitRouteMode.ClientOnly);

        var webAssemblyRenderMode = Assert.IsType<InteractiveWebAssemblyRenderMode>(renderMode);
        Assert.False(webAssemblyRenderMode.Prerender);
    }

    [Fact]
    public void GetDefaultTopLevelRenderMode_ShouldReturnNullForUnknownRouteMode()
    {
        var renderMode = RecrovitRouteModeMapper.GetDefaultTopLevelRenderMode((RecrovitRouteMode)999);

        Assert.Null(renderMode);
    }

    [Fact]
    public void GetAssignedModeLabel_ShouldReturnExpectedLabels()
    {
        Assert.Equal("Static SSR", RecrovitRouteModeMapper.GetAssignedModeLabel(null));
        Assert.Equal("Interactive Server", RecrovitRouteModeMapper.GetAssignedModeLabel(new InteractiveServerRenderMode()));
        Assert.Equal("Interactive Auto", RecrovitRouteModeMapper.GetAssignedModeLabel(new InteractiveAutoRenderMode()));
        Assert.Equal("Interactive WebAssembly", RecrovitRouteModeMapper.GetAssignedModeLabel(new InteractiveWebAssemblyRenderMode()));
        Assert.Equal(
            "Interactive WebAssembly (prerender off)",
            RecrovitRouteModeMapper.GetAssignedModeLabel(new InteractiveWebAssemblyRenderMode(prerender: false)));
        Assert.Equal("CustomRenderMode", RecrovitRouteModeMapper.GetAssignedModeLabel(new CustomRenderMode()));
    }

    [Theory]
    [InlineData("Server", true, "Server (interactive)")]
    [InlineData("Static", false, "Static (non-interactive)")]
    public void GetRuntimeLabel_ShouldFormatRendererInfo(string name, bool isInteractive, string expected)
    {
        var label = RecrovitRouteModeMapper.GetRuntimeLabel(new RendererInfo(name, isInteractive));

        Assert.Equal(expected, label);
    }

    private sealed class CustomRenderMode : IComponentRenderMode;
}
