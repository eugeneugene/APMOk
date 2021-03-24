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

            try
            {
                int apm0 = HW.GetAPM(0);
                Console.WriteLine("Drive 0 APM: {0}", apm0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }
            try
            {
                int apm1 = HW.GetAPM(1);
                Console.WriteLine("Drive 1 APM: {0}", apm1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }
            try
            {
                int apm2 = HW.GetAPM(2);
                Console.WriteLine("Drive 2 APM: {0}", apm2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }
        }
    }
}
