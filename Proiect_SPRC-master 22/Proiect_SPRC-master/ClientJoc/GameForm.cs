using System.Drawing.Drawing2D;

namespace ClientJoc
{
    public class GameForm : Form
    {
        private const int GRID_SIZE = 8;
        private const int CELL_SIZE = 50;
        private const int MARGIN = 25;
        private const int TOP_OFFSET = 100;
        private const int GRID_PX = GRID_SIZE * CELL_SIZE;

        private int[,] myGrid = new int[GRID_SIZE, GRID_SIZE];
        private int[,] enemyGrid = new int[GRID_SIZE, GRID_SIZE];

        private bool myTurn = false;
        private bool gameOver = false;
        private string adversar = "";

        private Panel myPanel;
        private Panel enemyPanel;
        private Label lblStatus;
        private Label lblMyGrid;
        private Label lblEnemyGrid;
        private Panel chatPanel;
        private RichTextBox chatBox;
        private TextBox txtChat;
        private Button btnChat;
        private Label lblTurnIndicator;

        public event Action<int, int> OnAttack;
        public event Action<string> OnChatMessage;

        public GameForm(string adversarName, bool estiPrimul, int[,] grid)
        {
            adversar = adversarName;
            myTurn = estiPrimul;
            myGrid = grid ?? new int[GRID_SIZE, GRID_SIZE];
            InitUI();
            UpdateStatus();
        }

