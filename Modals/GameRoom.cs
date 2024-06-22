using System.Collections.Generic;

namespace BattleShip.Models
{
    public class GameRoom
    {
        public string RoomId { get; set; }
        public string Creator { get; set; }
        public List<string> Players { get; set; } = new List<string>();
        public Dictionary<string, bool> PlayersReady { get; set; } = new Dictionary<string, bool>();
    }
}
