using System.Net;

namespace CivitaiFirehose.Tests;

public class TestHttpMessageHandler() : HttpMessageHandler
{
    public string Response { get; set; } = "{}";
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Response)
        };

        return Task.FromResult(response);
    }
}