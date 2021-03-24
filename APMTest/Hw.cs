using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace APMTest
{
    public class HW
    {
        [DllImport("hw.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr EnumerateDisks([In, Out] IntPtr diskInfo);

        public static int EnumerateDisks(out IEnumerable<EnumDiskInfo> diskInfo)
        {
            int EnumDiskInfoSize = Marshal.SizeOf(new EnumDiskInfo());
            IntPtr ptr = Marshal.AllocHGlobal(EnumDiskInfoSize * 16);
            try
            {
                var res = EnumerateDisks(ptr).ToInt32();
                if (res > 0)
                {
                    var result = new EnumDiskInfo[16];
                    for (int i = 0; i < 16; i++)
                        result[i] = (EnumDiskInfo)Marshal.PtrToStructure(new IntPtr(ptr.ToInt64() + (i * EnumDiskInfoSize)), typeof(EnumDiskInfo));
                    diskInfo = result;
                    return res;
                }

                diskInfo = null;
                return 0;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
