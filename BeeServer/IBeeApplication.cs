using BeeGame.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeServer
{
    public interface IBeeApplication
    {
        void OnAccept(BeeClient beeClient);

        void OnDisconnect(BeeClient beeClient,string reason);

        void OnReceive(BeeClient beeClient, BeePacket packet);

        void SendMessage(string msg);
    }
}
