using Microsoft.AspNetCore.Components;

namespace Recrovit.AspNetCore.Components.Routing.Models;

public sealed record RecrovitLayoutResolverContext(
    Type PageType,
    RecrovitPageRouteDefinition Definition,
    Type? DefaultLayout,
    RouteData RouteData);
