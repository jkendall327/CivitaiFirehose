using System.Diagnostics.Metrics;

namespace CivitaiFirehose;

public static class Meters
{
    public static Meter Meter = new("CivitaiFirehose.Metrics");

    public static Counter<int> FoundImages = Meter
        .CreateCounter<int>("firehose.found_images", description: "Number of new images retrieved from Civitai API");
}