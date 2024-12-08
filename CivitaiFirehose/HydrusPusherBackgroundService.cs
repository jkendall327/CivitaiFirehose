using System.Threading.Channels;

namespace CivitaiFirehose;

public class HydrusPusherBackgroundService(
    ChannelReader<ImageModel> channel,
    HydrusClient client,
    ILogger<HydrusPusherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await client.VerifyAccess();
        
        await foreach (var image in channel.ReadAllAsync(stoppingToken))
        {
            await client.SendImageToHydrus(image.ImageUrl);
        }
    }
}