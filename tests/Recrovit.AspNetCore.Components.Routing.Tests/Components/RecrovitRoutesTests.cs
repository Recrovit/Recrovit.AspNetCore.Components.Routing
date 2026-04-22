using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Configuration;
using Recrovit.AspNetCore.Components.Routing.Models;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Components;

public sealed class RecrovitRoutesTests : BunitContext
{
    public RecrovitRoutesTests()
    {
        Services.AddTestRouting();
        Services.AddSingleton<IRecrovitLayoutResolver, RouteModeAwareLayoutResolver>();
    }

    [Fact]
    public void WithoutAppAssemblyParameter_ShouldUseFirstConfiguredRouteAssembly()
    {
        NavigateTo("http://localhost/probe");

        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client));

        AssertRouteModeMarkup(cut, "StaticServer", "StaticServer", "static-layout");
    }

    [Fact]
    public void WithoutAnyRouteAssembly_ShouldThrowComponentError()
    {
        Services.Clear();
        Services.AddLogging();
        Services.AddOptions<RecrovitRoutingOptions>();
        Services.AddSingleton<IRecrovitPageRouteDefinitionResolver, CustomPageRouteDefinitionResolver>();
        Services.AddSingleton<IRecrovitLayoutResolver, CustomLayoutResolver>();

        Assert.ThrowsAny<Exception>(() =>
            Render<RecrovitRoutes>(parameters => parameters.Add(component => component.Kind, RecrovitRoutesKind.Client)));
    }

    [Fact]
    public void AdditionalAssemblies_ShouldExposeRoutesOutsideAppAssembly()
    {
        NavigateTo("http://localhost/probe");

        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client)
            .Add(component => component.AppAssembly, typeof(RecrovitRoutes).Assembly)
            .Add(component => component.AdditionalAssemblies, [typeof(StaticServerPage).Assembly]));

        AssertRouteModeMarkup(cut, "StaticServer", "StaticServer", "static-layout");
    }

    [Fact]
    public void DefaultLayoutParameter_ShouldOverrideOptionsDefaultLayout()
    {
        Services.AddSingleton<IRecrovitLayoutResolver, TrackingLayoutResolver>();
        NavigateTo("http://localhost/layoutless");

        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client)
            .Add(component => component.AppAssembly, typeof(LayoutlessPage).Assembly)
            .Add(component => component.DefaultLayout, typeof(OverrideProbeLayout)));

        Assert.Equal("override-layout", cut.Find("#layout-marker").TextContent);
    }

    [Fact]
    public void NotFoundPageParameter_ShouldOverrideConfiguredNotFoundPage()
    {
        NavigateTo("http://localhost/missing");

        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client)
            .Add(component => component.NotFoundPage, typeof(OverrideNotFoundPage)));

        Assert.Equal("override-not-found", cut.Find("#not-found").TextContent);
    }

    [Theory]
    [InlineData(RecrovitRoutesKind.Host, "host-not-found")]
    [InlineData(RecrovitRoutesKind.Client, "client-not-found")]
    public void KindSpecificNotFoundPage_ShouldBeResolvedFromOptions(RecrovitRoutesKind kind, string expectedText)
    {
        NavigateTo("http://localhost/unknown");

        var cut = Render<RecrovitRoutes>(parameters => parameters.Add(component => component.Kind, kind));

        Assert.Equal(expectedText, cut.Find("#not-found").TextContent);
    }

    [Fact]
    public void FocusSelector_ShouldFlowToFoundContentHost()
    {
        NavigateTo("http://localhost/probe");

        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client)
            .Add(component => component.FocusSelector, "#shell"));

        var focusOnNavigate = cut.FindComponent<Microsoft.AspNetCore.Components.Routing.FocusOnNavigate>();
        Assert.Equal("#shell", focusOnNavigate.Instance.Selector);
    }

    [Fact]
    public void FoundContent_ShouldReceiveResolvedContextAndDefaultContent()
    {
        NavigateTo("http://localhost/layoutless");

        RecrovitFoundContentContext? capturedContext = null;
        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client)
            .Add(component => component.AppAssembly, typeof(LayoutlessPage).Assembly)
            .Add(component => component.DefaultLayout, typeof(OverrideProbeLayout))
            .Add(component => component.FoundContent, (RenderFragment<RecrovitFoundContentContext>)(context => builder =>
            {
                capturedContext = context;
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "id", "found-content-marker");
                builder.AddContent(2, context.FocusSelector);
                builder.AddContent(3, "|");
                builder.AddContent(4, context.Kind.ToString());
                builder.CloseElement();
                builder.AddContent(5, context.DefaultContent);
            })));

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedContext);
            Assert.Equal("h1|Client", cut.Find("#found-content-marker").TextContent);
            Assert.Equal(typeof(LayoutlessPage), capturedContext!.RouteData.PageType);
            Assert.Equal(RecrovitRouteMode.InteractiveServer, capturedContext.Definition.RouteMode);
            Assert.Equal(typeof(OverrideProbeLayout), capturedContext.DefaultLayout);
            Assert.NotNull(capturedContext.DefaultContent);
            Assert.Equal("override-layout", cut.Find("#layout-marker").TextContent);
        });
    }

    [Fact]
    public void FoundContent_ShouldSupportLayoutOverrideTogetherWithDefaultLayoutParameter()
    {
        NavigateTo("http://localhost/layoutless");

        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client)
            .Add(component => component.AppAssembly, typeof(LayoutlessPage).Assembly)
            .Add(component => component.DefaultLayout, typeof(DefaultProbeLayout))
            .Add(component => component.FoundContent, (RenderFragment<RecrovitFoundContentContext>)(context => builder =>
            {
                context.OverrideLayout(typeof(OverrideProbeLayout));
                builder.AddContent(0, context.DefaultContent);
            })));

        cut.WaitForAssertion(() => Assert.Equal("override-layout", cut.Find("#layout-marker").TextContent));
    }

    [Fact]
    public void FoundContent_ShouldBeResolvedFromOptionsWhenParameterIsNotProvided()
    {
        RecrovitFoundContentContext? capturedContext = null;
        Services.PostConfigure<RecrovitRoutingOptions>(options =>
        {
            options.SetFoundContent(RecrovitRoutesKind.Client, context => builder =>
            {
                capturedContext = context;
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "id", "configured-found-content-marker");
                builder.AddContent(2, context.Kind.ToString());
                builder.CloseElement();
                builder.AddContent(3, context.DefaultContent);
            });
        });

        NavigateTo("http://localhost/layoutless");

        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client)
            .Add(component => component.AppAssembly, typeof(LayoutlessPage).Assembly)
            .Add(component => component.DefaultLayout, typeof(OverrideProbeLayout)));

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedContext);
            Assert.Equal("Client", cut.Find("#configured-found-content-marker").TextContent);
            Assert.Equal("override-layout", cut.Find("#layout-marker").TextContent);
        });
    }

    [Fact]
    public void ExplicitFoundContent_ShouldOverrideConfiguredFoundContent()
    {
        Services.PostConfigure<RecrovitRoutingOptions>(options =>
        {
            options.SetFoundContent(RecrovitRoutesKind.Client, _ => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "id", "configured-found-content-marker");
                builder.AddContent(2, "configured");
                builder.CloseElement();
            });
        });

        NavigateTo("http://localhost/layoutless");

        var cut = Render<RecrovitRoutes>(parameters => parameters
            .Add(component => component.Kind, RecrovitRoutesKind.Client)
            .Add(component => component.AppAssembly, typeof(LayoutlessPage).Assembly)
            .Add(component => component.FoundContent, (RenderFragment<RecrovitFoundContentContext>)(_ => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "id", "explicit-found-content-marker");
                builder.AddContent(2, "explicit");
                builder.CloseElement();
            })));

        Assert.Equal("explicit", cut.Find("#explicit-found-content-marker").TextContent);
        Assert.Empty(cut.FindAll("#configured-found-content-marker"));
    }

    private void NavigateTo(string uri)
        => Services.GetRequiredService<NavigationManager>().NavigateTo(uri);

    private static void AssertRouteModeMarkup(IRenderedComponent<RecrovitRoutes> cut, string routeMode, string definitionMode, string layoutMarker)
    {
        Assert.Equal(routeMode, cut.Find("#current-route-mode").TextContent);
        Assert.Equal(definitionMode, cut.Find("#current-definition-mode").TextContent);
        Assert.Equal(layoutMarker, cut.Find("#layout-marker").TextContent);
    }
}
