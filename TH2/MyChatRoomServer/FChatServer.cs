using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net; //IP,IPAdress(port)
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace MyChatRoomServer
{
    public partial class FChatServer : Form
    {
        public FChatServer()
        {
            InitializeComponent();
            // Tắt kiểm tra thao tác đa luồng đối với TextBox
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }

        Thread threadWatch = null; // Luồng chịu trách nhiệm lắng nghe yêu cầu từ client
        Socket socketWatch = null; // Socket của server dùng để lắng nghe

        // Socket socketConnection = null; // Socket chịu trách nhiệm giao tiếp với client
        // Lưu tất cả socket mà server đang giao tiếp với các client
        Dictionary<string, Socket> dict = new Dictionary<string, Socket>();

        // Lưu tất cả các luồng Receive tương ứng với các socket giao tiếp của server
        Dictionary<string, Thread> dictThread = new Dictionary<string, Thread>();

        // Bắt đầu dịch vụ
        private void btnBeginListen_Click(object sender, EventArgs e)
        {
            // Tạo socket chịu trách nhiệm lắng nghe, sử dụng IPv4, kết nối dạng luồng, giao thức TCP
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Lấy địa chỉ IP trong ô văn bản
            IPAddress address = IPAddress.Parse(txtIP.Text.Trim());
            // Tạo đối tượng điểm cuối mạng gồm IP và cổng
            IPEndPoint endpoint = new IPEndPoint(address, int.Parse(txtPort.Text.Trim()));
            // Gán socket lắng nghe với IP và cổng duy nhất
            try
            {
                socketWatch.Bind(endpoint);
            }
            catch (SocketException ex)
            {
                ShowMsg("Xuất hiện lỗi khi gán IP: " + ex.Message);
                return;
            }
            catch (Exception ex)
            {
                ShowMsg("Xuất hiện lỗi khi gán IP: " + ex.Message);
                return;
            }

            // Thiết lập độ dài hàng đợi lắng nghe
            socketWatch.Listen(10);

            // Tạo luồng chịu trách nhiệm lắng nghe và truyền vào phương thức lắng nghe
            threadWatch = new Thread(WatchConnection);
            threadWatch.IsBackground = true; // Thiết lập là luồng nền
            threadWatch.Start(); // Bắt đầu luồng

            ShowMsg("Server khởi động lắng nghe thành công~");

        }

        // Gửi tin nhắn tới client
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lbOnline.Text))
            {
                MessageBox.Show("Vui lòng chọn bạn bè để gửi.");
            }
            else
            {
                string strMsg = txtMsgSend.Text.Trim();
                // Chuyển chuỗi cần gửi thành mảng byte UTF-8
                byte[] arrMsg = Encoding.UTF8.GetBytes(strMsg);

                // Lấy Key (IP từ xa) được chọn trong danh sách
                string strClientKey = lbOnline.Text;
                try
                {
                    // Dựa vào key tìm socket tương ứng trong từ điển, dùng phương thức Send để gửi dữ liệu
                    dict[strClientKey].Send(arrMsg);

                    ShowMsg(string.Format("Tôi nói với {0}: {1}", strClientKey, strMsg));
                    // Xóa nội dung trong ô nhập tin nhắn
                    this.txtMsgSend.Text = "";
                }
                catch (SocketException ex)
                {
                    ShowMsg("Xuất hiện lỗi khi gửi: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ShowMsg("Xuất hiện lỗi khi gửi: " + ex.Message);
                }
            }
        }

        // Gửi tin nhắn cho tất cả client
        private void btnSendToAll_Click(object sender, EventArgs e)
        {
            string strMsg = txtMsgSend.Text.Trim();
            // Chuyển chuỗi cần gửi thành mảng byte UTF-8
            byte[] arrMsg = Encoding.UTF8.GetBytes(strMsg);
            foreach (Socket s in dict.Values)
            {
                try
                {
                    s.Send(arrMsg);
                    ShowMsg("Gửi cho tất cả hoàn tất~ :)");
                }
                catch (SocketException ex)
                {
                    ShowMsg("Xuất hiện lỗi khi server gửi hàng loạt: " + ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    ShowMsg("Xuất hiện lỗi khi server gửi hàng loạt: " + ex.Message);
                    break;
                }
            }

        }


        /// <summary>
        /// Phương thức lắng nghe yêu cầu từ client
        /// </summary>
        void WatchConnection()
        {
            // Liên tục lắng nghe các yêu cầu kết nối mới từ client
            while (true)
            {
                Socket socketConnection = null;
                try
                {
                    // Bắt đầu lắng nghe, trả về socket mới chịu trách nhiệm giao tiếp với client
                    // Lưu ý: Accept sẽ chặn luồng hiện tại!
                    socketConnection = socketWatch.Accept();
                }
                catch (SocketException ex)
                {
                    ShowMsg("Xuất hiện lỗi khi server kết nối: " + ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    ShowMsg("Xuất hiện lỗi khi server kết nối: " + ex.Message);
                    break;
                }

                // Khi có socket mới kết nối đến server, thêm IP vào danh sách online như là định danh duy nhất của client
                lbOnline.Items.Add(socketConnection.RemoteEndPoint.ToString());

                // Lưu socket này vào từ điển, dùng IP:Port làm key
                dict.Add(socketConnection.RemoteEndPoint.ToString(), socketConnection);

                // Tạo luồng giao tiếp riêng cho từng socket, lắng nghe dữ liệu từ client
                Thread threadCommunicate = new Thread(ReceiveMsg);
                threadCommunicate.IsBackground = true;
                threadCommunicate.Start(socketConnection); // Truyền socket làm tham số

                dictThread.Add(socketConnection.RemoteEndPoint.ToString(), threadCommunicate);

                ShowMsg(string.Format("{0} đã trực tuyến. ", socketConnection.RemoteEndPoint.ToString()));
            }
        }

        /// <summary>
        /// Server lắng nghe dữ liệu được gửi từ client
        /// </summary>
        void ReceiveMsg(object socketClientPara)
        {
            Socket socketClient = socketClientPara as Socket;
            while (true)
            {
                // Định nghĩa bộ đệm nhận dữ liệu (kích thước 2MB)
                byte[] arrMsgRev = new byte[1024 * 1024 * 2];
                // Nhận dữ liệu vào bộ đệm và trả về độ dài thực tế
                int length = -1;
                try
                {
                    length = socketClient.Receive(arrMsgRev);
                }
                catch (SocketException ex)
                {
                    ShowMsg("Lỗi: " + ex.Message + ", RemoteEndPoint: " + socketClient.RemoteEndPoint.ToString());
                    // Xóa socket bị ngắt khỏi danh sách
                    dict.Remove(socketClient.RemoteEndPoint.ToString());
                    // Xóa luồng tương ứng
                    dictThread.Remove(socketClient.RemoteEndPoint.ToString());
                    // Xóa khỏi danh sách hiển thị
                    lbOnline.Items.Remove(socketClient.RemoteEndPoint.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    ShowMsg("Lỗi: " + ex.Message);
                    break;
                }
                if (arrMsgRev[0] == 0) // Nếu phần tử đầu tiên là 0 thì là tin nhắn văn bản
                {
                    string strMsgReceive = Encoding.UTF8.GetString(arrMsgRev, 1, length - 1);
                    ShowMsg(string.Format("{0} nói với tôi: {1}", socketClient.RemoteEndPoint.ToString(), strMsgReceive));
                }
                else if (arrMsgRev[0] == 1) // 1 nghĩa là tệp tin
                {
                    // Hộp thoại lưu tệp
                    SaveFileDialog sfd = new SaveFileDialog();
                    // ***** Quan trọng: do vấn đề an toàn luồng, phải thêm "this" mới mở được hộp thoại
                    if (sfd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        // Lấy đường dẫn nơi lưu tệp
                        string fileSavePath = sfd.FileName;
                        // Tạo luồng ghi file
                        using (FileStream fs = new FileStream(fileSavePath, FileMode.Create))
                        {
                            fs.Write(arrMsgRev, 1, length - 1);
                            ShowMsg("Lưu tệp thành công: " + fileSavePath);
                        }
                    }

                }
            }
        }

        void ShowMsg(string msg)
        {
            txtMsg.AppendText(msg + "\r\n");
        }



    }
}
