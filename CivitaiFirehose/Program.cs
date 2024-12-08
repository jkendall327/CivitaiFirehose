using System.Threading.Channels;
using CivitaiFirehose;
using CivitaiFirehose.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var channel = Channel.CreateUnbounded<ImageModel>();
builder.Services.AddSingleton(channel.Writer);
builder.Services.AddSingleton(channel.Reader);

builder.Services.Configure<CivitaiSettings>(builder.Configuration.GetSection(nameof(CivitaiSettings)));

builder.Services.AddHttpClient<CivitaiClient>();
builder.Services.AddHttpClient<HydrusClient>(s =>
{
    // TODO: get from config.
    s.BaseAddress = new("http://127.0.0.1:45869/");
    s.DefaultRequestHeaders.Add("Hydrus-Client-API-Access-Key", "0f7990f1516a53b2af4bd717df380005e9c17e880139ce0157e85d401b027846");
});

builder.Services.AddScoped<JsService>();
builder.Services.AddSingleton<ICivitaiPoller, CivitaiPoller>();

builder.Services.AddHostedService<CivitaiPollingBackgroundService>();
builder.Services.AddHostedService<HydrusPusherBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();