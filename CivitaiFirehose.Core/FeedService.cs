using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public sealed class FeedService(
    ICivitaiService civitaiService,
    ImageService imageService,
    IOptions<CivitaiSettings> settings,
    ILogger<FeedService> logger) : IDisposable
{
    private readonly PeriodicTimer _timer = new(settings.Value.PollingPeriod);
    private CancellationTokenSource _cts = new();
    private Task? _pollingTask;
    
    public void StartPollingForNewImages()
    {
        if (_pollingTask != null) return;

        _cts = new();
        _pollingTask = PollAsync();
    }

    private void StopPolling()
    {
        _cts.Cancel();
        _pollingTask = null;
    }

    private async Task PollAsync()
    {
        var img = await civitaiService.GetNewestImages();
        await imageService.ClearAndEnqueue(img);
        
        try
        {
            while (!_cts.Token.IsCancellationRequested && await _timer.WaitForNextTickAsync(_cts.Token))
            {
                logger.LogInformation("Polling Civitai for new images...");
                
                var images = await civitaiService.GetNewestImages();
                await imageService.Enqueue(images);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Polling cancelled");
        }
    }

    public async Task LoadPostImages(int postId)
    {
        StopPolling();
        
        var images = await civitaiService.GetImagesFromPost(postId);
        
        await imageService.ClearAndEnqueue(images);
    }

    public async Task LoadModelImages(int modelId)
    {
        StopPolling();
        
        var images = await civitaiService.GetImagesFromModel(modelId);

        await imageService.ClearAndEnqueue(images);
    }

    public async Task LoadUserImages(string userId)
    {
        StopPolling();
        
        var images = await civitaiService.GetImagesFromUser(userId);
        
        await imageService.ClearAndEnqueue(images);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _timer.Dispose();
    }
}