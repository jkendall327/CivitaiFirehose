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
    string prompt,
    string negativePrompt);

public record Metadata(
    string nextCursor,
    string nextPage
);