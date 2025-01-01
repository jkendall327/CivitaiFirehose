using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace CivitaiFirehose;

public sealed class HydrusClient(HttpClient client)
{
    public async Task VerifyAccess(CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync("verify_access_key", cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private class HydrusService
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("type")] public int Type { get; set; }
        [JsonPropertyName("type_pretty")] public string TypePretty { get; set; }
        [JsonPropertyName("star_shape")] public string? StarShape { get; set; }
    }

    public async Task<Dictionary<string, string>> GetServices(CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync("get_services", cancellationToken);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var servicesElement = root.GetProperty("services");
        var services = servicesElement.Deserialize<Dictionary<string, HydrusService>>();

        return services.ToDictionary(s => s.Value.Name, s => s.Key);
    }

    private record ImportImageResult(int Status, string Hash, string Note);

    public async Task<string> SendImageToHydrus(byte[] bytes, CancellationToken cancellationToken = default)
    {
        client.DefaultRequestHeaders.Accept.Add(new("application/octet-stream"));
        var byteContent = new ByteArrayContent(bytes);
        byteContent.Headers.ContentType = new("application/octet-stream");

        var response = await client.PostAsync("add_files/add_file", byteContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ImportImageResult>(cancellationToken: cancellationToken);

        return result!.Hash;
    }

    public async Task AssociateUrlWithImage(string hash, IEnumerable<string> urls, CancellationToken cancellationToken = default)
    {
        var response = await client.PostAsJsonAsync("add_urls/associate_url",
            new
            {
                hash,
                urls_to_add = urls
            },
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task AddTagsToImage(string hash, IEnumerable<string> tags, string service, CancellationToken cancellationToken = default)
    {
        var serviceTags = new Dictionary<string, List<string>>
        {
            {
                service, tags.ToList()
            }
        };

        var body = new
        {
            hash,
            service_keys_to_tags = serviceTags
        };

        var response = await client.PostAsJsonAsync("add_tags/add_tags", body, cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}