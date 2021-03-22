using System;

namespace APMTest
{
    class Program
    {
        static void Main(string[] args)
        {
            HW.EnumerateDisks(out EnumDiskInfo[] diskInfo);
            Console.WriteLine("Hello World!");
        }
    }
}
