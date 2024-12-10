using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public interface ICivitaiPoller
{
    Task PollCivitai(CancellationToken ct);
    Func<int, Task>? NewImagesFound { get; set; }
    List<ImageModel> GetImages();
    Task<List<ImageModel>> GetAllImagesFromPost(int postId, CancellationToken ct = default);
}

public class CivitaiPoller(CivitaiClient client, IOptions<CivitaiSettings> options, ILogger<CivitaiPoller> logger) : ICivitaiPoller
{
    private readonly Stack<ImageModel> _images = new(options.Value.QueryDefaults.Limit ?? 20);

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

    private async Task PollCore(CancellationToken ct)
    {
        var response = await client.GetImages(ct);

        var found = 0;

        foreach (var img in response.items)
        {
            if (_images.Any(s => s.ImageUrl == img.url))
            {
                continue;
            }
            
            var tags = TagExtractor.GetTagsFromResponse(img);
            
            var image = new ImageModel(img.url, img.postId, tags);

            found++;

            _images.Push(image);
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
            
            var image = new ImageModel(s.url, s.postId, tags);

            return image;
        });

        return images.ToList();
    }
    
    public List<ImageModel> GetImages() => _images.ToList();

    public Func<int, Task>? NewImagesFound { get; set; }
}