using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Lab5_As_Copy
{
    class ParamObj
    {
        public int Size;
        public long Offcet;
        public ParameterizedThreadStart Task;
    }


    class Program
    {
        private static FileStream SorceStream { get; set; }
        private static FileStream DestStream { get; set; }
        private static FileInfo SrcFileInfo { get; set; }
        private static Pool ThreadPool { get; set; }
        private static int ThreadsCount { get; set; }
        private static object ReadLoker = new object();
        private static object WriteLoker = new object();
        const int PART_SIZE = 1000;
        static void Main(string[] args)
        {
            args = new string[2];
            args[0] = "C:\\Test\\Lab5_Test_1\\music.flac";
            args[1] = "C:\\Test\\Lab5_Test_2\\music.flac";
            try
            {
                ThreadsCount = GetThreadsCount();
                if (CheckArgs(args[0], args[1]))
                {                   
                    SrcFileInfo = new FileInfo(args[0]);
                    ThreadPool = new Pool(ThreadsCount);
                    SorceStream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
                    DestStream = new FileStream(args[1], FileMode.Create, FileAccess.Write);
                    Thread Thread = new Thread(new ThreadStart(InitCopy));
                    Thread.Start();
                    Console.ReadLine();
                    ThreadPool.AbortThreads();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
            finally
            {
               if (SorceStream != null) SorceStream.Close();
               if (DestStream != null) DestStream.Close();
            }
            
            
        }

        public static int GetThreadsCount()
        {
            Console.WriteLine("Введите количество потоков : ");
            string str = Console.ReadLine();
            int ThreadCount = Convert.ToInt32(str);
            if (ThreadCount < 0 || ThreadCount > 1000) throw new Exception("Неверное число потоков!");
            return ThreadCount;
            
        }

        public static bool CheckArgs(string Sorce, string Dest)
        {
            if (!File.Exists(Sorce)) return false;
            if (File.Exists(Dest))
            {
                Console.WriteLine("Файл с таким именеи уже существует. Заменить?  y(да)");
                string inp = Console.ReadLine();
                if (inp != "y") return false;               
            }
            return true;
        }

        public static void InitCopy()
        {
            long NotCopiedBytes = SrcFileInfo.Length;
            long Offcet = 0;
            int Size;

            while(NotCopiedBytes > 0)
            {
                if (NotCopiedBytes >= PART_SIZE) Size = PART_SIZE;             
                else Size = (int)NotCopiedBytes;
                ParamObj obj = new ParamObj();
                obj.Offcet = Offcet;
                obj.Size = Size;
                obj.Task = new ParameterizedThreadStart(Copy);
                ThreadPool.AddTask(obj);
                Offcet += Size;
                NotCopiedBytes -= Size;
            }

            ThreadPool.WaitForAllToFinish();
            Console.WriteLine("Копирование завершено");
        }

        public static void Copy(object Params)
        {
            ParamObj obj = Params as ParamObj;
            int Size = obj.Size;
            long Offcet = obj.Offcet;

            byte[] array = new byte[Size];
            lock(ReadLoker)
            {
                SorceStream.Seek(Offcet, SeekOrigin.Begin);
                SorceStream.Read(array, 0, Size);
            }
            lock(WriteLoker)
            {
                DestStream.Seek(Offcet, SeekOrigin.Begin);
                DestStream.Write(array, 0, Size);
            }
        }
    }
}
