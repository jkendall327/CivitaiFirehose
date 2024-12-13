using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class ImageService(BlacklistStore blacklist, IOptions<CivitaiSettings> options, ILogger<ImageService> logger)
{
    private readonly BoundedQueue<ImageModel> _images = new(options.Value.QueryDefaults.Limit ?? 20);
    
    public Func<int, Task> NewImagesFound { get; set; } = _ => Task.CompletedTask;
    public IReadOnlyList<ImageModel> Images => _images.AsReadOnly();
    
    public async Task Enqueue(IEnumerable<ImageModel> images)
    {
        var found = 0;
        
        foreach (var image in images)
        {
            if (Images.Any(s => s.ImageUrl == image.ImageUrl)) continue;
            if (blacklist.IsBlacklisted(image.Username)) continue;
            
            _images.Enqueue(image);
            
            found++;
        }

        if (found is 0) return;
        
        logger.LogInformation("Found {NewImages} new images", found);

        // Tell the UI we have new images.
        await NewImagesFound(found);
    }

    public async Task ClearAndEnqueue(IEnumerable<ImageModel> images)
    {
        _images.Clear();
        await Enqueue(images);
    }
}