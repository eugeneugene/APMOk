using System.Runtime.InteropServices;

namespace APMTest
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class EnumDiskInfo
    {
        public ushort DiskIndex;
        public ushort Availability;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Caption;
        public uint ConfigManagerErrorCode;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Description;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DeviceID;
        public uint Index;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string InterfaceType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Manufacturer;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Model;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string SerialNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Status;
    }
}
