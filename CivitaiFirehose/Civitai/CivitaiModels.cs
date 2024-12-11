using System.Text.Json.Serialization;

namespace CivitaiFirehose;

public record CivitaiResponse(
    Item[] items,
    Metadata metadata
);

public record Item(
    int id,
    string url,
    string hash,
    int width,
    int height,
    string nsfwLevel,
    bool nsfw,
    int browsingLevel,
    string createdAt,
    int postId,
    Stats stats,
    Meta? meta,
    string username,
    string baseModel
);

public record Stats(
    int cryCount,
    int laughCount,
    int likeCount,
    int dislikeCount,
    int heartCount,
    int commentCount
);

public record Meta(
    string Size,
    long seed,
    string Model,
    int steps,
    Hashes hashes,
    string prompt,
    string Version,
    string sampler,
    string CFG_Scale,
    Resources[] resources,
    string Model_hash,
    string negativePrompt,
    bool nsfw,
    bool draft,
    Extra extra,
    int width,
    int height,
    float? cfgScale,
    int clipSkip,
    int quantity,
    string workflow,
    string baseModel,
    string Created_Date,
    bool fluxUltraRaw,
    CivitaiResources[] civitaiResources
)
{
    /// <summary>
    /// These are both extremely cursed in the Civitai JSON from time to time.
    /// We don't care about them, so just ignore them.
    /// </summary>
    [JsonIgnore]
    public int width { get; init; } = width;

    [JsonIgnore]
    public int height { get; init; } = height;
}

public record Hashes(
    string model
);

public record Resources(
    string hash,
    string name,
    string type
);

public record Extra(
    int remixOfId
);

public record CivitaiResources(
    string type,
    int modelVersionId,
    string modelVersionName,
    float weight
);

public record Metadata(
    string nextCursor,
    string nextPage
);