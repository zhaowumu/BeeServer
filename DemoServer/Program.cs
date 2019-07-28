using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DemoServer
{
    class Program
    {

        static byte[] dataBuffer = new byte[1024];
        static void Main(string[] args)
        {
            Console.WriteLine("运行DemoServer...");

            //StartServerSync(); // 同步
            //StartServerBeigin(); // 异步Beigin

            StartServerAsync();

            Console.ReadKey();
        }

        #region 异步Async

        private static Socket _serverSocket;

        private static void StartServerAsync()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 8888);
            _serverSocket.Bind(iPEndPoint);
            _serverSocket.Listen(10);

            // 开始监听连接
            StartAccept(null);
            


        }

        private static void StartAccept(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += AcceptCompleted;
            }

            bool result = _serverSocket.AcceptAsync(eventArgs);

            if (result)
            {
                Console.WriteLine("StartAccept:IO挂起状态");
            }
            else
            {
                Console.WriteLine("StartAccept:I/O 操作同步完成状态");
                DoAccept(eventArgs);
            }

            
        }

        private static void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("lianjie");
            DoAccept(e);
        }

        private static void DoAccept(SocketAsyncEventArgs eventArgs)
        {
            Console.WriteLine("这里才是处理连接事件的地方");
            Socket clientSocket = eventArgs.AcceptSocket;
            var msg = $"hello, {clientSocket.LocalEndPoint.ToString()}你好！你连接到sever了";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);

            StartAccept(null);
        }


        #endregion

        #region 异步Beigin

        private static void StartServerBeigin()
        {
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 8888);
            serverSocket.Bind(iPEndPoint);
            serverSocket.Listen(10);

            // 开始监听连接
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
            
        }

        /// <summary>
        /// 接收到连接对象处理
        /// </summary>
        /// <param name="ar"></param>
        private static void AcceptCallBack(IAsyncResult ar)
        {
            // 处理当前连接对象
            var serverSocket = ar.AsyncState as Socket;
            var clientSocket = serverSocket.EndAccept(ar);
            var msg = $"hello, {clientSocket.LocalEndPoint.ToString()}你好！你连接到sever了";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);

            // 开始接收当前连接对象的数据
            clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);

            // 上一个业务都做完了，开始监听下一个连接对象，进行尾递归
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
        }

        /// <summary>
        /// 收到数据处理
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket clientSocket = null;

            try
            {
                clientSocket = ar.AsyncState as Socket;
                int length = clientSocket.EndReceive(ar);

                if (length ==0)
                {
                    clientSocket.Close();return;
                }

                string msg = Encoding.UTF8.GetString(dataBuffer, 0, length);
                Console.WriteLine("接收到一条数据：" + msg);

                // 尾递归 准备接收下一条信息
                clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);
            }
            catch (Exception e)
            {
                if (clientSocket != null)
                {
                    clientSocket.Close();
                }
                Console.WriteLine(e.Message);
                //throw;
            }

        }
        
        #endregion


        #region 同步
        private static void StartServerSync()
        {
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 8888);
            serverSocket.Bind(iPEndPoint);
            serverSocket.Listen(10);

            var clientSocket = serverSocket.Accept();

            var msg = "hello, 你好！你连接到sever了";
            byte[] data = Encoding.UTF8.GetBytes(msg);

            clientSocket.Send(data);

            byte[] dataBuffer = new byte[1024];
            int length = clientSocket.Receive(dataBuffer);

            string msgReceive = Encoding.UTF8.GetString(dataBuffer, 0, length);


            Console.WriteLine(msgReceive);
            Console.ReadKey();
            clientSocket.Close();
            serverSocket.Close();
        }
        #endregion

    }
}
