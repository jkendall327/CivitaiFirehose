using System.Diagnostics.Metrics;

namespace CivitaiFirehose;

public sealed class Meters
{
    public const string MeterName = "CivitaiFirehose.Metrics";
    
    private readonly Meter _meter = new(MeterName);
    private readonly Counter<int> _foundImages;

    public Meters()
    {
        _foundImages = _meter.CreateCounter<int>("firehose.found_images",
            description: "Number of new images retrieved from Civitai API");
    }
    
    public void ReportNewImages(int count)
    {
        var now = DateTime.UtcNow;
        
        _foundImages.Add(count, [
            new("hour", now.Hour),
            new("day_of_week", (int)now.DayOfWeek)
        ]);
    }
}