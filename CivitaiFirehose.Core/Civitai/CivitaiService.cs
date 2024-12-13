using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiService(
    CivitaiClient client,
    ImageMapper mapper,
    ImageService imageService,
    IOptions<CivitaiSettings> options) : ICivitaiService
{
    public async Task GetNewestImages(CancellationToken ct)
    {
        var query = options.Value.QueryDefaults.Clone();

        query.Sort = SortOrder.Newest;
        
        var response = await client.GetImages(query, ct);

        var images = response.items.Select(mapper.ToImageModel);
        
        await imageService.Enqueue(images);
    }
    
    public async Task<List<ImageModel>> GetImagesFromModel(int modelId, int? modelVersionId = null, CancellationToken ct = default)
    {
        var query = options.Value.QueryDefaults.Clone();
        
        query.ModelId = modelId;
        query.ModelVersionId = modelVersionId;
        query.Limit = 200;
        
        var response = await client.GetImages(query, ct);
        
        var images = response.items.Select(mapper.ToImageModel);

        return images.ToList();
    }
    
    public async Task<List<ImageModel>> GetImagesFromPost(int postId, CancellationToken ct = default)
    {
        var query = options.Value.QueryDefaults.Clone();
        
        query.PostId = postId;
        query.Limit = 200;
        
        var response = await client.GetImages(query, ct);
        
        var images = response.items.Select(mapper.ToImageModel);

        return images.ToList();
    }
}