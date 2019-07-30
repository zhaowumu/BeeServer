
namespace BeeGame.Protocol
{
    /// <summary>
    /// 蜜蜂送的快递，其中必须有地址收件人
    /// </summary>
    public class BeePacket
    {
        public int OpCode { get; set; }

        public int SubCode { get; set; }

        public BeeMessage Message { get; set; }

        public BeePacket()
        {

        }

        public BeePacket(int opCode,int subCode, BeeMessage Message)
        {
            this.OpCode = opCode;
            this.SubCode = subCode;
            this.Message = Message;
        }
    }
}
