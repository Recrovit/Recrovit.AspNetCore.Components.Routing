using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Models;
using Recrovit.AspNetCore.Components.Routing.Tests.Testing;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Components;

public sealed class RecrovitModeAwareNavLinkTests : BunitContext
{
    public RecrovitModeAwareNavLinkTests()
    {
        Services.AddTestRouting();
    }

    [Fact]
    public void SameRouteMode_ShouldRenderNavLinkMarkup()
    {
        var cut = RenderNavLink(RecrovitRouteMode.StaticServer, "/probe");

        var anchor = cut.Find("a.mode-nav-link");

        Assert.Equal("/probe", anchor.GetAttribute("href"));
        Assert.Null(anchor.GetAttribute("data-enhance-nav"));
        Assert.Equal("Target", anchor.TextContent);
    }

    [Fact]
    public void DifferentRouteMode_ShouldRenderForceLoadAnchor()
    {
        var cut = RenderNavLink(RecrovitRouteMode.StaticServer, "/client-only-probe");

        var anchor = cut.Find("a.mode-nav-link");

        Assert.Equal("/client-only-probe", anchor.GetAttribute("href"));
        Assert.Equal("false", anchor.GetAttribute("data-enhance-nav"));
        Assert.Equal("Target", anchor.TextContent);
        Assert.Empty(cut.FindComponents<NavLink>());
    }

    [Fact]
    public void CustomReloadPolicy_ShouldInfluenceRenderedBranch()
    {
        Services.AddSingleton<IRecrovitReloadPolicy, CustomReloadPolicy>();

        var cut = RenderNavLink(RecrovitRouteMode.InteractiveServer, "/client-only-probe");

        Assert.Single(cut.FindComponents<NavLink>());
        Assert.Null(cut.Find("a.mode-nav-link").GetAttribute("data-enhance-nav"));
    }

    [Fact]
    public void ClickingForceLoadAnchor_ShouldNavigateWithForceLoad()
    {
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo("http://localhost/probe");

        var cut = RenderNavLink(RecrovitRouteMode.StaticServer, "/client-only-probe");
        cut.Find("a.mode-nav-link").Click();

        var bunitNavigationManager = Assert.IsType<BunitNavigationManager>(navigationManager);
        var history = bunitNavigationManager.History.First();
        Assert.Equal("/client-only-probe", history.Uri);
        Assert.True(history.Options.ForceLoad);
    }

    [Fact]
    public void MatchAll_ShouldOnlyBeActiveForExactPath()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("http://localhost/items/special");

        var exact = RenderNavLink(RecrovitRouteMode.StaticServer, "/items", NavLinkMatch.All);
        var prefix = RenderNavLink(RecrovitRouteMode.StaticServer, "/items", NavLinkMatch.Prefix);

        Assert.DoesNotContain("active", exact.Find("a.mode-nav-link").ClassList);
        Assert.Contains("active", prefix.Find("a.mode-nav-link").ClassList);
    }

    [Fact]
    public void ForceLoadAnchor_ShouldUseActiveCssClassWhenCurrentTargetMatches()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("http://localhost/client-only-probe");

        var cut = RenderNavLink(RecrovitRouteMode.StaticServer, "/CLIENT-ONLY-PROBE/");

        Assert.Contains("active", cut.Find("a.mode-nav-link").ClassList);
    }

    [Fact]
    public void UnknownTarget_ShouldUseFallbackRouteModeWhenEvaluatingReload()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("http://localhost/probe");

        var cut = RenderNavLink(RecrovitRouteMode.StaticServer, "/does-not-exist");

        Assert.Null(cut.Find("a.mode-nav-link").GetAttribute("data-enhance-nav"));
    }

    [Fact]
    public void HrefNormalization_ShouldHandleCaseAndSlashDifferences()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("http://localhost/items/special");

        var cut = RenderNavLink(RecrovitRouteMode.StaticServer, "/ITEMS/SPECIAL/");

        Assert.Contains("active", cut.Find("a.mode-nav-link").ClassList);
    }

    private IRenderedComponent<ModeAwareNavLinkHost> RenderNavLink(
        RecrovitRouteMode currentRouteMode,
        string href,
        NavLinkMatch match = NavLinkMatch.Prefix)
        => Render<ModeAwareNavLinkHost>(parameters => parameters
            .Add(component => component.CurrentRouteMode, currentRouteMode)
            .Add(component => component.Href, href)
            .Add(component => component.Match, match)
            .Add(component => component.ChildContent, (RenderFragment)(builder => builder.AddContent(0, "Target"))));
}
