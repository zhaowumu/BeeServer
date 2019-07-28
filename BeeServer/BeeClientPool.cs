using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeServer
{
    /// <summary>
    /// 客户蜜蜂池,重复使用蜜蜂
    /// </summary>
    public class BeeClientPool
    {
        private Queue<BeeClient> clientBeeQueue;

        private int _workingBee;

        public BeeClientPool(int capacity)
        {
            clientBeeQueue = new Queue<BeeClient>(capacity);
            _workingBee = capacity;
        }

        public void EnQueue(BeeClient clientBee)
        {
            // 它的容量会自动扩增，不需要你去担心，也就是说Queue的容量基本是无限的。
            clientBeeQueue.Enqueue(clientBee);
            --_workingBee;
        }
        public BeeClient DeQueue()
        {
            // 队列为空情况
            if (clientBeeQueue.Count == 0)
                return null;

            ++_workingBee;
            return clientBeeQueue.Dequeue();
        }

        public int Count => _workingBee;

    }
}
