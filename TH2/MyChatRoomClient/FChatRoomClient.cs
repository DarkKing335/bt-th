using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace MyChatRoomClient
{
    public partial class FChatRoomClient : Form
    {
        Thread threadClient = null;
        Socket socketClient = null;

        public FChatRoomClient()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }

        // Nút "Kết nối"
        private void btnConnect_Click(object sender, EventArgs e)
        {
            IPAddress address = IPAddress.Parse(txtIP.Text.Trim());
            IPEndPoint endpoint = new IPEndPoint(address, int.Parse(txtPort.Text.Trim()));
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socketClient.Connect(endpoint);
                ShowMsg($"Đã kết nối đến máy chủ {socketClient.RemoteEndPoint}");
            }
            catch (SocketException ex)
            {
                ShowMsg("Lỗi khi kết nối đến máy chủ: " + ex.Message);
                return;
            }
            catch (Exception ex)
            {
                ShowMsg("Lỗi khi kết nối đến máy chủ: " + ex.Message);
                return;
            }

            // Bắt đầu luồng nhận dữ liệu từ máy chủ
            threadClient = new Thread(ReceiveMsg)
            {
                IsBackground = true
            };
            threadClient.Start();
        }

        // Nút "Gửi tin nhắn"
        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            string strMsg = txtMsgSend.Text.Trim();
            if (string.IsNullOrEmpty(strMsg))
            {
                ShowMsg("Không thể gửi tin nhắn rỗng!");
                return;
            }

            byte[] arrMsg = Encoding.UTF8.GetBytes(strMsg);
            byte[] arrMsgSend = new byte[arrMsg.Length + 1];
            arrMsgSend[0] = 0; // 0 = Tin nhắn văn bản
            Buffer.BlockCopy(arrMsg, 0, arrMsgSend, 1, arrMsg.Length);

            try
            {
                socketClient.Send(arrMsgSend);
                ShowMsg($"Tôi nói với {socketClient.RemoteEndPoint}: {strMsg}");
                txtMsgSend.Clear();
            }
            catch (SocketException ex)
            {
                ShowMsg("Lỗi khi gửi tin nhắn: " + ex.Message);
            }
            catch (Exception ex)
            {
                ShowMsg("Lỗi khi gửi tin nhắn: " + ex.Message);
            }
        }

        // Nút "Chọn tệp"
        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    ShowMsg($"Đã chọn tệp: {ofd.FileName}");
                }
            }
        }

        // Nút "Gửi tệp"
        private void btnSendFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                ShowMsg("Vui lòng chọn tệp hợp lệ để gửi!");
                return;
            }

            try
            {
                using (FileStream fs = new FileStream(txtFilePath.Text, FileMode.Open))
                {
                    byte[] arrFile = new byte[1024 * 1024 * 2]; // 2MB buffer
                    int length = fs.Read(arrFile, 0, arrFile.Length);

                    byte[] arrFileSend = new byte[length + 1];
                    arrFileSend[0] = 1; // 1 = Gửi file
                    Buffer.BlockCopy(arrFile, 0, arrFileSend, 1, length);

                    socketClient.Send(arrFileSend);
                    ShowMsg($"Đã gửi tệp: {Path.GetFileName(txtFilePath.Text)} ({length} bytes)");
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Lỗi khi gửi tệp: " + ex.Message);
            }
        }

        // Hiển thị tin nhắn ra hộp văn bản
        void ShowMsg(string msg)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            txtMsg.AppendText($"[{time}] {msg}\r\n");
        }

        // Nhận dữ liệu từ máy chủ
        void ReceiveMsg()
        {
            while (true)
            {
                byte[] arrMsgRev = new byte[1024 * 1024 * 2]; // 2MB buffer
                int length;

                try
                {
                    length = socketClient.Receive(arrMsgRev);
                    if (length == 0) break;
                }
                catch (SocketException ex)
                {
                    ShowMsg("Lỗi khi nhận tin nhắn từ máy chủ: " + ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    ShowMsg("Lỗi khi nhận tin nhắn từ máy chủ: " + ex.Message);
                    break;
                }

                string strMsgReceive = Encoding.UTF8.GetString(arrMsgRev, 0, length);
                ShowMsg($"{socketClient.RemoteEndPoint} nói với tôi: {strMsgReceive}");
            }
        }
    }
}
