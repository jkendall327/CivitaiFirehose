namespace CivitaiFirehose;

public interface ICivitaiPoller
{
    Task PollCivitai(CancellationToken ct);
    Func<int, Task>? NewImagesFound { get; set; }
    List<ImageModel> GetImages();
}

public class CivitaiPoller(CivitaiClient client, ILogger<CivitaiPoller> logger) : ICivitaiPoller
{
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
            var postUrl = $"https://civitai.com/posts/{img.postId.ToString()}";

            var image = new ImageModel(img.url, postUrl);
            
            if (_images.Contains(image))
            {
                continue;
            }
                
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

    public List<ImageModel> GetImages() => _images.ToList();

    public Func<int, Task>? NewImagesFound { get; set; }
}