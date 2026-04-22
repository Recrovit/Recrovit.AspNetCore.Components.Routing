using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Models;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Components;

public sealed class RecrovitFoundContentHostTests : BunitContext
{
    [Fact]
    public void FoundContentNull_ShouldRenderDefaultRouteContent()
    {
        var cut = RenderHost();

        AssertRouteMarkup(cut, "StaticServer", "StaticServer", "static-layout");
    }

    [Fact]
    public void FoundContentNull_ShouldExposeCascadingRouteValues()
    {
        var cut = RenderHost();

        Assert.Equal("StaticServer", cut.Find("#current-route-mode").TextContent);
        Assert.Equal("StaticServer", cut.Find("#current-definition-mode").TextContent);
        Assert.Equal("null", cut.Find("#current-definition-layout").TextContent);
    }

    [Fact]
    public void OverrideLayout_ShouldUpdateResolvedLayoutWithoutChangingDefinition()
    {
        var cut = RenderHost(
            definition: new(RecrovitRouteMode.InteractiveServer, null),
            routeData: CreateRouteData<InteractiveServerPage>(),
            foundContent: context => builder =>
            {
                context.OverrideLayout(typeof(OverrideProbeLayout));
                builder.AddContent(0, context.DefaultContent);
            });

        cut.WaitForAssertion(() => AssertRouteMarkup(cut, "InteractiveServer", "InteractiveServer", "override-layout"));
        Assert.Equal("null", cut.Find("#current-definition-layout").TextContent);
    }

    [Fact]
    public void OverrideLayoutWithNull_ShouldReturnToFallbackLayout()
    {
        var cut = RenderHost(
            definition: new(RecrovitRouteMode.InteractiveServer, typeof(OverrideProbeLayout)),
            routeData: CreateRouteData<InteractiveServerPage>(),
            foundContent: context => builder =>
            {
                context.OverrideLayout(null);
                builder.AddContent(0, context.DefaultContent);
            });

        cut.WaitForAssertion(() => AssertRouteMarkup(cut, "InteractiveServer", "InteractiveServer", "default-layout"));
        Assert.Equal("OverrideProbeLayout", cut.Find("#current-definition-layout").TextContent);
    }

    [Fact]
    public void OverrideLayoutWithSameValueTwice_ShouldNotTriggerExtraLayoutRefresh()
    {
        var layoutResolver = new TrackingLayoutResolver();

        var cut = RenderHost(
            definition: new(RecrovitRouteMode.InteractiveServer, null),
            routeData: CreateRouteData<InteractiveServerPage>(),
            layoutResolver: layoutResolver,
            foundContent: context => builder =>
            {
                context.OverrideLayout(typeof(OverrideProbeLayout));
                context.OverrideLayout(typeof(OverrideProbeLayout));
                builder.AddContent(0, context.DefaultContent);
            });

        cut.WaitForAssertion(() => AssertRouteMarkup(cut, "InteractiveServer", "InteractiveServer", "override-layout"));
        Assert.Equal(2, layoutResolver.InvocationCount);
    }

    [Fact]
    public void FocusSelector_ShouldFlowToFocusOnNavigate()
    {
        var cut = RenderHost(focusSelector: "#main-content");

        var focusOnNavigate = cut.FindComponent<FocusOnNavigate>();
        Assert.NotNull(focusOnNavigate.Instance.RouteData);

        Assert.Equal("#main-content", focusOnNavigate.Instance.Selector);
        Assert.Equal(typeof(StaticServerPage), focusOnNavigate.Instance.RouteData!.PageType);
    }

