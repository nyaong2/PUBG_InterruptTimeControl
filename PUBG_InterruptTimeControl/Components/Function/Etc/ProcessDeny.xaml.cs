using Microsoft.Win32;
using PUBG_InterruptTimeControl.Service.PgReg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PUBG_InterruptTimeControl.Components.Function.Etc
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProcessDeny : UserControl
    {
        private ProgramUtilService pgUtilService;

        private const string regPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        private const string regName = "Debugger";
        private const string regValue = @"%WINDIR%\System32\taskkill.exe";
        private const Util.Reg.RegValueKind regType = Util.Reg.RegValueKind.SZ;
        private const string processName_Se = "tslgame_se.exe";
        private const string processName_Ucldr = "ucldr_battlegrounds_gl.exe";

        public ProcessDeny()
        {
            InitializeComponent();
            pgUtilService = new ProgramUtilService();
        }
        private void ProcessDeny_Loaded(object sender, RoutedEventArgs e)
        {
            SetLabelCurrentValue();
            DisableSeProcessButton();
        }

        #region Function
        private void DisableSeProcessButton()
        {
            if (String.IsNullOrEmpty(pgUtilService.reg_KakaoPath))
            {
                Button_Process_Se_Apply.IsEnabled = false;
                Button_Process_Se_Restore.IsEnabled = false;
            }
        }
        private void SetLabelCurrentValue()
        {
            Label_CurrentProcess_Se.Content = FindDenyProcess(processName_Se) ? "적용" : "미적용";
            Label_CurrentProcess_Ucldr.Content = FindDenyProcess(processName_Ucldr) ? "적용" : "미적용";
        }

        private void DenyProcess_Se()
        {
            var key = new StringBuilder(regPath);
            key.Append(@"\").Append(processName_Se);
            Util.Reg.Write(key.ToString(), regName, regValue, regType);
            Label_CurrentProcess_Se.Content = "적용";

        }
        private void RestoreProcess_Se()
        {
            Util.Reg.DeleteSubKey(regPath, processName_Se);
            Label_CurrentProcess_Se.Content = "미적용";
        }

        private void DenyProcess_Ucldr()
        {
            var key = new StringBuilder(regPath);
            key.Append(@"\").Append(processName_Ucldr);
            Util.Reg.Write(key.ToString(), regName, regValue, regType);
            Label_CurrentProcess_Ucldr.Content = "적용";
        }
        private void RestoreProcess_Ucldr()
        {
            Util.Reg.DeleteSubKey(regPath, processName_Ucldr);
            Label_CurrentProcess_Ucldr.Content = "미적용";
        }


        private bool FindDenyProcess(string processName)
        {
            var path = regPath.Remove(0, regPath.Split('\\')[0].Length + 1);
            using (var registryKey = Registry.LocalMachine.OpenSubKey(path))
            {
                if (registryKey == null)
                    return false;
                foreach (var subKey in registryKey.GetSubKeyNames())
                {
                    if (subKey.ToLowerInvariant().Contains(processName))
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region Handler
        private void Button_Process_Se_Apply_Click(object sender, RoutedEventArgs e)
        {
            DenyProcess_Se();
        }

        private void Button_Process_Se_Restore_Click(object sender, RoutedEventArgs e)
        {
            RestoreProcess_Se();
        }

        private void Button_Process_Ucldr_Apply_Click(object sender, RoutedEventArgs e)
        {
            DenyProcess_Ucldr();
        }

        private void Button_Process_Ucldr_Restore_Click(object sender, RoutedEventArgs e)
        {
            RestoreProcess_Ucldr();
        }
        #endregion

    }
}
