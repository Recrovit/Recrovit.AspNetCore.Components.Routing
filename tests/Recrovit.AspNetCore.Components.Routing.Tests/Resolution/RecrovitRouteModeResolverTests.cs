using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Models;
using Recrovit.AspNetCore.Components.Routing.Resolution;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Resolution;

public sealed class RecrovitRouteModeResolverTests
{
    private static readonly RecrovitPageRouteDefinition FallbackDefinition = new(RecrovitRouteMode.StaticServer, null);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/")]
    public void Resolve_ShouldReturnFallbackForEmptyOrRootPath(string? requestPath)
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly);

        var definition = resolver.Resolve(requestPath);

        Assert.Equal(FallbackDefinition, definition);
    }

    [Fact]
    public void Resolve_ShouldMatchFixedRoute()
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly);

        var definition = resolver.Resolve("/probe");

        Assert.Equal(RecrovitRouteMode.StaticServer, definition.RouteMode);
    }

    [Fact]
    public void Resolve_ShouldBeCaseInsensitive()
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly);

        var definition = resolver.Resolve("/WASM-PROBE");

        Assert.Equal(RecrovitRouteMode.InteractiveWebAssembly, definition.RouteMode);
    }

    [Theory]
    [InlineData("probe")]
    [InlineData("/probe/")]
    [InlineData(" /probe/ ")]
    public void Resolve_ShouldIgnoreLeadingAndTrailingSlashes(string requestPath)
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly);

        var definition = resolver.Resolve(requestPath);

        Assert.Equal(RecrovitRouteMode.StaticServer, definition.RouteMode);
    }

    [Fact]
    public void Resolve_ShouldMatchParameterizedRoute()
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly);

        var definition = resolver.Resolve("/items/123");

        Assert.Equal(RecrovitRouteMode.InteractiveAuto, definition.RouteMode);
    }

    [Fact]
    public void Resolve_ShouldPreferSpecificRouteOverParameterizedRoute()
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly);

        var definition = resolver.Resolve("/items/special");

        Assert.Equal(RecrovitRouteMode.StaticServer, definition.RouteMode);
    }

    [Fact]
    public void Resolve_ShouldNotMatchShorterOrLongerRoute()
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly);

        Assert.Equal(FallbackDefinition, resolver.Resolve("/items"));
        Assert.Equal(FallbackDefinition, resolver.Resolve("/items/123/details"));
    }

    [Fact]
    public void Resolve_ShouldIgnoreAssembliesWithoutMatchingRoutes()
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly, typeof(RecrovitRoutes).Assembly);

        var definition = resolver.Resolve("/missing");

        Assert.Equal(FallbackDefinition, definition);
    }

    [Fact]
    public void Resolve_ShouldUseFallbackWhenRecrovitPageRouteAttributeIsMissing()
    {
        var resolver = CreateResolver(typeof(FallbackRoutePage).Assembly);

        var definition = resolver.Resolve("/fallback-only");

        Assert.Equal(FallbackDefinition, definition);
    }

    [Fact]
    public void Resolve_ShouldIgnoreDuplicateAssemblies()
    {
        var resolver = CreateResolver(typeof(StaticServerPage).Assembly, typeof(StaticServerPage).Assembly);

        var definition = resolver.Resolve("/probe");

        Assert.Equal(RecrovitRouteMode.StaticServer, definition.RouteMode);
    }

    private static RecrovitRouteModeResolver CreateResolver(params System.Reflection.Assembly[] assemblies)
        => new(new StubPageRouteDefinitionResolver(FallbackDefinition), assemblies);

    private sealed class StubPageRouteDefinitionResolver(RecrovitPageRouteDefinition fallbackDefinition)
        : IRecrovitPageRouteDefinitionResolver
    {
        public RecrovitPageRouteDefinition GetDefinition(Type pageType)
            => pageType.GetCustomAttributes(typeof(global::Recrovit.AspNetCore.Components.Routing.Attributes.RecrovitPageRouteAttribute), inherit: false)
                .OfType<global::Recrovit.AspNetCore.Components.Routing.Attributes.RecrovitPageRouteAttribute>()
                .SingleOrDefault() is { } attribute
                ? new RecrovitPageRouteDefinition(attribute.RouteMode, attribute.LayoutType)
                : fallbackDefinition;

        public RecrovitPageRouteDefinition GetFallbackDefinition()
            => fallbackDefinition;
    }
}
