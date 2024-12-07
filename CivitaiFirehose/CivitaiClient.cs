using Microsoft.Extensions.Options;

namespace CivitaiFirehose;

public class CivitaiClient(HttpClient client, IOptions<CivitaiSettings> options)
{
    public async Task<CivitaiResponse> GetImages(CancellationToken cancellationToken = default)
    {
        var opt = options.Value;

        var uri = $"https://civitai.com/api/v1/images?sort=Newest&limit={opt.ImageCount}";
        
        var response = await client.GetFromJsonAsync<CivitaiResponse>(uri, cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException();
        }

        return response;
    }
}