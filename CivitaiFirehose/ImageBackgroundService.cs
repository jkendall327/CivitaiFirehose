namespace CivitaiFirehose;

public class ImageBackgroundService : BackgroundService
{
    readonly IImageService _imageService;
    readonly ILogger<ImageBackgroundService> _logger;

    public ImageBackgroundService(
        IImageService imageService,
        ILogger<ImageBackgroundService> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            
            while (await timer.WaitForNextTickAsync(ct))
                await _imageService.StartMonitoring(ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Image monitoring stopped");
        }
    }
}