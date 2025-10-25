using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class DictionaryServer
{
    private static readonly Dictionary<string, string> DICTIONARY = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"hello", "xin chao"},
        {"world", "the gioi"},
        {"love", "tinh yeu"},
        {"socket", "o cam"},
        {"computer", "may tinh"},
        {"network", "mang may tinh"},
        {"teacher", "giao vien"},
        {"student", "hoc sinh, sinh vien"},
        {"server", "may chu"},
        {"client", "may khach"}
    };

    static void Main()
    {
        int port = 9000;
        IPAddress ip = IPAddress.Any; // Lắng nghe trên tất cả các IP của máy

        TcpListener listener = null;

        try
        {
            listener = new TcpListener(ip, port);
            listener.Start();
            Console.WriteLine($"[Server] Dang chay tren {GetLocalIPAddress()}:{port}");
            Console.WriteLine("[Server] Dang cho client ket noi...\n");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine($"[Server] Client moi ket noi tu {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Loi: {ex.Message}");
        }
        finally
        {
            listener?.Stop();
        }
    }

    private static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                string received = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"[Server] Nhan tu '{clientIP}': {received}");

                if (received.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] bye = Encoding.UTF8.GetBytes("Goodbye\n");
                    stream.Write(bye, 0, bye.Length);
                    Console.WriteLine($"[Server] Client {clientIP} ngat ket noi.\n");
                    break;
                }

                string meaning;
                if (DICTIONARY.TryGetValue(received, out meaning))
                    meaning = $"→ {meaning}";
                else
                    meaning = "Khong tim thay tu nay trong tu dien.";

                byte[] data = Encoding.UTF8.GetBytes(meaning + "\n");
                stream.Write(data, 0, data.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Loi khi xu ly client {clientIP}: {ex.Message}");
        }
        finally
        {
            stream.Close();
            client.Close();
        }
    }

    // Hàm lấy IP nội bộ của máy server (để client dùng kết nối)
    private static string GetLocalIPAddress()
    {
        string localIP = "10.1.234.92";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                localIP = ip.ToString();
        }
        return localIP;
    }
}
