using System.Runtime.Serialization;

namespace APMData
{
    [DataContract]
    public class DiskInfoEntry
    {
        [DataMember(Order = 1)]
        public ushort DiskIndex { get; set; }
        [DataMember(Order = 2)]
        public uint Index { get; set; }
        [DataMember(Order = 3)]
        public ushort Availability { get; set; }
        [DataMember(Order = 4)]
        public string Caption { get; set; }
        [DataMember(Order = 5)]
        public uint ConfigManagerErrorCode { get; set; }
        [DataMember(Order = 6)]
        public string Description { get; set; }
        [DataMember(Order = 7)]
        public string DeviceID { get; set; }
        [DataMember(Order = 8)]
        public string InterfaceType { get; set; }
        [DataMember(Order = 9)]
        public string Manufacturer { get; set; }
        [DataMember(Order = 10)]
        public string Model { get; set; }
        [DataMember(Order = 11)]
        public string Name { get; set; }
        [DataMember(Order = 12)]
        public string SerialNumber { get; set; }
        [DataMember(Order = 13)]
        public string Status { get; set; }
    }
}
