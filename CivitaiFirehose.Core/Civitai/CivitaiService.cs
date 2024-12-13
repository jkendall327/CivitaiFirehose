using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiService(
    CivitaiClient client,
    BlacklistStore blacklist,
    ImageMapper mapper,
    ImageService imageService,
    IOptions<CivitaiSettings> options, 
    ILogger<CivitaiService> logger) : ICivitaiService
{
    // Given a default value to avoid annoying nullability. We assume the UI will always actually hook onto this.
    public Func<int, Task> NewImagesFound { get; set; } = _ => Task.CompletedTask;
    public BoundedQueue<ImageModel> Images { get; } = new(options.Value.QueryDefaults.Limit ?? 20);
    
    public async Task PollCivitai(CancellationToken ct)
    {
        var query = options.Value.QueryDefaults.Clone();
        
        var response = await client.GetImages(query, ct);

        var images = response.items.Select(mapper.ToImageModel);
        
        await imageService.EnqueueImages(images);
    }
    
    public async Task<List<ImageModel>> GetAllImagesFromPost(int postId, CancellationToken ct = default)
    {
        var query = options.Value.QueryDefaults.Clone();
        
        query.PostId = postId;
        query.Limit = 200;
        
        var response = await client.GetImages(query, ct);
        
        var images = response.items.Select(mapper.ToImageModel);

        return images.ToList();
    }
}