using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

/// <summary>
/// Encapsulates storage for the currently-loaded images.
/// Ensures images are added in the right order, aren't duplicated, and aren't on the username blacklist.
/// </summary>
public sealed class ImageService(
    BlacklistStore blacklist, 
    IOptions<CivitaiSettings> options, 
    Meters meters,
    ILogger<ImageService> logger)
{
    private readonly BoundedQueue<ImageModel> _images = new(options.Value.QueryDefaults.Limit ?? 20);

    public event Func<int, Task>? NewImagesFound;
    public IReadOnlyList<ImageModel> Images => _images.AsReadOnly();
    
    public async Task Enqueue(IList<ImageModel> images)
    {
        var found = 0;
        
        foreach (var image in images)
        {
            if (Images.Any(s => s.ImageUrl == image.ImageUrl)) continue;
            if (blacklist.IsBlacklisted(image.Username)) continue;
            
            _images.Enqueue(image);
            
            found++;
        }

        if (found is 0)
        {
            logger.LogInformation("Processed {ImageCount}, but none were new", images.Count);
            return;
        }

        logger.LogInformation("Found {NewImages} new images", found);
        meters.ReportNewImages(found);

        if (NewImagesFound is not null)
        {
            await NewImagesFound(found);
        }
    }

    public async Task ClearAndEnqueue(IList<ImageModel> images)
    {
        images.Clear();
        await Enqueue(images);
    }
    
    public void Clear() => _images.Clear();
}