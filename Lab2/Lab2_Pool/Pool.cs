using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lab2_Pool
{
    class Pool
    {
        public Thread[] CurrPool { get; set; }
        public int ThreadsCount { get; set; }
        public List<ThreadStart> Query { get; set; }
        private bool IsAbort { get; set; }
        private object locker { get; set; }

        public Pool()
        {
            IsAbort = false;
            locker = new object();
            ThreadsCount = 5;
            Query = new List<ThreadStart>();
            CurrPool = new Thread[ThreadsCount];
            for (int i = 0; i < ThreadsCount; i++)
            {
                CurrPool[i] = new Thread(new ThreadStart(WaitForTask));
                CurrPool[i].Start();
            }
        }

        public void AddTask (ThreadStart start)
        {
            Thread NewTask = new Thread(start);
            Query.Add(start);
            
        }
        public void WaitForAllToFinish()
        {
            while (Query.Count > 1) Thread.Sleep(50);
        }

        public void WaitForTask()
        {
            while(!IsAbort)
            {
                ThreadStart Task = null;
                lock (locker)
                {
                    if (Query.Count > 1)
                    {
                        Task = (ThreadStart)Query[0].Clone();
                        Query.RemoveAt(0);
                    }
                }
                if (Task != null) Task();
            }
        }
    }
}
