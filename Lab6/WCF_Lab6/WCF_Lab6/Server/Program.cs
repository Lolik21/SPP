using System;
using System.ServiceModel;
using Service_Contract;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "SERVER";

                ServiceHost host = new ServiceHost(typeof(Service));

                host.Open();

                Console.WriteLine("Приложение готово к прему сообщений");
                Console.ReadKey();
                host.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }
    }
}
