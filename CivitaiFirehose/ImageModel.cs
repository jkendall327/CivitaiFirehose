namespace CivitaiFirehose;

public record ImageModel(string ImageUrl, int PostId, List<string> Tags)
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