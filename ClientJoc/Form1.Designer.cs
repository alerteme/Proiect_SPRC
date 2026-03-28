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
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.btnConnect  = new System.Windows.Forms.Button();
            this.lstChat     = new System.Windows.Forms.ListBox();
            this.txtMessage  = new System.Windows.Forms.TextBox();
            this.btnSend     = new System.Windows.Forms.Button();
            this.lblUser     = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // lblUser
            this.lblUser.Text     = "Username:";
            this.lblUser.Location = new System.Drawing.Point(10, 15);
            this.lblUser.Size     = new System.Drawing.Size(70, 20);

            // txtUsername
            this.txtUsername.Location = new System.Drawing.Point(85, 12);
            this.txtUsername.Size     = new System.Drawing.Size(200, 23);

            // btnConnect
            this.btnConnect.Text     = "Conectează";
            this.btnConnect.Location = new System.Drawing.Point(295, 11);
            this.btnConnect.Size     = new System.Drawing.Size(100, 25);
            this.btnConnect.Click   += new System.EventHandler(this.btnConnect_Click);

            // lstChat
            this.lstChat.Location             = new System.Drawing.Point(10, 50);
            this.lstChat.Size                 = new System.Drawing.Size(460, 290);
            this.lstChat.ScrollAlwaysVisible  = true;

            // txtMessage
            this.txtMessage.Location = new System.Drawing.Point(10, 355);
            this.txtMessage.Size     = new System.Drawing.Size(355, 23);
            this.txtMessage.Enabled  = false;
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);

            // btnSend
            this.btnSend.Text     = "Trimite";
            this.btnSend.Location = new System.Drawing.Point(370, 354);
            this.btnSend.Size     = new System.Drawing.Size(100, 25);
            this.btnSend.Enabled  = false;
            this.btnSend.Click   += new System.EventHandler(this.btnSend_Click);

            // Form1
            this.Text          = "Bătălie Navală - Lobby Chat";
            this.ClientSize    = new System.Drawing.Size(484, 391);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblUser, this.txtUsername, this.btnConnect,
                this.lstChat, this.txtMessage, this.btnSend
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Button  btnConnect;
        private System.Windows.Forms.ListBox lstChat;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button  btnSend;
        private System.Windows.Forms.Label   lblUser;
    }
}