using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

class DllImport
{
    #region Privileges

    public class Privileges
    {
        // OpenProcessToken
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, TokenDesiredAccess DesiredAccess, out IntPtr TokenHandle); //2번째 인자 기본 : UInt32
        /// <summary>
        /// <param name="ProcessHandle"> 접근하고자 하는 프로세스 핸들</param>
        /// <param name="DesiredAccess">토근 접근을 하기위한 접근 권한</param>
        /// </summary>

        [Flags]
        public enum TokenDesiredAccess : UInt32
        {
            SE_PRIVILEGE_ENABLED = 0x00000002,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = 0x00020000,
            TOKEN_ASSIGN_PRIMARY = 0x00000001,
            TOKEN_DUPLICATE = 0x00000002,
            TOKEN_IMPERSONATE = 0x00000004,
            TOKEN_QUERY = 0x00000008,
            TOKEN_QUERY_SOURCE = 0x00000010,
            TOKEN_ADJUST_PRIVILEGES = 0x00000020,
            TOKEN_ADJUST_GROUPS = 0x00000040,
            TOKEN_ADJUST_DEFAULT = 0x00000080,
            TOKEN_ADJUST_SESSIONID = 0x00000100,
            TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY,
            TOKEN_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID
        }

        // LookupPrivilegeValue
        [DllImport("advapi32.dll")]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);
        /// <summary>
        /// <param name="lpSystemName"> 특권을 찾기위한 시스템 이름</param>
        /// <param name="lpName"> 특권이름</param>
        /// <param name="processId"> 검색된 LUID</param>
        /// </summary>

        public struct PRIVILEGESLIST
        {
            public const string SeAuditPrivilege = "SeAuditPrivilege";
            public const string SeAssignPrimaryTokenPrivilege = "SeAssignPrimaryTokenPrivilege";
            public const string SeBackupPrivilege = "SeBackupPrivilege";
            public const string SeCreateGlobalPrivilege = "SeCreateGlobalPrivilege";
            public const string SeCreatePagefilePrivilege = "SeCreatePagefilePrivilege";
            public const string SeCreatePermanentPrivilege = "SeCreatePermanentPrivilege";
            public const string SeCreateTokenPrivilege = "SeCreateTokenPrivilege";
            public const string SeCreateSymbolicLinkPrivilege = "SeCreateSymbolicLinkPrivilege";
            public const string SeChangeNotifyPrivilege = "SeChangeNotifyPrivilege";
            public const string SeDebugPrivilege = "SeDebugPrivilege"; // 프로세스 관련 권한
            public const string SeEnableDelegationPrivilege = "SeEnableDelegationPrivilege";
            public const string SeIncreaSeQuotaPrivilege = "SeIncreaSeQuotaPrivilege";
            public const string SeIncreaSeBaSePriorityPrivilege = "SeIncreaSeBaSePriorityPrivilege";
            public const string SeIncreaSeWorkingSetPrivilege = "SeIncreaSeWorkingSetPrivilege";
            public const string SeImpersonatePrivilege = "SeImpersonatePrivilege";
            public const string SeLockMemoryPrivilege = "SeLockMemoryPrivilege";
            public const string SeLoadDriverPrivilege = "SeLoadDriverPrivilege";
            public const string SeManageVolumePrivilege = "SeManageVolumePrivilege";
            public const string SeMachineAccountPrivilege = "SeMachineAccountPrivilege";
            public const string SeTakeOwnershipPrivilege = "SeTakeOwnershipPrivilege";
            public const string SeProfileSingleProcessPrivilege = "SeProfileSingleProcessPrivilege";
            public const string SeRemoteShutdownPrivilege = "SeRemoteShutdownPrivilege";
            public const string SeRestorePrivilege = "SeRestorePrivilege";
            public const string SeRelabelPrivilege = "SeRelabelPrivilege";
            public const string SeSecurityPrivilege = "SeSecurityPrivilege";
            public const string SeSystemProfilePrivilege = "SeSystemProfilePrivilege";
            public const string SeSystemtimePrivilege = "SeSystemtimePrivilege";
            public const string SeSystemEnvironmentPrivilege = "SeSystemEnvironmentPrivilege";
            public const string SeShutdownPrivilege = "SeShutdownPrivilege";
            public const string SeSyncAgentPrivilege = "SeSyncAgentPrivilege";
            public const string SeTcbPrivilege = "SeTcbPrivilege";
            public const string SeTrustedCredManAccessPrivilege = "SeTrustedCredManAccessPrivilege";
            public const string SeTimeZonePrivilege = "SeTimeZonePrivilege";
            public const string SeUnsolicitedInputPrivilege = "SeUnsolicitedInputPrivilege";
            public const string SeUndockPrivilege = "SeUndockPrivilege";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public UInt32 LowPart;
            public Int32 HighPart;
        }


        // AdjustTokenPrivileges
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
           [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
           ref TOKEN_PRIVILEGES NewState,
           UInt32 BufferLengthInBytes,
           ref TOKEN_PRIVILEGES PreviousState,
           out UInt32 ReturnLengthInBytes);
        /// <summary>
        /// <param name="TokenHandle"> 토큰 핸들</param>
        /// <param name="DisableAllPrivileges">특권 Enable / Disable 결정 (1: disable(true) , 0 : enable(false) )</param>
        /// <param name="NewState"> 새로 설정할 권한</param>
        /// <param name="BufferLengthInBytes"> PreviousState 버퍼사이즈 </param>
        /// <param name="PreviousState"> 이전 권한 저장</param>
        /// <param name="ReturnLengthInBytes"> 이전 권한 사이즈 저장</param>
        /// </summary>


        const int ANYSIZE_ARRAY = 1;
        public struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ANYSIZE_ARRAY)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public UInt32 Attributes;

            public const UInt32 SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
            public const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
            public const UInt32 SE_PRIVILEGE_REMOVED = 0x00000004;
            public const UInt32 SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
        }

        //ImpersonateLoggedOnUser
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
    }

    #endregion

    //----------------------------------------------------------------------------------------------------

    #region Registrys
    public class Registrys
    {
        /// <summary>
        /// RegOpenKeyEx
        /// </summary>
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint RegOpenKeyEx(IntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, ref IntPtr phkResult);

        public enum RegRootKeyKind : uint
        {
            HKEY_CLASSES_ROOT = 0x80000000,
            HKEY_CURRENT_USER = 0x80000001,
            HKEY_LOCAL_MACHINE = 0x80000002,
            HKEY_USERS = 0x80000003,
            HKEY_CURRENT_CONFIG = 0x80000005
        }

        /// <summary>
        /// SetSecurityInfo
        /// </summary>
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint SetSecurityInfo(IntPtr handle, Permissions.SE_OBJECT_TYPE ObjectType, uint SecurityInfo,
            IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);

        /// <summary>
        /// RegCloseKey
        /// </summary>
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint RegCloseKey(IntPtr hKey);
    }
    #endregion

    //----------------------------------------------------------------------------------------------------

    #region Permission
    public class Permissions
    {
        /// <summary>
        /// ConvertStringSidToSid
        /// </summary>
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ConvertStringSidToSid(string StringSid, out IntPtr Sid);

        /// <summary>
        /// AllocateAndInitializeSid
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AllocateAndInitializeSid(
        ref SidIdentifierAuthority pIdentifierAuthority,
        byte nSubAuthorityCount,
        int dwSubAuthority0, int dwSubAuthority1,
        int dwSubAuthority2, int dwSubAuthority3,
        int dwSubAuthority4, int dwSubAuthority5,
        int dwSubAuthority6, int dwSubAuthority7,
        out IntPtr pSid);

        [StructLayout(LayoutKind.Sequential)]
        public struct SidIdentifierAuthority
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.I1)]
            public byte[] Value;
        }

        /// <summary>
        /// FreeSid
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern IntPtr FreeSid(IntPtr pSid); // AllocateAndInitializeSid 해제


        /// <summary>
        /// SetNamedSecurityInfo
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern uint SetNamedSecurityInfo(
        string pObjectName,
        SE_OBJECT_TYPE ObjectType,
        SECURITY_INFORMATION SecurityInfo,
        IntPtr psidOwner,
        IntPtr psidGroup,
        IntPtr pDacl,
        IntPtr pSacl);

        [Flags]
        public enum SE_OBJECT_TYPE : uint
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY
        }

        [Flags]
        public enum SECURITY_INFORMATION : uint
        {
            Owner = 0x00000001,
            Group = 0x00000002,
            Dacl = 0x00000004,
            Sacl = 0x00000008,
            ProtectedDacl = 0x80000000,
            ProtectedSacl = 0x40000000,
            UnprotectedDacl = 0x20000000,
            UnprotectedSacl = 0x10000000
        }

        /// <summary>
        /// GetNamedSecurityInfo
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetNamedSecurityInfo(
        string pObjectName,
        SE_OBJECT_TYPE ObjectType,
        SECURITY_INFORMATION SecurityInfo,
        out IntPtr pSidOwner,
        out IntPtr pSidGroup,
        out IntPtr pDacl,
        out IntPtr pSacl,
        out IntPtr pSecurityDescriptor);


        /// <summary>
        /// SetEntriesInAcl
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int SetEntriesInAcl(
            int cCountOfExplicitEntries,
            EXPLICIT_ACCESS[] pListOfExplicitEntries,
            IntPtr OldAcl,
            out IntPtr NewAcl);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 0)] //Platform independent 32 & 64 bit - use Pack = 0 for both platforms
        public struct EXPLICIT_ACCESS
        {
            public uint grfAccessPermissions;
            public uint grfAccessMode;
            public uint grfInheritance;
            public TRUSTEE Trustee;
        }

        //Platform independent (32 & 64 bit) - use Pack = 0 for both platforms. IntPtr works as well.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 0)]
        public struct TRUSTEE : IDisposable
        {
            public IntPtr pMultipleTrustee;
            public MULTIPLE_TRUSTEE_OPERATION MultipleTrusteeOperation;
            public TRUSTEE_FORM TrusteeForm;
            public TRUSTEE_TYPE TrusteeType;
            public IntPtr ptstrName;

            void IDisposable.Dispose()
            {
                if (ptstrName != IntPtr.Zero) Marshal.Release(ptstrName);
            }

            public string Name { get { return Marshal.PtrToStringAuto(ptstrName); } }
        }
        public enum MULTIPLE_TRUSTEE_OPERATION
        {
            NO_MULTIPLE_TRUSTEE,
            TRUSTEE_IS_IMPERSONATE
        }

        public enum TRUSTEE_FORM
        {
            TRUSTEE_IS_SID,
            TRUSTEE_IS_NAME,
            TRUSTEE_BAD_FORM,
            TRUSTEE_IS_OBJECTS_AND_SID,
            TRUSTEE_IS_OBJECTS_AND_NAME
        }

        public enum TRUSTEE_TYPE
        {
            TRUSTEE_IS_UNKNOWN,
            TRUSTEE_IS_USER,
            TRUSTEE_IS_GROUP,
            TRUSTEE_IS_DOMAIN,
            TRUSTEE_IS_ALIAS,
            TRUSTEE_IS_WELL_KNOWN_GROUP,
            TRUSTEE_IS_DELETED,
            TRUSTEE_IS_INVALID,
            TRUSTEE_IS_COMPUTER
        }
    }
    #endregion

    //----------------------------------------------------------------------------------------------------

    #region Service

    public class Service
    {
        /// <summary>
        /// OpenService
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, SERVICE_ACCESS dwDesiredAccess);

        [Flags]
        public enum SERVICE_ACCESS : uint
        {
            STANDARD_RIGHTS_REQUIRED = 0xF0000,
            SERVICE_NO_CHANGE = 0xFFFFFFFF,
            SERVICE_QUERY_CONFIG = 0x00001,
            SERVICE_CHANGE_CONFIG = 0x00002,
            SERVICE_QUERY_STATUS = 0x00004,
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,
            SERVICE_START = 0x00010,
            SERVICE_STOP = 0x00020,
            SERVICE_PAUSE_CONTINUE = 0x00040,
            SERVICE_INTERROGATE = 0x00080,
            SERVICE_USER_DEFINED_CONTROL = 0x00100,
            SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | //F01FF
                              SERVICE_QUERY_CONFIG |
                              SERVICE_CHANGE_CONFIG |
                              SERVICE_QUERY_STATUS |
                              SERVICE_ENUMERATE_DEPENDENTS |
                              SERVICE_START |
                              SERVICE_STOP |
                              SERVICE_PAUSE_CONTINUE |
                              SERVICE_INTERROGATE |
                              SERVICE_USER_DEFINED_CONTROL)
        }

        /// <summary>
        /// OpenSCManager
        /// </summary>
        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, SCM_ACCESS dwAccess);

        [Flags]
        public enum SCM_ACCESS : uint
        {
            SC_MANAGER_CONNECT = 0x00001,
            SC_MANAGER_CREATE_SERVICE = 0x00002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,
            SC_MANAGER_LOCK = 0x00008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,

            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT | SC_MANAGER_LOCK,

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
        }

        [Flags]
        public enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000F0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001F0000,

            SPECIFIC_RIGHTS_ALL = 0x0000FFFF,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037F
        }

        [Flags]
        public enum ACCESS_MODE : uint
        {
            NOT_USED_ACCESS = 0,
            GRANT_ACCESS,
            SET_ACCESS,
            DENY_ACCESS,
            REVOKE_ACCESS,
            SET_AUDIT_SUCCESS,
            SET_AUDIT_FAILURE
        }

        /// <summary>
        /// ChangeServiceConfig
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ChangeServiceConfig(
        IntPtr hService, //ServiceHandle
        SERVICE_ACCESS nServiceType, // Service Type
        ServiceStartupType nStartType, //Startup Type
        SERVICE_ACCESS nErrorControl, // Service Not Start Control
        string lpBinaryPathName, // not change = null apply
        string lpLoadOrderGroup, //not change = null apply
        IntPtr lpdwTagId, //not change = 0 apply
        [In] char[] lpDependencies, // not change = null apply
        string lpServiceStartName, // not change = null apply
        string lpPassword, // not change = null apply
        string lpDisplayName); // not change = null apply

        [Flags]
        public enum ServiceStartupType : uint
        {
            BootStart = 0,
            SystemStart = 1,
            Automatic = 2,
            Manual = 3,
            Disabled = 4
        }

        /// <summary>
        /// QueryServiceStatusEx
        /// </summary>
        [DllImport("advapi32.dll")]
        public static extern bool QueryServiceStatusEx(IntPtr hService, int InfoLevel, ref SERVICE_STATUS_PROCESS lpBuffer, int cbBufSize, out int pcbBytesNeeded); 
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        internal const int SC_STATUS_PROCESS_INFO = 0;

        [StructLayout(LayoutKind.Sequential)]
        internal sealed class SERVICE_STATUS_PROCESS
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint dwServiceType;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwCurrentState;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwControlsAccepted;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwWin32ExitCode;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwServiceSpecificExitCode;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwCheckPoint;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwWaitHint;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwProcessId;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwServiceFlags;
        }

        /// <summary>
        /// StartService
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StartService(IntPtr hService, int dwNumServiceArgs, string[] lpServiceArgVectors);


        /// <summary>
        /// ControlService
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ControlService(IntPtr hService, SERVICE_CONTROL dwControl, ref SERVICE_STATUS lpServiceStatus);

        [StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_STATUS
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
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
            Paused = 7
        }

        [Flags]
        public enum SERVICE_CONTROL : uint
        {
            STOP = 0x00000001,
            PAUSE = 0x00000002,
            CONTINUE = 0x00000003,
            INTERROGATE = 0x00000004,
            SHUTDOWN = 0x00000005,
            PARAMCHANGE = 0x00000006,
            NETBINDADD = 0x00000007,
            NETBINDREMOVE = 0x00000008,
            NETBINDENABLE = 0x00000009,
            NETBINDDISABLE = 0x0000000A,
            DEVICEEVENT = 0x0000000B,
            HARDWAREPROFILECHANGE = 0x0000000C,
            POWEREVENT = 0x0000000D,
            SESSIONCHANGE = 0x0000000E
        }

        /// <summary>
        /// DeleteService
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteService(IntPtr hService);
    }
    #endregion

    //----------------------------------------------------------------------------------------------------

    #region Processes
    public class Processes
    {
        // GetCurrentProcess
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32")]
        public static extern IntPtr CreateToolhelp32Snapshot(Int32 dwFlags, Int32 th32ProcessID);
        [DllImport("kernel32")]
        public static extern Int32 Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 pe32);
        [DllImport("kernel32")]
        public static extern Int32 Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 pe32);

        public const Int32 MAX_PATH = 260;
        public const Int32 TH32CS_SNAPPROCESS = 2;

        public struct PROCESSENTRY32
        {
            public Int32 dwSize;
            public Int32 cntUsage;
            public Int32 th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public Int32 th32ModuleID;
            public Int32 cntThreads;
            public Int32 th32ParentProcessID;
            public Int32 pcPriClassBase;
            public Int32 dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szExeFile;

            public static Int32 Size
            {
                get { return Marshal.SizeOf(typeof(PROCESSENTRY32)); }
            }
        }

        //QueryFullProcessImageName 폴더 위치
        [DllImport("kernel32.dll", SetLastError = true)] 
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccess Access, Boolean InheritHandle, uint ProcessId);
        
        [Flags]
        internal enum ProcessAccess : uint
        { //OpenProcess 
            PROCESS_ALL_ACCESS = 0x001F0FFF,
            PROCESS_TERMINATE = 0x00000001,
            PROCESS_CREATE_THREAD = 0x00000002,
            PROCESS_VM_OPERATION = 0x00000008,
            PROCESS_VM_READ = 0x00000010,
            PROCESS_VM_WRITE = 0x00000020,
            PROCESS_DUP_HANDLE = 0x00000040,
            PROCESS_CREATE_PROCESS = 0x000000080,
            PROCESS_SET_QUOTA = 0x00000100,
            PROCESS_SET_INFORMATION = 0x00000200,
            PROCESS_QUERY_INFORMATION = 0x00000400,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000,
            SYNCHRONIZE = 0x00100000,
            PROCESS_SUSPEND_RESUME = 0x800
        }
    }
    #endregion

    //----------------------------------------------------------------------------------------------------

    #region Close
    public class Close
    {
        /// <summary>
        /// CloseHandle
        /// </summary>
        [DllImportAttribute("kernel32.dll", EntryPoint = "CloseHandle")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool CloseHandle([InAttribute] IntPtr hObject);

        //CloseServiceHandle
        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);
    }
    #endregion

    //----------------------------------------------------------------------------------------------------
    #region Etc
    public class Etc
    {
        [DllImport("kernel32")]
        public static extern Int32 GetLastError();
    }
    #endregion
}

