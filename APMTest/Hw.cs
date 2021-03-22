using System;
using System.Runtime.InteropServices;

namespace APMTest
{
    public class HW
    {
        [DllImport("hw.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr EnumerateDisks(ref EnumDiskInfo[] diskInfo);
    }
}
