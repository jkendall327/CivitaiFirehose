using System.Threading.Channels;
using CivitaiFirehose;
using CivitaiFirehose.Components;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var channel = Channel.CreateUnbounded<ImageModel>();
builder.Services.AddSingleton(channel.Writer);
builder.Services.AddSingleton(channel.Reader);

builder.Services.Configure<CivitaiSettings>(builder.Configuration.GetSection(nameof(CivitaiSettings)));
builder.Services.Configure<HydrusSettings>(builder.Configuration.GetSection(nameof(HydrusSettings)));

builder.Services.AddHttpClient<CivitaiClient>();
builder.Services.AddHttpClient<HydrusClient>((s, c) =>
{
    var opt = s.GetRequiredService<IOptions<HydrusSettings>>().Value;
    c.BaseAddress = new(opt.BaseUrl);
    c.DefaultRequestHeaders.Add("Hydrus-Client-API-Access-Key", opt.ApiKey);
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