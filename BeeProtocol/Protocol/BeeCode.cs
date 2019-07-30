using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BeeGame.Protocol
{
    /// 蜜蜂语言(工具类)，进行各种消息转码的工具
    public static class BeeCode
    {
        #region 包头取包

            /// <summary>
            /// 加包头 包头+数据
            /// </summary>
            public static byte[] AddHeadLength(byte[] data)
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
            /// 尝试从收到dataCache里的，包含包头的字节数据，尝试取出一条完整的数据
            /// </summary>
            public static byte[] TryGetPacket(ref List<byte> dataCache)
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
        /// 序列化包裹
        /// </summary>
        public static byte[] EncodePacket(BeePacket packet)
        {
            using(var ms = new MemoryStream())
            {
                using(var bw = new BinaryWriter(ms))
                {
                    bw.Write(packet.OpCode);
                    bw.Write(packet.SubCode);

                    if(packet.Message != null)
                    {
                        // todo 这里object类型转换为接口
                        var valueBytes = EncodeMessage(packet.Message);
                        bw.Write(valueBytes);
                    }

                    var data = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int) ms.Length);

                    return data;
                }
            }
        }

        /// <summary>
        /// 反序列化包裹(快递)
        /// </summary>
        public static BeePacket DecodePacket(byte[] data)
        {
            using(var ms = new MemoryStream(data))
            {
                using(var br = new BinaryReader(ms))
                {
                    var msg = new BeePacket();
                    msg.OpCode  = br.ReadInt32();
                    msg.SubCode = br.ReadInt32();

                    //FIXME 有必要判断？类型待商榷 

                    var count = (int) (ms.Length - ms.Position);

                    if(count > 0)
                    {
                        var valueBytes = br.ReadBytes(count);
                        msg.Message = DecodeMessage(valueBytes);
                    }
                    else
                    {
                        msg.Message = null;
                    }

                    return msg;
                }
            }
        }

        #endregion

        #region 序列化转码



        ///TODO 把包裹内容message序列化为byte
        public static byte[] EncodeMessage(BeeMessage message)
        {
            using(var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();

                // todo 此处可能有错
                bf.Serialize(ms, message);
                var valueBytes = new byte[ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, valueBytes, 0, (int) ms.Length);

                return valueBytes;
            }
        }

        /// 把byte 反序列化为 真正包裹内容消息
        public static BeeMessage DecodeMessage(byte[] valueBytes)
        {
            using(var ms = new MemoryStream(valueBytes))
            {
                var bf = new BinaryFormatter();

                // todo 不知道可不可行，可以考虑用其他序列化
                return bf.Deserialize(ms) as BeeMessage;
            }
        }

    #endregion
    }
}