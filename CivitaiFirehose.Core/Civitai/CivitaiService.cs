using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public sealed class CivitaiService(CivitaiClient client, ImageMapper mapper, IOptions<CivitaiSettings> options) : ICivitaiService
{
    public async Task<List<ImageModel>> GetNewestImages(CancellationToken ct)
    {
        var images = await GetImages(Set, ct);

        return images.OrderByDescending(s => s.CreatedAt).ToList();
        
        void Set(CivitaiQuery query)
        {
            query.Sort = SortOrder.Newest;
        }
    }

    public async Task<List<ImageModel>> GetImagesFromModel(int modelId, int? modelVersionId = null, CancellationToken ct = default)
    {
        return await GetImages(Set, ct);

        void Set(CivitaiQuery query)
        {
            query.ModelId = modelId;
            query.ModelVersionId = modelVersionId;
            query.Limit = 200;
        }
    }

    public async Task<List<ImageModel>> GetImagesFromPost(int postId, CancellationToken ct = default)
    {
        return await GetImages(Set, ct);

        void Set(CivitaiQuery query)
        {
            query.PostId = postId;
            query.Limit = 200;
        }
    }

    public async Task<List<ImageModel>> GetImagesFromUser(string userName, CancellationToken ct)
    {
        // TODO: use the cursor to ensure we get everything.
        return await GetImages(Set, ct);

        void Set(CivitaiQuery query)
        {
            query.Username = userName;
            query.Limit = 200;
        }
    }

    private async Task<List<ImageModel>> GetImages(Action<CivitaiQuery> action, CancellationToken ct = default)
    {
        var query = options.Value.QueryDefaults.Clone();

        action(query);

        var response = await client.GetImages(query, ct);

        var images = response.items.Select(mapper.ToImageModel);

        return images.ToList();
    }
}