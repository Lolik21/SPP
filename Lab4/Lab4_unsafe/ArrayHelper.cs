using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4_unsafe
{
    static class ArrayHelper
    {
        static public unsafe void Sum(int[] Arr,int LastInd)
        {
            try
            {
                int MySum = 0;
                for (int i = 0; i < LastInd;i++)
                {
                    MySum = MySum + Arr[i];
                }
                Console.WriteLine("Сумма : " + MySum);
            }catch(Exception ex)
            {
                throw new OverflowException("Overflow", ex);
            }
        }
    }
}
