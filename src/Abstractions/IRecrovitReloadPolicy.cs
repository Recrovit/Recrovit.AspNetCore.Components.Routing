using Recrovit.AspNetCore.Components.Routing.Models;

namespace Recrovit.AspNetCore.Components.Routing.Abstractions;

public interface IRecrovitReloadPolicy
{
    bool RequiresFullReload(RecrovitRouteMode currentMode, RecrovitRouteMode targetMode);
}
