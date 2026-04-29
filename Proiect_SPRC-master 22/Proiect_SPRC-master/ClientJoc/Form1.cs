using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClientJoc
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private string username = "";
        private bool connected = false;
        private GameForm gameForm = null;
        private bool isMyTurn = false;
        private int[,] pendingGrid = null;
        private string adversarName = "";

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (connected) return;
            string user = txtUsername.Text.Trim();
            if (string.IsNullOrEmpty(user))
            {
                MessageBox.Show("Introdu un username!");
                return;
            }
            try
            {
                client = new TcpClient("127.0.0.1", 8888);
                stream = client.GetStream();
                username = user;
                connected = true;

                Send(Protocol.BuildLogin(username));

                receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                receiveThread.Start();

                AddMessage("✅ Conectat ca '" + username + "' — aștepți adversar...");
                btnConnect.Enabled = false;
                txtUsername.Enabled = false;
                btnSend.Enabled = true;
                txtMessage.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e) => SendChatMessage();

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) SendChatMessage();
        }

        private void SendChatMessage()
        {
            if (!connected) return;
            string text = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            Send(Protocol.BuildChat(username, text));
            txtMessage.Clear();
        }

        private void ReceiveLoop()
        {
            byte[] buffer = new byte[4096];
            while (connected)
            {
                try
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break;

                    string raw = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                    string[] fields = Protocol.Parse(raw);
                    string type = fields[0].Trim().ToUpper();

                    switch (type)
                    {
                        case Protocol.SERVER_MSG:
                            if (fields.Length >= 2)
                                AddMessage("🔔 " + fields[1]);
                            break;

                        case Protocol.CHAT:
                            if (fields.Length >= 3)
                            {
                                string chatMsg = "💬 " + fields[1] + ": " + fields[2];
                                AddMessage(chatMsg);
                                if (gameForm != null)
                                    gameForm.AddChat(chatMsg);
                            }
                            break;

                        case Protocol.YOUR_GRID:
                            if (fields.Length >= 2)
                                pendingGrid = DecodeGrid(fields[1]);
                            break;

                        case Protocol.GAME_START:
                            if (fields.Length >= 3)
                            {
                                string adv = fields[1];
                                bool estiPrimul = fields[2] == "1";
                                adversarName = adv;
                                isMyTurn = estiPrimul;
                                OpenGameForm(adv, estiPrimul);
                            }
                            break;

                        case Protocol.RESULT:
                            if (fields.Length >= 5 && gameForm != null)
                            {
                                string[] coords = fields[1].Split(',');
                                int row = int.Parse(coords[0]);
                                int col = int.Parse(coords[1]);
                                string outcome = fields[2];
                                bool myTurnNow = fields[3] == "1";
                                bool gOver = fields[4] == "1";

                                // Salvăm ÎNAINTE de orice modificare
                                bool iWasAttacker = isMyTurn;
                                isMyTurn = myTurnNow;

                                int r = row;
                                int c = col;
                                bool iat = iWasAttacker;
                                string oc = outcome;
                                bool mtn = myTurnNow;
                                bool go = gOver;
                                string un = username;
                                string an = adversarName;

                                Invoke((MethodInvoker)(() =>
                                {
                                    if (iat)
                                    {
                                        // EU am atacat
                                        gameForm.ApplyResult(r, c, oc, mtn);
                                        if (go)
                                        {
                                            // Eu am câștigat
                                            gameForm.SetGameOver(un, true);
                                            AddMessage("🏆 Ai câștigat!");
                                        }
                                    }
                                    else
                                    {
                                        // ADVERSARUL a atacat
                                        gameForm.ApplyEnemyAttack(r, c, oc, mtn);
                                        if (go)
                                        {
                                            // Adversarul a câștigat
                                            gameForm.SetGameOver(an, false);
                                            AddMessage("😞 " + an + " a câștigat!");
                                        }
                                    }
                                }));
                            }
                            break;
                    }
                }
                catch { break; }
            }
            AddMessage("❌ Deconectat.");
        }

        private int[,] DecodeGrid(string encoded)
        {
            const int SIZE = 8;
            var grid = new int[SIZE, SIZE];
            for (int i = 0; i < encoded.Length && i < SIZE * SIZE; i++)
                grid[i / SIZE, i % SIZE] = encoded[i] - '0';
            return grid;
        }

        private void OpenGameForm(string adversar, bool estiPrimul)
        {
            Invoke((MethodInvoker)(() =>
            {
                gameForm = new GameForm(adversar, estiPrimul,
                    pendingGrid ?? new int[8, 8]);

                gameForm.OnAttack += (row, col) =>
                {
                    // NU modificăm isMyTurn aici — serverul ne spune rezultatul
                    Send(Protocol.BuildAttack(row, col));
                };

                gameForm.OnChatMessage += (text) =>
                    Send(Protocol.BuildChat(username, text));

                gameForm.Show();
                AddMessage("🎮 Meci vs '" + adversar + "'! "
                    + (estiPrimul ? "Tu ești primul!" : "Adversarul începe."));
            }));
        }

        private void Send(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch { }
        }

        private void AddMessage(string msg)
        {
            if (lstChat.InvokeRequired)
                Invoke((MethodInvoker)(() =>
                {
                    lstChat.Items.Add(msg);
                    lstChat.TopIndex = lstChat.Items.Count - 1;
                }));
            else
            {
                lstChat.Items.Add(msg);
                lstChat.TopIndex = lstChat.Items.Count - 1;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connected)
            {
                Send(Protocol.BuildDisconnect(username));
                connected = false;
                try { stream?.Close(); client?.Close(); } catch { }
            }
        }
    }
}