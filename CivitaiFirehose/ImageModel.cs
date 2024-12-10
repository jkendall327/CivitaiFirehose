namespace CivitaiFirehose;

public record ImageModel(string ImageUrl, int PostId, List<string> Tags)
{
    public string PostUrl => $"https://civitai.com/posts/{PostId.ToString()}";
}
