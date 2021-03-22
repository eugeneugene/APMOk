using System;
using System.Runtime.InteropServices;

namespace APMTest
{
    public class HW
    {
        [DllImport("hw.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr EnumerateDisks(out EnumDiskInfo[] diskInfo);
    }
}
