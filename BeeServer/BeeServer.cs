using BeeGame.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeeServer
{
    /// <summary>
    /// 服务端蜜蜂
    /// </summary>
    public class BeeServer
    {
        private Socket _socket;

        /// <summary>
        /// 连接线程控制
        /// </summary>
        private Semaphore _semaphore;

        private BeeClientPool _clientBeePool;

        private IBeeApplication _application;

        private int _maxClientBee;

        /// <summary>
        /// 创建蜂王
        /// </summary>
        /// <param name="app">和蜂王绑定的Application</param>
        public BeeServer(IBeeApplication app)
        {
            this._application = app;
        }

        #region 开启

        /// <summary>
        /// 开启服务端蜜蜂
        /// </summary>
        public void Start(int port, int maxCount)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _semaphore = new Semaphore(maxCount, maxCount);

                // 初始化客户蜜蜂池
                InitClientBeePool(maxCount);

                var ipEndPoint = new IPEndPoint(IPAddress.Any, port);
                _socket.Bind(ipEndPoint);
                _socket.Listen(maxCount);


                Console.WriteLine($"服务器开启成功...当前蜜蜂池:{_clientBeePool.Count}/{_maxClientBee}");
                StartAccept(null);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw new Exception(e.Message);
            }
        }

        private void InitClientBeePool(int capacity)
        {
            _maxClientBee = capacity;
            _clientBeePool = new BeeClientPool(capacity);
            for (int i = 0; i < capacity; i++)
            {
                var clientBee = new BeeClient();
                clientBee.DecodeOver = ReceiveData;// 注册收到消息回调
                clientBee.SendOver = SendPacket;// 注册发送完包回调
                clientBee.Disconnected = CloseConnect;// 注册发送完包回调
                _clientBeePool.EnQueue(clientBee);
            }
        }

        

        #endregion

        #region 连接

        /// <summary>
        /// 开始连接
        /// </summary>
        private void StartAccept(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += AcceptCompleted;
            }

            

            var result = _socket.AcceptAsync(eventArgs);
            
            if (result)
            {
                // IO挂起状态，忙碌，忙完后执行 eventArgs.Completed 事件（先注册）
            }
            else
            {
                // IO完成，空闲，不会主动触发 eventArgs.Completed 事件，那就手动执行
                AcceptCompleted(_socket, eventArgs);
            }
        }

        /// <summary>
        /// 连接完成事件
        /// </summary>
        private void AcceptCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            
            //计数，限制连接数量
            _semaphore.WaitOne();

            // 完成连接后的一些处理
            var clientBee = _clientBeePool.DeQueue();
            clientBee.Socket = eventArgs.AcceptSocket;
            Console.WriteLine($"{clientBee.Socket.RemoteEndPoint} -连接完成！当前蜜蜂池:{_clientBeePool.Count}/{_maxClientBee}");
            clientBee.StartReceive();
            // 返回给应用层 通知连接成功
            _application.OnAccept(clientBee);


            // 尾递归，eventArgs参数复用
            eventArgs.AcceptSocket = null;
            StartAccept(eventArgs);
            
        }



        #endregion

        #region 接收消息
        /// <summary>
        /// 服务蜜蜂收到一条消息
        /// </summary>
        private void ReceiveData(BeeClient beeClient, BeePacket packet)
        {
            // undone 给应用层
            _application.OnReceive(beeClient, packet);
        }


        #endregion

        #region 发送消息
        // 回调
        private void SendPacket(BeeClient clientBee, string status)
        {

        }

        #endregion

        #region 断开

        public void CloseConnect(BeeClient beeClient, string reason)
        {
            try
            {
                if (beeClient == null)
                {
                    Console.WriteLine("clientBee NULL");
                    return;
                }

                var ip = beeClient.Socket.RemoteEndPoint;
                beeClient.Close();
                _clientBeePool.EnQueue(beeClient);
                // TODO 此处单单使用这个并不科学，没有给客户端回应，应该设置一个tmp缓存服务层
                _semaphore.Release();
                Console.WriteLine($"{ip} -断开连接！当前蜜蜂池:{_clientBeePool.Count}/{_maxClientBee}");

                // 通知应用层
                _application.OnDisconnect(beeClient,reason);

                
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
        }

        #endregion

        #region 关闭
        // TODO 关闭蜂王服务器
        #endregion
    }
}
