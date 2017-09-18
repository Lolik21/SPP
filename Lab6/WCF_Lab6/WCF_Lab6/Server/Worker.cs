using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.IO;
using System.Runtime.Serialization.Json;
using Service_Contract;
using System.Reflection;
using StackExchange.Redis;


namespace Server
{
    class Worker
    {
        public string Name { get; set; }
        public object Loker = new object();
        public List<Query> Queryes { get; set; }
        public ConnectionMultiplexer Redis { get; set; }
        private Timer WorkerTimer;
        private TimerCallback WorkerCallback;
        public Worker(ConnectionMultiplexer Redis,object Loker)
        {
            Queryes = new List<Query>();
            this.Redis = Redis;
            this.Loker = Loker;
            WorkerCallback = new TimerCallback(DoWork);
            WorkerTimer = new Timer(WorkerCallback, null, 0, 5000);
        }

        private Type GetType(string Name)
        {
            DirectoryInfo Folder = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var Assemblyes = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".dll") || s.EndsWith(".exe"));
            foreach (string FileName in Assemblyes)
            {
                Assembly asm = Assembly.LoadFile(FileName);
                Type type = asm.GetType(Name);
                if (type != null) return type;
            }
            return null;
        }

        private object DeserializeObj(string JSON_Str, Type type)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(type);
            object Obj;
            using (MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(JSON_Str)))
            {
                Obj = jsonFormatter.ReadObject(stream);
            }

            return Obj;
        }

        private void PerformWork(string Message_Str)
        {
            try
            {
                QMessage Message = (QMessage)DeserializeObj(Message_Str, typeof(QMessage));
                Type type = GetType(Message.ClassName);
                if (type != null)
                {
                    object DesObj = DeserializeObj(Message.Obj, type);
                    if (DesObj is IBaseJob)
                    {
                        Console.WriteLine("Работник " + this.Name + " выполняет работу :");
                        IBaseJob Job = DesObj as IBaseJob;
                        Job.Perform();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DoWork(object obj)
        {
            lock (Loker)
            {             
                IDatabase db = Redis.GetDatabase();
                for (int i = 0; i<Queryes.Count; i++)
                {
                    for (int j = 0; j<Queryes[i].Priority;j++)
                    {
                        string Message = db.ListLeftPop(Queryes[i].RedisKey);
                        if (Message != null) PerformWork(Message);
                    }
                }
            }
        }
    }
}
