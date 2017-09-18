using System;
using System.ServiceModel;
using Service_Contract;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Data.SQLite;
using System.Data.Common;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Linq;
using StackExchange.Redis;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service : IContract
    {       
        private const string DB_NAME = "QueryDump.db";
        private const string DB_TABLE_NAME = "dump";
        private const string DB_FIELD_NAME = "obj";
        private const string DB_FIELD_QNAME = "qname";
        private const string Q_START_NAMES = "Query";
        private const string W_START_NAMES = "Worker";

        private TimerCallback DumpCallback;
        private Timer  DumpTimer;      
        private object Loker = new object();
        
        private List<string> Query { get; set; }
        private string FileForDump { get; set; }
        private List<Query> Querys { get; set; }
        private List<Worker> Workers { get; set; }
        private ConnectionMultiplexer Redis { get; set; }

        public Service()
        {
            Query = new List<string>();
            InitConnectToDB();

            Querys = new List<Query>();
            Workers = new List<Worker>();

            Redis = ConnectionMultiplexer.Connect("localhost");

            string[] keys = ConfigurationManager.AppSettings.AllKeys;

            SetQuerys(keys);
            SetWorkers(keys); 

            int Time = Convert.ToInt32(ConfigurationManager.AppSettings["Dump"]);
            
            DumpCallback = new TimerCallback(DumpQuery);
            DumpTimer = new Timer(DumpCallback, null, Time, Time);
        }

        void SetWorkerQuerys(string Q_Names, Worker NewWorker)
        {
            for (int j = 0; j < Querys.Count; j++)
            {
                if (Q_Names.IndexOf(Querys[j].RedisKey) != -1)
                {
                    NewWorker.Queryes.Add(Querys[j]);
                }
            }
        }

        void SetWorkers(string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].IndexOf(W_START_NAMES) != -1)
                {
                    string Q_Names = ConfigurationManager.AppSettings[keys[i]];
                    Worker NewWorker = new Worker(Redis,this.Loker);
                    NewWorker.Name = keys[i];
                    SetWorkerQuerys(Q_Names, NewWorker);
                    Workers.Add(NewWorker);
                }
            }
        }

        void SetQuerys(string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].IndexOf(Q_START_NAMES) != -1)
                {
                    int Priority = Convert.ToInt32(ConfigurationManager.AppSettings[keys[i]]);
                    Query NewQuery = new Query(keys[i], Priority);
                    Querys.Add(NewQuery);
                }
            }
        }

        private void InitConnectToDB()
        {
            lock (Loker)
            {
                FileForDump = GetDumpFile();
                if (FileForDump != null)
                {
                   ConnectToDB();
                }
            }
          
        }

        private void ConnectToDB()
        {
            using (var connection = new SQLiteConnection(
                   string.Format("Data Source={0}", FileForDump)))
            {
                using (var command = new SQLiteCommand(string.Format("SELECT " +
                            "name FROM sqlite_master " +
                         "WHERE type='table' AND name='{0}'", DB_TABLE_NAME), connection))
                {
                    command.Connection.Open();
                    SQLiteDataReader reader = command.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        CreateDumpTable(connection);
                        Console.WriteLine("Создана новая таблица dump");
                    }
                    else
                    {
                        Console.WriteLine("Таблица уже существует");
                    }
                    reader.Close();
                }
            }
            Console.WriteLine("Сервис подключился к БД");
        }

        private void CreateDumpTable(SQLiteConnection connection)
        {
            SQLiteCommand command = new SQLiteCommand(string.Format("CREATE TABLE `{0}` " +
                "(id INTEGER PRIMARY KEY, {1} TEXT, {2} TEXT)",DB_TABLE_NAME, 
                DB_FIELD_NAME, DB_FIELD_QNAME));
            command.Connection = connection;
            command.ExecuteNonQuery();
            Console.WriteLine("Таблица в БД успешно создана");
        }

        private string GetDumpFile()
        {
            if (!File.Exists(DB_NAME))
            {
                string DbName = CreateDBFile(DB_NAME);
                Console.WriteLine("БД создана");
                return DbName;
            }
            else
            {
                return DB_NAME;
            }
        }

        private string CreateDBFile(string Name)
        {
            SQLiteConnection.CreateFile(Name);
            if (File.Exists(Name))
            {
                Console.WriteLine("База для дампа создана");
                return Name;
            }
            else
            {
                Console.WriteLine("Ошибка в создании базы для дампа");
                return null;
            }
        }

        private bool CheckQuerys(string QName)
        {
            foreach(Query q in Querys)
            {
                if (q.RedisKey == QName) return true;
            }
            return false;
        }

        private string GetQName(string Obj)
        {
            for(int i = 0; i < Querys.Count; i++)
            {
                if (Obj.IndexOf(Querys[i].RedisKey) != -1)
                {
                    return Querys[i].RedisKey;
                }
            }
            return null;
        }

        private string GetQMessage(string Obj)
        {
            for (int i = 0; i < Querys.Count; i++)
            {
                if (Obj.IndexOf(Querys[i].RedisKey) != -1)
                {
                    return Obj.Replace(Querys[i].RedisKey, "");
                }
            }
            return null;
        }

        public void AddMessage(string Obj)
        {
            lock(Loker)
            {
                string QName = GetQName(Obj);
                string QMessage = GetQMessage(Obj);
                if (QName == null) return;
                if (QMessage == null) return;
                if (CheckQuerys(QName))
                {
                    IDatabase db = Redis.GetDatabase();
                    db.ListRightPush(QName, QMessage);
                    Console.WriteLine("Пришло:" + Obj);
                }
                
            }
        }

        public bool RemoveMessage(string Obj)
        {
            lock (Loker)
            {
                string QName = GetQName(Obj);
                string QMessage = GetQMessage(Obj);
                if (QName == null) return false;
                if (QMessage == null) return false;
                IDatabase db = Redis.GetDatabase();
                long ret = db.ListRemove(QName, QMessage);
                if (ret == 0) return false; 
                Console.WriteLine("Удалено: " + Obj);
                return true;
            }
        }

        public void ClearDumps()
        {
            using (var connection = new SQLiteConnection(
                    string.Format("Data Source={0}", FileForDump)))
            {
                using (var command = new SQLiteCommand(string.Format("DELETE FROM {0} WHERE id>0",
                    DB_TABLE_NAME), connection))
                {
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Dump()
        {
            using (var connection = new SQLiteConnection(
                   string.Format("Data Source={0}", FileForDump)))
            {
                using (var command = new SQLiteCommand(connection))
                {
                    command.Connection.Open();
                    IDatabase db = Redis.GetDatabase();
                    int K = 0;
                    foreach(Query query in Querys)
                    {
                        string Message = null;
                        while ((Message = db.ListGetByIndex(query.RedisKey, K)) != null)
                        {
                            command.CommandText = string.Format("INSERT INTO {0} ({1},{2}) VALUES ('{3}','{4}')",
                                                    DB_TABLE_NAME, DB_FIELD_NAME, DB_FIELD_QNAME, Message, query.RedisKey);
                            command.ExecuteNonQuery();
                            K++;
                        }
                        
                    }
                }
                
            }
        }

        public void DumpQuery(object obj)
        {
            lock (Loker)
            {
                ClearDumps();
                Dump();
                Console.WriteLine("Дамп завершился успешно");
            }

        }

        private void RestoreMessages(SQLiteDataReader reader)
        {
            IDatabase db = Redis.GetDatabase();
            foreach (DbDataRecord record in reader)
            {
                string QMessage = record[DB_FIELD_NAME].ToString();
                string QKey = record[DB_FIELD_QNAME].ToString();
                RedisValue[] strs = db.ListRange(QKey);

                bool IsExists = false;

                foreach(string str in strs)
                {
                    if (QMessage == str) IsExists = true;
                }

                if (!IsExists)
                {
                    db.ListRightPush(QKey, QMessage);
                }
            }
            
        }

        public void Restore()
        {
            using (var connection = new SQLiteConnection(
                   string.Format("Data Source={0}", FileForDump)))
            {
                using (var command = new SQLiteCommand(string.Format("SELECT {0},{1} FROM {2} "
                    , DB_FIELD_NAME, DB_FIELD_QNAME, DB_TABLE_NAME), connection))
                {
                    command.Connection.Open();
                    SQLiteDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        RestoreMessages(reader);
                    reader.Close();
                }
            }
        }

        public void RestoreQuery()
        {
            lock (Loker)
            {
                Restore();
                Console.WriteLine("Дамп восстановлен");
            }
        }
    }
}
