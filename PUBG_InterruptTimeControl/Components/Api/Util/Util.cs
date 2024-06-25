using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using System.Drawing;
using static DllImport.Close;
using static DllImport.Processes;
using static DllImport.Service;

class Util
{
    /// <summary>
    /// 최종수정 : 2024-03-16
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
            public static int GetWindowsVersion()
            {
                //app.manifest에서 supportedOS 주석처리 해제해줘야 윈도우10까지 인식함.
                OperatingSystem os = Environment.OSVersion;

                if (os.Version.Major >= new Version(10, 0, 22000, 0).Major)
                    return 11;
                else
                    return os.Version.Major;

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
            RegistryKey rk = RegPathCreate(ref regPath);
            if (rk == null)
                return result;

            try
            {
                if (rk.OpenSubKey(regPath, true).GetValue(SubKey) != null)
                    result = true;
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
                catch { }
                finally
                {
                    rk.Close();
                }
            }
            return getValue.ToString();
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
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return false;
                };

                return false;
            }

            private static WellKnownSidType GetUserSid(string accountName)
            {
                switch (accountName.ToLower())
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

            if (StatusChange(ServiceName, ServiceControllerStatus.Stopped)) // 서비스 종료 정상적으로 됐을 때
            {
                IntPtr scmHandle = OpenSCManager(null, null, SCM_ACCESS.GENERIC_ALL);

                if (scmHandle != IntPtr.Zero)
                {
                    IntPtr serviceHandle = OpenService(scmHandle, ServiceName, SERVICE_ACCESS.SERVICE_ALL_ACCESS);
                    //if (serviceHandle != IntPtr.Zero)
                    //    result = DeleteService(serviceHandle);

                    CloseServiceHandle(serviceHandle);
                    CloseServiceHandle(scmHandle);
                    result = true;
                }
            }
            else
            {
                if (MessageBox.Show("정상적인 방법으로 제거되지 않았습니다.\r\n레지스트리를 지워서 서비스를 제거하시겠습니까?(재부팅 후 제거됨)", "Information", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Reg.DeleteSubKey(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services", ServiceName);
                    Reg.DeleteSubKey(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services", ServiceName);
                    result = true;
                }

            }

            return result;
        }
    }

    #endregion

    #region Processes
    public class Processes
    {
        public class ProcessStruct
        {
            public Icon Process_Icon { get; set; }
            public string Pid { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
        }

        public static List<ProcessStruct> Get()
        {
            var list = new List<ProcessStruct>();
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

            do
            {
                Icon icon = null;

                // 마이크로소프트 오진으로 인해 byte[]로 받은 후 변환해서 사용
                byte[] path = GetPath(pe32.th32ProcessID);
                string path2 = Encoding.Default.GetString(path);
                try
                {
                    icon = Icon.ExtractAssociatedIcon(path2);
                }
                catch { };

                list.Add(new ProcessStruct()
                {
                    Process_Icon = icon,
                    Pid = pe32.th32ProcessID.ToString(),
                    Name = pe32.szExeFile,
                    Path = (path2.Length > 5) ? path2 : "Unknown"
                });

            } while (Process32Next(hSnapshot, ref pe32) != 0);
            CloseHandle(hSnapshot);


            return list;
        }
        private static byte[] GetPath(int pid)
        {
            IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, pid);
            if (processHandle == IntPtr.Zero)
                return null;

            int bufferSize = 512;
            StringBuilder sb = new StringBuilder(bufferSize);
            try
            {
                sb.Clear();
                QueryFullProcessImageName(processHandle, 0, sb, out bufferSize);
            }
            catch { };

            CloseHandle(processHandle);
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public static bool FindName(string[] name)
        {
            var list = new List<ProcessStruct>();//GetAllProcessList();
            HashSet<string> hashSet = new HashSet<string>(list.Select(data => data.Name));

            return name.Any(p => hashSet.Contains(p));
        }
    }
    #endregion

    #region Dos
    public class Dos
    {
        public static string Cmd(in string command)
        {
            var result = "";
            using (Process ps = new Process())
            {
                var cmdPath = Environment.GetEnvironmentVariable("WINDIR") + @"\System32";
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = @"cmd.exe";
                psi.WorkingDirectory = cmdPath;
                psi.CreateNoWindow = true;

                psi.UseShellExecute = false; //true = Process클래스가 ShellExecute 함수 사용. False로하면 CreateProcess로 사용. WorkingDirectory를 쓰려면 True로 해야됨.
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                ps.EnableRaisingEvents = false;
                ps.StartInfo = psi;
                ps.Start();
                ps.StandardInput.WriteLine(command);
                ps.StandardInput.Close();

                var data = ps.StandardOutput.ReadToEnd();
                try
                {
                    data = data.Substring(data.IndexOf(command)); // 맨처음 감지되는 값 제거
                    data = data.Replace(cmdPath + ">", "");
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

                    if (result == null)
                    {
                        data = data.Substring(0, data.LastIndexOf("\r\n"));
                        data = data.Remove(0, data.IndexOf("\r\n") + 2);
                        result = data;
                    }
                }
                catch { result = null; }
                ps.WaitForExit();
                return result;
            }
        }

        public static string Ps(string argument)
        {
            using (Process ps = new Process())
            {
                ps.StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = argument,
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

    //#region Log
    //public class Log
    //{
    //    private static string currentFileName;

    //    internal static bool WriteCreateLogFile()
    //    {
    //        string folderPath = Directory.GetCurrentDirectory() + @"\Log\";
    //        string filePath = folderPath + Time.GetCurrentTimeStr() + ".log";

    //        try
    //        {
    //            DirectoryInfo DI = new DirectoryInfo(folderPath);
    //            FileInfo FI = new FileInfo(filePath);

    //            if (DI.Exists == false)
    //                Directory.CreateDirectory(folderPath);
    //            if (FI.Exists == false)
    //            {
    //                using (StreamWriter SW = new StreamWriter(filePath, true, Encoding.Default))
    //                {
    //                    //StreamWriter = 프로그램 데이터를 텍스트파일로 보낼때 || StreamReader = 텍스트파일을 프로그램상으로 불러올때 || FileStream으로 파일 경로,옵션,접근타입의 옵션을 설정하는 클래스
    //                    currentFileName = filePath;
    //                    SW.Write("");
    //                    SW.Close();
    //                }
    //            }
    //        }
    //        catch { };

    //        return true;
    //    }

    //    internal static void WriteLogFile(in string msg)
    //    {
    //        try
    //        {
    //            FileInfo FI = new FileInfo(currentFileName);
    //            if (FI.Exists == false)
    //                return;

    //        }
    //        catch { };

    //        using (StreamWriter SW = new StreamWriter(currentFileName, true, Encoding.Default))
    //        {
    //            SW.WriteLine(Time.GetCurrentTimeStr() + ": " + msg);
    //            SW.Close();
    //        }
    //    }
    //}
    //#endregion

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