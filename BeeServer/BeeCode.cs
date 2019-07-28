using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BeeServer
{
    /// 蜜蜂语言，进行各种消息转码的工具
    public static class BeeCode
    {
    #region 包头转码

        /// <summary>
        /// 封包,把将要发送的数据编码为带 包头+数据
        /// </summary>
        public static byte[] EncodePaket(byte[] data)
        {
            using(var ms = new MemoryStream())
            {
                using(var bw = new BinaryWriter(ms))
                {
                    bw.Write(data.Length);
                    bw.Write(data);
                    var byteArray = new byte[(int) ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, byteArray, 0, (int) ms.Length);

                    return byteArray;
                }
            }
        }

        /// <summary>
        /// 解包,把收到dataCache里的，包含包头的字节数据，尝试取出一条完整的数据
        /// </summary>
        public static byte[] DecodePacket(ref List<byte> dataCache)
        {
            if(dataCache.Count < 4)
                return null;

            using(var ms = new MemoryStream(dataCache.ToArray()))
            {
                using(var br = new BinaryReader(ms))
                {
                    var length     = br.ReadInt32();
                    var dataRemain = (int) (ms.Length - ms.Position);

                    if(length > dataRemain)
                    {
                        return null;
                    }

                    var data = br.ReadBytes(length);
                    dataCache.Clear();
                    dataCache.AddRange(br.ReadBytes(dataRemain));

                    return data;
                }
            }
        }

    #endregion

    #region 操作码转码

        /// <summary>
        /// 将一个带操作码的消息体，转化为socket蜜蜂可以传输，对方接收的包
        /// </summary>
        public static byte[] EncodeMessage(BeeMessage msg)
        {
            using(var ms = new MemoryStream())
            {
                using(var bw = new BinaryWriter(ms))
                {
                    bw.Write(msg.OpCode);
                    bw.Write(msg.SubCode);

                    if(msg.Value != null)
                    {
                        var valueBytes = EncodeObj(msg.Value);
                        bw.Write(valueBytes);
                    }

                    var data = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int) ms.Length);

                    return data;
                }
            }
        }

        /// <summary>
        /// 将一个socket包，解析出带操作码的消息体
        /// </summary>
        public static BeeMessage DecodeMessage(byte[] data)
        {
            using(var ms = new MemoryStream(data))
            {
                using(var br = new BinaryReader(ms))
                {
                    var msg = new BeeMessage();
                    msg.OpCode  = br.ReadInt32();
                    msg.SubCode = br.ReadInt32();

                    //FIXME 有必要判断？类型待商榷 

                    var count = (int) (ms.Length - ms.Position);

                    if(count > 0)
                    {
                        var valueBytes = br.ReadBytes(count);
                        msg.Value = DecodeObj(valueBytes);
                    }

                    return msg;
                }
            }
        }

    #endregion

    #region 序列化转码

        //TODO 做成接口 

        public static byte[] EncodeObj(object value)
        {
            using(var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();

                bf.Serialize(ms, value);
                var valueBytes = new byte[ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, valueBytes, 0, (int) ms.Length);

                return valueBytes;
            }
        }

        public static object DecodeObj(byte[] valueBytes)
        {
            using(var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();

                return bf.Deserialize(ms);
            }
        }

    #endregion
    }
}