using System.Diagnostics.Metrics;

namespace CivitaiFirehose;

public sealed class Meters
{
    public const string MeterName = "CivitaiFirehose.Metrics";
    
    private readonly Meter _meter = new(MeterName);
    private readonly Counter<int> _foundImages;
    private readonly Counter<int> _imagesPushedToHydrus;
    private readonly Counter<int> _failedPushes;

    public Meters()
    {
        _foundImages = _meter.CreateCounter<int>("firehose.found_images",
            description: "Number of new images retrieved from Civitai API");
        
        _imagesPushedToHydrus = _meter.CreateCounter<int>(
            "firehose.images_pushed_to_hydrus",
            description: "Number of images attempted to push to Hydrus");
        
        _failedPushes = _meter.CreateCounter<int>(
            "firehose.failed_pushes",
            description: "Number of images which couldn't be pushed to Hydrus");
    }
    
    public void ReportNewImages(int count)
    {
        var now = DateTime.UtcNow;
        
        _foundImages.Add(count, [
            new("hour", now.Hour),
            new("day_of_week", (int)now.DayOfWeek)
        ]);
    }
    
    public void ReportImagePushed() => _imagesPushedToHydrus.Add(1);
    public void ReportPushFailed() => _failedPushes.Add(1);
}