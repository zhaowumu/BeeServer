using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeServer
{
    public class BeeMessage
    {
        public int OpCode { get; set; }

        public int SubCode { get; set; }

        public object Value { get; set; }

        public BeeMessage()
        {

        }

        public BeeMessage(int opCode,int subCode,object value)
        {
            this.OpCode = opCode;
            this.SubCode = subCode;
            this.Value = value;
        }
    }
}
