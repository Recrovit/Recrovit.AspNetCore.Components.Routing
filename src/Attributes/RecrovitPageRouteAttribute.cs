using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RecrovitPageRouteAttribute(
    RecrovitRouteMode routeMode) : Attribute
{
    public RecrovitRouteMode RouteMode { get; } = routeMode;

    public Type? LayoutType { get; init; }
}
