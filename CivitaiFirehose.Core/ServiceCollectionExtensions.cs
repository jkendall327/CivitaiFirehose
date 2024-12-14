using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCivitaiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var channel = Channel.CreateUnbounded<ImageModel>();
        services.AddSingleton(channel.Writer);
        services.AddSingleton(channel.Reader);

        services.Configure<CivitaiSettings>(configuration.GetSection(nameof(CivitaiSettings)));
        
        services.AddHttpClient<CivitaiClient>();
        services.AddSingleton<ICivitaiService, CivitaiService>();
        services.AddSingleton<ImageMapper>();
        
        services.AddScoped<BlacklistStore>();
        services.AddScoped<ImageService>();
        services.AddScoped<HomeViewmodel>();
        
        return services;
    }

    public static IServiceCollection AddHydrusServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HydrusSettings>(configuration.GetSection(nameof(HydrusSettings)));
            
        services.AddHttpClient<HydrusClient>((s, c) =>
        {
            var opt = s.GetRequiredService<IOptions<HydrusSettings>>().Value;
            c.BaseAddress = new(opt.BaseUrl);
            c.DefaultRequestHeaders.Add("Hydrus-Client-API-Access-Key", opt.ApiKey);
        });

        services.AddSingleton<HydrusPusher>();

        return services;
    }
}