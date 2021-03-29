using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APMData
{
    [DataContract]
    public class SystemDiskInfoResponse
    {
        [DataMember(Order = 1)]
        public int ResponseResult { get; set; }
        [DataMember(Order = 2)]
        public DateTime ResponseTimeStamp { get; set; }
        [DataMember(Order = 3)]
        public List<DiskInfoEntry> DiskInfoEntries { get; set; }
    }
}
