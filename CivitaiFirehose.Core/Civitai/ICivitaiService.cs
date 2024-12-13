namespace CivitaiFirehose;

public interface ICivitaiService
{
    /// <summary>
    /// Scrapes Civitai for new images.
    /// </summary>
    Task PollCivitai(CancellationToken ct);
    
    /// <summary>
    /// Gets all the images from a specified Civitai post.
    /// </summary>
    Task<List<ImageModel>> GetAllImagesFromPost(int postId, CancellationToken ct = default);
}