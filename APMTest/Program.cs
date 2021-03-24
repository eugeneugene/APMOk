using System;
using System.Linq;

namespace APMTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int res = HW.EnumerateDisks(out var diskInfoEnum);

            switch (res)
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

            foreach (int i in new int[] { 0, 1, 2 })
            {
                try
                {
                    if (HW.GetAPM(i, out int apm0))
                        Console.WriteLine("Drive {0} APM: {1}", i, apm0);
                    else
                        Console.WriteLine("Drive {0} APM is not available", i);
                    HW.SetAPM(i, 1, false);
                    if (HW.GetAPM(i, out int apm1))
                        Console.WriteLine("Drive {0} APM: {1}", i, apm1);
                    else
                        Console.WriteLine("Drive {0} APM is not available", i);
                    HW.SetAPM(i, 1, true);
                    if (HW.GetAPM(i, out int apm2))
                        Console.WriteLine("Drive {0} APM: {1}", i, apm2);
                    else
                        Console.WriteLine("Drive {0} APM is not available", i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Drive {0} Exception: {1}", i, ex.Message);
                }
            }
        }
    }
}
