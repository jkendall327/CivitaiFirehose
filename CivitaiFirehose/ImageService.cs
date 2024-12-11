using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public interface ICivitaiPoller
{
    Task PollCivitai(CancellationToken ct);
    BoundedQueue<ImageModel> Images { get; }
    Func<int, Task>? NewImagesFound { get; set; }
    void BlacklistUser(string username);
    Task<List<ImageModel>> GetAllImagesFromPost(int postId, CancellationToken ct = default);
}

public class CivitaiService(CivitaiClient client, IOptions<CivitaiSettings> options, ILogger<CivitaiService> logger) : ICivitaiPoller
{
    public BoundedQueue<ImageModel> Images { get; } = new(options.Value.QueryDefaults.Limit ?? 20);
    private readonly HashSet<string> _blacklistedUsers = [..options.Value.ExcludedCreators];

    public async Task PollCivitai(CancellationToken ct)
    {
        try
        {
            await PollCore(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to poll for new images");
        }
    }

    public void BlacklistUser(string username)
    {
        _blacklistedUsers.Add(username);
    }

    private async Task PollCore(CancellationToken ct)
    {
        var response = await client.GetImages(ct);

        var found = 0;

        foreach (var img in response.items)
        {
            if (Images.Any(s => s.ImageUrl == img.url))
            {
                continue;
            }

            if (_blacklistedUsers.Contains(img.username))
            {
                continue;
            }
            
            var tags = TagExtractor.GetTagsFromResponse(img);
            
            var image = new ImageModel(img.url, img.postId, img.username, tags);

            found++;

            Images.Enqueue(image);
        }

        if (found > 0)
        {
            logger.LogInformation("Found {NewImages} new images", found);

            // Tell the UI we have new images.
            var t = NewImagesFound?.Invoke(found);
            if (t != null) await t;
        }
    }

    public async Task<List<ImageModel>> GetAllImagesFromPost(int postId, CancellationToken ct = default)
    {
        var response = await client.GetImagesFromPost(postId, ct);
        
        // TODO: factor out repeated code.
        var images = response.items.Select(s =>
        {
            var tags = TagExtractor.GetTagsFromResponse(s);
            
            var image = new ImageModel(s.url, s.postId, s.username, tags);

            return image;
        });

        return images.ToList();
    }
    
    public Func<int, Task>? NewImagesFound { get; set; }
}