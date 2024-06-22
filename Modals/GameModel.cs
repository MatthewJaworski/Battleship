namespace BattleShip.Models
{
    public class GameModel
    {
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string[,] Board1 { get; set; } = new string[10, 10];
        public string[,] Board2 { get; set; } = new string[10, 10];
        // Dodaj inne właściwości i metody według potrzeby
    }
}