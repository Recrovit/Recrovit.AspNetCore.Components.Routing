using Microsoft.Extensions.Options;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Attributes;
using Recrovit.AspNetCore.Components.Routing.Configuration;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Infrastructure.Resolvers;

internal sealed class DefaultRecrovitPageRouteDefinitionResolver(IOptions<RecrovitRoutingOptions> options)
    : IRecrovitPageRouteDefinitionResolver
{
    private readonly RecrovitRoutingOptions _options = options.Value;

    public RecrovitPageRouteDefinition GetDefinition(Type pageType)
    {
        var attribute = pageType.GetCustomAttributes(typeof(RecrovitPageRouteAttribute), inherit: false)
            .OfType<RecrovitPageRouteAttribute>()
            .SingleOrDefault();

        return attribute is null
            ? GetFallbackDefinition()
            : RecrovitRouteModeMapper.CreateDefinition(attribute.RouteMode, attribute.LayoutType);
    }

    public RecrovitPageRouteDefinition GetFallbackDefinition()
        => _options.FallbackDefinition;
}
