using System;
using System.Linq;

namespace APMTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int res = HW.EnumerateDisks(out var diskInfoEnum);
            
            switch(res)
            {
                case 0:
                    Console.WriteLine("No hard drives were found");
                    break;
                case -1:
                    Console.WriteLine("Error occurred");
                    break;
                default:
                    {
                        var diskInfo = diskInfoEnum.Take(res);

                        foreach (var di in diskInfo)
                        {
                            Console.WriteLine(di.Caption);
                        }
                    }
                    break;
            }            
        }
    }
}
