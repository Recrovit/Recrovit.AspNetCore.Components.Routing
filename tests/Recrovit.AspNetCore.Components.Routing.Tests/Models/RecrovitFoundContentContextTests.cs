using Bunit;
using Microsoft.AspNetCore.Components;
using Recrovit.AspNetCore.Components.Routing;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Models;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Models;

public sealed class RecrovitFoundContentContextTests : BunitContext
{
    [Fact]
    public void Context_ShouldExposePublicValuesAndResolvedContent()
    {
        RecrovitFoundContentContext? capturedContext = null;
        var routeData = CreateRouteData<InteractiveServerPage>(new Dictionary<string, object?> { ["id"] = "42" });
        var definition = new RecrovitPageRouteDefinition(RecrovitRouteMode.InteractiveServer, typeof(OverrideProbeLayout));

        var cut = RenderHost(
            routeData: routeData,
            definition: definition,
            defaultLayout: typeof(DefaultProbeLayout),
            focusSelector: "#main",
            kind: RecrovitRoutesKind.Host,
            foundContent: context => builder =>
            {
                capturedContext = context;
                builder.AddContent(0, context.DefaultContent);
            });

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedContext);
            Assert.Same(routeData, capturedContext!.RouteData);
            Assert.Equal(definition, capturedContext.Definition);
            Assert.Equal(typeof(OverrideProbeLayout), capturedContext.DefaultLayout);
            Assert.Equal("#main", capturedContext.FocusSelector);
            Assert.Equal(RecrovitRoutesKind.Host, capturedContext.Kind);
            Assert.NotNull(capturedContext.DefaultContent);
            Assert.Equal("override-layout", cut.Find("#layout-marker").TextContent);
            Assert.Equal("42", cut.Find("#route-parameter-id").TextContent);
        });
    }

    [Fact]
    public void OverrideLayout_ShouldBeIdempotentForSameValue()
    {
        var layoutResolver = new TrackingLayoutResolver();
        RecrovitFoundContentContext? capturedContext = null;

        var cut = RenderHost(
            definition: new(RecrovitRouteMode.InteractiveServer, null),
            layoutResolver: layoutResolver,
            foundContent: context => builder =>
            {
                capturedContext = context;
                context.OverrideLayout(typeof(OverrideProbeLayout));
                context.OverrideLayout(typeof(OverrideProbeLayout));
                builder.AddContent(0, context.DefaultContent);
            });

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedContext);
            Assert.Equal(typeof(OverrideProbeLayout), capturedContext!.DefaultLayout);
            Assert.Equal("override-layout", cut.Find("#layout-marker").TextContent);
        });

        Assert.Equal(2, layoutResolver.InvocationCount);
        Assert.All(layoutResolver.Contexts, context => Assert.Equal(RecrovitRouteMode.InteractiveServer, context.Definition.RouteMode));
        Assert.Equal(2, layoutResolver.Contexts.Count);
        Assert.Null(layoutResolver.Contexts[0].Definition.LayoutType);
        Assert.Equal(typeof(OverrideProbeLayout), layoutResolver.Contexts[1].Definition.LayoutType);
        Assert.Null(capturedContext!.Definition.LayoutType);
    }

    [Fact]
    public void OverrideLayout_WithNullShouldUseFallbackLayoutWithoutMutatingDefinition()
    {
        var layoutResolver = new TrackingLayoutResolver();
        RecrovitFoundContentContext? capturedContext = null;

        var cut = RenderHost(
            definition: new(RecrovitRouteMode.InteractiveServer, typeof(OverrideProbeLayout)),
            defaultLayout: typeof(DefaultProbeLayout),
            layoutResolver: layoutResolver,
            foundContent: context => builder =>
            {
                capturedContext = context;
                context.OverrideLayout(null);
                builder.AddContent(0, context.DefaultContent);
            });

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedContext);
            Assert.Equal(typeof(DefaultProbeLayout), capturedContext!.DefaultLayout);
            Assert.Equal("default-layout", cut.Find("#layout-marker").TextContent);
        });

        Assert.Equal(2, layoutResolver.InvocationCount);
        Assert.Equal(typeof(OverrideProbeLayout), layoutResolver.Contexts[0].Definition.LayoutType);
        Assert.Null(layoutResolver.Contexts[1].Definition.LayoutType);
        Assert.Equal(typeof(OverrideProbeLayout), capturedContext!.Definition.LayoutType);
    }

    private IRenderedComponent<RecrovitFoundContentHost> RenderHost(
        RouteData? routeData = null,
        RecrovitPageRouteDefinition? definition = null,
        IRecrovitLayoutResolver? layoutResolver = null,
        Type? defaultLayout = null,
        string focusSelector = "h1",
        RecrovitRoutesKind kind = RecrovitRoutesKind.Client,
        RenderFragment<RecrovitFoundContentContext>? foundContent = null)
        => Render<RecrovitFoundContentHost>(parameters => parameters
            .Add(component => component.RouteData, routeData ?? CreateRouteData<StaticServerPage>())
            .Add(component => component.Definition, definition ?? new(RecrovitRouteMode.StaticServer, null))
            .Add(component => component.LayoutResolver, layoutResolver ?? new RouteModeAwareLayoutResolver())
            .Add(component => component.DefaultLayout, defaultLayout ?? typeof(DefaultProbeLayout))
            .Add(component => component.FocusSelector, focusSelector)
            .Add(component => component.Kind, kind)
            .Add(component => component.FoundContent, foundContent));

    private static RouteData CreateRouteData<TComponent>(IReadOnlyDictionary<string, object?>? routeValues = null)
        where TComponent : IComponent
        => new(typeof(TComponent), routeValues ?? new Dictionary<string, object?>());
}
