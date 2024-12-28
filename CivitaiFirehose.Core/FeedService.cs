using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class FeedService(
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
        //_timer.Dispose();
        
        _pollingTask = null;
    }

    private async Task PollAsync()
    {
        imageService.Clear();

        var img = await civitaiService.GetNewestImages();
        await imageService.Enqueue(img);
        
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
        
        // TODO: clear the image service here before enqueuing?
        var images = await civitaiService.GetImagesFromPost(postId);
        
        imageService.Clear();
        await imageService.Enqueue(images);
    }

    public async Task LoadModelImages(int modelId)
    {
        StopPolling();
        
        var images = await civitaiService.GetImagesFromModel(modelId);

        imageService.Clear();
        await imageService.Enqueue(images);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _timer.Dispose();
    }
}