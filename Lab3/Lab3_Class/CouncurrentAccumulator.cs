using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lab3_Class
{
    class CouncurrentAccumulator
    {
        private object Locker { get; set; }
        public delegate void  FlushCallback(List<object> items);
        private FlushCallback CounDelegate; 
        private List<object> Buffer { get; set; }
        private int MaxObjCount { get; set; }
        private IAsyncResult Rez { get; set; }
        private delegate List<object> FlushHandle();
        private FlushHandle Fl;
        private Timer Timer { get; set; }


        public delegate int BinaryOp(int data, int time);

        public CouncurrentAccumulator(int MaxObjects, TimeSpan Time , FlushCallback Deleg)
        {
            Locker = new object();
            Buffer = new List<object>();
            Fl = Flush;
            TimerCallback tm = new TimerCallback(TimerStr);
            Timer = new Timer(tm, null, Time.Milliseconds, Time.Milliseconds);
            CounDelegate = Deleg;
        }

        public void Add(object item)
        {
            lock (Locker)
            {
                if (Rez != null)
                {
                    while (!Rez.IsCompleted) Thread.Sleep(50);
                    Buffer = Fl.EndInvoke(Rez);
                }
                
                if (Buffer.Count < MaxObjCount)
                {
                    Buffer.Add(item);
                    if (Buffer.Count == MaxObjCount)
                    {
                        Rez = Fl.BeginInvoke(null,null);                  
                    }
                }              
            }
            
        }

        static int DelegateThread(int data, int time)
        {
            Console.WriteLine("DelegateThread запущен");
            // Делаем задержку, для эмуляции длительной операции
            Thread.Sleep(time);
            Console.WriteLine("DelegateThread завершен");
            return ++data;
        }

        public void TimerStr(object obj)
        {
            lock (Locker)
            {
                Rez = Fl.BeginInvoke(null, null);
            }
        }

        private List<object> Flush()
        {            
            CounDelegate(Buffer);
            return new List<object>();           
        }
    }
}
