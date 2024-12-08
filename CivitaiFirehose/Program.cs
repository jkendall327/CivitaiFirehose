using CivitaiFirehose;
using CivitaiFirehose.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, options) => {
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddCivitaiServices(builder.Configuration);
builder.Services.AddHydrusServices(builder.Configuration);
builder.Services.AddWebServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();