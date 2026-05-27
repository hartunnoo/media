using Microsoft.AspNetCore.SignalR;

namespace Media.Infrastructure.Hubs;

public class MediaHub : Hub
{
    public async Task SubscribeToMedia(Guid mediaId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, mediaId.ToString());
}
