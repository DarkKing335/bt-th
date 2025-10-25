using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading;

class UdpPingClient
{
    static void Main(string[] args)
    {
        Console.Write("Nhap IP cua server: ");
        string serverIP = Console.ReadLine()?.Trim();
        Console.Write("Nhap port cua server: ");
        int serverPort = int.Parse(Console.ReadLine() ?? "12000");

        UdpClient client = new UdpClient();
        client.Client.ReceiveTimeout = 10000; 

        IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

        int packetsSent = 0;
        int packetsReceived = 0;
        double totalRTT = 0;
        double minRTT = double.MaxValue;
        double maxRTT = double.MinValue;

        Console.WriteLine($"\n=== Bat dau UDP Ping den {serverIP}:{serverPort} ===\n");

        for (int seq = 1; seq <= 10; seq++)
        {
            string message = $"PING {seq} {DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
            byte[] sendBytes = Encoding.ASCII.GetBytes(message);
            packetsSent++;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Gửi gói tin đến server
                client.Send(sendBytes, sendBytes.Length, serverEP);

                // Chờ phản hồi
                byte[] receiveBytes = client.Receive(ref serverEP);
                stopwatch.Stop();

                string receiveData = Encoding.ASCII.GetString(receiveBytes);
                double rtt = stopwatch.Elapsed.TotalMilliseconds;

                packetsReceived++;
                totalRTT += rtt;
                if (rtt < minRTT) minRTT = rtt;
                if (rtt > maxRTT) maxRTT = rtt;

                Console.WriteLine($"Reply from {serverIP}: PING Seq={seq}, RTT={rtt:F2} ms");
            }
            catch (SocketException)
            {
                // Timeout
                stopwatch.Stop();
                Console.WriteLine($"Request timed out for PING Seq={seq}");
            }

            Thread.Sleep(10000);
        }

        Console.WriteLine("\n=== Thong ke tong ket ===");
        Console.WriteLine($"Tong so goi tin gui: {packetsSent}");
        Console.WriteLine($"Tong so goi tin nhan: {packetsReceived}");
        double lossRate = ((packetsSent - packetsReceived) / (double)packetsSent) * 100;
        Console.WriteLine($"Ti le mat goi: {lossRate:F1}%");

        if (packetsReceived > 0)
        {
            double avgRTT = totalRTT / packetsReceived;
            Console.WriteLine($"RTT (min/avg/max): {minRTT:F2}/{avgRTT:F2}/{maxRTT:F2} ms");
        }
        else
        {
            Console.WriteLine("Khong co goi tin nao duoc nhan thanh cong.");
        }

        client.Close();
        Console.WriteLine("\nHoan tat chuong trinh UDP Ping Client.");
    }
}
