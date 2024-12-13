namespace CivitaiFirehose;

public interface ICivitaiService
{
    /// <summary>
    /// Scrapes Civitai for new images.
    /// </summary>
    Task GetNewestImages(CancellationToken ct);
    
    /// <summary>
    /// Gets all the images from a specified Civitai post.
    /// </summary>
    Task<List<ImageModel>> GetImagesFromPost(int postId, CancellationToken ct = default);
}