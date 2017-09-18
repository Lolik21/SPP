using System;
using System.ServiceModel;
using Service_Contract;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Collections.Generic;

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

                QClass[] arr = new QClass[50];
                List<string> strs = new List<string>();
                Random rnd = new Random();

                //for (int i = 0; i < arr.Length; i++)
                //{
                //    arr[i] = new QClass(i, RandomString(3), RandomString(3));
                //    string SerializedObj = SerializeObj(arr[i], arr[i].GetType());
                //    QMessage msg = new QMessage();
                //    msg.Obj = SerializedObj;
                //    Type t = arr[i].GetType();
                //    msg.ClassName = t.Namespace + "." + t.Name;
                //    strs.Add("Query_" + Convert.ToString(rnd.Next(3) + 1) + SerializeObj(msg, msg.GetType()));
                //    channel.AddMessage(strs[i]);
                //}
                //channel.RemoveMessage(strs[49]);

                channel.RestoreQuery();



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
