using System.Net;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiPollingBackgroundService(
    ICivitaiService civitaiService,
    IOptions<CivitaiSettings> options,
    ILogger<CivitaiPollingBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            // Run immediately once on startup.
            await civitaiService.PollCivitai(ct);

            var period = options.Value.PollingPeriod;

            logger.LogInformation("Polling Civitai every {PollingPeriod}", period);

            using var timer = new PeriodicTimer(period);

            while (await timer.WaitForNextTickAsync(ct))
            {
                logger.LogInformation("Timer elapsed, polling Civitai");

                try
                {
                    await civitaiService.PollCivitai(ct);
                }
                catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    logger.LogWarning("Civitai is currently unavailable, waiting...");
                    await Task.Delay(TimeSpan.FromMinutes(5), ct);
                    logger.LogInformation("Wait elapsed; trying to poll Civitai again");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Image monitoring stopped");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error polling Civitai");
        }
    }
}