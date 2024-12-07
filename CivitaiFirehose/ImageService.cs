namespace CivitaiFirehose;

public interface IImageService
{
    Task PollCivitai(CancellationToken ct);
    Func<Task>? NewImagesFound { get; set; }
    List<ImageModel> GetImages();
}

public class CivitAiImageService(HttpClient http, ILogger<CivitAiImageService> logger) : IImageService
{
    private readonly HashSet<string> _seenUrls = new();
    private readonly Stack<ImageModel> _images = new(20);

    public async Task PollCivitai(CancellationToken ct)
    {
        try
        {
            var response = await http.GetFromJsonAsync<CivitaiResponse>("https://civitai.com/api/v1/images?sort=Newest&limit=5", ct);
            if (response?.items == null) return;

            var any = false;
            
            foreach (var img in response.items)
            {
                if (_seenUrls.Add(img.url)) // Only process new images
                {
                    any = true;
                    _images.Push(new(img.url, img.postId.ToString()));
                }
            }

            if (any)
            {
                var t = NewImagesFound?.Invoke();
                if (t != null) await t;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to poll for new images");
        }
    }

    public List<ImageModel> GetImages()
    {
        return _images.ToList();
    }
    
    public Func<Task>? NewImagesFound { get; set; }
}