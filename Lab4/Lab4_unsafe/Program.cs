using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4_unsafe
{
    class Program
    {
        const int N = 100;
        static void Main(string[] args)
        {
            int[] arr = new int[N];
            for (int i = 0; i<N; i++)
            {
                arr[i] = i;
            }
            try
            {
                ArrayHelper.Sum(arr, 100);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
