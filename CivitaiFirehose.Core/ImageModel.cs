namespace CivitaiFirehose;

public record ImageModel(string ImageUrl, int PostId, string Username, List<string> Tags, DateTime CreatedAt)
{
    public string PostUrl => $"https://civitai.com/posts/{PostId.ToString()}";
    
    public ImagePushStatus PushStatus { get; set; } = ImagePushStatus.NotPushed;
    public string? ErrorMessage { get; set; }
}

public enum ImagePushStatus
{
    NotPushed,
    Pushing,
    Failed,
    Succeeded
}