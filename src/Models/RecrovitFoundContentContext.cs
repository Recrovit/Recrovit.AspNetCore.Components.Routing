using Microsoft.AspNetCore.Components;

namespace Recrovit.AspNetCore.Components.Routing.Models;

public sealed class RecrovitFoundContentContext
{
    private readonly Action _notifyLayoutOverrideChanged;
    private bool _hasLayoutOverride;
    private Type? _layoutOverride;

    internal RecrovitFoundContentContext(
        RouteData routeData,
        RecrovitPageRouteDefinition definition,
        string focusSelector,
        RecrovitRoutesKind kind,
        Action notifyLayoutOverrideChanged)
    {
        RouteData = routeData;
        Definition = definition;
        FocusSelector = focusSelector;
        Kind = kind;
        _notifyLayoutOverrideChanged = notifyLayoutOverrideChanged;
    }

    public RouteData RouteData { get; }

    public RecrovitPageRouteDefinition Definition { get; }

    public Type? DefaultLayout { get; private set; }

    public string FocusSelector { get; }

    public RecrovitRoutesKind Kind { get; }

    public RenderFragment? DefaultContent { get; private set; }

    public void OverrideLayout(Type? layoutType)
    {
        if (_hasLayoutOverride && _layoutOverride == layoutType)
        {
            return;
        }

        _hasLayoutOverride = true;
        _layoutOverride = layoutType;
        _notifyLayoutOverrideChanged();
    }

    internal void SetResolvedContent(Type? defaultLayout, RenderFragment defaultContent)
    {
        DefaultLayout = defaultLayout;
        DefaultContent = defaultContent;
    }

    internal RecrovitPageRouteDefinition GetLayoutOverrideDefinition()
        => _hasLayoutOverride
            ? Definition with { LayoutType = _layoutOverride }
            : Definition;
}
