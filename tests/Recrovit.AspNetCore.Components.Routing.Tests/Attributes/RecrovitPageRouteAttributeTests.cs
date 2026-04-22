using System.Reflection;
using Recrovit.AspNetCore.Components.Routing.Attributes;
using Recrovit.AspNetCore.Components.Routing.Models;
using Xunit;

namespace Recrovit.AspNetCore.Components.Routing.Tests.Attributes;

public sealed class RecrovitPageRouteAttributeTests
{
    [Fact]
    public void Constructor_ShouldPreserveRouteMode()
    {
        var attribute = new RecrovitPageRouteAttribute(RecrovitRouteMode.InteractiveAuto);

        Assert.Equal(RecrovitRouteMode.InteractiveAuto, attribute.RouteMode);
    }

    [Fact]
    public void LayoutType_ShouldBeSettableViaObjectInitializer()
    {
        var attribute = new RecrovitPageRouteAttribute(RecrovitRouteMode.InteractiveServer)
        {
            LayoutType = typeof(TestLayout),
        };

        Assert.Equal(typeof(TestLayout), attribute.LayoutType);
    }

    [Fact]
    public void AttributeUsage_ShouldMatchPublicContract()
    {
        var usage = typeof(RecrovitPageRouteAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        Assert.NotNull(usage);
        Assert.Equal(AttributeTargets.Class, usage!.ValidOn);
        Assert.False(usage.AllowMultiple);
        Assert.False(usage.Inherited);
    }

    private sealed class TestLayout;
}
