using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lab5_As_Copy
{
    class Pool
    {
        public Thread[] CurrPool { get; set; }
        public int ThreadsCount { get; set; }
        public List<ParamObj> Query { get; set; }
        private bool IsAbort { get; set; }
        private object locker { get; set; }

        public Pool(int ThreadsCount)
        {
            IsAbort = false;
            locker = new object();
            if (ThreadsCount <= 0) ThreadsCount = 5;
            Query = new List<ParamObj>();
            CurrPool = new Thread[ThreadsCount];
            for (int i = 0; i < ThreadsCount; i++)
            {
                CurrPool[i] = new Thread(new ThreadStart(WaitForTask));
                CurrPool[i].Start();
            }
        }

        public void AddTask(ParamObj Params)
        {
            while (Query.Count > 1000) Thread.Sleep(500);
            lock (locker)
            {
                Query.Add(Params);
            }
        }

        public void AbortThreads()
        {
            foreach(Thread thr in CurrPool)
            {
                thr.Abort();
            }
        }

        public void WaitForAllToFinish()
        {
            while (Query.Count > 0) Thread.Sleep(50);
        }

        public void WaitForTask()
        {
            ParamObj Par = new ParamObj();
            while (!IsAbort)
            {
                ParameterizedThreadStart Task = null;
                lock (locker)
                {
                    if (Query.Count >= 1)
                    {
                        Task = (ParameterizedThreadStart)Query[0].Task.Clone();
                        Par.Offcet = Query[0].Offcet;
                        Par.Size = Query[0].Size;
                        Query.RemoveAt(0);
                    }
                }
                if (Task != null) Task(Par);
            }
        }
    }
}
