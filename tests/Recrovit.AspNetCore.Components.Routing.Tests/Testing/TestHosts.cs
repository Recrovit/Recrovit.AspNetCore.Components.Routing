using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Rendering;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Testing;

public sealed class ModeAwareNavLinkHost : ComponentBase
{
    [Parameter]
    public RecrovitRouteMode CurrentRouteMode { get; set; }

    [Parameter]
    public string Href { get; set; } = string.Empty;

    [Parameter]
    public NavLinkMatch Match { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<RecrovitRouteMode>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<RecrovitRouteMode>.Name), "RecrovitCurrentRouteMode");
        builder.AddAttribute(2, nameof(CascadingValue<RecrovitRouteMode>.Value), CurrentRouteMode);
        builder.AddAttribute(3, nameof(CascadingValue<RecrovitRouteMode>.ChildContent), (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenComponent<RecrovitModeAwareNavLink>(0);
            childBuilder.AddAttribute(1, nameof(RecrovitModeAwareNavLink.Href), Href);
            childBuilder.AddAttribute(2, nameof(RecrovitModeAwareNavLink.Match), Match);
            childBuilder.AddAttribute(3, nameof(RecrovitModeAwareNavLink.ChildContent), ChildContent);
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}
