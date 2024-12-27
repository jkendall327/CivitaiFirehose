using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public sealed class HomeViewmodel(
    ICivitaiService civitaiService,
    HydrusPusher pusher,
    BlacklistStore blacklist,
    ImageService imageService,
    ChannelWriter<ImageModel> writer,
    IOptions<CivitaiSettings> settings,
    ILogger<HomeViewmodel> logger) : IDisposable
{
    // Events
    public event Func<Task>? StateUpdated;
    public event Func<string, Task>? TitleUpdated;

    // Bound properties
    public IEnumerable<ImageModel> Images => imageService.Images;
    public string PageTitle { get; private set; } = OriginalTitle;
    private int Unseen { get; set; }
    public int? HighlightedPostId { get; private set; }
    public int? ImagesInHighlightedPost { get; set; }
    
    // Private state
    private readonly CancellationTokenSource _timerCancellationToken = new();
    private int? _postId;
    private int? _modelId;
    private const string OriginalTitle = "Civitai Firehose";
    private readonly PeriodicTimer _timer = new(settings.Value.PollingPeriod);
    
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

    private async Task NotifyTitleChanged(string newTitle)
    {
        if (TitleUpdated is null) return;
        await TitleUpdated(newTitle);
    }

    public async Task OnAfterRenderAsync()
    {
        if (_postId is not null || _modelId is not null)
        {
            // Only set up the live feed when polling for newest images.
            // No point continually polling for new pics when looking at a model or post!
            return;
        }
        
        while (await _timer.WaitForNextTickAsync(_timerCancellationToken.Token))
        {
            logger.LogInformation("Polling Civitai for new images...");
            
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

        await NotifyTitleChanged(PageTitle);
    }

    public async Task OnTabFocused()
    {
        logger.LogDebug("Tab focused, clearing any unread notifications");

        Unseen = 0;
        PageTitle = OriginalTitle;

        await NotifyTitleChanged(PageTitle);
    }

    public async Task OnImageButtonClick(ImageModel image)
    {
        logger.LogInformation("Sending {ImageUrl} to Hydrus service", image.ImageUrl);
        await writer.WriteAsync(image);
    }

    public async Task OnDownloadAllClick(ImageModel image)
    {
        var images = await civitaiService.GetImagesFromPost(image.PostId);

        logger.LogInformation("Got {ImagesInPost} images from {PostId}", images.Count, image.PostId);
        
        foreach (var imageModel in images)
        {
            await writer.WriteAsync(imageModel);
        }
    }

    public Task OnBlacklistUser(ImageModel image)
    {
        logger.LogInformation("Blacklisting user {Username}", image.Username);
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

    public void Dispose()
    {
        imageService.NewImagesFound -= OnNewImagesFound;
        pusher.OnStateChanged -= NotifyStateChanged;

        _timerCancellationToken.Dispose();
        _timer.Dispose();
    }
}