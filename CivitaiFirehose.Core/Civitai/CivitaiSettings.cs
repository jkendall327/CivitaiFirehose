using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public sealed class CivitaiSettings
{
    [Range(typeof(TimeSpan), "00:00:05", "05:00:00")]
    public TimeSpan PollingPeriod { get; init; }
    public List<string> ExcludedCreators { get; init; } = [];
    public required CivitaiQuery QueryDefaults { get; init; } = new();
}

[OptionsValidator]
public sealed partial class CivitaiSettingsValidation : IValidateOptions<CivitaiSettings>
{
}