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

        // Mapă: TcpClient -> username (pentru a ști cine e cine)
        private Dictionary<TcpClient, string> clientNames
            = new Dictionary<TcpClient, string>();

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serverThread = new Thread(StartServer) { IsBackground = true };
            serverThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { server?.Stop(); } catch { }
        }

        // ─── PORNIRE SERVER ───────────────────────────────────────────
        private void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 8888);
                server.Start();
                Log("✅ Serverul a pornit pe portul 8888...");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    lock (Program.clienti) { Program.clienti.Add(client); }
                    Log("🔌 Client nou conectat (în așteptare LOGIN)");

                    Thread t = new Thread(() => HandleClient(client))
                    { IsBackground = true };
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                // Serverul s-a oprit (ex: la închiderea formei)
                if (ex is ObjectDisposedException || ex is SocketException) return;
                Log("❌ Eroare server: " + ex.Message);
            }
        }

        // ─── GESTIONARE CLIENT ────────────────────────────────────────
        private void HandleClient(TcpClient c)
        {
            NetworkStream stream = null;
            string username = "Necunoscut";

            try
            {
                stream = c.GetStream();
                byte[] buffer = new byte[2048];
                int bytes;

                while ((bytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string raw = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                    var (type, fields) = Protocol.Parse(raw);

                    Log($"📨 Primit [{type}]: {raw}");

                    switch (type)
                    {
                        case Protocol.LOGIN:
                            // LOGIN|NumeUser
                            if (fields.Length >= 2)
                            {
                                username = fields[1];
                                lock (clientNames) { clientNames[c] = username; }
                                Log($"👤 '{username}' s-a autentificat.");
                                // Anunță toți că a intrat cineva nou
                                Broadcast(Protocol.BuildServerMsg(
                                    $"'{username}' a intrat în lobby!"), null);
                            }
                            break;

                        case Protocol.CHAT:
                            // CHAT|NumeUser|Mesaj
                            if (fields.Length >= 3)
                            {
                                string sender = fields[1];
                                string text = fields[2];
                                Log($"💬 {sender}: {text}");
                                // Re-broadcast mesajul chat la toți
                                Broadcast(Protocol.BuildChat(sender, text), null);
                            }
                            break;

                        case Protocol.DISCONNECT:
                            // Client anunță că se deconectează
                            Log($"👋 '{username}' s-a deconectat.");
                            Broadcast(Protocol.BuildServerMsg(
                                $"'{username}' a ieșit din lobby."), c);
                            goto CleanExit;

                        default:
                            Log($"⚠️ Mesaj necunoscut: {raw}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ObjectDisposedException))
                    Log($"⚠️ Eroare client '{username}': {ex.Message}");
            }

        CleanExit:
            RemoveClient(c, username);
            try { stream?.Close(); c.Close(); } catch { }
        }

        // ─── BROADCAST ────────────────────────────────────────────────
        private void Broadcast(string message, TcpClient exceptClient)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            TcpClient[] targets;
            lock (Program.clienti) { targets = Program.clienti.ToArray(); }

            foreach (var tc in targets)
            {
                if (tc == exceptClient) continue;
                try { tc.GetStream().Write(data, 0, data.Length); }
                catch { RemoveClient(tc, "?"); }
            }
        }

        // ─── REMOVE CLIENT ────────────────────────────────────────────
        private void RemoveClient(TcpClient c, string name)
        {
            lock (Program.clienti) { Program.clienti.Remove(c); }
            lock (clientNames) { clientNames.Remove(c); }
            Log($"🗑️ Client '{name}' eliminat din listă.");
        }

        // ─── HELPER LOG (thread-safe) ─────────────────────────────────
        private void Log(string msg)
        {
            if (listBox1.InvokeRequired)
                Invoke((MethodInvoker)(() => listBox1.Items.Add(msg)));
            else
                listBox1.Items.Add(msg);
        }

        private void btnConnect_Click(object sender, EventArgs e)
            => MessageBox.Show("Serverul ascultă pe portul 8888");

        private void btnSend_Click(object sender, EventArgs e)
            => MessageBox.Show("Serverul transmite automat mesajele primite");

        private void textBox2_TextChanged(object sender, EventArgs e) { }
    }
}