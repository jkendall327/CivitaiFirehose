namespace CivitaiFirehose;

public class ImageBackgroundService(IImageService imageService, ILogger<ImageBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            
            while (await timer.WaitForNextTickAsync(ct))
            {
                logger.LogInformation("Timer elapsed, polling Civitai");
                await imageService.PollCivitai(ct);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Image monitoring stopped");
        }
    }
}