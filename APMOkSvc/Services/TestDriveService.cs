using APMData;
using APMOkLib;

namespace APMOkSvc.Services
{
    /// <summary>
    /// Drive for testing purpose
    /// DI Lifetime: Singleton
    /// </summary>
    public class TestDriveService
    {
        public TestDriveService()
        {
            OnMainsApmValue = (uint)APMLevel.Maximum;
            OnBatteriesApmValue = (uint)APMLevel.MinimumWithStandby;
        }

        public DiskInfoEntry TestDriveDiskInfoEntry => new()
        {
            Index = 100U,
            Availability = 0,
            Caption = "APM Test Disk",
            Description = "Disk for testing",
            DeviceID = @"\\.\TESTDRIVE100",
            InterfaceType = "SCSI",
            Manufacturer = "Test Drive Company",
            Model = "Test Drive 100",
            SerialNumber = "TESTDRIVE100"
        };

        public uint OnMainsApmValue { get; set; }
        public uint OnBatteriesApmValue { get; set; }
    }
}
