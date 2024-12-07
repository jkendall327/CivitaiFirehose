namespace CivitaiFirehose;

public interface IImageService
{
    Task PollCivitai(CancellationToken ct);
}

public class CivitAiImageService : IImageService
{
    readonly HttpClient _http;
    readonly ILogger<CivitAiImageService> _logger;
    readonly HashSet<string> _seenUrls = new();
    
    public CivitAiImageService(
        HttpClient http, 
        ILogger<CivitAiImageService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task PollCivitai(CancellationToken ct)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<RootObject>("https://civitai.com/api/v1/images?sort=Newest&limit=5", ct);
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
            _logger.LogError(ex, "Failed to poll for new images");
        }
    }
}

public record RootObject(
    Items[] items,
    Metadata metadata
);

public record Items(
    int id,
    string url,
    string hash,
    int width,
    int height,
    string nsfwLevel,
    bool nsfw,
    int browsingLevel,
    string createdAt,
    int postId,
    Stats stats,
    Meta meta,
    string username,
    string baseModel
);

public record Stats(
    int cryCount,
    int laughCount,
    int likeCount,
    int dislikeCount,
    int heartCount,
    int commentCount
);

public record Meta(
    string Size,
    long seed,
    string Model,
    int steps,
    Hashes hashes,
    string prompt,
    string Version,
    string sampler,
    string CFG_Scale,
    Resources[] resources,
    string Model_hash,
    string negativePrompt,
    bool nsfw,
    bool draft,
    Extra extra,
    int width,
    int height,
    float? cfgScale,
    int clipSkip,
    int quantity,
    string workflow,
    string baseModel,
    string Created_Date,
    bool fluxUltraRaw,
    CivitaiResources[] civitaiResources
);

public record Hashes(
    string model
);

public record Resources(
    string hash,
    string name,
    string type
);

public record Extra(
    int remixOfId
);

public record CivitaiResources(
    string type,
    int modelVersionId,
    string modelVersionName,
    float weight
);

public record Metadata(
    string nextCursor,
    string nextPage
);

