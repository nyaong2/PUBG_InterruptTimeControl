using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using static DllImport.Close;
using static DllImport.Permissions;
using static DllImport.Privileges;
using static DllImport.Processes;
using static DllImport.Registrys;
using static DllImport.Service;

class Util
{
    /// <summary>
    /// 최종수정 : 2024-03-13
    /// </summary>

    #region System
    public class Systems
    {
        public class Account
        {
            public static string GetUserSid()
            {
                return WindowsIdentity.GetCurrent().User.ToString();
            }
        }

        public class Information
        {
            public enum WindowVersionEnum
            {
                Unknown,
                Vista,
                Win7,
                Win8,
                Win10,
                Win11
            }

            public static string GetWindowsVersion()
            {
                //app.manifest에서 supportedOS 주석처리 해제해줘야 윈도우10까지 인식함.
                OperatingSystem os = Environment.OSVersion;

                if (os.Platform == PlatformID.Win32NT)
                {
                    var Minor = os.Version.Minor;
                    switch (os.Version.Major)
                    {
                        case 6:
                            {
                                switch (Minor)
                                {
                                    case 0:
                                        return WindowVersionEnum.Vista.ToString();
                                    case 1:
                                        return WindowVersionEnum.Win7.ToString();
                                    default:
                                        return WindowVersionEnum.Win8.ToString();
                                }
                            }
                        case 10:
                            return WindowVersionEnum.Win10.ToString();
                        default:
                            break;
                    }

                    if(os.Version.Major >= new Version(10,0,22000,0).Major)
                        return WindowVersionEnum.Win11.ToString();

                }
                return WindowVersionEnum.Unknown.ToString();
            }

        }
    }
    #endregion

    #region Time
    public class Time
    {
        internal static string GetCurrentTimeStr()
        {
            DateTime DT = DateTime.Now;
            return DT.ToString("yyyy-MM-dd_HH-mm-ss");
        }
    }
    #endregion

