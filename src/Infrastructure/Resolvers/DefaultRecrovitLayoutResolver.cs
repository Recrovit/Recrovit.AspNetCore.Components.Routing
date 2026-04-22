using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Infrastructure.Resolvers;

internal sealed class DefaultRecrovitLayoutResolver : IRecrovitLayoutResolver
{
    public Type? ResolveLayout(RecrovitLayoutResolverContext context)
        => context.Definition.LayoutType ?? context.DefaultLayout;
}
