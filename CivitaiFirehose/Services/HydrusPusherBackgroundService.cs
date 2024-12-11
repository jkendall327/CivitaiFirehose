using System.Threading.Channels;

namespace CivitaiFirehose;

public class HydrusPusherBackgroundService(
    ChannelReader<ImageModel> channel,
    HydrusClient client,
    HydrusPusher pusher,
    ILogger<HydrusPusherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Dictionary<string, string> services;
        
        try
        {
            await client.VerifyAccess();
            services = await client.GetServices();
        }
        catch (HttpRequestException e)
        {
            logger.LogError(e, "Error when contacting Hydrus; discontinuing service");
            return;
        }
        
        await foreach (var image in channel.ReadAllAsync(stoppingToken))
        {
            await pusher.Push(image, services, stoppingToken);
        }
    }
}