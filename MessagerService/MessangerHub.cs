using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MessagerService
{
    public class MessangerHub: Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Call the "ReceiveMessage" method on all connected clients
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
