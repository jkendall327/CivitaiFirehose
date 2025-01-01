using System.ComponentModel.DataAnnotations;

namespace CivitaiFirehose;

public sealed class HydrusSettings
{
    [Url]
    public string? BaseUrl { get; init; }
    public string? ApiKey { get; init; }
    public TimeSpan AvailabilityWaitPeriod { get; init; }
}