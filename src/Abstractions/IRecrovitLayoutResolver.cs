using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Abstractions;

public interface IRecrovitLayoutResolver
{
    Type? ResolveLayout(RecrovitLayoutResolverContext context);
}
