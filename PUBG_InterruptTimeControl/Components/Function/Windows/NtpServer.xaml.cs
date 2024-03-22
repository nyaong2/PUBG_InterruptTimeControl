using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Enum;
using PUBG_InterruptTimeControl.Service.Msg;
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

namespace PUBG_InterruptTimeControl.Components.Function.Windows
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NtpServer : UserControl
    {
        private readonly MsgService msgService;
        private const string regPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\W32Time\Parameters";
        private const string regName = "NtpServer";
        private const string originalServer = "time.windows.com";
        
        public NtpServer()
        {
            InitializeComponent();
            msgService = new MsgService();
        }
        private void NtpServer_Loaded(object sender, RoutedEventArgs e)
        {
            ListBox_NtpServer.ItemsSource = GetNtpServerList();
            ChangedLabelCurrentServer();
        }


        #region Function

        private void ChangedLabelCurrentServer()
        {
            Label_CurrentValue.Content = Util.Reg.Read(regPath, regName);
        }

        private string SetFlagConvertString(int flags)
        {
            switch(flags)
            {
                case 8:
                    return ",0x08";
                case 9:
                    return ",0x09";
                default:
                    return null;
            }
        }

        private List<string> GetNtpServerList()
        {
            //https://gist.github.com/mutin-sa/eea1c396b1e610a2da1e5550d94b0453
            var list = new List<string>
            {
                "time.windows.com",
                "time.bora.net",
                "time2.kriss.re.kr",
                "kr.pool.ntp.org",
                "jp.pool.ntp.org",
                "asia.pool.ntp.org",
                "time.advtimesync.com",
                "utcnist.colorado.edu",
                "time.google.com",
                "time1.google.com",
                "time2.google.com",
                "time3.google.com",
                "time4.google.com",
                "time.facebook.com",
                "time1.facebook.com",
                "time2.facebook.com",
                "time3.facebook.com",
                "time4.facebook.com",
                "time5.facebook.com",
                "time.nist.gov",
                "time-a.nist.gov",
                "time-b.nist.gov",
                "time-a.timefreq.bldrdoc.gov",
                "time-b.timefreq.bldrdoc.gov",
                "time-c.timefreq.bldrdoc.gov",
                "0.cn.pool.ntp.org",
                "1.cn.pool.ntp.org",
                "2.cn.pool.ntp.org",
                "3.cn.pool.ntp.org",
                "0.amazon.pool.ntp.org",
                "1.amazon.pool.ntp.org",
                "2.amazon.pool.ntp.org",
                "3.amazon.pool.ntp.org"
            };
            return list;
        }

        private void Apply()
        {
            var selectItem = int.Parse((ComboBox_Flags.SelectedValue as ComboBoxItem).Content.ToString());
            string regValue = ListBox_NtpServer.SelectedValue.ToString() + SetFlagConvertString(selectItem);
            Util.Reg.Write(regPath, regName, regValue, Util.Reg.RegValueKind.SZ);
            Util.Dos.Cmd("sc config \"W32Time\" start= auto");
            Util.Dos.Cmd("sc start W32Time");
            Util.Dos.Cmd("w32tm /config /update");
            var result = Util.Dos.Cmd("w32tm /resync");

            if (result.Contains("완료") == false)
            {
                if (msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.YesNo,
                                "적용에 실패했습니다.\r\n원상복구 하시겠습니까?") == (int)MsgEnum.Result.Yes)
                {
                    Util.Reg.Write(regPath, regName, originalServer, Util.Reg.RegValueKind.SZ);
                    Util.Dos.Cmd("w32tm /config /update");
                    Util.Dos.Cmd("w32tm /resync");
                }
                ChangedLabelCurrentServer();
            } else
            {
                Label_CurrentValue.Content = regValue;
                Util.Dos.Cmd("sc config \"W32Time\" start= demand");
                Util.Dos.Cmd("sc stop W32Time");
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
            }
        }
        private void Apply(string server)
        {
            Util.Reg.Write(regPath, regName, server, Util.Reg.RegValueKind.SZ);
            Util.Dos.Cmd("sc config \"W32Time\" start= auto");
            Util.Dos.Cmd("sc start W32Time");
            Util.Dos.Cmd("w32tm /config /update");
            var result = Util.Dos.Cmd("w32tm /resync");

            if (result.Contains("완료") == false)
            {
                if (msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.YesNo,
                                "적용에 실패했습니다.\r\n원상복구 하시겠습니까?") == (int)MsgEnum.Result.Yes)
                {
                    Util.Reg.Write(regPath, regName, originalServer, Util.Reg.RegValueKind.SZ);
                    Util.Dos.Cmd("w32tm /config /update");
                    Util.Dos.Cmd("w32tm /resync");
                }
                ChangedLabelCurrentServer();
            } else
            {
                Label_CurrentValue.Content = server;
                msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
            }
        }

        private void Restore()
        {
            Util.Reg.Write(regPath, regName, originalServer, Util.Reg.RegValueKind.SZ);
            Util.Dos.Cmd("w32tm /config /update");
            Util.Dos.Cmd("w32tm /resync");
            Label_CurrentValue.Content = originalServer;
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "원상복구 되었습니다.");

        }
        #endregion

        #region Handler
        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            Label_CurrentValue.Content = "적용중 잠시만 기다려주세요.";
            Apply();
        }

        private void Button_InputApply_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(Label_CurrentValue.Content.ToString()))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "서버를 입력해주세요.");
                return;
            }

            Label_CurrentValue.Content = "적용중 잠시만 기다려주세요.";
            Apply(TextBox_Input.Text);
        }

        private void Buttn_Restore_Click(object sender, RoutedEventArgs e)
        {
            Restore();
        }

        #endregion
    }
}
