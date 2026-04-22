namespace Recrovit.AspNetCore.Components.Routing.Models;

public sealed record RecrovitPageRouteDefinition(
    RecrovitRouteMode RouteMode,
    Type? LayoutType);
