using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class UdpPingServer
{
    static void Main(string[] args)
    {
        const int listenPort = 12000;
        UdpClient server = new UdpClient(listenPort);
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        Random random = new Random();

        Console.WriteLine("UDP Ping Server dang chay tai {0}:{1}", "127.0.0.1", listenPort);
        Console.WriteLine("Nhan goi tin ping tu client...\n");

        while (true)
        {
            try
            {
                byte[] receiveBytes = server.Receive(ref remoteEP);
                string receiveData = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine("Nhan tu {0}: {1}", remoteEP, receiveData);

                int rand = random.Next(1, 11); 
                if (rand < 4)
                {
                    Console.WriteLine(">>> Mat goi tin (khong phan hoi).");
                    continue; 
                }// giả lập mất gói
                Thread.Sleep(random.Next(10, 200));

                server.Send(receiveBytes, receiveBytes.Length, remoteEP);
                Console.WriteLine("<<< Da gui lai goi tin cho client.\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Loi: " + e.Message);
            }
        }
    }
}
