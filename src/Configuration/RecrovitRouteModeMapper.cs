using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Configuration;

public static class RecrovitRouteModeMapper
{
    private static readonly InteractiveWebAssemblyRenderMode ClientOnlyRenderMode = new(prerender: false);

    public static RecrovitPageRouteDefinition CreateDefaultFallbackDefinition() => new(RecrovitRouteMode.StaticServer, null);

    public static RecrovitPageRouteDefinition CreateDefinition(
        RecrovitRouteMode routeMode,
        Type? layoutType)
        => routeMode switch
        {
            RecrovitRouteMode.StaticServer => new(routeMode, layoutType),
            RecrovitRouteMode.InteractiveServer => new(routeMode, layoutType),
            RecrovitRouteMode.InteractiveWebAssembly => new(routeMode, layoutType),
            RecrovitRouteMode.InteractiveAuto => new(routeMode, layoutType),
            RecrovitRouteMode.ClientOnly => new(routeMode, layoutType),
            _ => CreateDefaultFallbackDefinition()
        };

    public static IComponentRenderMode? GetDefaultTopLevelRenderMode(RecrovitRouteMode routeMode)
        => routeMode switch
        {
            RecrovitRouteMode.StaticServer => null,
            RecrovitRouteMode.InteractiveServer => RenderMode.InteractiveServer,
            RecrovitRouteMode.InteractiveWebAssembly => RenderMode.InteractiveWebAssembly,
            RecrovitRouteMode.InteractiveAuto => RenderMode.InteractiveAuto,
            RecrovitRouteMode.ClientOnly => ClientOnlyRenderMode,
            _ => null
        };

    public static string GetAssignedModeLabel(IComponentRenderMode? assignedRenderMode)
        => assignedRenderMode switch
        {
            null => "Static SSR",
            InteractiveServerRenderMode => "Interactive Server",
            InteractiveAutoRenderMode => "Interactive Auto",
            InteractiveWebAssemblyRenderMode webAssemblyMode when !webAssemblyMode.Prerender => "Interactive WebAssembly (prerender off)",
            InteractiveWebAssemblyRenderMode _ => "Interactive WebAssembly",
            _ => assignedRenderMode.GetType().Name
        };

    public static string GetRuntimeLabel(RendererInfo rendererInfo)
        => $"{rendererInfo.Name} ({(rendererInfo.IsInteractive ? "interactive" : "non-interactive")})";
}
