using System.Reflection;
using Microsoft.AspNetCore.Components;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Configuration;

public sealed class RecrovitRoutingOptions
{
    public IList<Assembly> RouteAssemblies { get; } = [];

    public IDictionary<RecrovitRoutesKind, Type> NotFoundPages { get; } = new Dictionary<RecrovitRoutesKind, Type>();

    public IDictionary<RecrovitRoutesKind, RenderFragment<RecrovitFoundContentContext>> FoundContents { get; }
        = new Dictionary<RecrovitRoutesKind, RenderFragment<RecrovitFoundContentContext>>();

    public RecrovitPageRouteDefinition FallbackDefinition { get; set; } = RecrovitRouteModeMapper.CreateDefaultFallbackDefinition();

    public Type? DefaultLayout { get; set; }

    public Func<RecrovitRouteMode, IComponentRenderMode?> TopLevelRenderModeFactory { get; set; } = RecrovitRouteModeMapper.GetDefaultTopLevelRenderMode;

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
