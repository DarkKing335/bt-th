using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TH1
{
    public partial class FormClient : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool connected = false;

        public FormClient()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!connected)
            {
                try
                {
                    string serverIP = txtIP.Text;
                    int port = int.Parse(txtPort.Text);

                    client = new TcpClient();
                    client.Connect(serverIP, port);
                    stream = client.GetStream();

                    connected = true;
                    Log($"Da ket noi den server {serverIP}:{port}");

                    receiveThread = new Thread(ReceiveData);
                    receiveThread.IsBackground = true;
                    receiveThread.Start();

                    btnConnect.Enabled = false;
                }
                catch (Exception ex)
                {
                    Log("Loi: " + ex.Message);
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                string msg = txtMessage.Text;
                if (string.IsNullOrEmpty(msg)) return;

                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);
                Log("Ban: " + msg);

                txtMessage.Clear();
            }
            else
            {
                Log("Chua ket noi den server!");
            }
        }

        private void ReceiveData()
        {
            byte[] buffer = new byte[1024];
            while (connected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Log("Server: " + response);
                }
                catch
                {
                    break;
                }
            }

            Log("Mat ket noi voi server.");
            connected = false;
        }

        private void Log(string msg)
        {
            if (rtbChat.InvokeRequired)
            {
                rtbChat.Invoke(new Action(() => rtbChat.AppendText(msg + "\n")));
            }
            else
            {
                rtbChat.AppendText(msg + "\n");
            }
        }

        private void txtIP_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
