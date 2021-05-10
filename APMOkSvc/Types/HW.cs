﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace APMOkSvc.Types
{
    public static class HW
    {
        public static int LastWin32Error { get; private set; } = 0;

        [DllImport("hw.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "EnumerateDisks")]
        private static extern int EnumerateDisksPriv([In, Out] IntPtr diskInfo);

        [DllImport("hw.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetAPM")]
        private static extern ushort GetAPMPriv([In] string dskName);

        [DllImport("hw.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "SetAPM")]
        private static extern int SetAPMPriv([In] string dskName, [In] byte val, [In] bool disable);

        public static int EnumerateDisks(out IEnumerable<HWDiskInfo> diskInfo)
        {
            int EnumDiskInfoSize = Marshal.SizeOf(new HWDiskInfo());
            IntPtr ptr = Marshal.AllocHGlobal(EnumDiskInfoSize * 16);
            try
            {
                var res = EnumerateDisksPriv(ptr);
                if (res == 0)
                {
                    var result = new HWDiskInfo[16];
                    for (int i = 0; i < 16; i++)
                        result[i] = (HWDiskInfo)Marshal.PtrToStructure(new IntPtr(ptr.ToInt64() + (i * EnumDiskInfoSize)), typeof(HWDiskInfo));
                    diskInfo = result;
                    LastWin32Error = 0;
                }
                else
                {
                    diskInfo = null;
                    LastWin32Error = Marshal.GetLastWin32Error();
                }
                return res;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static bool GetAPM(int Index, out uint apm)
        {
            string dskName = $"\\\\.\\PhysicalDrive{Index}";
            var result = GetAPM(dskName, out apm);
            return result;
        }

        public static bool GetAPM(string dskName, out uint apm)
        {
            var result = GetAPMPriv(dskName);
            if (result == ushort.MaxValue || result == ushort.MaxValue - 1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            if (result == ushort.MaxValue - 2)
            {
                apm = 0;
                return false;
            }
            apm = result;
            return true;
        }

        public static bool SetAPM(int Index, byte val, bool disable)
        {
            string dskName = $"\\\\.\\PhysicalDrive{Index}";
            var result = SetAPM(dskName, val, disable);
            return result;
        }

        public static bool SetAPM(string dskName, byte val, bool disable)
        {
            var result = SetAPMPriv(dskName, val, disable);
            if (result == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            if (result == 0)
            {
                var win32error = Marshal.GetLastWin32Error();
                if (win32error != 0)
                    throw new Win32Exception(win32error);
                return false;
            }
            return true;
        }
    }
}
