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

        private TimerCallback WorkerCallback,DumpCallback;
        private Timer WorkerTimer, DumpTimer;      
        private object Loker = new object();
        
        private List<string> Query { get; set; }
        private string FileForDump { get; set; }


        public Service()
        {
            Query = new List<string>();
            InitConnectToDB();
            int Time = Convert.ToInt32(ConfigurationManager.AppSettings["Dump"]);
            WorkerCallback = new TimerCallback(DoWork);
            WorkerTimer = new Timer(WorkerCallback, null, 0, 5000);
            DumpCallback = new TimerCallback(DumpQuery);
            DumpTimer = new Timer(DumpCallback, null, Time, Time);
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
                "(id INTEGER PRIMARY KEY, {1} TEXT)",DB_TABLE_NAME, DB_FIELD_NAME));
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

        public void AddMessage(string Obj)
        {
            lock(Loker)
            {
                Query.Add(Obj);
                Console.WriteLine("Пришло:"+Obj);
            }
        }

        public bool RemoveMessage(string Obj)
        {
            lock (Loker)
            {
                if (Query.Remove(Obj))
                {
                    Console.WriteLine("Удалено: " + Obj);
                    return true;
                }
                    
                else return false;
            }
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

        public void DoWork(object obj)
        {
            lock (Loker)
            {
                if (Query.Count != 0)
                {
                    try
                    {
                        QMessage Message = (QMessage)DeserializeObj(Query[0],typeof(QMessage));
                        Type type = GetType(Message.ClassName);
                        if (type != null)
                        {
                            object DesObj = DeserializeObj(Message.Obj, type);
                            if (DesObj is IBaseJob)
                            {
                                IBaseJob Job = DesObj as IBaseJob;
                                Job.Perform();
                            }
                        }                      
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        Query.RemoveAt(0);
                    }
                    
                }
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
                        
                    foreach (string ServiceMessage in Query)
                    {
                        command.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ('{2}')",
                        DB_TABLE_NAME, DB_FIELD_NAME, ServiceMessage);
                        command.ExecuteNonQuery();
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
            foreach (DbDataRecord record in reader)
            {
                string CurrStr = record[DB_FIELD_NAME].ToString();
                if (Query.Find(x => x == CurrStr) == null)
                {
                    Query.Add(CurrStr);
                }
            }
            
        }

        public void Restore()
        {
            using (var connection = new SQLiteConnection(
                   string.Format("Data Source={0}", FileForDump)))
            {
                using (var command = new SQLiteCommand(string.Format("SELECT {0} FROM {1} "
                    , DB_FIELD_NAME, DB_TABLE_NAME), connection))
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
