using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace DemoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("客户端开启");
            var clientSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 8888);
            clientSocket.Connect(iPEndPoint);
            Console.WriteLine("连接完毕");

            // 接收
            byte[] data = new byte[1024];
            int length = clientSocket.Receive(data);
            string msg = Encoding.UTF8.GetString(data,0,length);
            Console.WriteLine(msg);

            while (true)
            {
                // 发送
                string s = Console.ReadLine();
                if (s =="c")
                {
                    clientSocket.Close();
                    return;
                }

                clientSocket.Send(Encoding.UTF8.GetBytes(s));
            }

            

            clientSocket.Close();
            Console.ReadKey();

        }
    }
}
