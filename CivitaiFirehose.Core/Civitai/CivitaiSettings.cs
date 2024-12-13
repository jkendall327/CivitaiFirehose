using System.Runtime.Serialization;

namespace CivitaiFirehose;

public class CivitaiSettings
{
    public TimeSpan PollingPeriod { get; init; }
    public List<string> ExcludedCreators { get; set; } = [];
    public CivitaiQuery QueryDefaults { get; init; } = new();
}

public enum NsfwFilter
{
    None,
    Soft,
    Mature,
    X
}

public enum SortOrder
{
    [EnumMember(Value = "Most Reactions")]
    MostReactions,
    [EnumMember(Value = "Most Comments")]
    MostComments,
    [EnumMember(Value = "Newest")]
    Newest
}

public enum TimePeriod
{
    [EnumMember(Value = "AllTime")]
    AllTime,
    Year,
    Month,
    Week,
    Day
}