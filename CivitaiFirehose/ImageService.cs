using MediatR;

namespace CivitaiFirehose;

public interface IImageService
{
    Task StartMonitoring(CancellationToken ct);
}

public class CivitAiImageService : IImageService
{
    readonly HttpClient _http;
    readonly ILogger<CivitAiImageService> _logger;
    readonly IPublisher _publisher;
    readonly HashSet<string> _seenUrls = new();
    
    public CivitAiImageService(
        HttpClient http, 
        ILogger<CivitAiImageService> logger,
        IPublisher publisher)
    {
        _http = http;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task StartMonitoring(CancellationToken ct)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse>("https://civitai.com/api/v1/images?sort=Newest", ct);
            if (response?.Images == null) return;

            foreach (var img in response.Images)
            {
                if (_seenUrls.Add(img.Url)) // Only process new images
                    await _publisher.Publish(new NewImageNotification(new ImageData(img.Url, img.Title)), ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to poll for new images");
        }
    }
}

// Models for API response
public record ApiResponse(List<ApiImage> Images);
public record ApiImage(string Url, string? Title);