namespace CivitaiFirehose;

public interface IImageService
{
    Task PollCivitai(CancellationToken ct);
    Func<Task>? NewImagesFound { get; set; }
    List<ImageModel> GetImages();
}

public class CivitaiImageService(CivitaiClient client, ILogger<CivitaiImageService> logger) : IImageService
{
    private readonly HashSet<string> _seenUrls = new();
    private readonly Stack<ImageModel> _images = new(20);

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
            // Only process new images.
            if (!_seenUrls.Add(img.url)) continue;
                
            found++;
                    
            var postUrl = $"https://civitai.com/posts/{img.postId.ToString()}";
                    
            _images.Push(new(img.url, postUrl));
        }
            
        if (found > 0)
        {
            logger.LogInformation("Found {NewImages} new images", found);
                
            // Tell the UI we have new images.
            var t = NewImagesFound?.Invoke();
            if (t != null) await t;
        }
    }

    public List<ImageModel> GetImages() => _images.ToList();

    public Func<Task>? NewImagesFound { get; set; }
}