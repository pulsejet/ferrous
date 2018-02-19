﻿using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Ferrous.WebSocketHubs
{
    /* WebSocket Hub for Building Updates */
    public class BuildingUpdateHub : Hub
    {
        public async Task JoinBuilding(string building)
        {
            await Groups.AddAsync(Context.ConnectionId, building);
            return;
        }
    }
}