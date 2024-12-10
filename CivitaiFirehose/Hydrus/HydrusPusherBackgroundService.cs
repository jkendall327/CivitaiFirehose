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
        
        var rawClient = factory.CreateClient();

        await foreach (var image in channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                image.PushStatus = ImagePushStatus.Pushing;
                
                var bytes = await rawClient.GetByteArrayAsync(image.ImageUrl, stoppingToken);
                
                var hash = await client.SendImageToHydrus(bytes);
                
                await client.AssociateUrlWithImage(hash, [image.ImageUrl, image.PostUrl]);
                
                var service = services["my tags"];
                
                await client.AddTagsToImage(hash, image.Tags, service);
                
                image.PushStatus = ImagePushStatus.Succeeded;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to push image {ImageUrl} to Hydrus", image.ImageUrl);
                image.PushStatus = ImagePushStatus.Failed;
                image.ErrorMessage = ex.Message;
            }
        }
    }
}