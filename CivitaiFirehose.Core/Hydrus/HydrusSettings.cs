namespace CivitaiFirehose;

public sealed class HydrusSettings
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public TimeSpan AvailabilityWaitPeriod { get; init; }
}