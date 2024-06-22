using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleShip.Models;

namespace BattleShip.Hubs
{
    public class GameHub : Hub
    {
        private static ConcurrentDictionary<string, GameRoom> gameRooms;

        // Constructor to initialize the gameRooms dictionary
        public GameHub()
        {
            if (gameRooms == null)
            {
                gameRooms = new ConcurrentDictionary<string, GameRoom>();
            }
        }

        public async Task CreateRoom(string creator)
        {
            var existingRoom = gameRooms.Values.FirstOrDefault(r => r.Creator == creator);
            if (existingRoom != null)
            {
                await Clients.Caller.SendAsync("ErrorMessage", "You already have an active game room.");
                return;
            }

            var roomId = Guid.NewGuid().ToString();
            var gameRoom = new GameRoom
            {
                RoomId = roomId,
                Creator = creator,
                Players = new List<string> { creator },
                PlayersReady = new Dictionary<string, bool> { { creator, false } }
            };
            gameRooms[roomId] = gameRoom;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId); // Add creator to group
            await Clients.Caller.SendAsync("RedirectToGame", roomId, creator); // Redirect creator to game room
            await Clients.All.SendAsync("RoomCreated", gameRoom); // Notify all connected users
        }

        public async Task<List<GameRoom>> GetRooms()
        {
            return await Task.FromResult(gameRooms.Values.ToList());
        }

        public async Task<List<string>> GetPlayersInRoom(string roomId)
        {
            if (gameRooms.TryGetValue(roomId, out var gameRoom))
            {
                return await Task.FromResult(gameRoom.Players);
            }
            return new List<string>();
        }

        public async Task JoinRoom(string roomId, string player)
        {
            if (gameRooms.ContainsKey(roomId))
            {
                var gameRoom = gameRooms[roomId];
                if (gameRoom.Players.Count < 2)
                {
                    if (!gameRoom.Players.Contains(player))
                    {
                        gameRoom.Players.Add(player);
                    }

                    gameRoom.PlayersReady[player] = false;
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomId); // Add user to SignalR group
                    await Clients.Caller.SendAsync("RedirectToGame", roomId, player);
                    Console.WriteLine($"Player {player} (ConnectionId: {Context.ConnectionId}) added to group {roomId}"); // Log group addition
                    await Clients.Caller.SendAsync("ReceivePlayers", gameRoom.Players); // Send current players to the new user
                    await Clients.Group(roomId).SendAsync("PlayerJoined", gameRoom.Players);
                    await Clients.Group(roomId).SendAsync("ReceiveMessage", "System", $"{player} dołączył do gry.");
                    await Clients.All.SendAsync("RoomUpdated", roomId, gameRoom.Players.Count);
                }
                else
                {
                    await Clients.Caller.SendAsync("ErrorMessage", "Room is full.");
                }
            }
        }

        public async Task SendMove(string roomId, string player, string move)
        {
            if (gameRooms.TryGetValue(roomId, out var gameRoom))
            {
                if (gameRoom.PlayersReady.Values.All(ready => ready) && gameRoom.Players.Count == 2)
                {
                    await Clients.Group(roomId).SendAsync("ReceiveMove", player, move);
                }
            }
        }

        public async Task SendMessage(string roomId, string user, string message)
        {
            Console.WriteLine($"Sending message from {user} to room {roomId}: {message}");
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user, message);
            Console.WriteLine($"Message sent from {user} to room {roomId}: {message}");
        }

        public async Task LeaveRoom(string roomId, string player)
        {
            if (gameRooms.ContainsKey(roomId))
            {
                var gameRoom = gameRooms[roomId];
                gameRoom.Players.Remove(player);
                gameRoom.PlayersReady.Remove(player);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                await Clients.Group(roomId).SendAsync("PlayerLeft", player);
                await Clients.Group(roomId).SendAsync("ReceiveMessage", "System", $"{player} opuścił grę.");
                await Clients.All.SendAsync("RoomUpdated", roomId, gameRoom.Players.Count);

                if (!gameRooms[roomId].Players.Any())
                {
                    gameRooms.TryRemove(roomId, out _);
                    await Clients.All.SendAsync("RoomDeleted", roomId);
                }
            }
        }

        public async Task DeleteRoom(string roomId)
        {
            if (gameRooms.ContainsKey(roomId))
            {
                foreach (var player in gameRooms[roomId].Players)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                }
                gameRooms.TryRemove(roomId, out _);
                await Clients.All.SendAsync("RoomDeleted", roomId);
            }
        }

        public async Task SetReady(string roomId, string player, bool isReady)
        {
            if (gameRooms.ContainsKey(roomId))
            {
                gameRooms[roomId].PlayersReady[player] = isReady;
                await Clients.Group(roomId).SendAsync("PlayerReadyStatus", player, isReady);

                if (gameRooms[roomId].PlayersReady.Values.All(ready => ready) && gameRooms[roomId].Players.Count == 2)
                {
                    await Clients.Group(roomId).SendAsync("BothPlayersReady");
                }
            }
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            foreach (var room in gameRooms.Values)
            {
                if (room.Players.Contains(Context.ConnectionId))
                {
                    room.Players.Remove(Context.ConnectionId);
                    await Clients.Group(room.RoomId).SendAsync("PlayerLeft", Context.ConnectionId);
                    await Clients.Group(room.RoomId).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} opuścił grę.");
                    await Clients.All.SendAsync("RoomUpdated", room.RoomId, room.Players.Count);

                    if (!room.Players.Any())
                    {
                        gameRooms.TryRemove(room.RoomId, out _);
                        await Clients.All.SendAsync("RoomDeleted", room.RoomId);
                    }
                    break;
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
