namespace CivitaiFirehose;

public class HydrusPusher(    
    IHttpClientFactory factory,
    HydrusClient client,
    ILogger<HydrusPusherBackgroundService> logger)
{
    public async Task Push(ImageModel image, Dictionary<string, string> services, CancellationToken stoppingToken = default)
    {
        try
        {
            var rawClient = factory.CreateClient();

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