using BlazorAppRenderMode.Components;
using Recrovit.AspNetCore.Components.Routing.Configuration;
using Recrovit.AspNetCore.Components.Routing.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddRecrovitComponentRouting(options =>
    {
        options.AddRouteAssembly(typeof(App).Assembly);
        options.AddRouteAssembly(typeof(BlazorAppRenderMode.Client._Imports).Assembly);
        options.DefaultLayout = typeof(AppLayout);
        options.SetNotFoundPage(RecrovitRoutesKind.Host, typeof(BlazorAppRenderMode.Components.Pages.ServerNotFound));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorAppRenderMode.Client._Imports).Assembly);

app.Run();