    #region Path
    public class Path
    {
        public static string GetCurrentFolder()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return System.IO.Path.GetDirectoryName(path);
        }
    }
    #endregion

    #region Reg

    public class Reg
    {
        public const string ValueRegDelete = "RegDelete";
        public const string RegDeleteAndWrite = "S_RegDeleteAndWrite";

        public enum RegValueKind
        {
            SZ = RegistryValueKind.String,
            EXPAND_SZ = RegistryValueKind.ExpandString,
            BINARY = RegistryValueKind.Binary,
            DWORD = RegistryValueKind.DWord,
            MULTI_SZ = RegistryValueKind.MultiString,
            QWORD = RegistryValueKind.QWord,
            Unknown = RegistryValueKind.Unknown,
            [ComVisible(false)]
            None = RegistryValueKind.None
        }

        private static RegistryKey RegPathCreate(ref string regPath) //ref를 쓴 이유 : Replace를 쓰기위해
        {
            RegistryKey rk = null;

            if (regPath.ToUpper().Contains("HKEY_CLASSES_ROOT") || regPath.ToUpper().Contains("HKCR"))
            {
                rk = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot,
                                              Environment.Is64BitOperatingSystem
                                                  ? RegistryView.Registry64
                                                  : RegistryView.Registry32);
                regPath = regPath.Remove(0, regPath.Split('\\')[0].Length + 1); // rk 생성후 추후에 OpenSubKey같은 함수 쓸 때 앞부분이 잘려있어야 함.
            }

            else if (regPath.Contains("HKEY_CURRENT_USER") || regPath.ToUpper().Contains("HKCU"))
            {
                rk = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser,
                                              Environment.Is64BitOperatingSystem
                                                  ? RegistryView.Registry64
                                                  : RegistryView.Registry32);
                regPath = regPath.Remove(0, regPath.Split('\\')[0].Length + 1);
            }

            else if (regPath.Contains("HKEY_LOCAL_MACHINE") || regPath.ToUpper().Contains("HKLM"))
            {
                rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                                             Environment.Is64BitOperatingSystem
                                                 ? RegistryView.Registry64
                                                 : RegistryView.Registry32);
                regPath = regPath.Remove(0, regPath.Split('\\')[0].Length + 1);
            }

            return rk;
        }

        internal static bool Exist(string regPath, in string regName)
        {
            bool result = false;
            RegistryKey rk = RegPathCreate(ref regPath);

            if (rk == null)
                return result;

            try
            {
                if (rk.OpenSubKey(regPath, true).GetValue(regName) != null) // null이면 값이 없음 null이 아니면 값이 있음
                    result = true;
            }
            catch { }
            finally
            {
                rk.Close();
            }
            return result;
        }

        internal static bool ExistKey(string regPath, in string SubKey)
        {
            bool result = false;
            string regPathTemp = regPath; // Temp 쓴 이유 : 이것을 2번쓰는 함수의 경우 RegPathKeyCreate 시 regPath 앞에 루트위치가 지워져서 temp를 통해 그것을 방지
            
            RegistryKey rk = RegPathCreate(ref regPathTemp);

            if (rk == null)
                return result;

            try
            {
                var openSubKey = rk.OpenSubKey(regPathTemp, true);
                if (openSubKey != null) // null이면 값이 없음 null이 아니면 값이 있음
                {
                    if(openSubKey.GetValue(SubKey) != null)
                        result = true;
                }
            }
            catch { }
            finally
            {
                rk.Close();
            }

            return result;
        }


        internal static bool Write(string regPath, in string regName, in string regValue, in RegValueKind regType)
        {
            bool result = false;

            RegistryKey rk = RegPathCreate(ref regPath);
            if (rk == null)
                return false;
            try
            {
                rk.CreateSubKey(regPath, true).SetValue(regName, regValue, (RegistryValueKind)regType); // regPath에 적힌 키를 생성후 그 안에 Value를 생성한다.
                result = true; // 생성도중 문제가 발생하지 않았다면 TRUE
            }
            catch { }
            finally
            {
                rk.Close();
            }

            return result;
        }

        internal static bool Delete(string regPath, in string regName)
        {
            bool result = false;
            
            RegistryKey rk = RegPathCreate(ref regPath);
            if (rk == null)
                return result;

            try
            {
                    rk.OpenSubKey(regPath, true).DeleteValue(regName); // 객체에 대해 레지값을 열고 regName에 해당하는 벨류를 제거하라.
                    result = true; // 제거하는 도중 문제가 생기지 않았다면 true
            }
            catch { }
            finally
            {
                rk.Close();
            }

            return result;
        }



        internal static bool DeleteSubKey(string regPath, string subKey)
        {
            bool result = false;
            
            RegistryKey rk = RegPathCreate(ref regPath);

            if (rk == null)
                return result;

            try
            {

                rk = rk.OpenSubKey(regPath, true);
                if (rk != null)
                {
                    rk.DeleteSubKeyTree(subKey);
                    result = true; // 제거하는 도중 문제가 생기지 않았다면 true
                }
            }
            catch { }
            finally
            {
                rk.Close();
            }

            return result;
        }

        internal static string Read(string regPath, in string regName)
        {
            object getValue = null;
            RegistryKey rk = RegPathCreate(ref regPath);

            if (rk != null)
            {
                try
                {
                    if (rk != null)
                        getValue = rk.CreateSubKey(regPath, true).GetValue(regName);
                    if (getValue == null)
                        return null;
                }
                catch {}
                finally 
                { 
                    rk.Close();
                }
            }
            return getValue.ToString();
        }

        internal static bool SetOwnerPermission(in string AccountName, string RegPath)
        {
            bool result = false;
            if (Privilege.Set(PRIVILEGESLIST.SeTakeOwnershipPrivilege, true))
            {
                NTAccount ntAccount = new NTAccount(AccountName);
                string sSid = ntAccount.Translate(typeof(SecurityIdentifier)).Value;

                IntPtr pSid = IntPtr.Zero;
                ConvertStringSidToSid(sSid, out pSid);
                IntPtr hKey = IntPtr.Zero;

                IntPtr RegRootKey = IntPtr.Zero;
                if (RegPath.Contains("HKEY_CLASSES_ROOT"))
                {
                    RegRootKey = (IntPtr)RegRootKeyKind.HKEY_CLASSES_ROOT;
                    RegPath = RegPath.Replace(@"HKEY_CLASSES_ROOT\", "");
                }
                else if (RegPath.Contains("HKEY_CURRENT_USER"))
                {
                    RegRootKey = (IntPtr)RegRootKeyKind.HKEY_CURRENT_USER;
                    RegPath = RegPath.Replace(@"HKEY_CURRENT_USER\", "");
                }
                else if (RegPath.Contains("HKEY_LOCAL_MACHINE"))
                {
                    RegRootKey = (IntPtr)RegRootKeyKind.HKEY_LOCAL_MACHINE;
                    RegPath = RegPath.Replace(@"HKEY_LOCAL_MACHINE\", "");
                }

                const int OWNER_SECURITY_INFORMATION = 0x00000001;
                const int WRITE_OWNER = (int)DllImport.Service.ACCESS_MASK.WRITE_OWNER;
                uint dwErr = RegOpenKeyEx(RegRootKey, RegPath, 0,
                    (Environment.Is64BitOperatingSystem ? (int)RegistryView.Registry64 : (int)RegistryView.Registry32) | WRITE_OWNER, ref hKey);
                if (dwErr == 0)
                {
                    if (SetSecurityInfo(hKey,
                         SE_OBJECT_TYPE.SE_REGISTRY_KEY,
                         OWNER_SECURITY_INFORMATION,
                         pSid,
                         IntPtr.Zero,
                         IntPtr.Zero,
                         IntPtr.Zero) == 0)
                        result = true;
                }
                RegCloseKey(hKey);
            }
            return result;
        }

        internal static bool SetReadOnlyPermission(string RegPath)
        {
            bool result = false;
            //필수요소 권한 얻기
            if (Privilege.Set(PRIVILEGESLIST.SeBackupPrivilege, true) &&
                Privilege.Set(PRIVILEGESLIST.SeRestorePrivilege, true) &&
                Privilege.Set(PRIVILEGESLIST.SeTakeOwnershipPrivilege, true))
            {

                // SetNamedSecurityInfo 이용시에 알맞는 것으로 변경
                if (RegPath.Contains("HKEY_CLASSES_ROOT"))
                    RegPath = RegPath.Replace("HKEY_CLASSES_ROOT", "ROOT");
                else if (RegPath.Contains("HKEY_CURRENT_USER"))
                    RegPath = RegPath.Replace("HKEY_CURRENT_USER", "CURRENT_USER");
                else if (RegPath.Contains("HKEY_LOCAL_MACHINE"))
                    RegPath = RegPath.Replace("HKEY_LOCAL_MACHINE", "MACHINE");


                const int SECURITY_BUILTIN_DOMAIN_RID = 0x00000020;
                const int DOMAIN_ALIAS_RID_ADMINS = 0x00000220;

                const int worldAuthority = 1;
                const int ntAuthority = 5;
                SidIdentifierAuthority sid_auth_world = new SidIdentifierAuthority();
                sid_auth_world.Value = new byte[] { 0, 0, 0, 0, 0, worldAuthority };
                SidIdentifierAuthority sid_auth_nt = new SidIdentifierAuthority();
                sid_auth_nt.Value = new byte[] { 0, 0, 0, 0, 0, ntAuthority };

                //어드미니스트레이터 값 가져와
                IntPtr sid_Everyone = IntPtr.Zero;
                IntPtr sid_System = IntPtr.Zero;
                IntPtr sid_Admin = IntPtr.Zero;
                IntPtr sid_User = IntPtr.Zero;


                AllocateAndInitializeSid(ref sid_auth_world, 1, 0x0, 0, 0, 0, 0, 0, 0, 0, out sid_Everyone);
                AllocateAndInitializeSid(ref sid_auth_nt, 1, 0x12, 0, 0, 0, 0, 0, 0, 0, out sid_System);
                AllocateAndInitializeSid(ref sid_auth_nt, 2, SECURITY_BUILTIN_DOMAIN_RID, DOMAIN_ALIAS_RID_ADMINS, 0, 0, 0, 0, 0, 0, out sid_Admin);
                AllocateAndInitializeSid(ref sid_auth_nt, 2, 0x20, 0x221, 0, 0, 0, 0, 0, 0, out sid_User);

                IntPtr[] sidList = { sid_Everyone, sid_System, sid_Admin, sid_User };

                //1. EveryOne,System,Admin,User Sid ReadOnly Create
                EXPLICIT_ACCESS[] explicitAccesss = new EXPLICIT_ACCESS[4];
                for (int i = 0; i < sidList.Length; i++)
                {
                    explicitAccesss[i].grfAccessPermissions = (uint)DllImport.Service.ACCESS_MASK.GENERIC_READ;
                    explicitAccesss[i].grfAccessMode = (uint)DllImport.Service.ACCESS_MODE.SET_ACCESS;
                    explicitAccesss[i].grfInheritance = 3;
                    explicitAccesss[i].Trustee.TrusteeForm = TRUSTEE_FORM.TRUSTEE_IS_SID;
                    explicitAccesss[i].Trustee.TrusteeType = TRUSTEE_TYPE.TRUSTEE_IS_WELL_KNOWN_GROUP;
                    explicitAccesss[i].Trustee.ptstrName = sidList[i];
                }

                //1-1. 완성본으로 만들기
                IntPtr acl_main = Marshal.AllocHGlobal(8);
                SetEntriesInAcl(4, explicitAccesss, IntPtr.Zero, out acl_main);


                //2. 기존 권한 제거
                if (SetNamedSecurityInfo(RegPath
                    , SE_OBJECT_TYPE.SE_REGISTRY_KEY
                    , SECURITY_INFORMATION.Dacl
                    , IntPtr.Zero
                    , IntPtr.Zero
                    , IntPtr.Zero
                    , IntPtr.Zero
                    ) == 0)
                {
                    //3. 권한 만든거 추가
                    if (SetNamedSecurityInfo(RegPath
                    , SE_OBJECT_TYPE.SE_REGISTRY_KEY
                    , SECURITY_INFORMATION.Dacl | SECURITY_INFORMATION.Owner
                    , sid_System //레지 총괄 소유자
                    , IntPtr.Zero
                    , acl_main
                    , IntPtr.Zero) == 0)
                        result = true;
                }


                foreach (IntPtr sid in sidList)
                    FreeSid(sid);
                Marshal.FreeHGlobal(acl_main);
            }
            return result;
        }

        internal static bool SetRestorePermission(string RegPath)
        {
            //레지스트리에 파라미터 키값 없으면 안됨. 이부분은 나중에 하는걸로. 윈도우 업데이트만 막기에 나중에 쓰일곳 있으면 수정하는걸로
            bool result = false;

            if (Privilege.Set(PRIVILEGESLIST.SeBackupPrivilege, true) &&
                Privilege.Set(PRIVILEGESLIST.SeRestorePrivilege, true) &&
                Privilege.Set(PRIVILEGESLIST.SeTakeOwnershipPrivilege, true))
            {
                if (RegPath.Contains("HKEY_CLASSES_ROOT"))
                    RegPath = RegPath.Replace("HKEY_CLASSES_ROOT", "ROOT");
                else if (RegPath.Contains("HKEY_CURRENT_USER"))
                    RegPath = RegPath.Replace("HKEY_CURRENT_USER", "CURRENT_USER");
                else if (RegPath.Contains("HKEY_LOCAL_MACHINE"))
                    RegPath = RegPath.Replace("HKEY_LOCAL_MACHINE", "MACHINE");

                IntPtr sidOwner, sidGroup, dacl, sacl, daclDescriptor = IntPtr.Zero;

                if (GetNamedSecurityInfo(RegPath + @"\Parameters"
                    , SE_OBJECT_TYPE.SE_REGISTRY_KEY
                    , SECURITY_INFORMATION.Dacl | SECURITY_INFORMATION.UnprotectedDacl | SECURITY_INFORMATION.Owner
                    , out sidOwner
                    , out sidGroup
                    , out dacl//acl_main
                    , out sacl
                    , out daclDescriptor) == 0)
                {
                    if (SetNamedSecurityInfo(RegPath
                    , SE_OBJECT_TYPE.SE_REGISTRY_KEY
                    , SECURITY_INFORMATION.Dacl | SECURITY_INFORMATION.UnprotectedDacl | SECURITY_INFORMATION.Owner
                    , sidOwner //레지 총괄 소유자
                    , sidGroup
                    , dacl//acl_main
                    , sacl) == 0)
                        result = true;
                }
            }
            return result;
        }
    }

    #endregion

    #region File
    public class File
    {
        public class Permission
        {
            public static bool Add(string accountName, FileSystemRights fileSystemRight, AccessControlType accessType, string filePath)
            {
                try //실패시 예외가 뜨므로 제대로 동작하면 true 반환
                {
                    var fs = System.IO.File.GetAccessControl(filePath);
                    var userSid = new SecurityIdentifier(GetUserSid(accountName), null);
                    var accessRule = new FileSystemAccessRule(userSid, fileSystemRight, accessType);
                    fs.AddAccessRule(accessRule);
                    System.IO.File.SetAccessControl(filePath, fs);

                    return true;
                }
                catch { };

                return false;
            }
            public static bool Delete(string accountName, string filePath)
            {
                try //실패시 예외가 뜨므로 제대로 동작하면 true 반환
                {
                    var fs = System.IO.File.GetAccessControl(filePath);
                    var rules = fs.GetAccessRules(true, true, typeof(SecurityIdentifier));
                    var find = false;
                    foreach (FileSystemAccessRule rule in rules)
                    {
                        var sid = rule.IdentityReference as SecurityIdentifier;
                        var account = sid.Translate(typeof(NTAccount)) as NTAccount;

                        if (account.Value.ToString().ToLower().Equals(accountName))
                        {
                            find = true;
                            var accessRule = new FileSystemAccessRule(rule.IdentityReference, rule.FileSystemRights, rule.AccessControlType);
                            fs.RemoveAccessRule(accessRule);
                            System.IO.File.SetAccessControl(filePath, fs);
                            return true;
                        }
                    }
                    if (find == false) // 못찾고 이 메소드를 진행한 경우 true로.
                        return true;
                }
                catch { 
                    return false;
                };

                return false;
            }

            private static WellKnownSidType GetUserSid(string accountName)
            {
                switch(accountName.ToLower())
                {
                    case "everyone":
                        return WellKnownSidType.WorldSid;
                    default:
                        return WellKnownSidType.NullSid;
                }
            }
        }

        public static bool Exist(in string filePath, in string fileName)
        {
            try
            {
                if (System.IO.File.Exists(filePath + @"\" + fileName)) // 파일이 있는지 확인한다
                    return true; // 있다면 true
            }
            catch { };

            return false;
        }

        public static bool Copy(in string sourceFilePath, in string destmationFilePath)
        {
            try
            {
                System.IO.File.Copy(sourceFilePath, destmationFilePath); // 파일을 복사한다.
                return true; // 문제없이 잘 됐다면 성공
            }
            catch { };

            return false;
        }

        public static bool Rename(in string Path, in string CurrentName, in string ChangeName)
        {
            try
            {
                string CurrentFilePath = Path + @"\" + CurrentName;
                string ChangeFilePath = Path + @"\" + ChangeName;
                System.IO.File.Move(CurrentFilePath, ChangeFilePath);
                return true;
            }
            catch { };
            return false;
        }

        public static bool Delete(in string filePath, in string fileName)
        {
            try
            {
                if (!Exist(filePath, fileName)) //파일이 없을 경우
                    return true;

                System.IO.File.Delete(filePath + @"\" + fileName); // 파일을 제거한다.
                return true; // 문제없이 잘 됐다면 true
            }
            catch { };

            return false;
        }
    }
    #endregion

    #region Service

    public class Service
    {
        public static ServiceController[] GetServiceList()
        {
            Privilege.Set(PRIVILEGESLIST.SeBackupPrivilege, true);
            Privilege.Set(PRIVILEGESLIST.SeRestorePrivilege, true);
            Privilege.Set(PRIVILEGESLIST.SeTakeOwnershipPrivilege, true);
            return ServiceController.GetServices();
            //foreach (ServiceController service in services)
            //    MessageBox

            ///<summary>
            /// ServiceName = 실질적인 서비스 이름
            /// DisplayName = 표시되는 서비스 이름
            /// </summary>

        }
        /// <summary>
        /// 서비스 시작 상태 변경
        /// </summary>
        public static bool StatusChange(in string ServiceName, in ServiceControllerStatus serviceStatus)
        {
            bool result = false;

            using (ServiceController sc = new ServiceController(ServiceName))
            {
                if (sc != null)
                {
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Running:
                            if (serviceStatus == ServiceControllerStatus.Stopped)
                            {
                                sc.Stop();
                                sc.WaitForStatus(serviceStatus, TimeSpan.FromSeconds(5000));

                                if (sc.Status == ServiceControllerStatus.Stopped) // 정상 멈춤 확인
                                    result = true;
                                else if (!(sc.Status == ServiceControllerStatus.Stopped)) // 기본방법으로 꺼지지 않았을 때 프로세스 종료
                                {
                                    SERVICE_STATUS_PROCESS ssp = new SERVICE_STATUS_PROCESS();

                                    int ignored;
                                    QueryServiceStatusEx(sc.ServiceHandle.DangerousGetHandle(), SC_STATUS_PROCESS_INFO, ref ssp, Marshal.SizeOf(ssp), out ignored);
                                
                                    using (Process process = Process.GetProcessById((int)ssp.dwProcessId))
                                    {
                                        process.Kill();
                                        if (process.WaitForExit(5000))
                                            result = true;
                                    }
                                }
                            }
                            break;

                        case ServiceControllerStatus.Stopped:
                            if (serviceStatus == ServiceControllerStatus.Stopped)
                                result = true;
                            if (serviceStatus == ServiceControllerStatus.Running)
                            {
                                if (ConfigChange(ServiceName, ServiceStartupType.Automatic))
                                {
                                    sc.Start();
                                    sc.WaitForStatus(serviceStatus, TimeSpan.FromSeconds(5000));
                                    if (sc.Status == ServiceControllerStatus.Running)
                                        result = true;
                                }
                            }
                            break;
                    }
                    sc.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// 서비스 시작 유형 변경
        /// </summary>
        /// <returns></returns>
        public static bool ConfigChange(in string serviceName, in ServiceStartupType serviceStartupType)
        {
            bool result = false;

            IntPtr scmHandle = OpenSCManager(null, null, SCM_ACCESS.GENERIC_ALL);

            if (scmHandle != IntPtr.Zero)
            {
                IntPtr serviceHandle = OpenService(scmHandle, serviceName, SERVICE_ACCESS.SERVICE_QUERY_CONFIG | SERVICE_ACCESS.SERVICE_CHANGE_CONFIG);

                if (serviceHandle != IntPtr.Zero)
                {
                    result = ChangeServiceConfig(serviceHandle, SERVICE_ACCESS.SERVICE_NO_CHANGE, serviceStartupType,
                        SERVICE_ACCESS.SERVICE_NO_CHANGE, null, null, IntPtr.Zero, null, null, null, null);
                }
                CloseServiceHandle(serviceHandle);
            }
            CloseServiceHandle(scmHandle);

            return result;
        }



        /// <summary>
        /// 서비스 삭제
        /// </summary>
        public static bool Remove(in string ServiceName)
        {
            bool result = false;

            if(StatusChange(ServiceName,ServiceControllerStatus.Stopped)) // 서비스 종료 정상적으로 됐을 때
            { 
                IntPtr scmHandle = OpenSCManager(null, null, SCM_ACCESS.GENERIC_ALL);

                if (scmHandle != IntPtr.Zero)
                {
                    IntPtr serviceHandle = OpenService(scmHandle, ServiceName, SERVICE_ACCESS.SERVICE_ALL_ACCESS);
                    if (serviceHandle != IntPtr.Zero)
                        result = DeleteService(serviceHandle);

                    CloseServiceHandle(serviceHandle);
                    CloseServiceHandle(scmHandle);
                    return result;
                } 
            } 
            else
            {
                if (MessageBox.Show("정상적인 방법으로 제거되지 않았습니다.\r\n레지스트리를 지워서 서비스를 제거하시겠습니까?(재부팅 후 제거됨)", "Information", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return false;

                //두개로 나눈건 CurrentControlSet과 
                result = Reg.DeleteSubKey(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services", ServiceName);
                result = Reg.DeleteSubKey(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services", ServiceName);

            }

            return result;
        }


    }

    #endregion

    #region Processes
    public class Processes
    {
        public class ProcessCollection
        {
            public string PID { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
        }

        public static List<ProcessCollection> GetAllProcessList()
        {
            List<ProcessCollection> ReturnList = new List<ProcessCollection>();
            PROCESSENTRY32 pe32 = new PROCESSENTRY32();

            IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (hSnapshot == IntPtr.Zero)
                return null;

            pe32.dwSize = PROCESSENTRY32.Size;

            if (Process32First(hSnapshot, ref pe32) == 0)
            {
                CloseHandle(hSnapshot);
                return null;
            }

            StringBuilder SbProcessPath = new StringBuilder(260);
            IntPtr hProcess = IntPtr.Zero;
            do
            {
                SbProcessPath.Clear();
                hProcess = OpenProcess(ProcessAccess.PROCESS_ALL_ACCESS, false, (uint)pe32.th32ProcessID);
                int ProcessPathCapacity = SbProcessPath.Capacity;
                QueryFullProcessImageName(hProcess, 0, SbProcessPath, ref ProcessPathCapacity);
                SbProcessPath.Replace(@"\" + pe32.szExeFile, "");
                ReturnList.Add(new ProcessCollection()
                {
                    PID = pe32.th32ProcessID.ToString(),
                    Name = pe32.szExeFile,
                    Path = (SbProcessPath.Length > 5) ? SbProcessPath.ToString() : "Unknown"
                });

            } while (Process32Next(hSnapshot, ref pe32) != 0);
            CloseHandle(hSnapshot);

            if (hProcess != IntPtr.Zero)
                CloseHandle(hProcess);

            return ReturnList;
        }

        public static bool FindName(string[] processName)
        {
            var list = GetAllProcessList();
            HashSet<string> hashSet = new HashSet<string>(list.Select(data => data.Name));

            return processName.Any(p => hashSet.Contains(p));
        }


    }
    #endregion

    #region Dos
    public class Dos
    {
        public static string Cmd(in string Command)
        {
            var result = "";
            using (Process ps = new Process())
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = @"cmd.exe";
                psi.WorkingDirectory = Environment.GetEnvironmentVariable("WINDIR")+ @"\System32";
                psi.CreateNoWindow = true;

                psi.UseShellExecute = false; //true = Process클래스가 ShellExecute 함수 사용. False로하면 CreateProcess로 사용. WorkingDirectory를 쓰려면 True로 해야됨.
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                ps.EnableRaisingEvents = false;
                ps.StartInfo = psi;
                ps.Start();
                ps.StandardInput.WriteLine(Command);
                ps.StandardInput.Close();

                var data = ps.StandardOutput.ReadToEnd();
                try
                {
                    data = data.Substring(data.IndexOf(Command)); // 맨처음 감지되는 값 제거
                    //data = data.Substring(data.IndexOf("\r\n")+2, (data.IndexOf("\r\n\r\n") - data.IndexOf("\r\n") -2 )); //아래 match를 안쓸때 수작업 필요한 것
                    Match match = Regex.Match(data, @"\r\n(.+?)\r\n\r\n", RegexOptions.Singleline);
                    if (match.Success)
                        result = match.Groups[1].Value;  // 첫 번째 그룹에 있는 값을 가져옵니다.
                    else
                        result = null;

                    if (result == null)
                    {
                        Match match2 = Regex.Match(data, @"\r\n(.+?)\r\n");
                        if (match.Success)
                            result = match.Groups[1].Value;  // 첫 번째 그룹에 있는 값을 가져옵니다.
                        else
                            result = null;
                    }
                }
                catch { result = null; }
                ps.WaitForExit();
                return result;
            }
        }

        public static string Ps(in string Argument)
        {
            using (Process ps = new Process())
            {
                ps.StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = Argument,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true // 창 표시 
                };
                ps.Start();
                return ps.StandardOutput.ReadToEnd();
            };
        }
    }
    #endregion

    #region Privilege

    private static List<string> PrivilegeList = new List<string>();

    public class Privilege
    {
        public static bool Set(in string privilege, bool setApply)
        {
            bool result = false;

            bool ApplyChecking = false;
            foreach (string name in PrivilegeList) //이미 적용되어있는지 체크
            {
                if (name.Equals(privilege))
                    ApplyChecking = true;
            }

            if (ApplyChecking) // 적용이 되어있다면 넘기기
                return true;

            setApply = (setApply == true) ? setApply = false : setApply = true; //false가 활성화 시키는 것임


            //현재 프로세스에 대한 핸들 얻어옴
            IntPtr hProcess = GetCurrentProcess();

            //현재 프로세스에 대한 액세스 토큰 오픈(핸들값 획득)
            IntPtr hToken;
            if (OpenProcessToken(hProcess, TokenDesiredAccess.TOKEN_QUERY | TokenDesiredAccess.TOKEN_ADJUST_PRIVILEGES, out hToken) != false)
            {
                //명시된 권한을 표현할 LUID 검색 (LUID : 특정 권한을 표현하는 구조체)
                LUID luid = new LUID();
                if (LookupPrivilegeValue(null, privilege, out luid) != false) //해당 Privilege가 활성화가 안되어있다면
                {

                    // First, a LUID_AND_ATTRIBUTES structure that points to Enable a privilege.
                    LUID_AND_ATTRIBUTES luAttr = new LUID_AND_ATTRIBUTES
                    {
                        Luid = luid,
                        Attributes = LUID_AND_ATTRIBUTES.SE_PRIVILEGE_ENABLED
                    };

                    // Now we create a TOKEN_PRIVILEGES structure with our modifications
                    TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
                    {
                        PrivilegeCount = 1,
                        Privileges = new LUID_AND_ATTRIBUTES[1]
                    };
                    tp.Privileges[0] = luAttr;
                    TOKEN_PRIVILEGES oldState = new TOKEN_PRIVILEGES(); // Our old state.

                    //권한 조정
                    result = AdjustTokenPrivileges(hToken, setApply, ref tp, (UInt32)Marshal.SizeOf(tp), ref oldState, out UInt32 returnLength);

                    // 적용 리스트에 추가
                    PrivilegeList.Add(privilege);
                }
                CloseHandle(hProcess);
                CloseHandle(hToken);
            }
            return result;
        }
        public static bool BasePrivilageApply()
        {
            if (Set(DllImport.Privileges.PRIVILEGESLIST.SeBackupPrivilege, true) &&
                Set(DllImport.Privileges.PRIVILEGESLIST.SeRestorePrivilege, true) &&
                Set(DllImport.Privileges.PRIVILEGESLIST.SeTakeOwnershipPrivilege, true))
                return true;

            return false;
        }

        public static bool SetImpersonate(in string processName)
        {
            bool result = false;

            Process[] processlist = Process.GetProcesses();
            IntPtr tokenHandle = IntPtr.Zero;
            foreach (Process theProcess in processlist)
            {
                if (theProcess.ProcessName == processName)
                {
                    bool token = OpenProcessToken(theProcess.Handle, TokenDesiredAccess.TOKEN_READ | TokenDesiredAccess.TOKEN_IMPERSONATE | TokenDesiredAccess.TOKEN_DUPLICATE, out tokenHandle);
                    if (token)
                        result = ImpersonateLoggedOnUser(tokenHandle);
                    CloseHandle(theProcess.Handle);
                    break;
                }
            }
            return result;
        }


        public static bool SetTrustedInstaller()
        {
            bool result = false;
            if (SetImpersonate("winlogon")) // 권한을 얻어와야지만 TrustedInstaller를 쓸 수 있음.
            {
                IntPtr SCMHandle = DllImport.Service.OpenSCManager(null, null, DllImport.Service.SCM_ACCESS.GENERIC_ALL);
                if (SCMHandle != IntPtr.Zero)
                {
                    const string ServiceName = "TrustedInstaller";
                    IntPtr schService = DllImport.Service.OpenService(SCMHandle, ServiceName, DllImport.Service.SERVICE_ACCESS.SERVICE_START);

                    if (DllImport.Service.StartService(schService, 0, null))
                        result = SetImpersonate("TrustedInstaller");
                }
                CloseServiceHandle(SCMHandle);
            }
            return result;
        }
    }

    #endregion

    #region Log
    public class Log
    {
        private static string currentFileName;

        internal static bool WriteCreateLogFile()
        {
            string folderPath = Directory.GetCurrentDirectory() + @"\Log\";
            string filePath = folderPath + Time.GetCurrentTimeStr() + ".log";

            try
            {
                DirectoryInfo DI = new DirectoryInfo(folderPath);
                FileInfo FI = new FileInfo(filePath);

                if (DI.Exists == false)
                    Directory.CreateDirectory(folderPath);
                if (FI.Exists == false)
                {
                    using (StreamWriter SW = new StreamWriter(filePath, true, Encoding.Default))
                    {
                        //StreamWriter = 프로그램 데이터를 텍스트파일로 보낼때 || StreamReader = 텍스트파일을 프로그램상으로 불러올때 || FileStream으로 파일 경로,옵션,접근타입의 옵션을 설정하는 클래스
                        currentFileName = filePath;
                        SW.Write("");
                        SW.Close();
                    }
                }
            }
            catch { };

            return true;
        }

        internal static void WriteLogFile(in string msg)
        {
            try
            {
                FileInfo FI = new FileInfo(currentFileName);
                if (FI.Exists == false)
                    return;

            }
            catch { };

            using (StreamWriter SW = new StreamWriter(currentFileName, true, Encoding.Default))
            {
                SW.WriteLine(Time.GetCurrentTimeStr() + ": " + msg);
                SW.Close();
            }
        }
    }
    #endregion

    #region
    public class Convert
    {
        public static bool ParseStringToInt(object str)
        {
            int value;
            return Int32.TryParse(str.ToString(), out value);
        }
    }
    #endregion
}