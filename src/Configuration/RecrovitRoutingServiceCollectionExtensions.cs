using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Infrastructure.Policies;
using Recrovit.AspNetCore.Components.Routing.Infrastructure.Resolvers;
using Recrovit.AspNetCore.Components.Routing.Resolution;

namespace Recrovit.AspNetCore.Components.Routing.Configuration;

public static class RecrovitRoutingServiceCollectionExtensions
{
    public static IServiceCollection AddRecrovitComponentRouting(
        this IServiceCollection services,
        Action<RecrovitRoutingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<RecrovitRoutingOptions>()
            .Configure(configure)
            .Validate(
                options => options.RouteAssemblies.Count > 0,
                "At least one route assembly must be configured.")
            .ValidateOnStart();

        services.TryAddSingleton<IRecrovitPageRouteDefinitionResolver, DefaultRecrovitPageRouteDefinitionResolver>();
        services.TryAddSingleton<IRecrovitLayoutResolver, DefaultRecrovitLayoutResolver>();
        services.TryAddSingleton<IRecrovitReloadPolicy, DefaultRecrovitReloadPolicy>();
        services.TryAddSingleton<RecrovitRouteModeResolver>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RecrovitRoutingOptions>>().Value;
            var resolver = serviceProvider.GetRequiredService<IRecrovitPageRouteDefinitionResolver>();

            return new RecrovitRouteModeResolver(resolver, options.RouteAssemblies);
        });

        return services;
    }
}
