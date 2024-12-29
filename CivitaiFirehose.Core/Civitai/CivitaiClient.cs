using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CivitaiFirehose;

/// <summary>
/// Encapsulates making actual HTTP requests to the Civitai API.
/// </summary>
public sealed class CivitaiClient(HttpClient client, ILogger<CivitaiClient> logger)
{
    public async Task<CivitaiResponse> GetImages(CivitaiQuery query, CancellationToken cancellationToken = default)
    {
        var opt = query.ToDictionary();
        
        var uri = QueryHelpers.AddQueryString("https://civitai.com/api/v1/images", opt);
        
        logger.LogInformation("Getting images from URI {RequestUri}", uri);

        var response = await client.GetAsync(uri, cancellationToken);

        // Rather than retrying or whatever, just return an empty response as this will cause no trouble for the UI.
        if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            logger.LogError("Server error from Civitai (likely transient), with code {StatusCode}", response.StatusCode);
            return new([], new(string.Empty, string.Empty));
        }
        
        var result = await response.Content.ReadFromJsonAsync<CivitaiResponse>(cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("Error deserializing Civitai response JSON");
        }

        return result;
    }
}