        private void InitUI()
        {
            int chatW = 200;
            int formWidth = MARGIN * 3 + GRID_PX * 2 + chatW + MARGIN;
            int formHeight = TOP_OFFSET + GRID_PX + 120;

            Text = "⚓ Bătălie Navală vs " + adversar;
            ClientSize = new Size(formWidth, formHeight);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(15, 25, 50);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // ── TITLU ────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "⚓  BĂTĂLIE NAVALĂ  ⚓",
                ForeColor = Color.FromArgb(100, 200, 255),
                BackColor = Color.Transparent,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(0, 8),
                Size = new Size(formWidth, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblTitle);

            // ── STATUS (rândul cui e) ─────────────────────────────────
            lblStatus = new Label
            {
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 50, 90),
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(MARGIN, 42),
                Size = new Size(GRID_PX * 2 + MARGIN, 32),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(lblStatus);

            // ── INDICATOR TURN (săgeată animată) ─────────────────────
            lblTurnIndicator = new Label
            {
                Text = "",
                ForeColor = Color.LimeGreen,
                BackColor = Color.Transparent,
                Font = new Font("Arial", 20, FontStyle.Bold),
                Location = new Point(MARGIN * 2 + GRID_PX - 15, 38),
                Size = new Size(40, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblTurnIndicator);

            // ── LABEL GRILA MEA ───────────────────────────────────────
            lblMyGrid = new Label
            {
                Text = "🛡  GRILA MEA",
                ForeColor = Color.FromArgb(150, 220, 255),
                BackColor = Color.Transparent,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(MARGIN, TOP_OFFSET - 22),
                Size = new Size(GRID_PX, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblMyGrid);

            // ── LABEL GRILA INAMICULUI ────────────────────────────────
            lblEnemyGrid = new Label
            {
                Text = "🎯  GRILA LUI " + adversar.ToUpper(),
                ForeColor = Color.FromArgb(255, 120, 80),
                BackColor = Color.Transparent,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(MARGIN * 2 + GRID_PX, TOP_OFFSET - 22),
                Size = new Size(GRID_PX, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblEnemyGrid);

            // ── PANEL GRILA MEA ───────────────────────────────────────
            myPanel = new Panel
            {
                Location = new Point(MARGIN, TOP_OFFSET),
                Size = new Size(GRID_PX, GRID_PX),
                BackColor = Color.Transparent
            };
            myPanel.Paint += MyPanel_Paint;
            Controls.Add(myPanel);

            // ── PANEL GRILA INAMICULUI (clickabil) ────────────────────
            enemyPanel = new Panel
            {
                Location = new Point(MARGIN * 2 + GRID_PX, TOP_OFFSET),
                Size = new Size(GRID_PX, GRID_PX),
                BackColor = Color.Transparent,
                Cursor = Cursors.Cross
            };
            enemyPanel.Paint += EnemyPanel_Paint;
            enemyPanel.MouseClick += EnemyPanel_Click;
            Controls.Add(enemyPanel);

            // ── CHAT PANEL ────────────────────────────────────────────
            int chatX = MARGIN * 3 + GRID_PX * 2;
            chatPanel = new Panel
            {
                Location = new Point(chatX, TOP_OFFSET - 22),
                Size = new Size(chatW, GRID_PX + 22),
                BackColor = Color.FromArgb(20, 35, 65),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(chatPanel);

            var lblChat = new Label
            {
                Text = "💬 CHAT",
                ForeColor = Color.FromArgb(100, 200, 255),
                BackColor = Color.Transparent,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(0, 3),
                Size = new Size(chatW, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            chatPanel.Controls.Add(lblChat);

            chatBox = new RichTextBox
            {
                Location = new Point(3, 24),
                Size = new Size(chatW - 6, GRID_PX - 30),
                ReadOnly = true,
                BackColor = Color.FromArgb(12, 20, 45),
                ForeColor = Color.LightCyan,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 8)
            };
            chatPanel.Controls.Add(chatBox);

            txtChat = new TextBox
            {
                Location = new Point(3, GRID_PX - 2),
                Size = new Size(chatW - 50, 22),
                BackColor = Color.FromArgb(30, 50, 90),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Arial", 8)
            };
            txtChat.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SendChat(); };
            chatPanel.Controls.Add(txtChat);

            btnChat = new Button
            {
                Text = "▶",
                Location = new Point(chatW - 44, GRID_PX - 3),
                Size = new Size(40, 24),
                BackColor = Color.FromArgb(0, 120, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnChat.FlatAppearance.BorderSize = 0;
            btnChat.Click += (s, e) => SendChat();
            chatPanel.Controls.Add(btnChat);

            // ── LEGENDA ───────────────────────────────────────────────
            int legendY = TOP_OFFSET + GRID_PX + 10;
            AddLegend(MARGIN, legendY);
        }

        private void AddLegend(int x, int y)
        {
            var items = new[]
            {
                (Color.FromArgb(50, 80, 140),  "Apă"),
                (Color.FromArgb(80, 90, 100),  "Navă"),
                (Color.OrangeRed,               "Lovit 🔥"),
                (Color.FromArgb(180, 230, 255), "Ratat 💨")
            };

            int cx = x;
            foreach (var (color, label) in items)
            {
                var box = new Panel
                {
                    Location = new Point(cx, y),
                    Size = new Size(18, 18),
                    BackColor = color,
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(box);

                var lbl = new Label
                {
                    Text = label,
                    ForeColor = Color.LightGray,
                    BackColor = Color.Transparent,
                    Location = new Point(cx + 20, y + 1),
                    Size = new Size(70, 16),
                    Font = new Font("Arial", 8)
                };
                Controls.Add(lbl);
                cx += 95;
            }
        }

        // ── DESENARE GRILA MEA ────────────────────────────────────────
        private void MyPanel_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics, myGrid, false);
        }

        // ── DESENARE GRILA INAMICULUI ─────────────────────────────────
        private void EnemyPanel_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics, enemyGrid, true);
            // Overlay "Nu e rândul tău"
            if (!myTurn && !gameOver)
            {
                using (var brush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                    e.Graphics.FillRectangle(brush,
                        0, 0, GRID_PX, GRID_PX);
            }
        }

        private void DrawGrid(Graphics g, int[,] grid, bool isEnemy)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            for (int r = 0; r < GRID_SIZE; r++)
                for (int c = 0; c < GRID_SIZE; c++)
                {
                    var rect = new Rectangle(
                        c * CELL_SIZE, r * CELL_SIZE,
                        CELL_SIZE, CELL_SIZE);

                    int val = grid[r, c];

                    // Culoarea celulei
                    Color fill;
                    if (isEnemy && val == 1)
                        fill = Color.FromArgb(50, 80, 140); // navă inamică ascunsă
                    else
                        switch (val)
                        {
                            case 1: fill = Color.FromArgb(80, 90, 100); break; // navă proprie
                            case 2: fill = Color.OrangeRed; break; // lovit
                            case 3: fill = Color.FromArgb(180, 230, 255); break; // ratat
                            default: fill = Color.FromArgb(50, 80, 140); break; // apă
                        }

                    // Gradient pe celulă
                    using (var brush = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(20, Color.White),
                        Color.Transparent,
                        LinearGradientMode.ForwardDiagonal))
                    {
                        g.FillRectangle(new SolidBrush(fill), rect);
                        g.FillRectangle(brush, rect);
                    }

                    // Border celulă
                    g.DrawRectangle(
                        new Pen(Color.FromArgb(30, 60, 100), 1),
                        rect);

                    // Simbol navă proprie
                    if (!isEnemy && val == 1)
                    {
                        g.DrawString("▪",
                            new Font("Arial", 14, FontStyle.Bold),
                            new SolidBrush(Color.FromArgb(180, 200, 220)),
                            rect.X + CELL_SIZE / 2 - 9,
                            rect.Y + CELL_SIZE / 2 - 11);
                    }

                    // Simbol lovit
                    if (val == 2)
                    {
                        g.DrawString("✕",
                            new Font("Arial", 14, FontStyle.Bold),
                            Brushes.White,
                            rect.X + CELL_SIZE / 2 - 8,
                            rect.Y + CELL_SIZE / 2 - 11);
                    }

                    // Simbol ratat
                    if (val == 3)
                    {
                        g.DrawString("•",
                            new Font("Arial", 18, FontStyle.Bold),
                            new SolidBrush(Color.FromArgb(100, 150, 200)),
                            rect.X + CELL_SIZE / 2 - 6,
                            rect.Y + CELL_SIZE / 2 - 13);
                    }

                    // Header coloane (A-H)
                    if (r == 0)
                        g.DrawString(((char)('A' + c)).ToString(),
                            new Font("Arial", 7, FontStyle.Bold),
                            new SolidBrush(Color.FromArgb(150, 200, 255)),
                            rect.X + CELL_SIZE / 2 - 4, rect.Y + 2);

                    // Header rânduri (1-8)
                    if (c == 0)
                        g.DrawString((r + 1).ToString(),
                            new Font("Arial", 7, FontStyle.Bold),
                            new SolidBrush(Color.FromArgb(150, 200, 255)),
                            rect.X + 2, rect.Y + CELL_SIZE / 2 - 6);
                }

            // Border exterior grilă
            g.DrawRectangle(
                new Pen(Color.FromArgb(100, 160, 255), 2),
                0, 0, GRID_PX - 1, GRID_PX - 1);
        }

        // ── CLICK PE GRILA INAMICULUI ─────────────────────────────────
        private void EnemyPanel_Click(object sender, MouseEventArgs e)
        {
            if (!myTurn || gameOver) return;

            int col = e.X / CELL_SIZE;
            int row = e.Y / CELL_SIZE;

            if (row < 0 || row >= GRID_SIZE || col < 0 || col >= GRID_SIZE) return;
            if (enemyGrid[row, col] == 2 || enemyGrid[row, col] == 3) return;

            OnAttack?.Invoke(row, col);
            myTurn = false;
            UpdateStatus();
            enemyPanel.Invalidate();
        }

        // ── UPDATE DIN EXTERIOR ───────────────────────────────────────
        public void ApplyResult(int row, int col, string outcome, bool yourTurn)
        {
            bool isHit = outcome.StartsWith("HIT") || outcome.StartsWith("SINK");
            enemyGrid[row, col] = isHit ? 2 : 3; // ← grila inamicului TĂU
            myTurn = yourTurn;
            UpdateStatus();
            RefreshPanels();
            string symbol = isHit ? "🔥" : "💨";
            AddChat(symbol + " Tu ai atacat ("
                + (char)('A' + col) + (row + 1) + ") → " + outcome);
        }

        public void ApplyEnemyAttack(int row, int col, string outcome, bool yourTurn)
        {
            bool isHit = outcome.StartsWith("HIT") || outcome.StartsWith("SINK");
            myGrid[row, col] = isHit ? 2 : 3; // ← grila TA
            myTurn = yourTurn;
            UpdateStatus();
            RefreshPanels();
            string symbol = isHit ? "💥" : "💨";
            AddChat(symbol + " " + adversar + " a atacat ("
                + (char)('A' + col) + (row + 1) + ") → " + outcome);
        }
        public void SetGameOver(string winnerName, bool iWon)
        {
            if (gameOver) return;
            gameOver = true;

            lblStatus.Text = iWon
                ? "🏆 AI CÂȘTIGAT! FELICITĂRI!"
                : "😞 " + winnerName + " a câștigat!";
            lblStatus.BackColor = iWon
                ? Color.FromArgb(0, 100, 0)
                : Color.FromArgb(120, 0, 0);
            lblStatus.ForeColor = Color.White;
            lblTurnIndicator.Text = iWon ? "🏆" : "💀";

            AddChat("════════════════");
            AddChat(iWon
                ? "🏆 AI CÂȘTIGAT!"
                : "💀 Ai pierdut! " + winnerName + " câștigă!");
            AddChat("════════════════");

            myTurn = false;
            enemyPanel.Cursor = Cursors.Default;
            RefreshPanels();
        }
        public void AddChat(string msg)
        {
            if (chatBox.InvokeRequired)
                Invoke((MethodInvoker)(() =>
                {
                    chatBox.AppendText(msg + "\n");
                    chatBox.ScrollToCaret();
                }));
            else
            {
                chatBox.AppendText(msg + "\n");
                chatBox.ScrollToCaret();
            }
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

            if (myTurn)
            {
                lblStatus.Text = "🎯  RÂNDUL TĂU — Click pe grila inamicului!";
                lblStatus.BackColor = Color.FromArgb(0, 80, 30);
                lblStatus.ForeColor = Color.LimeGreen;
                lblTurnIndicator.Text = "▶";
                enemyPanel.Cursor = Cursors.Cross;
            }
            else
            {
                lblStatus.Text = "⏳  Aștepți...  " + adversar + " atacă";
                lblStatus.BackColor = Color.FromArgb(80, 50, 0);
                lblStatus.ForeColor = Color.Orange;
                lblTurnIndicator.Text = "⏳";
                enemyPanel.Cursor = Cursors.No;
            }
        }

        private void RefreshPanels()
        {
            myPanel?.Invalidate();
            enemyPanel?.Invalidate();
        }
    }
}