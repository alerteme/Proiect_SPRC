using System.Net.Sockets;

namespace ServerJoc
{
    public class GameRoom
    {
        public TcpClient Player1 { get; set; }
        public TcpClient Player2 { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }

        public ShipGrid Grid1 { get; set; } = new ShipGrid();
        public ShipGrid Grid2 { get; set; } = new ShipGrid();

        public bool IsPlayer1Turn { get; set; } = true;
        public bool GameStarted { get; set; } = false;

        public TcpClient CurrentPlayer
        {
            get { return IsPlayer1Turn ? Player1 : Player2; }
        }

        public TcpClient OtherPlayer
        {
            get { return IsPlayer1Turn ? Player2 : Player1; }
        }

        public ShipGrid GridOfCurrent
        {
            get { return IsPlayer1Turn ? Grid1 : Grid2; }
        }

        public ShipGrid GridOfOther
        {
            get { return IsPlayer1Turn ? Grid2 : Grid1; }
        }

        public void SwitchTurn()
        {
            IsPlayer1Turn = !IsPlayer1Turn;
        }

        public bool IsFull
        {
            get { return Player1 != null && Player2 != null; }
        }
    }
}