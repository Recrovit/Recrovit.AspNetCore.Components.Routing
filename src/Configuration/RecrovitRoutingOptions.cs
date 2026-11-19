using Microsoft.AspNetCore.Components;
using Recrovit.AspNetCore.Components.Routing.Models;
using System.Reflection;

namespace Recrovit.AspNetCore.Components.Routing.Configuration;

public sealed class RecrovitRoutingOptions
{
    private readonly Dictionary<Type, RecrovitPageRouteDefinition> _pageDefinitions = new();

    public IList<Assembly> RouteAssemblies { get; } = [];

    public IDictionary<RecrovitRoutesKind, Type> NotFoundPages { get; } = new Dictionary<RecrovitRoutesKind, Type>();

    public IDictionary<RecrovitRoutesKind, RenderFragment<RecrovitFoundContentContext>> FoundContents { get; }
        = new Dictionary<RecrovitRoutesKind, RenderFragment<RecrovitFoundContentContext>>();

    public RecrovitPageRouteDefinition FallbackDefinition { get; set; } = RecrovitRouteModeMapper.CreateDefaultFallbackDefinition();

    public Type? DefaultLayout { get; set; }

    public Func<RecrovitRouteMode, IComponentRenderMode?> TopLevelRenderModeFactory { get; set; } = RecrovitRouteModeMapper.GetDefaultTopLevelRenderMode;

    /// <summary>
    /// Sets the routing definition for a page type, taking precedence over its route mode attribute.
    /// </summary>
    /// <typeparam name="TPage">The routable component type.</typeparam>
    /// <param name="routeMode">The route mode to use for all routes of the page.</param>
    /// <param name="layoutType">The layout to use, or null to use the default layout.</param>
    public void ConfigurePage<TPage>(RecrovitRouteMode routeMode, Type? layoutType = null)
        where TPage : IComponent
        => _pageDefinitions[typeof(TPage)] = RecrovitRouteModeMapper.CreateDefinition(routeMode, layoutType);

    internal RecrovitPageRouteDefinition? GetConfiguredPage(Type pageType)
        => _pageDefinitions.GetValueOrDefault(pageType);

    public void AddRouteAssembly(Assembly assembly)
    {
        if (!RouteAssemblies.Contains(assembly))
        {
            RouteAssemblies.Add(assembly);
        }
    }

    public void SetNotFoundPage(RecrovitRoutesKind kind, Type pageType)
        => NotFoundPages[kind] = pageType;

    public Type? GetNotFoundPage(RecrovitRoutesKind kind)
        => NotFoundPages.TryGetValue(kind, out var pageType) ? pageType : null;

    public void SetFoundContent(RecrovitRoutesKind kind, RenderFragment<RecrovitFoundContentContext> foundContent)
    {
        ArgumentNullException.ThrowIfNull(foundContent);

        FoundContents[kind] = foundContent;
    }

    public RenderFragment<RecrovitFoundContentContext>? GetFoundContent(RecrovitRoutesKind kind)
        => FoundContents.TryGetValue(kind, out var foundContent) ? foundContent : null;
}
