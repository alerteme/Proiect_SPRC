using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ServerJoc
{
    public partial class Form1 : Form
    {
        private TcpListener server;
        private Thread serverThread;

        private Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();
        private List<GameRoom> rooms = new List<GameRoom>();
        private GameRoom waitingRoom = null; // cameră cu un singur jucător

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += (s, e) => { try { server?.Stop(); } catch { } };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serverThread = new Thread(StartServer) { IsBackground = true };
            serverThread.Start();
        }

        private void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 8888);
                server.Start();
                Log("✅ Serverul pornit pe portul 8888");
                while (true)
                {
                    var client = server.AcceptTcpClient();
                    Log("🔌 Client nou conectat");
                    new Thread(() => HandleClient(client))
                    { IsBackground = true }.Start();
                }
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException || ex is SocketException) return;
                Log("❌ " + ex.Message);
            }
        }

        private void HandleClient(TcpClient c)
        {
            NetworkStream stream = null;
            string username = "?";
            GameRoom myRoom = null;

            try
            {
                stream = c.GetStream();
                byte[] buffer = new byte[4096];
                int bytes;

                while ((bytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string raw = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                    string[] fields = Protocol.Parse(raw);
                    string type = fields[0].Trim().ToUpper();
                    Log($"📨 [{username}] {raw}");

                    switch (type)
                    {
                        case Protocol.LOGIN:
                            if (fields.Length >= 2)
                            {
                                username = fields[1];
                                lock (clientNames) { clientNames[c] = username; }
                                Log($"👤 '{username}' conectat");

                                // Adaugă în cameră
                                myRoom = AssignToRoom(c, username);
                                Send(c, Protocol.BuildServerMsg(
                                    myRoom.IsFull
                                        ? $"Meci găsit! Adversar: {(myRoom.Player1 == c ? myRoom.Player2Name : myRoom.Player1Name)}"
                                        : "Aștepți adversar..."));
                            }
                            break;

                        case Protocol.ATTACK:
                            if (myRoom != null && myRoom.GameStarted && fields.Length >= 2)
                            {
                                if (myRoom.CurrentPlayer != c)
                                {
                                    Send(c, Protocol.BuildServerMsg("Nu e randul tau!"));
                                    break;
                                }

                                string[] coords = fields[1].Split(',');
                                int row = int.Parse(coords[0]);
                                int col = int.Parse(coords[1]);

                                ShipGrid targetGrid = myRoom.GridOfOther;
                                string outcome = targetGrid.Attack(row, col);

                                if (outcome == "ALREADY")
                                {
                                    Send(c, Protocol.BuildServerMsg("Ai atacat deja acolo!"));
                                    break;
                                }

                                bool gameOver = targetGrid.AllSunk;

                                bool attackerKeepsTurn =
                                    (outcome == "HIT" || outcome.StartsWith("SINK"))
                                    && !gameOver;

                                if (!attackerKeepsTurn)
                                    myRoom.SwitchTurn();

                                if (gameOver)
                                    myRoom.GameStarted = false;

                                bool p1Turn = myRoom.IsPlayer1Turn;

                                // P1 a atacat = c este Player1
                                bool attackerIsP1 = (myRoom.Player1 == c);

                                // Trimitem la P1: yourTurn = p1Turn (e rândul lui acum?)
                                // Trimitem la P2: yourTurn = !p1Turn
                                // gameOver=true și atacatorIsP1 → P1 a câștigat
                                // winner=true la P1 dacă el a atacat, winner=false la P2
                                Send(myRoom.Player1, Protocol.BuildResult(
                                    row, col, outcome,
                                    p1Turn,              // e rândul lui P1 acum?
                                    gameOver));

                                Send(myRoom.Player2, Protocol.BuildResult(
                                    row, col, outcome,
                                    !p1Turn,             // e rândul lui P2 acum?
                                    gameOver));

                                // NU mai trimitem GAME_OVER separat - e gestionat în RESULT
                                Log("💥 " + username + " ataca (" + row + "," + col + ") → " + outcome
                                    + (gameOver ? " 🏆 GAME OVER" : ""));
                            }
                            break;

                        case Protocol.CHAT:
                            if (fields.Length >= 3)
                            {
                                string msg = Protocol.BuildChat(fields[1], fields[2]);
                                if (myRoom != null)
                                {
                                    // Chat doar în cameră
                                    Send(myRoom.Player1, msg);
                                    if (myRoom.Player2 != null)
                                        Send(myRoom.Player2, msg);
                                }
                            }
                            break;

                        case Protocol.DISCONNECT:
                            goto CleanExit;
                    }
                }
            }
            catch { }

        CleanExit:
            RemoveClient(c, username, myRoom);
            try { stream?.Close(); c.Close(); } catch { }
        }

        private GameRoom AssignToRoom(TcpClient c, string username)
        {
            lock (rooms)
            {
                if (waitingRoom == null)
                {
                    // Prima cameră nouă
                    var room = new GameRoom
                    {
                        Player1 = c,
                        Player1Name = username
                    };
                    waitingRoom = room;
                    rooms.Add(room);
                    Log($"🏠 Camera nouă pentru '{username}'");
                    return room;
                }
                else
                {
                    // Al doilea jucător — pornim jocul
                    var room = waitingRoom;
                    room.Player2 = c;
                    room.Player2Name = username;
                    waitingRoom = null;

                    // Plasăm navele aleatoriu
                    room.Grid1.PlaceShipsRandomly();
                    room.Grid2.PlaceShipsRandomly();
                    room.GameStarted = true;

                    Log($"🎮 Meci: '{room.Player1Name}' vs '{room.Player2Name}'");

                    // Trimitem fiecărui jucător grila lui + start
                    Send(room.Player1, Protocol.BuildYourGrid(room.Grid1.EncodeForOwner()));
                    Send(room.Player2, Protocol.BuildYourGrid(room.Grid2.EncodeForOwner()));

                    Send(room.Player1, Protocol.BuildGameStart(room.Player2Name, true));
                    Send(room.Player2, Protocol.BuildGameStart(room.Player1Name, false));

                    return room;
                }
            }
        }

        private void RemoveClient(TcpClient c, string username, GameRoom room)
        {
            lock (clientNames) { clientNames.Remove(c); }
            if (room != null)
            {
                lock (rooms)
                {
                    if (waitingRoom == room) waitingRoom = null;
                    // Anunță adversarul
                    var other = room.Player1 == c ? room.Player2 : room.Player1;
                    if (other != null)
                        Send(other, Protocol.BuildServerMsg(
                            $"'{username}' s-a deconectat. Jocul s-a terminat."));
                    rooms.Remove(room);
                }
            }
            Log($"👋 '{username}' deconectat");
        }

        private void Send(TcpClient c, string message)
        {
            if (c == null) return;
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                c.GetStream().Write(data, 0, data.Length);
            }
            catch { }
        }

        private void Log(string msg)
        {
            if (listBox1.InvokeRequired)
                Invoke((MethodInvoker)(() => listBox1.Items.Add(msg)));
            else
                listBox1.Items.Add(msg);
        }

        private void btnConnect_Click(object sender, EventArgs e) { }
        private void btnSend_Click(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
    }
}