namespace TH1
{
    partial class FormClient
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            label1 = new Label();
            txtIP = new TextBox();
            txtPort = new TextBox();
            rtbChat = new RichTextBox();
            label2 = new Label();
            btnSend = new Button();
            btnConnect = new Button();
            label3 = new Label();
            txtMessage = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(47, 30);
            label1.Name = "label1";
            label1.Size = new Size(24, 20);
            label1.TabIndex = 0;
            label1.Text = "IP:";
            // 
            // txtIP
            // 
            txtIP.Location = new Point(81, 27);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(125, 27);
            txtIP.TabIndex = 1;
            txtIP.Text = "10.1.234.92";
            txtIP.TextChanged += txtIP_TextChanged;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(404, 30);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(125, 27);
            txtPort.TabIndex = 2;
            txtPort.Text = "8080";
            // 
            // rtbChat
            // 
            rtbChat.Location = new Point(47, 88);
            rtbChat.Name = "rtbChat";
            rtbChat.Size = new Size(482, 222);
            rtbChat.TabIndex = 4;
            rtbChat.Text = "";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(341, 30);
            label2.Name = "label2";
            label2.Size = new Size(38, 20);
            label2.TabIndex = 5;
            label2.Text = "Port:";
            // 
            // btnSend
            // 
            btnSend.Location = new Point(379, 367);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(94, 29);
            btnSend.TabIndex = 6;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(610, 29);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(94, 29);
            btnConnect.TabIndex = 7;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(47, 372);
            label3.Name = "label3";
            label3.Size = new Size(105, 20);
            label3.TabIndex = 8;
            label3.Text = "Nhap tin nhan:";
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(189, 369);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(125, 27);
            txtMessage.TabIndex = 9;
            // 
            // FormClient
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(txtMessage);
            Controls.Add(label3);
            Controls.Add(btnConnect);
            Controls.Add(btnSend);
            Controls.Add(label2);
            Controls.Add(rtbChat);
            Controls.Add(txtPort);
            Controls.Add(txtIP);
            Controls.Add(label1);
            Name = "FormClient";
            Text = "TCP Client";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtIP;
        private TextBox txtPort;
        private RichTextBox rtbChat;
        private Label label2;
        private Button btnSend;
        private Button btnConnect;
        private Label label3;
        private TextBox txtMessage;
    }
}
