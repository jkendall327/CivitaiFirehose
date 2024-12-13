using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class ImageService(
    BlacklistStore blacklist,
    IOptions<CivitaiSettings> options, 
    ILogger<ImageService> logger)
{
    // Given a default value to avoid annoying nullability. We assume the UI will always actually hook onto this.
    public Func<int, Task> NewImagesFound { get; set; } = _ => Task.CompletedTask;
    public BoundedQueue<ImageModel> Images { get; } = new(options.Value.QueryDefaults.Limit ?? 20);

    private async Task EnqueueImages(IEnumerable<ImageModel> images)
    {
        var found = 0;

        foreach (var image in images)
        {
            if (Images.Any(s => s.ImageUrl == image.ImageUrl)) continue;
            if (blacklist.IsBlacklisted(image.Username)) continue;
            
            Images.Enqueue(image);
            
            found++;
        }

        if (found is 0) return;
        
        logger.LogInformation("Found {NewImages} new images", found);

        // Tell the UI we have new images.
        await NewImagesFound(found);
    }
}