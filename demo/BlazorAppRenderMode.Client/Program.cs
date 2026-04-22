using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Recrovit.AspNetCore.Components.Routing.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddRecrovitComponentRouting(options =>
{
    options.AddRouteAssembly(typeof(BlazorAppRenderMode.Client._Imports).Assembly);
});

await builder.Build().RunAsync();
