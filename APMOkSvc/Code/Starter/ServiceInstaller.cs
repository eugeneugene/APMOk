using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace APMOkSvc.Code
{
    public enum ServiceInstallOptions
    {
        Automatic = ServiceBootFlag.Automatic,
        Manual = ServiceBootFlag.Manual,
        Disabled = ServiceBootFlag.Disabled,
        Default = Automatic
    }

    /// <summary>
    /// Инструменты для работы со службами Windows
    /// </summary>
    public static class ServiceInstaller
    {
        //private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        private const int SERVICE_CONFIG_DESCRIPTION = 1;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS = 2;
        private const int ERROR_ACCESS_DENIED = 5;

        #region OpenSCManager
        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenSCManager(string? machineName, string? databaseName, ScmAccessRights dwDesiredAccess);
        #endregion

        #region OpenService
        [DllImport("advapi32.dll", EntryPoint = "OpenServiceW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceAccessRights dwDesiredAccess);
        #endregion

        #region CreateService
        [DllImport("advapi32.dll", EntryPoint = "CreateServiceW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, ServiceAccessRights dwDesiredAccess,
            int dwServiceType, ServiceBootFlag dwStartType, ServiceError dwErrorControl, string lpBinaryPathName, string? lpLoadOrderGroup,
            IntPtr lpdwTagId, string? lpDependencies, string? lp, string? lpPassword);
        #endregion

        #region CloseServiceHandle
        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);
        #endregion

        #region QueryServiceStatus
        [DllImport("advapi32.dll", EntryPoint = "QueryServiceStatus", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int QueryServiceStatus(IntPtr hService, SERVICE_STATUS lpServiceStatus);
        #endregion

        #region DeleteService
        [DllImport("advapi32.dll", EntryPoint = "DeleteService", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteService(IntPtr hService);
        #endregion

        #region ControlService
        [DllImport("advapi32.dll", EntryPoint = "ControlService", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int ControlService(IntPtr hService, ServiceControl dwControl, SERVICE_STATUS lpServiceStatus);
        #endregion

        #region StartService
        [DllImport("advapi32.dll", EntryPoint = "StartServiceW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);
        #endregion

        #region ChangeServiceConfig2
        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceDescription(IntPtr hService, int dwInfoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_DESCRIPTION lpInfo);

        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceFailureActions(IntPtr hService, int dwInfoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_FAILURE_ACTIONS lpInfo);
        #endregion

        public static void Install(string serviceName, string displayName, string fileName, ServiceBootFlag bootFlag = ServiceBootFlag.Automatic)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.AllAccess);

                if (service == IntPtr.Zero)
                    service = CreateService(hSCManager: scm,
                        lpServiceName: serviceName,
                        lpDisplayName: displayName,
                        dwDesiredAccess: ServiceAccessRights.AllAccess,
                        dwServiceType: SERVICE_WIN32_OWN_PROCESS,
                        dwStartType: bootFlag,
                        dwErrorControl: ServiceError.Normal,
                        lpBinaryPathName: fileName,
                        lpLoadOrderGroup: null,
                        lpdwTagId: IntPtr.Zero,
                        lpDependencies: null,
                        lp: null,
                        lpPassword: null);

                if (service == IntPtr.Zero)
                {
                    if (Marshal.GetLastWin32Error() == ERROR_ACCESS_DENIED)
                        throw new ApplicationException("Access denied while installing service");
                    else
                        throw new ApplicationException("Failed to install service " + Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public static void Uninstall(string serviceName)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.AllAccess);
                if (service == IntPtr.Zero)
                    throw new ApplicationException("Service not installed.");

                try
                {
                    StopService(service);
                    if (!DeleteService(service))
                    {
                        if (Marshal.GetLastWin32Error() == ERROR_ACCESS_DENIED)
                            throw new ApplicationException("Access denied while uninstalling service");
                        else
                            throw new ApplicationException("Could not delete service " + Marshal.GetLastWin32Error());
                    }
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public static bool ServiceIsInstalled(string serviceName)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus);

                if (service == IntPtr.Zero)
                    return false;

                CloseServiceHandle(service);
                return true;
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public static void InstallAndStart(string serviceName, string displayName, string fileName)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.AllAccess);

                if (service == IntPtr.Zero)
                    service = CreateService(scm, serviceName, displayName, ServiceAccessRights.AllAccess, SERVICE_WIN32_OWN_PROCESS, ServiceBootFlag.Automatic, ServiceError.Normal, fileName, null, IntPtr.Zero, null, null, null);

                if (service == IntPtr.Zero)
                {
                    if (Marshal.GetLastWin32Error() == ERROR_ACCESS_DENIED)
                        throw new ApplicationException("Access denied while installing service");
                    else
                        throw new ApplicationException("Failed to install service " + Marshal.GetLastWin32Error());
                }

                try
                {
                    StartService(service);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public static void StartService(string serviceName)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);
                if (service == IntPtr.Zero)
                {
                    if (Marshal.GetLastWin32Error() == ERROR_ACCESS_DENIED)
                        throw new ApplicationException("Access denied while opening service");
                    else
                        throw new ApplicationException("Failed to open service " + Marshal.GetLastWin32Error());
                }

                try
                {
                    StartService(service);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public static void StopService(string serviceName)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Stop);
                if (service == IntPtr.Zero)
                {
                    if (Marshal.GetLastWin32Error() == ERROR_ACCESS_DENIED)
                        throw new ApplicationException("Access denied while opening service");
                    else
                        throw new ApplicationException("Failed to open service " + Marshal.GetLastWin32Error());
                }

                try
                {
                    StopService(service);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        private static void StartService(IntPtr service)
        {
            _ = StartService(service, 0, 0);
            var changedStatus = WaitForServiceStatus(service, ServiceState.StartPending, ServiceState.Running);
            if (!changedStatus)
                throw new ApplicationException("Unable to start service");
        }

        private static void StopService(IntPtr service)
        {
            SERVICE_STATUS status = new();
            _ = ControlService(service, ServiceControl.Stop, status);
            var changedStatus = WaitForServiceStatus(service, ServiceState.StopPending, ServiceState.Stopped);
            if (!changedStatus)
                throw new ApplicationException("Unable to stop service");
        }

        public static ServiceState GetServiceStatus(string serviceName)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus);
                if (service == IntPtr.Zero)
                    return ServiceState.NotFound;

                try
                {
                    return GetServiceStatus(service);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public static bool SetServiceDescription(string serviceName, string description)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

            var pinfo = new SERVICE_DESCRIPTION
            {
                lpDescription = description
            };

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.ChangeConfig);
                if (service == IntPtr.Zero)
                    return false;
                try
                {
                    return ChangeServiceDescription(service, SERVICE_CONFIG_DESCRIPTION, ref pinfo);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        private static ServiceState GetServiceStatus(IntPtr service)
        {
            SERVICE_STATUS status = new();

            if (QueryServiceStatus(service, status) == 0)
                throw new ApplicationException("Failed to query service status.");

            return status.dwCurrentState;
        }

        private static bool WaitForServiceStatus(IntPtr service, ServiceState waitStatus, ServiceState desiredStatus)
        {
            SERVICE_STATUS status = new();

            _ = QueryServiceStatus(service, status);
            if (status.dwCurrentState == desiredStatus)
                return true;

            int dwStartTickCount = Environment.TickCount;
            int dwOldCheckPoint = status.dwCheckPoint;

            while (status.dwCurrentState == waitStatus)
            {
                // Do not wait longer than the wait hint. A good interval is
                // one tenth the wait hint, but no less than 1 second and no
                // more than 10 seconds.

                int dwWaitTime = status.dwWaitHint / 10;

                if (dwWaitTime < 1000)
                    dwWaitTime = 1000;
                else if (dwWaitTime > 10000)
                    dwWaitTime = 10000;

                Thread.Sleep(dwWaitTime);

                // Check the status again.
                if (QueryServiceStatus(service, status) == 0)
                    break;

                if (status.dwCheckPoint > dwOldCheckPoint)
                {
                    // The service is making progress.
                    dwStartTickCount = Environment.TickCount;
                    dwOldCheckPoint = status.dwCheckPoint;
                }
                else
                {
                    if (Environment.TickCount - dwStartTickCount > status.dwWaitHint)   // No progress made within the wait hint
                        break;

                }
            }
            return (status.dwCurrentState == desiredStatus);
        }

        private static IntPtr OpenSCManager(ScmAccessRights rights)
        {
            IntPtr scm = OpenSCManager(null, null, rights);
            if (scm == IntPtr.Zero)
            {
                if (Marshal.GetLastWin32Error() == ERROR_ACCESS_DENIED)
                    throw new ApplicationException("Access denied while connecting to service control manager");
                else
                    throw new ApplicationException("Failed to connect to service control manager " + Marshal.GetLastWin32Error());
            }

            return scm;
        }

        public static void SetRecoveryOptions(string serviceName, SC_ACTION pFirstFailure, SC_ACTION pSecondFailure, SC_ACTION pSubsequentFailures, int pDaysToResetFailureCount = 0)
        {
            int NUM_ACTIONS = 3;
            int[] arrActions = new int[NUM_ACTIONS * 2];
            int index = 0;
            arrActions[index++] = (int)pFirstFailure.Type;
            arrActions[index++] = pFirstFailure.Delay;
            arrActions[index++] = (int)pSecondFailure.Type;
            arrActions[index++] = pSecondFailure.Delay;
            arrActions[index++] = (int)pSubsequentFailures.Type;
            arrActions[index++] = pSubsequentFailures.Delay;

            IntPtr tmpBuff = Marshal.AllocHGlobal(NUM_ACTIONS * 8);

            try
            {
                Marshal.Copy(arrActions, 0, tmpBuff, NUM_ACTIONS * 2);
                SERVICE_FAILURE_ACTIONS sfa = new()
                {
                    cActions = 3,
                    dwResetPeriod = pDaysToResetFailureCount,
                    lpCommand = null,
                    lpRebootMsg = null,
                    lpsaActions = new IntPtr(tmpBuff.ToInt64())
                };

                IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.AllAccess);

                if (service == IntPtr.Zero)
                {
                    if (Marshal.GetLastWin32Error() == ERROR_ACCESS_DENIED)
                        throw new ApplicationException("Access denied while opening service");
                    else
                        throw new ApplicationException("Unknown error while opening service");
                }
                bool success = ChangeServiceFailureActions(service, SERVICE_CONFIG_FAILURE_ACTIONS, ref sfa);
                if (!success)
                {
                    if (Marshal.GetLastWin32Error() == ERROR_ACCESS_DENIED)
                        throw new ApplicationException("Access denied while setting failure actions");
                    else
                        throw new ApplicationException("Unknown error while setting failure actions");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tmpBuff);
            }
        }
    }

    public enum ServiceState
    {
        Unknown = -1, // The state cannot be (has not been) retrieved.
        NotFound = 0, // The service is not known on the host server.
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4,
        ContinuePending = 5,
        PausePending = 6,
        Paused = 7,
    }

    public enum ScmAccessRights
    {
        Connect = 0x0001,
        CreateService = 0x0002,
        EnumerateService = 0x0004,
        Lock = 0x0008,
        QueryLockStatus = 0x0010,
        ModifyBootConfig = 0x0020,
        StandardRightsRequired = 0xF0000,
        AllAccess = StandardRightsRequired | Connect | CreateService | EnumerateService | Lock | QueryLockStatus | ModifyBootConfig,
    }

    public enum ServiceAccessRights
    {
        QueryConfig = 0x1,
        ChangeConfig = 0x2,
        QueryStatus = 0x4,
        EnumerateDependants = 0x8,
        Start = 0x10,
        Stop = 0x20,
        PauseContinue = 0x40,
        Interrogate = 0x80,
        UserDefinedControl = 0x100,
        Delete = 0x00010000,
        StandardRightsRequired = 0xF0000,
        AllAccess = StandardRightsRequired | QueryConfig | ChangeConfig | QueryStatus | EnumerateDependants | Start | Stop | PauseContinue | Interrogate | UserDefinedControl,
    }

    [Flags]
    public enum ServiceBootFlag
    {
        Start = 0x00000000,
        SystemStart = 0x00000001,
        Automatic = 0x00000002,
        Manual = 0x00000003,
        Disabled = 0x00000004,
    }

    public enum ServiceControl
    {
        Stop = 0x00000001,
        Pause = 0x00000002,
        Continue = 0x00000003,
        Interrogate = 0x00000004,
        Shutdown = 0x00000005,
        ParamChange = 0x00000006,
        NetBindAdd = 0x00000007,
        NetBindRemove = 0x00000008,
        NetBindEnable = 0x00000009,
        NetBindDisable = 0x0000000A,
    }

    public enum ServiceError
    {
        Ignore = 0x00000000,
        Normal = 0x00000001,
        Severe = 0x00000002,
        Critical = 0x00000003,
    }

    public enum ScActionType
    {
        SC_ACTION_NONE = 0,         // No action.
        SC_ACTION_RESTART = 1,      // Restart the service.
        SC_ACTION_REBOOT = 2,       // Reboot the computer.
        SC_ACTION_RUN_COMMAND = 3,  // Run a command.
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SERVICE_DESCRIPTION
    {
        public string lpDescription;

        public override bool Equals(object? obj) => lpDescription.Equals(obj);

        public override int GetHashCode() => lpDescription.GetHashCode();

        public static bool operator ==(SERVICE_DESCRIPTION left, SERVICE_DESCRIPTION right) => left.Equals(right);

        public static bool operator !=(SERVICE_DESCRIPTION left, SERVICE_DESCRIPTION right) => !(left == right);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SERVICE_FAILURE_ACTIONS
    {
        public int dwResetPeriod;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpRebootMsg;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpCommand;
        public int cActions;
        public IntPtr lpsaActions;

        public override bool Equals(object? obj) => obj switch
        {
            SERVICE_FAILURE_ACTIONS o => dwResetPeriod == o.dwResetPeriod && lpRebootMsg == o.lpRebootMsg && lpCommand == o.lpCommand && cActions == o.cActions && lpsaActions == o.lpsaActions,
            _ => false
        };

        public override int GetHashCode() => HashCode.Combine(dwResetPeriod, lpRebootMsg, lpCommand, cActions, lpsaActions);

        public static bool operator ==(SERVICE_FAILURE_ACTIONS left, SERVICE_FAILURE_ACTIONS right) => left.Equals(right);

        public static bool operator !=(SERVICE_FAILURE_ACTIONS left, SERVICE_FAILURE_ACTIONS right) => !(left == right);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SERVICE_STATUS
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SC_ACTION
    {
        public ScActionType Type;
        public int Delay;

        public override bool Equals(object? obj) => obj switch
        {
            SC_ACTION o => Type == o.Type && Delay == o.Delay,
            _ => false
        };

        public override int GetHashCode() => HashCode.Combine(Type, Delay);

        public static bool operator ==(SC_ACTION left, SC_ACTION right) => left.Equals(right);

        public static bool operator !=(SC_ACTION left, SC_ACTION right) => !(left == right);
    }
}
