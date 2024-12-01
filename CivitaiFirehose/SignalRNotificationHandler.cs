using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CivitaiFirehose;

public class SignalRNotificationHandler : INotificationHandler<NewImageNotification>
{
    readonly IHubContext<ImageHub> _hubContext;

    public SignalRNotificationHandler(IHubContext<ImageHub> hubContext) => _hubContext = hubContext;

    public Task Handle(NewImageNotification notification, CancellationToken ct)
        => _hubContext.Clients.All.SendAsync("NewImage", notification.Image, ct);
}