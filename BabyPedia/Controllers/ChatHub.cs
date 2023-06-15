using Microsoft.AspNetCore.SignalR;

namespace BabyPedia.Controllers;

public class ChatHub : Hub
{
    public async Task SendMessage(string from, string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", from, user, message);
    }

    public async Task SendNotification(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", user, message);
    }
}