    [Fact]
    public void FoundContent_ShouldReceiveContextPropertiesAndResolvedDefaultContent()
    {
        RecrovitFoundContentContext? capturedContext = null;

        var cut = RenderHost(
            routeData: CreateRouteData<InteractiveServerPage>(new Dictionary<string, object?> { ["id"] = "42" }),
            definition: new(RecrovitRouteMode.InteractiveServer, typeof(OverrideProbeLayout)),
            defaultLayout: typeof(DefaultProbeLayout),
            focusSelector: "#main",
            kind: RecrovitRoutesKind.Host,
            foundContent: context => builder =>
            {
                capturedContext = context;

                builder.OpenElement(0, "section");
                builder.AddAttribute(1, "id", "context-probe");
                builder.AddContent(2, context.RouteData.PageType.Name);
                builder.AddContent(3, "|");
                builder.AddContent(4, context.Definition.RouteMode.ToString());
                builder.AddContent(5, "|");
                builder.AddContent(6, context.DefaultLayout?.Name ?? "null");
                builder.AddContent(7, "|");
                builder.AddContent(8, context.FocusSelector);
                builder.AddContent(9, "|");
                builder.AddContent(10, context.Kind.ToString());
                builder.CloseElement();

                builder.AddContent(11, context.DefaultContent);
            });

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedContext);
            Assert.Equal("InteractiveServerPage|InteractiveServer|OverrideProbeLayout|#main|Host", cut.Find("#context-probe").TextContent);
            Assert.Equal("override-layout", cut.Find("#layout-marker").TextContent);
            Assert.Equal("42", cut.Find("#route-parameter-id").TextContent);
        });

        Assert.NotNull(capturedContext!.DefaultContent);
    }

    [Fact]
    public void RelevantParameterChanges_ShouldRebuildFoundContentContext()
    {
        var seenContexts = new List<RecrovitFoundContentContext>();
        RenderFragment<RecrovitFoundContentContext> foundContent = context => builder =>
        {
            seenContexts.Add(context);
            builder.AddContent(0, context.DefaultContent);
        };

        var routeData = CreateRouteData<StaticServerPage>();
        var cut = RenderHost(routeData: routeData, foundContent: foundContent);
        var initialContext = seenContexts[^1];

        cut.Render(ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(RecrovitFoundContentHost.RouteData)] = routeData,
            [nameof(RecrovitFoundContentHost.Definition)] = new RecrovitPageRouteDefinition(RecrovitRouteMode.StaticServer, null),
            [nameof(RecrovitFoundContentHost.LayoutResolver)] = new RouteModeAwareLayoutResolver(),
            [nameof(RecrovitFoundContentHost.DefaultLayout)] = typeof(OverrideProbeLayout),
            [nameof(RecrovitFoundContentHost.FocusSelector)] = "h1",
            [nameof(RecrovitFoundContentHost.Kind)] = RecrovitRoutesKind.Client,
            [nameof(RecrovitFoundContentHost.FoundContent)] = foundContent,
        }));
        var contextAfterDefaultLayoutChange = seenContexts[^1];

        cut.Render(ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(RecrovitFoundContentHost.RouteData)] = routeData,
            [nameof(RecrovitFoundContentHost.Definition)] = new RecrovitPageRouteDefinition(RecrovitRouteMode.StaticServer, null),
            [nameof(RecrovitFoundContentHost.LayoutResolver)] = new RouteModeAwareLayoutResolver(),
            [nameof(RecrovitFoundContentHost.DefaultLayout)] = typeof(OverrideProbeLayout),
            [nameof(RecrovitFoundContentHost.FocusSelector)] = "#content",
            [nameof(RecrovitFoundContentHost.Kind)] = RecrovitRoutesKind.Client,
            [nameof(RecrovitFoundContentHost.FoundContent)] = foundContent,
        }));
        var contextAfterFocusSelectorChange = seenContexts[^1];

        Assert.Same(initialContext, contextAfterDefaultLayoutChange);
        Assert.NotSame(contextAfterDefaultLayoutChange, contextAfterFocusSelectorChange);
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

    private static void AssertRouteMarkup(IRenderedComponent<RecrovitFoundContentHost> cut, string routeMode, string definitionMode, string layoutMarker)
    {
        Assert.Equal(routeMode, cut.Find("#current-route-mode").TextContent);
        Assert.Equal(definitionMode, cut.Find("#current-definition-mode").TextContent);
        Assert.Equal(layoutMarker, cut.Find("#layout-marker").TextContent);
    }
}
