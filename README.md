# Recrovit.AspNetCore.Components.Routing
[![NuGet Version](https://img.shields.io/nuget/v/Recrovit.AspNetCore.Components.Routing.svg)](https://www.nuget.org/packages/Recrovit.AspNetCore.Components.RoutingCore/)

This repository contains the source code, tests, and demo application for the `Recrovit.AspNetCore.Components.Routing` Blazor package.

## What does the repository contain?

- [src](src): the NuGet package source
- [tests/Recrovit.AspNetCore.Components.Routing.Tests](tests/Recrovit.AspNetCore.Components.Routing.Tests): bUnit and xUnit v3 based tests
- [demo/BlazorAppRenderMode](demo/BlazorAppRenderMode): a sample application for mixed render-mode scenarios

## What is its purpose?

The repository exists to develop and demonstrate a set of Blazor routing components that make it easier to combine different page-level render modes in a single application. The package is especially useful when a Blazor Web App mixes static SSR, interactive server, interactive WebAssembly, interactive auto, and client-only pages.

## How does it work?

The solution builds on top of Blazor's built-in router. Routable components receive a route mode and an optional layout through a custom attribute, and the `RecrovitRoutes` component renders the page according to the resolved route definition. Navigation can be aligned with the target page's runtime mode through `RecrovitModeAwareNavLink`, which triggers a full reload when necessary.

The intended composition is demonstrated in [demo/BlazorAppRenderMode](demo/BlazorAppRenderMode), where the host application selects a top-level render mode from the current route, and client-side navigation follows that choice.

## Detailed documentation

`Recrovit.AspNetCore.Components.Routing` is a small Blazor routing library for applications that need to mix different page render modes in a single app.

It helps you:

- assign a route mode to pages with a custom attribute
- resolve layouts per page or fall back to a default layout
- host routes through a dedicated `RecrovitRoutes` component
- detect when navigation crosses route mode boundaries
- force a full page reload when a target page requires a different runtime mode

The library is useful when your Blazor application contains a combination of:

- static server rendered pages
- interactive server pages
- interactive WebAssembly pages
- interactive auto pages
- client only pages (`InteractiveWebAssembly` with prerender disabled)

## Main building blocks

- `RecrovitPageRouteAttribute` declares the route mode and optional layout for a page
- `RecrovitRoutes` wraps Blazor's router and applies the resolved route definition
- `RecrovitModeAwareNavLink` chooses enhanced navigation or full reload based on the target page's route mode
- `AddRecrovitComponentRouting(...)` registers the routing services and options
- `RecrovitRouteModeResolver` resolves the effective route definition for an arbitrary path

## Quick start

Register the routing services and the assemblies that contain routable pages:

```csharp
using Recrovit.AspNetCore.Components.Routing.Configuration;
using Recrovit.AspNetCore.Components.Routing.Models;

builder.Services.AddRecrovitComponentRouting(options =>
{
    options.AddRouteAssembly(typeof(App).Assembly);
    options.AddRouteAssembly(typeof(Client._Imports).Assembly);
    options.DefaultLayout = typeof(MainLayout);
    options.SetNotFoundPage(RecrovitRoutesKind.Host, typeof(Pages.ServerNotFound));
});
```

Mark pages with a route mode:

```razor
@page "/reports"
@attribute [RecrovitPageRoute(RecrovitRouteMode.InteractiveServer, LayoutType = typeof(MainLayout))]
```

Render routes through `RecrovitRoutes`:

```razor
<RecrovitRoutes Kind="RecrovitRoutesKind.Client"
                AppAssembly="typeof(_Imports).Assembly"
                DefaultLayout="typeof(MainLayout)"
                NotFoundPage="typeof(NotFound)" />
```

Use `RecrovitModeAwareNavLink` when navigation may cross runtime boundaries:

```razor
<RecrovitModeAwareNavLink Href="/reports">
    Reports
</RecrovitModeAwareNavLink>
```

## Route modes and fallbacks

Supported route modes:

- `StaticServer`
- `InteractiveServer`
- `InteractiveWebAssembly`
- `InteractiveAuto`
- `ClientOnly`

`ClientOnly` maps to `InteractiveWebAssemblyRenderMode(prerender: false)`.

If a page has no `RecrovitPageRouteAttribute`, or a path does not match any discovered route template, the default fallback definition is `StaticServer` with no explicit layout. You can replace that fallback through `RecrovitRoutingOptions.FallbackDefinition`.

## Configuration and extensibility

`RecrovitRoutingOptions` provides these main hooks:

- `RouteAssemblies`
- `DefaultLayout`
- `FallbackDefinition`
- `NotFoundPages`
- `FoundContents`
- `TopLevelRenderModeFactory` for applications that want to keep the render-mode mapping next to the rest of the routing configuration

The default services can also be replaced:

- `IRecrovitPageRouteDefinitionResolver`
- `IRecrovitLayoutResolver`
- `IRecrovitReloadPolicy`

## `FoundContent` layout override

`RecrovitRoutes` can expose the matched route through `FoundContent`. You can either pass it directly on the component, or register a default wrapper per `RecrovitRoutesKind` through `RecrovitRoutingOptions`. The context supports wrapping the default route view and overriding only the resolved layout:

```csharp
builder.Services.AddRecrovitComponentRouting(options =>
{
    options.SetFoundContent(RecrovitRoutesKind.Client, context => builder =>
    {
        builder.AddContent(0, context.DefaultContent);
    });
});
```

Component-level `FoundContent` still takes precedence over the configured default.

When you need a per-instance wrapper, the component parameter works the same way:

```razor
<RecrovitRoutes Kind="RecrovitRoutesKind.Client"
                AppAssembly="typeof(_Imports).Assembly"
                DefaultLayout="typeof(MainLayout)"
                FoundContent="RenderRoute" />

@code {
    private static RenderFragment RenderRoute(RecrovitFoundContentContext context) => builder =>
    {
        context.OverrideLayout(typeof(SpecialLayout));

        builder.AddContent(0, context.DefaultContent);
    };
}
```

When no override is applied, `Definition`, the cascaded `RecrovitCurrentRouteMode`, and the default `RouteView` behavior stay unchanged. `FoundContent` does not change the route mode, navigation behavior, or the top-level render mode selection.

You can also override the layout directly. Passing `null` resets the effective layout to the `RecrovitRoutes` or routing options fallback layout:

```razor
@code {
    private static RenderFragment RenderRoute(RecrovitFoundContentContext context) => builder =>
    {
        context.OverrideLayout(typeof(SpecialLayout));

        if (ShouldUseFallbackLayout(context))
        {
            context.OverrideLayout(null);
        }

        builder.AddContent(0, context.DefaultContent);
    };
}
```
