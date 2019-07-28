using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeServer;

namespace ServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("——————游戏App——————");

            var server = new BeeServer.BeeServer(new MyGameServer());
            server.Start(8888,2);

            Console.ReadKey();
        }
    }
}
