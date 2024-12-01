using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CivitaiFirehose;

public class SignalRNotificationHandler : INotificationHandler<NewImageNotification>
{
    readonly IHubContext<ImageHub> _hubContext;
    readonly ILogger<SignalRNotificationHandler> _logger;

    public SignalRNotificationHandler(
        IHubContext<ImageHub> hubContext,
        ILogger<SignalRNotificationHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(NewImageNotification notification, CancellationToken ct)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("NewImage", notification.Image, ct);
            _logger.LogDebug("Sent new image notification to clients");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send image notification to clients");
            throw;
        }
    }
}