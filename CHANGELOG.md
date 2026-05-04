# Release History

This file contains the release history for `Recrovit.AspNetCore.Components.Routing`.

## [10.0.0] - 2026-04-22

- Initial release
  - Published the first stable package version on the `main` branch.

### Features

- Routing model
  - `RecrovitPageRouteAttribute` for declaring the route mode and optional layout of routable pages.
  - `RecrovitRouteModeResolver` for resolving the effective route definition of an arbitrary path.
  - Cascaded route context for hosted pages through the current route mode and current page route definition.
  - Support for mixed page-level render modes in the same application:
    - `StaticServer`
    - `InteractiveServer`
    - `InteractiveWebAssembly`
    - `InteractiveAuto`
    - `ClientOnly`
- Components
  - `RecrovitRoutes` for hosting Blazor routes through resolved page route definitions.
  - Route discovery across the configured app assembly and additional route assemblies.
  - `RecrovitModeAwareNavLink` for switching between enhanced navigation and full page reload when the target page requires a different runtime mode.
  - `FoundContent` support for wrapping matched route content and overriding the resolved layout when needed.
  - `FocusSelector` support for route-transition focus management.
- Configuration
  - `AddRecrovitComponentRouting(...)` for registering routing services, route assemblies, fallback behavior, and route-specific configuration.
  - Default and per-page layout resolution with configurable fallback layout support.
  - Configurable `NotFound` page mapping per `RecrovitRoutesKind`.
  - Configurable top-level render mode selection through `TopLevelRenderModeFactory`.
- Extensibility
  - Replaceable default services for page route definition resolution, layout resolution, and reload policy behavior.
