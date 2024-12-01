using Microsoft.AspNetCore.SignalR;

namespace CivitaiFirehose;

public class ImageHub : Hub
{
    private readonly ILogger<ImageHub> _logger;

    public ImageHub(ILogger<ImageHub> logger) 
        => _logger = logger;

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public async Task ClientReady(string connectionId)
    {
        _logger.LogInformation("Client ready: {ConnectionId}", connectionId);
        // Let's test if we can send a message directly
        await Clients.Client(connectionId).SendAsync("NewImage", new ImageData("test", "test"));
    }
}