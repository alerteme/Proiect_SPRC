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
                client    = new TcpClient("127.0.0.1", 8888);
                stream    = client.GetStream();
                username  = user;
                connected = true;

                Send(Protocol.BuildLogin(username));

                receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                receiveThread.Start();

                AddMessage($"✅ Conectat ca '{username}'");
                btnConnect.Enabled  = false;
                txtUsername.Enabled = false;
                btnSend.Enabled     = true;
                txtMessage.Enabled  = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare conectare: " + ex.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendChatMessage();
        }

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
            byte[] buffer = new byte[2048];
            while (connected)
            {
                try
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break;

                    string raw = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                    var (type, fields) = Protocol.Parse(raw);

                    switch (type)
                    {
                        case Protocol.CHAT:
                            if (fields.Length >= 3)
                                AddMessage($"💬 {fields[1]}: {fields[2]}");
                            break;
                        case Protocol.SERVER_MSG:
                            if (fields.Length >= 2)
                                AddMessage($"🔔 [Server] {fields[1]}");
                            break;
                        default:
                            AddMessage($"[{type}] {raw}");
                            break;
                    }
                }
                catch { break; }
            }
            AddMessage("❌ Deconectat de la server.");
        }

        private void Send(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                AddMessage("❌ Eroare trimitere: " + ex.Message);
            }
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