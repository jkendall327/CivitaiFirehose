using System.Threading.Channels;
using CivitaiFirehose.Components.Pages;

namespace CivitaiFirehose;

public sealed class HomeViewmodel(
    ICivitaiService civitaiService,
    JsService jsService,
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
    
    private readonly CancellationTokenSource _timerCancellationToken = new();

    public async Task OnInitialized()
    {
        imageService.NewImagesFound += OnNewImagesFound;
        pusher.OnStateChanged += NotifyStateChanged;

        await PopulateFeedWithNewestImages();
    }

    private async Task NotifyStateChanged()
    {
        if (StateUpdated is null) return;
        await StateUpdated();
    }

    public async Task OnAfterRenderAsync(Home home)
    {
        await jsService.Initialise(home);
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        
        while (!_timerCancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync())
        {
            await PopulateFeedWithNewestImages();
            await NotifyStateChanged();
        }
    }

    private async Task PopulateFeedWithNewestImages()
    {
        var results = await civitaiService.GetNewestImages();
        await imageService.Enqueue(results);
    }

    private async Task OnNewImagesFound(int newCount)
    {
        logger.LogInformation("Got {ImageCount} new images from service, updating UI", newCount);
        
        Unseen += newCount;
        
        PageTitle = $"Civitai Firehose ({Unseen})";
        
        await jsService.SetTabTitle(PageTitle);

        await NotifyStateChanged();
    }

    public async Task OnTabFocused()
    {
        logger.LogDebug("Tab focused, clearing any unread notifications");

        Unseen = 0;

        PageTitle = "Civitai Firehose";

        await jsService.SetTabTitle(PageTitle);

        await NotifyStateChanged();
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
        jsService.Dispose();
        _timerCancellationToken.Dispose();
    }
}