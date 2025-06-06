using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace CivitaiFirehose;

/// <summary>
/// Encapsulates the Blazor-agnostic functionality for the main screen.
/// </summary>
public sealed class HomeViewmodel(
    ICivitaiService civitaiService,
    HydrusPusher pusher,
    BlacklistStore blacklist,
    ImageService imageService,
    FeedService feedService,
    ChannelWriter<ImageModel> writer,
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
    private const string OriginalTitle = "Civitai Firehose";
    
    public void OnInitialized()
    {
        imageService.NewImagesFound += OnNewImagesFound;
        pusher.OnStateChanged += NotifyStateChanged;
    }
    
    public async Task UpdateFeedSource(int? postId = null, int? modelId = null, string? userId = null)
    {
        object?[] ids = [postId, modelId, userId];

        if (ids.Count(s => s is not null) > 1)
        {
            throw new InvalidOperationException("Feed set to more than one kind of resource");
        }
        
        // Default behaviour when no explicit source is set.
        if (ids.All(s => s is null))
        {
            feedService.StartPollingForNewImages();
        }

        if (postId is not null)
        {
            await feedService.LoadPostImages(postId.Value);
        }

        if (modelId is not null)
        {
            await feedService.LoadModelImages(modelId.Value);
        }

        if (userId is not null)
        {
            await feedService.LoadUserImages(userId);
        }
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
    
    public async Task OnPushAll()
    {
        logger.LogInformation("Pushing all visible images to Hydrus");
    
        foreach (var image in Images)
        {
            await writer.WriteAsync(image);
        }
    }

    public async Task OnPushImage(ImageModel image)
    {
        logger.LogInformation("Sending {ImageUrl} to Hydrus service", image.ImageUrl);
        await writer.WriteAsync(image);
    }

    public async Task OnPushPost(ImageModel image)
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
    }
}