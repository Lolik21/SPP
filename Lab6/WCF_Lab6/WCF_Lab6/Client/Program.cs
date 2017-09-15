using System;
using System.ServiceModel;
using Service_Contract;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace Client
{
    class Program
    {
       
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "CLIENT";
                Uri address = new Uri("http://localhost:64133/IContract");
                BasicHttpBinding binding = new BasicHttpBinding();

                EndpointAddress endpoint = new EndpointAddress(address);

                ChannelFactory<IContract> factory = new ChannelFactory<IContract>(binding, endpoint);

                IContract channel = factory.CreateChannel();

                QClass[] arr = new QClass[10];

                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = new QClass(i, RandomString(3), RandomString(3));
                    string SerializedObj = SerializeObj(arr[i], arr[i].GetType());
                    QMessage msg = new QMessage();
                    msg.Obj = SerializedObj;
                    msg.ClassName = arr[i].GetType().Name;
                    msg.QueryName = "asdasd";
                    channel.AddMessage(SerializeObj(msg, msg.GetType()));
                }
                //Thread.Sleep(1000);
                //channel.DumpQuery(new object());
                //Thread.Sleep(30000);
                //channel.RestoreQuery();
                //QClass cl = new QClass(777, RandomString(3), RandomString(3));
                //string SObj = SerializeObj(cl);
                //channel.AddMessage(SObj);
                //channel.RemoveMessage(SObj);

                //channel.RestoreQuery();


            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
            }
                    
        }

        private static string RandomString(int size)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private static string SerializeObj(object obj, Type type)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(type);
                jsonFormatter.WriteObject(stream, obj);
                byte[] bytes = stream.ToArray();
                string str = Encoding.Default.GetString(bytes);
                return str;
            }
        }
    }
}
