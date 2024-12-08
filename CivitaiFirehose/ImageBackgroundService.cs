using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class ImageBackgroundService(
    IImageService imageService,
    IOptions<CivitaiSettings> options,
    ILogger<ImageBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            var period = options.Value.PollingPeriod;
            
            logger.LogInformation("Polling Civitai every {PollingPeriod}", period);
            
            using var timer = new PeriodicTimer(period);
            
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