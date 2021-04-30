using System.Collections.Generic;

namespace APMOkSvc.Code
{
    public static class EnumerateDisksErrors
    {
        public static Dictionary<int, string> Errors { get; } = new()
        {
            { 1, "Failed to get Disk Drive information" },
            { 2, "Failed to set proxy blanket" },
            { 3, "Failed to connect to root namespace" },
            { 4, "Failed to create IWbemLocator object" },
            { 5, "Failed to set COM Security" },
            { 6, "Failed to initialize COM" },
        };
    }
}
