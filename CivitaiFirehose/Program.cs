using CivitaiFirehose;
using CivitaiFirehose.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((_, options) => {
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.AddLogging();
builder.AddObservability();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSystemd();

builder.Services.AddAppOptions(builder.Configuration);
builder.Services.AddCivitaiServices();
builder.Services.AddHydrusServices();
builder.Services.AddHostedService<HydrusPusherBackgroundService>();
builder.Services.AddScoped<JsService>();
builder.Services.AddSingleton<Meters>();

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