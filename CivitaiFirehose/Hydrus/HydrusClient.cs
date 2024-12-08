using System.Text.Json;
using System.Text.Json.Serialization;

namespace CivitaiFirehose;

public class HydrusClient(HttpClient client, ILogger<HydrusClient> logger)
{
    public async Task VerifyAccess()
    {
        var response = await client.GetAsync("verify_access_key");

        response.EnsureSuccessStatusCode();
    }

    private class HydrusService
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("type")] public int Type { get; set; }
        [JsonPropertyName("type_pretty")] public string TypePretty { get; set; }
        [JsonPropertyName("star_shape")] public string? StarShape { get; set; }
    }

    public async Task<Dictionary<string, string>> GetServices()
    {
        var response = await client.GetAsync("get_services");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var servicesElement = root.GetProperty("services");
        var services = servicesElement.Deserialize<Dictionary<string, HydrusService>>();

        return services.ToDictionary(s => s.Value.Name, s => s.Key);
    }

    private record ImportImageResult(int Status, string Hash, string Note);

    public async Task<string> SendImageToHydrus(byte[] bytes)
    {
        client.DefaultRequestHeaders.Accept.Add(new("application/octet-stream"));
        var byteContent = new ByteArrayContent(bytes);
        byteContent.Headers.ContentType = new("application/octet-stream");

        var response = await client.PostAsync("add_files/add_file", byteContent);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ImportImageResult>();

        return result!.Hash;
    }

    public async Task AssociateUrlWithImage(string hash, IEnumerable<string> urls)
    {
        var response = await client.PostAsJsonAsync("add_urls/associate_url",
            new
            {
                hash,
                urls_to_add = urls
            });

        response.EnsureSuccessStatusCode();
    }

    public async Task AddTagsToImage(string hash, IEnumerable<string> tags, string service)
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

        var response = await client.PostAsJsonAsync("add_tags/add_tags", body);

        response.EnsureSuccessStatusCode();
    }
}