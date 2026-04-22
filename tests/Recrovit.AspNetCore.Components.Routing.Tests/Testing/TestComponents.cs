using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Attributes;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Testing;

public abstract class RouteProbePageBase : ComponentBase
{
    [CascadingParameter(Name = "RecrovitCurrentRouteMode")]
    public RecrovitRouteMode CurrentRouteMode { get; set; }

    [CascadingParameter(Name = "RecrovitCurrentPageRouteDefinition")]
    public RecrovitPageRouteDefinition? CurrentDefinition { get; set; }

    [Parameter]
    public string? Id { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "p");
        builder.AddAttribute(1, "id", "current-route-mode");
        builder.AddContent(2, CurrentRouteMode.ToString());
        builder.CloseElement();

        builder.OpenElement(3, "p");
        builder.AddAttribute(4, "id", "current-definition-mode");
        builder.AddContent(5, CurrentDefinition?.RouteMode.ToString() ?? "missing");
        builder.CloseElement();

        builder.OpenElement(6, "p");
        builder.AddAttribute(7, "id", "current-definition-layout");
        builder.AddContent(8, CurrentDefinition?.LayoutType?.Name ?? "null");
        builder.CloseElement();

        builder.OpenElement(9, "p");
        builder.AddAttribute(10, "id", "route-parameter-id");
        builder.AddContent(11, Id ?? "null");
        builder.CloseElement();
    }
}

[Route("/probe")]
[RecrovitPageRoute(RecrovitRouteMode.StaticServer)]
public sealed class StaticServerPage : RouteProbePageBase;

[Route("/layout-probe")]
[RecrovitPageRoute(RecrovitRouteMode.InteractiveServer)]
public sealed class InteractiveServerPage : RouteProbePageBase;

[Route("/wasm-probe")]
[RecrovitPageRoute(RecrovitRouteMode.InteractiveWebAssembly)]
public sealed class InteractiveWebAssemblyPage : RouteProbePageBase;

[Route("/auto-probe")]
[RecrovitPageRoute(RecrovitRouteMode.InteractiveAuto)]
public sealed class InteractiveAutoPage : RouteProbePageBase;

[Route("/client-only-probe")]
[RecrovitPageRoute(RecrovitRouteMode.ClientOnly)]
public sealed class ClientOnlyPage : RouteProbePageBase;

[Route("/layoutless")]
[RecrovitPageRoute(RecrovitRouteMode.InteractiveServer)]
public sealed class LayoutlessPage : RouteProbePageBase;

[Route("/items/{id}")]
[RecrovitPageRoute(RecrovitRouteMode.InteractiveAuto)]
public sealed class ParameterizedPage : RouteProbePageBase;

[Route("/items/special")]
[RecrovitPageRoute(RecrovitRouteMode.StaticServer)]
public sealed class SpecificPage : RouteProbePageBase;

[Route("/fallback-only")]
public sealed class FallbackRoutePage : RouteProbePageBase;

public sealed class DefaultProbeLayout : LayoutComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", "layout-marker");
        builder.AddContent(2, "default-layout");
        builder.CloseElement();
        builder.AddContent(3, Body);
    }
}

public sealed class StaticProbeLayout : LayoutComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", "layout-marker");
        builder.AddContent(2, "static-layout");
        builder.CloseElement();
        builder.AddContent(3, Body);
    }
}

public sealed class InteractiveProbeLayout : LayoutComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", "layout-marker");
        builder.AddContent(2, "interactive-layout");
        builder.CloseElement();
        builder.AddContent(3, Body);
    }
}

public sealed class OverrideProbeLayout : LayoutComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", "layout-marker");
        builder.AddContent(2, "override-layout");
        builder.CloseElement();
        builder.AddContent(3, Body);
    }
}

public sealed class WebAssemblyProbeLayout : LayoutComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", "layout-marker");
        builder.AddContent(2, "webassembly-layout");
        builder.CloseElement();
        builder.AddContent(3, Body);
    }
}

[Route("/host-not-found")]
public sealed class HostNotFoundPage : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", "not-found");
        builder.AddContent(2, "host-not-found");
        builder.CloseElement();
    }
}

[Route("/client-not-found")]
public sealed class ClientNotFoundPage : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", "not-found");
        builder.AddContent(2, "client-not-found");
        builder.CloseElement();
    }
}

[Route("/override-not-found")]
public sealed class OverrideNotFoundPage : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", "not-found");
        builder.AddContent(2, "override-not-found");
        builder.CloseElement();
    }
}

public sealed class RouteModeAwareLayoutResolver : IRecrovitLayoutResolver
{
    public Type? ResolveLayout(RecrovitLayoutResolverContext context)
        => context.Definition.LayoutType
            ?? context.Definition.RouteMode switch
            {
                RecrovitRouteMode.StaticServer => typeof(StaticProbeLayout),
                RecrovitRouteMode.InteractiveWebAssembly => typeof(WebAssemblyProbeLayout),
                RecrovitRouteMode.InteractiveAuto => typeof(InteractiveProbeLayout),
                _ => context.DefaultLayout
            };
}

public sealed class TrackingLayoutResolver : IRecrovitLayoutResolver
{
    public int InvocationCount { get; private set; }

    public List<RecrovitLayoutResolverContext> Contexts { get; } = [];

    public Type? ResolveLayout(RecrovitLayoutResolverContext context)
    {
        InvocationCount++;
        Contexts.Add(context);
        return context.Definition.LayoutType ?? context.DefaultLayout;
    }
}

public sealed class CustomReloadPolicy : IRecrovitReloadPolicy
{
    public bool RequiresFullReload(RecrovitRouteMode currentMode, RecrovitRouteMode targetMode)
        => currentMode == RecrovitRouteMode.StaticServer && targetMode == RecrovitRouteMode.ClientOnly;
}

public sealed class CustomLayoutResolver : IRecrovitLayoutResolver
{
    public Type? ResolveLayout(RecrovitLayoutResolverContext context)
        => typeof(OverrideProbeLayout);
}

public sealed class CustomPageRouteDefinitionResolver : IRecrovitPageRouteDefinitionResolver
{
    public RecrovitPageRouteDefinition GetDefinition(Type pageType)
        => new(RecrovitRouteMode.ClientOnly, typeof(OverrideProbeLayout));

    public RecrovitPageRouteDefinition GetFallbackDefinition()
        => new(RecrovitRouteMode.ClientOnly, typeof(OverrideProbeLayout));
}
