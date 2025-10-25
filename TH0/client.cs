using System;
using System.Net.Sockets;
using System.Text;

class DictionaryClient
{
    static void Main(string[] args)
    {
        string serverIp = args.Length > 0 ? args[0] : "192.168.152.1";
        int port = args.Length > 1 ? int.Parse(args[1]) : 9000;

        Console.WriteLine($"[Client] Ket noi đen {serverIp}:{port}...");

        try
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(serverIp, port);
                Console.WriteLine("[Client] Ket noi thanh cong!");
                Console.WriteLine("Nhap tu tieng Anh de tra. Go 'exit' de thoat.\n");

                NetworkStream stream = client.GetStream();

                while (true)
                {
                    Console.Write("Nhap tu: ");
                    string word = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(word))
                        continue;

                    byte[] data = Encoding.UTF8.GetBytes(word + "\n");
                    stream.Write(data, 0, data.Length);

                    if (word.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] goodbye = new byte[1024];
                        int n = stream.Read(goodbye, 0, goodbye.Length);
                        string msg = Encoding.UTF8.GetString(goodbye, 0, n);
                        Console.WriteLine("[Server] " + msg);
                        break;
                    }

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("[Server] " + response.Trim());
                }

                stream.Close();
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("[Client] Khong the ket noi đen server. Kiem tra IP, port hoac firewall.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Loi: {ex.Message}");
        }

        Console.WriteLine("[Client] Thoat chuong trinh.");
    }
}
