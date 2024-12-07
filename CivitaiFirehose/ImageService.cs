namespace CivitaiFirehose;

public interface IImageService
{
    Task PollCivitai(CancellationToken ct);
}

public class CivitAiImageService(HttpClient http, ILogger<CivitAiImageService> logger) : IImageService
{
    private readonly HashSet<string> _seenUrls = new();

    public async Task PollCivitai(CancellationToken ct)
    {
        try
        {
            var response = await http.GetFromJsonAsync<CivitaiResponse>("https://civitai.com/api/v1/images?sort=Newest&limit=5", ct);
            if (response?.items == null) return;

            foreach (var img in response.items)
            {
                if (_seenUrls.Add(img.url) && true) // Only process new images
                {
                    ;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to poll for new images");
        }
    }
}