using Microsoft.Extensions.DependencyInjection;
using Recrovit.AspNetCore.Components.Routing.Configuration;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Testing;

internal static class TestServiceCollectionExtensions
{
    public static IServiceCollection AddTestRouting(
        this IServiceCollection services,
        Action<RecrovitRoutingOptions>? configure = null)
    {
        services.AddRecrovitComponentRouting(options =>
        {
            options.AddRouteAssembly(typeof(StaticServerPage).Assembly);
            options.DefaultLayout = typeof(DefaultProbeLayout);
            options.SetNotFoundPage(RecrovitRoutesKind.Host, typeof(HostNotFoundPage));
            options.SetNotFoundPage(RecrovitRoutesKind.Client, typeof(ClientNotFoundPage));
            configure?.Invoke(options);
        });

        return services;
    }
}
