using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public sealed class HydrusPusherBackgroundService(
    ChannelReader<ImageModel> channel,
    HydrusClient client,
    HydrusPusher pusher,
    IOptions<HydrusSettings> settings,
    ILogger<HydrusPusherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var services = await AwaitHydrusAvailable(stoppingToken);
        
        await foreach (var image in channel.ReadAllAsync(stoppingToken))
        {
            logger.LogDebug("Got push request: {@Request}", image);
            
            try
            {
                await pusher.Push(image, services, stoppingToken);
            }
            catch (HttpRequestException e)
            {
                logger.LogError(e, "HTTP error after Hydrus was already alive; waiting");

                services = await AwaitHydrusAvailable(stoppingToken);
            }
        }
        
        logger.LogWarning("Channel exit");
    }

    private async Task<Dictionary<string, string>> AwaitHydrusAvailable(CancellationToken cancellationToken = default)
    {
        using var timer = new PeriodicTimer(settings.Value.AvailabilityWaitPeriod);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                await client.VerifyAccess(cancellationToken);
                
                logger.LogInformation("Hydrus available, getting services");
                
                var services = await client.GetServices(cancellationToken);
                
                logger.LogInformation("Got services: {@Services}", services);
                
                return services;
            }
            catch (HttpRequestException e)
            {
                logger.LogError(e, "Error when contacting Hydrus; waiting...");
            }
        }

        throw new InvalidOperationException("Timer was somehow disposed before Hydrus was available");
    }
}