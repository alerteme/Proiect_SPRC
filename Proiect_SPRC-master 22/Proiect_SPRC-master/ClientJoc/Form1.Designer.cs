
namespace ClientJoc
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtUsername = new TextBox();
            btnConnect = new Button();
            lstChat = new ListBox();
            txtMessage = new TextBox();
            btnSend = new Button();
            lblUser = new Label();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(85, 12);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(200, 27);
            txtUsername.TabIndex = 1;
            this.txtUsername.TextChanged += new System.EventHandler(this.txtUsername_TextChanged);
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(295, 11);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(100, 25);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "Conectează";
            btnConnect.Click += btnConnect_Click;
            // 
            // lstChat
            // 
            lstChat.Location = new Point(10, 50);
            lstChat.Name = "lstChat";
            lstChat.ScrollAlwaysVisible = true;
            lstChat.Size = new Size(460, 284);
            lstChat.TabIndex = 3;
            // 
            // txtMessage
            // 
            txtMessage.Enabled = false;
            txtMessage.Location = new Point(10, 355);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(355, 27);
            txtMessage.TabIndex = 4;
            txtMessage.KeyDown += txtMessage_KeyDown;
            // 
            // btnSend
            // 
            btnSend.Enabled = false;
            btnSend.Location = new Point(370, 354);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(100, 25);
            btnSend.TabIndex = 5;
            btnSend.Text = "Trimite";
            btnSend.Click += btnSend_Click;
            // 
            // lblUser
            // 
            lblUser.Location = new Point(10, 15);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(70, 20);
            lblUser.TabIndex = 0;
            lblUser.Text = "Username:";
            // 
            // Form1
            // 
            ClientSize = new Size(484, 391);
            Controls.Add(lblUser);
            Controls.Add(txtUsername);
            Controls.Add(btnConnect);
            Controls.Add(lstChat);
            Controls.Add(txtMessage);
            Controls.Add(btnSend);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bătălie Navală - Lobby Chat";
            ResumeLayout(false);
            PerformLayout();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            // nefolosit
        }

        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Button  btnConnect;
        private System.Windows.Forms.ListBox lstChat;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button  btnSend;
        private System.Windows.Forms.Label   lblUser;
    }
}