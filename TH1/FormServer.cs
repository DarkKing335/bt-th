using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TH1
{
    public partial class FormServer : Form
    {
        private TcpListener? server;
        private Thread? listenThread;
        private bool isRunning = false;

        public FormServer()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                int port = int.Parse(txtPort.Text);
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                listenThread = new Thread(ListenForClients);
                listenThread.IsBackground = true;
                listenThread.Start();

                isRunning = true;
                Log($"Server dang chay tren port {port}");
                btnStart.Text = "Dang chay...";
                btnStart.Enabled = false;
            }
        }

        private void ListenForClients()
        {
            while (isRunning)
            {
                TcpClient client = server.AcceptTcpClient();
                Log("Client ket noi: " + client.Client.RemoteEndPoint);

                Thread clientThread = new Thread(HandleClientComm);
                clientThread.IsBackground = true;
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string noAccent = RemoveDiacritics(message);
                    char[] arr = noAccent.ToCharArray();
                    Array.Reverse(arr);
                    string reversed = new string(arr).ToUpper();

                    Log($"Nhan tu client: {message} → Gui lai: {reversed}");
                    byte[] msg = Encoding.UTF8.GetBytes(reversed);
                    stream.Write(msg, 0, msg.Length);

                    if (message.ToLower() == "bye")
                        break;
                }
                catch { break; }
            }

            Log("Client da thoat.");
            client.Close();
        }

        private void Log(string msg)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => rtbLog.AppendText(msg + "\n")));
            }
            else
            {
                rtbLog.AppendText(msg + "\n");
            }
        }

        public static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
