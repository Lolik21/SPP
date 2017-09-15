using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Lab_1_Reflections
{
    class Program
    {
        static void Main(string[] args)
        {
            
            if ((args.Length != 0) && File.Exists(args[0]))
            {
                if (Path.GetExtension(args[0]) == ".dll" || 
                    Path.GetExtension(args[0]) == ".exe")
                {
                    List<Conteiner> Rezult = new List<Conteiner>();
                    GetAssemble(args[0],Rezult);
                    WriteRezult(Rezult);
                }
                else
                {
                    Console.WriteLine("Неверное расширение исходного файла");
                }
            }
            else
            {
                Console.WriteLine("Файла не существует");
            }
            Console.ReadLine();
        }
        
        static void WriteRezult(List<Conteiner> Rezult)
        {
            foreach(Conteiner Cont in Rezult)
            {
                Console.WriteLine("Namespace : " + Cont.Namespace);
                Cont.TypesNames.Sort();
                foreach (string str in Cont.TypesNames)
                {
                    Console.WriteLine(str);
                }
            }
        }

        static void GetAssemble(string AssemblePath, List<Conteiner> Rezult)
        {
            Assembly assembly = Assembly.LoadFile(AssemblePath);
            foreach (Type Type in assembly.GetTypes())
            {
                if (!IsNamespasePresents(Type.Namespace,Rezult))
                {
                    Rezult.Add(new Conteiner());
                    Rezult[Rezult.Count - 1].Namespace = Type.Namespace;
                    Rezult[Rezult.Count - 1].TypesNames = new List<string>();
                }
                AddName(Type.Namespace, Type.Name,Rezult);
            }
        }

        static void AddName(string Namespace, string Name, List<Conteiner> Rezult)
        {
            for (int i = 0; i<Rezult.Count; i++)
            {
                if (Rezult[i].Namespace == Namespace)
                {
                    Rezult[i].TypesNames.Add(Name);
                }
            }
        }

        static bool IsNamespasePresents(string Namespace, List<Conteiner> Rezult)
        {
            foreach (Conteiner Cont in Rezult)
            {
                if (Cont.Namespace == Namespace) return true;
            }
            return false;
        }

        static bool IsNamePresents(string Name, List<string> Names)
        {
            foreach (string CurrName in Names)
            {
                if (CurrName == Name) return true;
            }
            return false;
        }
    }
}
