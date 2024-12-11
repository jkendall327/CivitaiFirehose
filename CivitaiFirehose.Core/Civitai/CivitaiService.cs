using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiService(
    CivitaiClient client, 
    IOptions<CivitaiSettings> options, 
    ILogger<CivitaiService> logger) : ICivitaiPoller
{
    // Given a default value to avoid annoying nullability. We assume the UI will always actually hook onto this.
    public Func<int, Task> NewImagesFound { get; set; } = _ => Task.CompletedTask;
    public BoundedQueue<ImageModel> Images { get; } = new(options.Value.QueryDefaults.Limit ?? 20);

    private readonly HashSet<string> _blacklistedUsers = [..options.Value.ExcludedCreators];
    
    public async Task PollCivitai(CancellationToken ct)
    {
        var response = await client.GetImages(ct);

        var found = 0;

        foreach (var img in response.items)
        {
            if (Images.Any(s => s.ImageUrl == img.url)) continue;
            if (_blacklistedUsers.Contains(img.username)) continue;
            
            var image = ToImageModel(img);
            
            Images.Enqueue(image);
            
            found++;
        }

        if (found is 0) return;
        
        logger.LogInformation("Found {NewImages} new images", found);

        // Tell the UI we have new images.
        await NewImagesFound(found);
    }
    
    public void BlacklistUser(string username) => _blacklistedUsers.Add(username);

    public async Task<List<ImageModel>> GetAllImagesFromPost(int postId, CancellationToken ct = default)
    {
        var response = await client.GetImagesFromPost(postId, ct);
        
        var images = response.items.Select(ToImageModel);

        return images.ToList();
    }

    private ImageModel ToImageModel(Item item)
    {
        var tags = TagExtractor.GetTagsFromResponse(item);
            
        var image = new ImageModel(item.url, item.postId, item.username, tags);

        return image;
    }
}