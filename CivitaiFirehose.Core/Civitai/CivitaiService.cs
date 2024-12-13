using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiService(
    CivitaiClient client,
    BlacklistStore blacklist,
    IOptions<CivitaiSettings> options, 
    ILogger<CivitaiService> logger) : ICivitaiService
{
    // Given a default value to avoid annoying nullability. We assume the UI will always actually hook onto this.
    public Func<int, Task> NewImagesFound { get; set; } = _ => Task.CompletedTask;
    public BoundedQueue<ImageModel> Images { get; } = new(options.Value.QueryDefaults.Limit ?? 20);
    
    public async Task PollCivitai(CancellationToken ct)
    {
        var query = options.Value.QueryDefaults.Clone();
        
        var response = await client.GetImages(query, ct);

        await EnqueueImages(response);
    }
    
    public async Task<List<ImageModel>> GetAllImagesFromPost(int postId, CancellationToken ct = default)
    {
        var query = options.Value.QueryDefaults.Clone();
        
        query.PostId = postId;
        query.Limit = 200;
        
        var response = await client.GetImages(query, ct);
        
        var images = response.items.Select(ToImageModel);

        return images.ToList();
    }

    private async Task EnqueueImages(CivitaiResponse response)
    {
        var found = 0;

        foreach (var img in response.items)
        {
            if (Images.Any(s => s.ImageUrl == img.url)) continue;
            if (blacklist.IsBlacklisted(img.username)) continue;
            
            var image = ToImageModel(img);
            
            Images.Enqueue(image);
            
            found++;
        }

        if (found is 0) return;
        
        logger.LogInformation("Found {NewImages} new images", found);

        // Tell the UI we have new images.
        await NewImagesFound(found);
    }

    private ImageModel ToImageModel(Item item)
    {
        var tags = TagExtractor.GetTagsFromResponse(item);
            
        var image = new ImageModel(item.url, item.postId, item.username, tags);

        return image;
    }
}