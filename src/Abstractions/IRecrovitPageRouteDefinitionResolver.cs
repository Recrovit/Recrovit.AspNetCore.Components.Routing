using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Abstractions;

public interface IRecrovitPageRouteDefinitionResolver
{
    RecrovitPageRouteDefinition GetDefinition(Type pageType);

    RecrovitPageRouteDefinition GetFallbackDefinition();
}
