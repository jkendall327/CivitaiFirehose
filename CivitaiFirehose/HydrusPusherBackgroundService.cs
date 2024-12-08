using System.Threading.Channels;

namespace CivitaiFirehose;

public class HydrusPusherBackgroundService(
    ChannelReader<ImageModel> channel,
    IHttpClientFactory factory,
    HydrusClient client,
    ILogger<HydrusPusherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await client.VerifyAccess();
        var services = await client.GetServices();
        
        await foreach (var image in channel.ReadAllAsync(stoppingToken))
        {
            var rawClient = factory.CreateClient();
            
            var bytes = await rawClient.GetByteArrayAsync(image.ImageUrl, stoppingToken);
            
            var hash = await client.SendImageToHydrus(bytes);

            await client.AssociateUrlWithImage(hash, [image.ImageUrl]);

            var service = services["my tags"];
            
            await client.AddTagsToImage(hash, ["civitai"], service);
        }
    }
}