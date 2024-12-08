namespace CivitaiFirehose;

public class HydrusClient(HttpClient client, ILogger<HydrusClient> logger)
{
    public async Task VerifyAccess()
    {
        var response = await client.GetAsync("verify_access_key");
        
        response.EnsureSuccessStatusCode();
    }
    
    private record ImportImageRequest(string Path);
    public async Task SendImageToHydrus(string path)
    {
        var response = await client.PostAsJsonAsync<ImportImageRequest>("add_files/add_file", new(path));
        
        response.EnsureSuccessStatusCode();
    }

    public async Task AddTagsToImage()
    {
        throw new NotImplementedException();
    }

    public async Task AssociateUrlWithImage()
    {
        throw new NotImplementedException();
    }
}