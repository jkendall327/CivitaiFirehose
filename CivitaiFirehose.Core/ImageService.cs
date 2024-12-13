using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class ImageService(BlacklistStore blacklist, IOptions<CivitaiSettings> options, ILogger<ImageService> logger)
{
    private readonly BoundedQueue<ImageModel> _images = new(options.Value.QueryDefaults.Limit ?? 20);

    public event Func<int, Task>? NewImagesFound;
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

        if (found is 0)
        {
            return;
        }

        logger.LogInformation("Found {NewImages} new images", found);

        if (NewImagesFound is not null)
        {
            await NewImagesFound(found);
        }
    }

    public async Task ClearAndEnqueue(IEnumerable<ImageModel> images)
    {
        _images.Clear();
        await Enqueue(images);
    }
}