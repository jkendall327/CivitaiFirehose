namespace CivitaiFirehose;

public interface ICivitaiService
{
    /// <summary>
    /// The most recent haul of images from Civitai.
    /// </summary>
    BoundedQueue<ImageModel> Images { get; }
    
    /// <summary>
    /// Triggers when new images are found; returns the count of new ones.
    /// </summary>
    Func<int, Task> NewImagesFound { get; set; }
    
    /// <summary>
    /// Scrapes Civitai for new images.
    /// </summary>
    Task PollCivitai(CancellationToken ct);
    
    /// <summary>
    /// Gets all the images from a specified Civitai post.
    /// </summary>
    Task<List<ImageModel>> GetAllImagesFromPost(int postId, CancellationToken ct = default);
}