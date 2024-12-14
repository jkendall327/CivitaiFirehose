using System.Threading.Channels;

namespace CivitaiFirehose;

public sealed class HomeViewmodel(
    ICivitaiService civitaiService,
    HydrusPusher pusher,
    BlacklistStore blacklist,
    ImageService imageService,
    ChannelWriter<ImageModel> writer,
    ILogger<HomeViewmodel> logger) : IDisposable
{
    public IEnumerable<ImageModel> Images => imageService.Images;
    public string PageTitle { get; private set; } = "Civitai Firehose";
    private int Unseen { get; set; }
    public int? HighlightedPostId { get; private set; }
    public int? ImagesInHighlightedPost { get; set; }
    public event Func<Task>? StateUpdated;
    public event Func<string, Task>? TitleUpdated;
    
    private readonly CancellationTokenSource _timerCancellationToken = new();

    private int? _postId;
    private int? _modelId;
    
    public async Task OnInitialized(int? postId = null, int? modelId = null)
    {
        _postId = postId;
        _modelId = modelId;
        
        imageService.NewImagesFound += OnNewImagesFound;
        pusher.OnStateChanged += NotifyStateChanged;

        await PopulateFeed();
    }

    private async Task NotifyStateChanged()
    {
        if (StateUpdated is null) return;
        await StateUpdated();
    }

    public async Task OnAfterRenderAsync()
    {
        if (_postId is not null || _modelId is not null)
        {
            // Only set up the live feed when polling for newest images.
            // No point continually polling for new pics when looking at a model or post!
            return;
        }
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        
        while (!_timerCancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync())
        {
            await PopulateFeed();
            await NotifyStateChanged();
        }
    }

    private async Task PopulateFeed()
    {
        if (_postId is not null && _modelId is not null)
        {
            throw new InvalidOperationException("Cannot be set to both a post and a model");
        }
        
        if (_postId is null && _modelId is null)
        {
            var results = await civitaiService.GetNewestImages();
            await imageService.Enqueue(results);
        }

        if (_postId is not null)
        {
            var result = await civitaiService.GetImagesFromPost(_postId.Value);
            await imageService.Enqueue(result);
        }

        if (_modelId is not null)
        {
            var result = await civitaiService.GetImagesFromModel(_modelId.Value);
            await imageService.Enqueue(result);
        }
    }

    private async Task OnNewImagesFound(int newCount)
    {
        logger.LogInformation("Got {ImageCount} new images from service, updating UI", newCount);
        
        Unseen += newCount;
        PageTitle = $"Civitai Firehose ({Unseen})";

        if (TitleUpdated is not null)
        {
            await TitleUpdated.Invoke(PageTitle);
        }
    }

    public async Task OnTabFocused()
    {
        logger.LogDebug("Tab focused, clearing any unread notifications");

        Unseen = 0;

        PageTitle = "Civitai Firehose";

        if (TitleUpdated is not null)
        {
            await TitleUpdated.Invoke(PageTitle);
        }
    }

    public async Task OnImageButtonClick(ImageModel image)
    {
        logger.LogInformation("Sending {ImageUrl} to Hydrus service", image.ImageUrl);
        await writer.WriteAsync(image);
    }

    public async Task OnDownloadAllClick(ImageModel image)
    {
        var images = await civitaiService.GetImagesFromPost(image.PostId);

        foreach (var imageModel in images)
        {
            await writer.WriteAsync(imageModel);
        }
    }

    public Task OnBlacklistUser(ImageModel image)
    {
        blacklist.BlacklistUser(image.Username);
        return Task.CompletedTask;
    }
    
    public async Task OnHighlightRelatedImages(ImageModel image)
    {
        // If clicking the same post ID, clear the highlight.
        if (HighlightedPostId == image.PostId)
        {
            HighlightedPostId = null;
            ImagesInHighlightedPost = null;
        }
        else
        {
            HighlightedPostId = image.PostId;
            ImagesInHighlightedPost = Images.Count(s => s.PostId == image.PostId);
        }
        
        await NotifyStateChanged();
    }
    
    public string GetDownloadStatusIcon(ImageModel image)
    {
        return image.PushStatus switch
        {
            // ⭐
            ImagePushStatus.NotPushed => "\u2b50",
            // ⏳
            ImagePushStatus.Pushing => "\u23f3",
            // ❌
            ImagePushStatus.Failed => "\u274c",
            // ✓
            ImagePushStatus.Succeeded => "\u2713",
            var _ => throw new ArgumentOutOfRangeException(nameof(image))
        };
    }

    public string GetHighlightStatusIcon()
    {
        if (ImagesInHighlightedPost is null)
        {
            // 🔍
            return "\ud83d\udd0d";
        }

        var icon = ImagesInHighlightedPost switch
        {
            0 => throw new ArgumentOutOfRangeException(nameof(ImagesInHighlightedPost)),
            1 => "1️⃣",
            2 => "2️⃣",
            3 => "3️⃣",
            4 => "4️⃣",
            5 => "5️⃣",
            6 => "6️⃣",
            7 => "7️⃣",
            8 => "8️⃣",
            9 => "9️⃣",
            var _ => "➕"
        };

        return icon;
    }

    public void Dispose()
    {
        imageService.NewImagesFound -= OnNewImagesFound;
        pusher.OnStateChanged -= NotifyStateChanged;
        _timerCancellationToken.Dispose();
    }
}