﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeServer;
using BeeGame.Protocol;

namespace ServerApp
{
    public class MyGameServer : IBeeApplication
    {
        /// 连接事件
        public void OnAccept(BeeClient beeClient)
        {
            Console.WriteLine("应用层---OnAccepte");
            beeClient.Send(1,1,new BeeMessage() { BeeName = "bee" });
        }
        /// 断开事件
        public void OnDisconnect(BeeClient beeClient,string reason)
        {
            Console.WriteLine("应用层---OnDisconnect");
        }
        /// 接收事件
        public void OnReceive(BeeClient beeClient, BeePacket packet)
        {
            
        }
    }
}
