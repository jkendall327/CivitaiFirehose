using System.Reflection;
using System.Runtime.Serialization;

namespace CivitaiFirehose;

public class CivitaiSettings
{
    public TimeSpan PollingPeriod { get; init; }
    public List<string> ExcludedCreators { get; set; } = [];
    public CivitaiImageQuery QueryDefaults { get; init; } = new();
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

public class CivitaiImageQuery
{
    /// <summary>
    /// The number of results to be returned per page (0-200, default 100)
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// The ID of a post to get images from
    /// </summary>
    public int? PostId { get; set; }

    /// <summary>
    /// The ID of a model to get images from (model gallery)
    /// </summary>
    public int? ModelId { get; set; }

    /// <summary>
    /// The ID of a model version to get images from (model gallery filtered to version)
    /// </summary>
    public int? ModelVersionId { get; set; }

    /// <summary>
    /// Filter to images from a specific user
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Filter to images that contain mature content flags or not
    /// </summary>
    public NsfwFilter? Nsfw { get; set; }

    /// <summary>
    /// The order in which you wish to sort the results
    /// </summary>
    public SortOrder? Sort { get; set; }

    /// <summary>
    /// The time frame in which the images will be sorted
    /// </summary>
    public TimePeriod? Period { get; set; }

    /// <summary>
    /// The page from which to start fetching creators
    /// </summary>
    public int? Page { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        var dict = new Dictionary<string, string?>();

        if (Limit.HasValue) dict.Add("limit", Limit.Value.ToString());
        if (PostId.HasValue) dict.Add("postId", PostId.Value.ToString());
        if (ModelId.HasValue) dict.Add("modelId", ModelId.Value.ToString());
        if (ModelVersionId.HasValue) dict.Add("modelVersionId", ModelVersionId.Value.ToString());
        if (Username is { } username) dict.Add("username", username);
        if (Nsfw.HasValue) dict.Add("nsfw", Nsfw.Value.ToString());
        if (Period.HasValue) dict.Add("period", Period.Value.ToString().ToLowerInvariant());
        if (Page.HasValue) dict.Add("page", Page.Value.ToString());
        
        if (Sort.HasValue)
        {
            // This handles the space in the enum, which is required when sending to Civitai.
            var enumMember = typeof(SortOrder)
                .GetField(Sort.Value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>()
                ?.Value ?? Sort.Value.ToString();
            dict.Add("sort", enumMember);
        }
        
        return dict;
    }

    public CivitaiImageQuery Clone()
    {
        return new()
        {
            Limit = Limit,
            PostId = PostId,
            ModelId = ModelId,
            ModelVersionId = ModelVersionId,
            Username = Username,
            Nsfw = Nsfw,
            Sort = Sort,
            Period = Period,
            Page = Page,
        };
    }
}