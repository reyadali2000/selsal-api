using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WadiAlAin.Application.Common.Interfaces;

namespace WadiAlAin.Infrastructure.Services
{
    public class RealtimeNotificationHub : IRealtimeNotificationHub
    {
        private class ConnectedClient
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public WebSocket Socket { get; set; } = null!;
            public string? Role { get; set; }
            public int? BranchId { get; set; }
            public string? Username { get; set; }
        }

        private readonly ConcurrentDictionary<Guid, ConnectedClient> _clients = new();
        private readonly ILogger<RealtimeNotificationHub> _logger;

        public RealtimeNotificationHub(ILogger<RealtimeNotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
        {
            var role = context.Request.Query["role"].ToString();
            var branchStr = context.Request.Query["branchId"].ToString();
            int? branchId = int.TryParse(branchStr, out var b) && b > 0 ? b : null;
            var username = context.Request.Query["username"].ToString();

            var client = new ConnectedClient
            {
                Socket = webSocket,
                Role = string.IsNullOrWhiteSpace(role) ? null : role,
                BranchId = branchId,
                Username = string.IsNullOrWhiteSpace(username) ? null : username
            };

            _clients.TryAdd(client.Id, client);
            _logger.LogInformation("Realtime notification client connected: ID={ClientId}, Role={Role}, Branch={Branch}, User={User}", 
                client.Id, client.Role, client.BranchId, client.Username);

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("WebSocket client disconnected: {Message}", ex.Message);
            }
            finally
            {
                _clients.TryRemove(client.Id, out _);
                _logger.LogInformation("Realtime notification client disconnected: ID={ClientId}", client.Id);
            }
        }

        public async Task BroadcastNotificationAsync(object notification, string? targetRole = null, int? branchId = null)
        {
            var json = JsonSerializer.Serialize(new
            {
                type = "NEW_NOTIFICATION",
                data = notification,
                timestamp = DateTime.UtcNow.ToString("o")
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var bytes = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(bytes);

            var deadClients = new List<Guid>();

            foreach (var kvp in _clients)
            {
                var client = kvp.Value;
                if (client.Socket.State != WebSocketState.Open)
                {
                    deadClients.Add(kvp.Key);
                    continue;
                }

                // Filter by role if specified
                if (!string.IsNullOrWhiteSpace(targetRole) && !string.Equals(targetRole, "All", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(client.Role) && 
                        !string.Equals(client.Role, targetRole, StringComparison.OrdinalIgnoreCase) && 
                        !string.Equals(client.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                // Filter by branch if specified
                if (branchId.HasValue && branchId.Value > 0 && client.BranchId.HasValue && client.BranchId.Value > 0)
                {
                    if (client.BranchId.Value != branchId.Value)
                    {
                        continue;
                    }
                }

                try
                {
                    await client.Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    deadClients.Add(kvp.Key);
                }
            }

            foreach (var id in deadClients)
            {
                _clients.TryRemove(id, out _);
            }
        }
    }
}
