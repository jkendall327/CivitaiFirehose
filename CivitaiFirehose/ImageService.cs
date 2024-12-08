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
            if (_images.Any(s => s.ImageUrl == img.url))
            {
                continue;
            }
            
            var postUrl = $"https://civitai.com/posts/{img.postId.ToString()}";

            var tags = GetTagsFromResponse(img);
            
            var image = new ImageModel(img.url, postUrl, tags);

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

    private static List<string> GetTagsFromResponse(Items item)
    {
        var tags = new HashSet<string>
        {
            "source:civitai-firehose",
            $"image id:{item.id}",
            $"post id:{item.postId}",
            $"nsfw level:{item.nsfwLevel}",
            $"creator:{item.username}",
            $"base model:{item.baseModel}"
        };
        
        if (item.meta is null) return tags.ToList();

        if (!string.IsNullOrWhiteSpace(item.meta.prompt))
        {
            var prompt = item.meta.prompt.Replace("\n", " ").Trim();
            tags.Add($"prompt:{prompt}");

            var promptedTags = prompt
                .Split(",")
                .Select(x =>
                {
                    // Gets rid of some syntax when invoking LORAs in prompts.
                    x = x.Replace("<", string.Empty).Replace(">", string.Empty);
                    return x.Trim();
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            
            foreach (var promptedTag in promptedTags)
            {
                tags.Add(promptedTag);
            }
        }

        if (!string.IsNullOrWhiteSpace(item.meta.negativePrompt))
        {
            tags.Add($"negative prompt:{item.meta.negativePrompt.Replace("\n", " ").Trim()}");
        }

        return tags.ToList();
    }

    public List<ImageModel> GetImages() => _images.ToList();

    public Func<int, Task>? NewImagesFound { get; set; }
}