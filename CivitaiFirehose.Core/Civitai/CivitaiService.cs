using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiService(
    CivitaiClient client,
    ImageMapper mapper,
    ImageService imageService,
    IOptions<CivitaiSettings> options) : ICivitaiService
{
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