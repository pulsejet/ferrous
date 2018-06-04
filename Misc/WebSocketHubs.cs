using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Ferrous.WebSocketHubs
{
    /* WebSocket Hub for Building Updates */
    public class BuildingUpdateHub : Hub
    {
        public static readonly string BuildingWebsocketUrl = "/api/websocket/building";
        public async Task JoinBuilding(string building)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, building);
        }
    }
}
