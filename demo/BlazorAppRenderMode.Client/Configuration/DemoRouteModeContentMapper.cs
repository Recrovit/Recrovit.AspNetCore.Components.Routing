using Recrovit.AspNetCore.Components.Routing.Models;

namespace BlazorAppRenderMode.Client.Configuration;

public static class DemoRouteModeContentMapper
{
    public static DemoRouteModeContent GetContent(RecrovitRouteMode routeMode)
        => routeMode switch
        {
            RecrovitRouteMode.StaticServer => new(
                "Static SSR",
                "Static SSR page",
                "This page renders as static server-side content, so returning here always requires a full document request."),
            RecrovitRouteMode.InteractiveServer => new(
                "Interactive Server",
                "Top-level InteractiveServer page",
                "This page runs in interactive server-side render mode, so it can stay within the same top-level mode through SPA navigation."),
            RecrovitRouteMode.InteractiveWebAssembly => new(
                "Interactive WebAssembly",
                "Top-level InteractiveWebAssembly page",
                "This page runs in prerendered InteractiveWebAssembly mode, so it is reachable through SPA navigation within the same top-level WebAssembly mode."),
            RecrovitRouteMode.InteractiveAuto => new(
                "Interactive Auto",
                "Top-level InteractiveAuto page",
                "This page runs in InteractiveAuto mode and can be reached within the same top-level mode without a new document request."),
            RecrovitRouteMode.ClientOnly => new(
                "Client Only",
                "InteractiveWebAssembly(prerender: false) page",
                "This page runs only on the client without prerendering, so it counts as a separate top-level mode and switching to it requires a full reload."),
            _ => GetFallbackContent()
        };

    public static DemoRouteModeContent GetFallbackContent()
        => new(
            "Static SSR",
            "Fallback route",
            "Unknown routes are shown as a static server-side fallback.");
}
