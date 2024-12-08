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
builder.Services.AddScoped<JsService>();
builder.Services.AddSingleton<ICivitaiPoller, CivitaiPoller>();
builder.Services.AddHostedService<CivitaiPollingBackgroundService>();

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