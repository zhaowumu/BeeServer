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
    public class BeeClient
    {
        public  Socket        Socket { get; set; }
        private List<byte>    _dataCache;
        private Queue<byte[]> _sendQueue;

        private SocketAsyncEventArgs _receiveArgs;
        private SocketAsyncEventArgs _sendArgs;

        private bool _isDecoding;

        private bool _isSending;

        public delegate void DecodeDataOver(BeeClient clientBee, BeePacket packet);

        /// 一条消息解析完成回调
        public DecodeDataOver DecodeOver;

        public delegate void SendPacketOver(BeeClient clientBee, string status);

        /// 包发送完成回调
        public SendPacketOver SendOver;

        public delegate void Disconnect(BeeClient clientBee, string reason);

        /// 断开连接事件
        public Disconnect Disconnected;

        public BeeClient()
        {
            _receiveArgs = new SocketAsyncEventArgs();
            _receiveArgs.SetBuffer(new byte[1024], 0, 1024);
            _receiveArgs.Completed += ReceiveCompleted;
            _receiveArgs.UserToken =  this; // hack 可以传别的吗

            _sendArgs           =  new SocketAsyncEventArgs();
            _sendArgs.Completed += SendCompleted;

            _dataCache = new List<byte>();
            _sendQueue = new Queue<byte[]>();

            _isDecoding = false;
            _isSending  = false;
        }

    #region 接收

        public void StartReceive()
        {
            try
            {
                var reslut = Socket.ReceiveAsync(_receiveArgs);

                if(!reslut)
                {
                    ReceiveCompleted(Socket, _receiveArgs);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                var byteArray = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, 0, byteArray, 0, e.BytesTransferred);

                _dataCache.AddRange(byteArray);

                if(!_isDecoding)
                    DecodingPacket();

                // 尾递归，处理一条数据后，再进行处理下一条
                StartReceive();
            }
            else
            {
                // 断开连接
                if(e.BytesTransferred == 0)
                {
                    // 主动断开
                    if(e.SocketError == SocketError.Success)
                    {
                        Console.WriteLine("主动断开");

                        // FIXME 
                        Disconnected?.Invoke(this, "主动断开");
                    }
                    else
                    {
                        // 网络异常断开
                        Disconnected?.Invoke(this, e.SocketError.ToString());
                        Console.WriteLine("网络异常断开");
                    }
                }
            }
        }

        private void DecodingPacket()
        {
            _isDecoding = true;

            var data = BeeCode.TryGetPacket(ref _dataCache);

            if(data == null)
            {
                _isDecoding = false;

                return;
            }
            
            // todo  转为类
            var packet = BeeCode.DecodePacket(data);

            DecodeOver?.Invoke(this, packet);

            // 尾递归 一直处理缓存数据
            DecodingPacket();
        }

    #endregion

    #region 发送

        /// 我要寄蜜蜂快递，地址收件人包裹都给你了，你看着办
        public void Send(int opCode, int subCode, BeeMessage message)
        {
            // todo 发送的object可以优化,每次都new 不好吧

            // bee 打个包
            var packet = new BeePacket(opCode, subCode, message);

            // 转成二进制
            var data   = BeeCode.EncodePacket(packet);

            // 称重，收费
            var bytes = BeeCode.AddHeadLength(data);

            // 加入发送队列
            _sendQueue.Enqueue(bytes);

            // 发走
            if(!_isSending)
            {
                SendPacket();
            }
        }

        private void SendPacket()
        {
            _isSending = true;

            if(_sendQueue.Count == 0)
            {
                _isSending = false;

                return;
            }

            try
            {
                // 取出一条数据
                var packet = _sendQueue.Dequeue();

                // 设置消息异步发送对象的数据缓冲区
                _sendArgs.SetBuffer(packet, 0, packet.Length);

                var result = Socket.SendAsync(_sendArgs);

                if(!result)
                {
                    SendCompleted(this, _sendArgs);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if(e.SocketError != SocketError.Success)
            {
                // 发送出错了， 通知上层
                SendOver?.Invoke(this, e.SocketError.ToString());
            }
            else
            {
                // 成功进行下一条发送
                SendPacket();
            }
        }

    #endregion

    #region 断开

        public void Close()
        {
            try
            {
                // 清空数据 UNDONE
                _dataCache.Clear();
                _isDecoding = false;
                _sendQueue.Clear();
                _isSending = false;
                Socket?.Shutdown(SocketShutdown.Both);
                Socket?.Close();
                Socket = null;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

    #endregion
    }
}