using BeeGame.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeGame.Net.Message
{
    [Serializable]
    class LoginQ : BeeMessage
    {
        public string Name { get; set; }

        public string Password { get; set; }
    }
}
