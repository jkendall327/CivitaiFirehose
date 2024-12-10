using System.Threading.Channels;
using CivitaiFirehose.Components.Pages;

namespace CivitaiFirehose;

public class HomeViewmodel(
    ICivitaiPoller CivitaiPoller,
    JsService JsService,
    HydrusPusher Pusher,
    ChannelWriter<ImageModel> Writer,
    ILogger<HomeViewmodel> logger) : IDisposable
{
    public List<ImageModel> Images { get; private set; } = [];

    public string PageTitle { get; set; } = "Civitai Firehose";

    public int Unseen { get; set; }

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
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected void OnInitialized()
    {
        Images = CivitaiPoller.GetImages();

        CivitaiPoller.NewImagesFound += SetImages;
        Pusher.OnStateChanged += OnImageStateChanged;
    }

    private async Task OnImageStateChanged()
    {
        //await InvokeAsync(StateHasChanged);
    }

    protected async Task OnAfterRenderAsync(Home home)
    {
        await JsService.Initialise(home);
    }

    private async Task SetImages(int newCount)
    {
        Images = CivitaiPoller.GetImages();

        logger.LogInformation("Got {ImageCount} new images from service, updating UI", newCount);

        Unseen += newCount;
        PageTitle = $"Civitai Firehose ({Unseen})";
        await JsService.SetTabTitle(PageTitle);

        // await InvokeAsync(async () => {
        //
        //     StateHasChanged();
        // });
    }

    public async Task OnTabFocused()
    {
        logger.LogDebug("Tab focused, clearing any unread notifications");

        Unseen = 0;

        PageTitle = "Civitai Firehose";

        await JsService.SetTabTitle(PageTitle);
    }

    // TODO: proper MVVM etc. for this page, getting too big.
    private async Task OnImageButtonClick(ImageModel image)
    {
        logger.LogInformation("Sending {ImageUrl} to Hydrus service", image.ImageUrl);
        await Writer.WriteAsync(image);
    }

    private async Task OnDownloadAllClick(ImageModel image)
    {
        var images = await CivitaiPoller.GetAllImagesFromPost(image.PostId);

        foreach (var imageModel in images)
        {
            await Writer.WriteAsync(imageModel);
        }
    }

    private Task OnBlacklistUser(ImageModel image)
    {
        CivitaiPoller.BlacklistUser(image.Username);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        CivitaiPoller.NewImagesFound -= SetImages;
        Pusher.OnStateChanged -= OnImageStateChanged;
        JsService.Dispose();
    }
}