using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiClient(HttpClient client, IOptionsMonitor<CivitaiSettings> options, ILogger<CivitaiClient> logger)
{
    public async Task<CivitaiResponse> GetImages(CancellationToken cancellationToken = default)
    {
        var opt = options.CurrentValue.QueryDefaults;
        
        var uri = QueryHelpers.AddQueryString("https://civitai.com/api/v1/images", opt.ToDictionary());
        
        logger.LogInformation("Getting images with URI {RequestUri}", uri);
        
        var response = await client.GetFromJsonAsync<CivitaiResponse>(uri, cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException("Error while getting images");
        }

        return response;
    }
    
    public async Task<CivitaiResponse> GetImagesFromPost(int postId, CancellationToken cancellationToken = default)
    {
        var query = options.CurrentValue.QueryDefaults;

        // Maximum to ensure we get all images.
        query.Limit = 100;
        query.PostId = postId;
        
        var uri = QueryHelpers.AddQueryString("https://civitai.com/api/v1/images", query.ToDictionary());
        
        logger.LogInformation("Getting all images from post {PostId}", postId);
        
        var response = await client.GetFromJsonAsync<CivitaiResponse>(uri, cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException("Error while getting images from post");
        }

        return response;
    }
}