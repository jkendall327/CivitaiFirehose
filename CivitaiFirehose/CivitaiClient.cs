using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiClient(HttpClient client, IOptionsMonitor<CivitaiSettings> options)
{
    public async Task<CivitaiResponse> GetImages(CancellationToken cancellationToken = default)
    {
        var opt = options.CurrentValue.QueryDefaults;
        
        var uri = QueryHelpers.AddQueryString("https://civitai.com/api/v1/images", opt.ToDictionary());
        
        var response = await client.GetFromJsonAsync<CivitaiResponse>(uri, cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException();
        }

        return response;
    }
}