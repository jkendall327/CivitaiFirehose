using Microsoft.Extensions.Logging;

namespace CivitaiFirehose;

public sealed class HydrusPusher(    
    IHttpClientFactory factory,
    HydrusClient client,
    Meters meters,
    ILogger<HydrusPusher> logger)
{
    public event Func<Task>? OnStateChanged;
    private void NotifyStateChanged() => OnStateChanged?.Invoke();
    
    public async Task Push(ImageModel image, Dictionary<string, string> services, CancellationToken stoppingToken = default)
    {
        meters.ReportImagePushed();
        
        try
        {
            var rawClient = factory.CreateClient();

            image.PushStatus = ImagePushStatus.Pushing;
            NotifyStateChanged();
            
            var bytes = await rawClient.GetByteArrayAsync(image.ImageUrl, stoppingToken);
                
            var hash = await client.SendImageToHydrus(bytes);
                
            await client.AssociateUrlWithImage(hash, [image.ImageUrl, image.PostUrl]);
                
            var service = services["my tags"];
                
            await client.AddTagsToImage(hash, image.Tags, service);
                
            image.PushStatus = ImagePushStatus.Succeeded;
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to push image {ImageUrl} to Hydrus", image.ImageUrl);
            meters.ReportPushFailed();
            
            image.PushStatus = ImagePushStatus.Failed;
            image.ErrorMessage = ex.Message;
            
            NotifyStateChanged();

            throw;
        }
    }
}