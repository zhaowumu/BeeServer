using System;

namespace BeeGame.Protocol
{
    /// <summary>
    /// 能被发送或接收的消息内容，就像被蜜蜂运输的 快递物品
    /// </summary>
    [Serializable]
    public class BeeMessage
    {
        /// <summary>
        /// 节构说明，预留
        /// </summary>
        public string BeeName { get; set; }
    }
}
