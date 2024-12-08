using System.Threading.Channels;

namespace CivitaiFirehose;

public class HydrusPusherBackgroundService(ChannelReader<ImageModel> channel, ILogger<HydrusPusherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var image in channel.ReadAllAsync(stoppingToken))
        {
            
        }
    }
}