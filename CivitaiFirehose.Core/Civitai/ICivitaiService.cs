namespace CivitaiFirehose;

/// <summary>
/// Encapsulates getting images from Civitai specifically, i.e. generating query URLs to pass to the HTTP layer. 
/// </summary>
public interface ICivitaiService
{
    /// <summary>
    /// Scrapes Civitai for new images.
    /// </summary>
    Task<List<ImageModel>> GetNewestImages(CancellationToken ct = default);
    
    /// <summary>
    /// Gets all the images from a specified Civitai post.
    /// </summary>
    Task<List<ImageModel>> GetImagesFromPost(int postId, CancellationToken ct = default);

    Task<List<ImageModel>> GetImagesFromModel(int modelId, int? modelVersionId = null, CancellationToken ct = default);
    Task<List<ImageModel>> GetImagesFromUser(string userId, CancellationToken ct = default);
}