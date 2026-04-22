using Recrovit.AspNetCore.Components.Routing.Abstractions;
using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Infrastructure.Policies;

internal sealed class DefaultRecrovitReloadPolicy : IRecrovitReloadPolicy
{
    public bool RequiresFullReload(RecrovitRouteMode currentMode, RecrovitRouteMode targetMode)
        => currentMode != targetMode;
}
