using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public sealed class CivitaiClient(HttpClient client, ILogger<CivitaiClient> logger)
{
    public async Task<CivitaiResponse> GetImages(CivitaiQuery query, CancellationToken cancellationToken = default)
    {
        var opt = query.ToDictionary();
        
        var uri = QueryHelpers.AddQueryString("https://civitai.com/api/v1/images", opt);
        
        logger.LogInformation("Getting images from URI {RequestUri}", uri);
        
        var response = await client.GetFromJsonAsync<CivitaiResponse>(uri, cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException("Error while getting images");
        }

        return response;
    }
}