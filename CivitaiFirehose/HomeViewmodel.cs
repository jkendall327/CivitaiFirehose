using System.Threading.Channels;
using CivitaiFirehose.Components.Pages;

namespace CivitaiFirehose;

public sealed class HomeViewmodel(
    ICivitaiPoller civitaiPoller,
    JsService jsService,
    HydrusPusher pusher,
    ChannelWriter<ImageModel> writer,
    ILogger<HomeViewmodel> logger) : IDisposable
{
    public Stack<ImageModel> Images => civitaiPoller.Images;
    public string PageTitle { get; private set; } = "Civitai Firehose";
    private int Unseen { get; set; }
    public event Func<Task>? StateUpdated;

    public void OnInitialized()
    {
        civitaiPoller.NewImagesFound += SetImages;
        pusher.OnStateChanged += NotifyStateChanged;
    }

    private async Task NotifyStateChanged()
    {
        if (StateUpdated is null) return;
        await StateUpdated();
    }

    public async Task OnAfterRenderAsync(Home home) => await jsService.Initialise(home);

    private async Task SetImages(int newCount)
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
        var images = await civitaiPoller.GetAllImagesFromPost(image.PostId);

        foreach (var imageModel in images)
        {
            await writer.WriteAsync(imageModel);
        }
    }

    public Task OnBlacklistUser(ImageModel image)
    {
        civitaiPoller.BlacklistUser(image.Username);
        return Task.CompletedTask;
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

    public void Dispose()
    {
        civitaiPoller.NewImagesFound -= SetImages;
        pusher.OnStateChanged -= NotifyStateChanged;
        jsService.Dispose();
    }
}