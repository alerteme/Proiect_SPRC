using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClientJoc
{
    public class GameForm : Form
    {
        private const int GRID_SIZE  = 8;
        private const int CELL_SIZE  = 45;
        private const int MARGIN     = 20;
        private const int GRID_WIDTH = GRID_SIZE * CELL_SIZE;

        // Grilelele: 0=apă, 1=navă, 2=hit, 3=miss
        private int[,] myGrid     = new int[GRID_SIZE, GRID_SIZE];
        private int[,] enemyGrid  = new int[GRID_SIZE, GRID_SIZE];

        private bool myTurn       = false;
        private bool gameOver     = false;
        private string adversar   = "";

        private Label lblStatus;
        private Label lblMyGrid;
        private Label lblEnemyGrid;
        private RichTextBox chatBox;
        private TextBox txtChat;
        private Button btnChat;

        // Evenimente pentru comunicare cu Form1
        public event Action<int, int> OnAttack;
        public event Action<string>   OnChatMessage;

        public GameForm(string adversarName, bool estiPrimul, int[,] grid)
        {
            this.adversar = adversarName;
            this.myTurn   = estiPrimul;
            this.myGrid   = grid;

            InitUI();
            UpdateStatus();
        }

        private void InitUI()
        {
            int totalWidth  = MARGIN * 3 + GRID_WIDTH * 2 + 200;
            int totalHeight = MARGIN * 3 + GRID_WIDTH + 120;

            this.Text          = $"Bătălie Navală vs {adversar}";
            this.ClientSize    = new Size(totalWidth, totalHeight);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            // Status
            lblStatus = new Label
            {
                Location  = new Point(MARGIN, 5),
                Size      = new Size(totalWidth - 200, 25),
                Font      = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblStatus);

            // Label grila mea
            lblMyGrid = new Label
            {
                Text     = "GRILA MEA",
                Location = new Point(MARGIN + GRID_WIDTH / 2 - 30, 30),
                Size     = new Size(100, 20),
                Font     = new Font("Arial", 9, FontStyle.Bold)
            };
            this.Controls.Add(lblMyGrid);

            // Label grila inamicului
            lblEnemyGrid = new Label
            {
                Text     = $"GRILA LUI {adversar.ToUpper()}",
                Location = new Point(MARGIN * 2 + GRID_WIDTH + GRID_WIDTH / 2 - 50, 30),
                Size     = new Size(200, 20),
                Font     = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };
            this.Controls.Add(lblEnemyGrid);

            // Panel pentru grila inamicului (clickabil)
            var enemyPanel = new Panel
            {
                Location  = new Point(MARGIN * 2 + GRID_WIDTH, 55),
                Size      = new Size(GRID_WIDTH, GRID_WIDTH),
                BackColor = Color.Transparent
            };
            enemyPanel.Paint     += EnemyPanel_Paint;
            enemyPanel.MouseClick += EnemyPanel_Click;
            this.Controls.Add(enemyPanel);

            // Panel pentru grila mea (doar afișare)
            var myPanel = new Panel
            {
                Location  = new Point(MARGIN, 55),
                Size      = new Size(GRID_WIDTH, GRID_WIDTH),
                BackColor = Color.Transparent
            };
            myPanel.Paint += MyPanel_Paint;
            this.Controls.Add(myPanel);

            int chatX = MARGIN * 3 + GRID_WIDTH * 2;

            // Chat
            chatBox = new RichTextBox
            {
                Location = new Point(chatX, 55),
                Size     = new Size(180, GRID_WIDTH - 30),
                ReadOnly = true,
                BackColor = Color.WhiteSmoke
            };
            this.Controls.Add(chatBox);

            txtChat = new TextBox
            {
                Location = new Point(chatX, 55 + GRID_WIDTH - 25),
                Size     = new Size(120, 23)
            };
            txtChat.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SendChat(); };
            this.Controls.Add(txtChat);

            btnChat = new Button
            {
                Text     = "💬",
                Location = new Point(chatX + 122, 55 + GRID_WIDTH - 25),
                Size     = new Size(58, 23)
            };
            btnChat.Click += (s, e) => SendChat();
            this.Controls.Add(btnChat);

            // Refresh panels când se invalidează
            this.Tag = new[] { myPanel, enemyPanel };
        }

        // ─── PAINT GRILA MEA ─────────────────────────────────────────
        private void MyPanel_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics, myGrid, false);
        }

        // ─── PAINT GRILA INAMICULUI ───────────────────────────────────
        private void EnemyPanel_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics, enemyGrid, true);
        }

        private void DrawGrid(Graphics g, int[,] grid, bool isEnemy)
        {
            for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
            {
                var rect  = new Rectangle(c * CELL_SIZE, r * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                Color fill;

                switch (grid[r, c])
                {
                    case 1:  fill = isEnemy ? Color.SteelBlue : Color.DimGray;   break; // navă
                    case 2:  fill = Color.OrangeRed;  break; // hit
                    case 3:  fill = Color.LightCyan;  break; // miss
                    default: fill = Color.SteelBlue;  break; // apă
                }

                // Pe grila inamicului nu arătăm navele
                if (isEnemy && grid[r, c] == 1)
                    fill = Color.SteelBlue;

                g.FillRectangle(new SolidBrush(fill), rect);
                g.DrawRectangle(Pens.Navy, rect);

                // Litere/numere pe bordură
                if (r == 0)
                    g.DrawString(((char)('A' + c)).ToString(),
                        new Font("Arial", 7), Brushes.White,
                        rect.X + CELL_SIZE / 2 - 4, rect.Y + 2);
                if (c == 0)
                    g.DrawString((r + 1).ToString(),
                        new Font("Arial", 7), Brushes.White,
                        rect.X + 2, rect.Y + CELL_SIZE / 2 - 6);
            }
        }

        // ─── CLICK PE GRILA INAMICULUI ────────────────────────────────
        private void EnemyPanel_Click(object sender, MouseEventArgs e)
        {
            if (!myTurn || gameOver) return;

            int col = e.X / CELL_SIZE;
            int row = e.Y / CELL_SIZE;

            if (row < 0 || row >= GRID_SIZE || col < 0 || col >= GRID_SIZE) return;
            if (enemyGrid[row, col] == 2 || enemyGrid[row, col] == 3) return; // deja atacat

            OnAttack?.Invoke(row, col);
            myTurn = false;
            UpdateStatus();
        }

        // ─── UPDATE DIN EXTERIOR ──────────────────────────────────────
        public void ApplyResult(int row, int col, string outcome, bool yourTurn)
        {
            // Actualizăm grila inamicului
            bool isHit = outcome.StartsWith("HIT") || outcome.StartsWith("SINK");
            enemyGrid[row, col] = isHit ? 2 : 3;

            myTurn = yourTurn;
            UpdateStatus();
            RefreshPanels();

            AddChat($"💥 ({(char)('A'+col)}{row+1}) → {outcome}");
        }

        public void ApplyEnemyAttack(int row, int col, string outcome)
        {
            bool isHit = outcome.StartsWith("HIT") || outcome.StartsWith("SINK");
            myGrid[row, col] = isHit ? 2 : 3;
            RefreshPanels();
        }

        public void SetGameOver(string winner)
        {
            gameOver = true;
            bool iWon = winner == this.Text.Replace($"Bătălie Navală vs {adversar}", "").Trim();
            lblStatus.Text      = iWon ? "🏆 AI CÂȘTIGAT!" : $"😞 '{winner}' a câștigat!";
            lblStatus.ForeColor = iWon ? Color.Green : Color.Red;
            AddChat($"🏆 Câștigător: {winner}");
        }

        public void AddChat(string msg)
        {
            if (chatBox.InvokeRequired)
                Invoke((MethodInvoker)(() => { chatBox.AppendText(msg + "\n"); }));
            else
                chatBox.AppendText(msg + "\n");
        }

        private void SendChat()
        {
            string text = txtChat.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            OnChatMessage?.Invoke(text);
            txtChat.Clear();
        }

        private void UpdateStatus()
        {
            if (lblStatus == null) return;
            lblStatus.Text      = myTurn ? "🎯 RÂNDUL TĂU — Click pe grila inamicului!" 
                                         : $"⏳ Aștepți... {adversar} atacă";
            lblStatus.ForeColor = myTurn ? Color.DarkGreen : Color.DarkOrange;
        }

        private void RefreshPanels()
        {
            if (this.Tag is Control[] panels)
                foreach (var p in panels)
                    p.Invalidate();
        }
    }
}