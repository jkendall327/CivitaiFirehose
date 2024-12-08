using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiPollingBackgroundService(
    ICivitaiPoller civitaiPoller,
    IOptions<CivitaiSettings> options,
    ILogger<CivitaiPollingBackgroundService> logger) : BackgroundService
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
                await civitaiPoller.PollCivitai(ct);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Image monitoring stopped");
        }
    }
}