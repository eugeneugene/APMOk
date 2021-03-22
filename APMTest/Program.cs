using System;

namespace APMTest
{
    class Program
    {
        static void Main(string[] args)
        {
            EnumDiskInfo[] diskInfo = new EnumDiskInfo[16];
            HW.EnumerateDisks(ref diskInfo);
            Console.WriteLine("Hello World!");
        }
    }
}
