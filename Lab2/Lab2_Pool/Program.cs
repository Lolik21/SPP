using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Lab2_Pool
{
    class Program
    {
        private static int OpCount;
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                if (CheackArgs(args))
                {
                    Pool ThreadP = new Pool();
                    OpCount = 0;
                    StartCopy(args[0], args[1], ThreadP);
                    Console.WriteLine("Файлов скопированно : " + OpCount);
                }
            }
            else
            {
                Console.WriteLine("Неверное количество аргументов");
            }
            Console.ReadLine();
        }

        static bool CheackArgs(string[] args)
        {
            if (Directory.Exists(args[0]) && Directory.Exists(args[1]))
                return true;
            return false;
        }

        static void StartCopy(string Sorce, string Dest,Pool ThreadP)
        {
            
            foreach (string FileName in Directory.GetFiles(Sorce))
            {
                FileInfo Info = new FileInfo(FileName);
                ThreadP.AddTask(new ThreadStart(() => Copy(Sorce + "\\" + Info.Name, Dest + "\\" + Info.Name)));                
            }
            foreach(string DirName in Directory.GetDirectories(Sorce))
            {
                string NewSorceDir = DirName;
                DirectoryInfo DInfo = new DirectoryInfo(DirName);
                string NewDestDir = Dest +"\\"+ DInfo.Name;
                if (!Directory.Exists(NewDestDir)) Directory.CreateDirectory(NewDestDir);
                StartCopy(NewSorceDir, NewDestDir, ThreadP);
            }
        }

        static void Copy(string Sorce, string Dest)
        {
            try
            {
                File.Copy(Sorce, Dest);
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " " + Sorce + " " + Dest);
                OpCount++;
            }
            catch(Exception ex)
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId+" "+ex.Message);              
            }        
        }
    }
}
