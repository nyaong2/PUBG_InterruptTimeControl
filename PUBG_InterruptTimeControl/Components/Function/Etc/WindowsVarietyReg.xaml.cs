using Microsoft.Win32;
using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Msg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public partial class WindowsVarietyReg : UserControl
    {
        private readonly MsgService msgService;
        private const string buffer_KeyboardRegPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\kbdclass\Parameters";
        private const string buffer_KeyboardRegName = "KeyboardDataQueueSize";
        private const string buffer_MouseRegPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\mouclass\Parameters";
        private const string buffer_MouseRegName = "MouseDataQueueSize";

        private const string networkInterfacesRegPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
        private readonly string findNetworkInterfaceRegPath = null;
        private const string networkNagleRegName = "TCPNoDelay";

        private const string gameDVR_RegPath = @"HKEY_CURRENT_USER\System\GameConfigStore";
        private const string gameDVR_RegName = "GameDVR_FSEBehaviorMode";

        public WindowsVarietyReg()
        {
            InitializeComponent();
            msgService = new MsgService();
            findNetworkInterfaceRegPath = GetNetworkInterfaceRegPath();
            SetLabelKeyboardBufferReg();
            SetLabelMouseBufferReg();
            SetLabelCurrentNetworkNagle();
            SetLabelGameDVRReg();
        }

        #region Function
        private void SetLabelKeyboardBufferReg()
        {
            var value = Util.Reg.Read(buffer_KeyboardRegPath, buffer_KeyboardRegName);
            Label_CurrentKeyboard.Content = value == null ? "미적용" : value;
        }
        private void SetLabelMouseBufferReg()
        {
            var value = Util.Reg.Read(buffer_MouseRegPath, buffer_MouseRegName);
            Label_CurrentMouse.Content = value == null ? "미적용" : value;
        }

        private void SetLabelGameDVRReg()
        {
            var value = Util.Reg.Read(gameDVR_RegPath, gameDVR_RegName);
            Label_CurrentGameDVR.Content = value == null ? "미적용" : value;
        }

        private void BufferKeyboardApply()
        {
            if (String.IsNullOrEmpty(TextBox_InputKeyboard.Text))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "정수를 입력해주세요.");
                return;
            }
            Util.Reg.Write(buffer_KeyboardRegPath, buffer_KeyboardRegName, TextBox_InputKeyboard.Text, Util.Reg.RegValueKind.DWORD);
            Label_CurrentKeyboard.Content = TextBox_InputKeyboard.Text;
            TextBox_InputKeyboard.Text = "";

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }
        private void BufferKeyboardRestore()
        {
            Util.Reg.Write(buffer_KeyboardRegPath, buffer_KeyboardRegName, "100", Util.Reg.RegValueKind.DWORD);
            Label_CurrentKeyboard.Content = 100;

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "원상복구 되었습니다.");
        }

        private void BufferMouseApply()
        {
            if (String.IsNullOrEmpty(TextBox_InputMouse.Text))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "정수를 입력해주세요.");
                return;
            }
            Util.Reg.Write(buffer_MouseRegPath, buffer_MouseRegName, TextBox_InputMouse.Text, Util.Reg.RegValueKind.DWORD);
            Label_CurrentMouse.Content = TextBox_InputMouse.Text;
            TextBox_InputMouse.Text = "";
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");

        }
        private void BufferMouseRestore()
        {
            Util.Reg.Delete(buffer_MouseRegPath, buffer_MouseRegName);
            Label_CurrentMouse.Content = "NULL";
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "원상복구 되었습니다.");

        }

        private void SetLabelCurrentNetworkNagle()
        {
            Label_CurrentNagle.Content = Util.Reg.ExistKey(findNetworkInterfaceRegPath, networkNagleRegName) == true ? "적용" : "미적용";
        }
        private void NetworkNagleDisable()
        {
            Util.Reg.Write(findNetworkInterfaceRegPath, networkNagleRegName, "1", Util.Reg.RegValueKind.DWORD);
            Label_CurrentNagle.Content = "적용";
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");

        }
        private void NetworkNagleRestore()
        {
            Util.Reg.Delete(findNetworkInterfaceRegPath, networkNagleRegName);
            Label_CurrentNagle.Content = "미적용";
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "원상복구 되었습니다.");

        }

        private string GetNetworkInterfaceRegPath()
        {
            var key = networkInterfacesRegPath;
            key = key.Replace(@"HKEY_LOCAL_MACHINE\", "");
            using (var interfacesKey = Registry.LocalMachine.OpenSubKey(key))
            {
                if (interfacesKey == null)
                    return null;

                var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                var ipv4Address = hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();

                foreach (var name in interfacesKey.GetSubKeyNames())
                {
                    using (var interfaceKey = interfacesKey.OpenSubKey(name))
                    {
                        foreach(var regName in interfaceKey.GetValueNames())
                        {
                            if(regName.ToLower().Equals("dhcpipaddress"))
                            {
                                if (interfaceKey.GetValue(regName).Equals(ipv4Address))
                                    return interfaceKey.Name;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void GameDVRApply()
        {
            Util.Reg.Write(gameDVR_RegPath, gameDVR_RegName, TextBox_GameDVR.Text, Util.Reg.RegValueKind.DWORD);
            Label_CurrentGameDVR.Content = TextBox_GameDVR.Text;
            TextBox_GameDVR.Text = "";
            msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }

        private void GameDVRRestore()
        {
            Util.Reg.Write(gameDVR_RegPath, gameDVR_RegName, "2", Util.Reg.RegValueKind.DWORD);
            Label_CurrentGameDVR.Content = "2";

            msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "원상복구 되었습니다.");
        }
        #endregion

        #region Handler
        private void Button_KeyboardApply_Click(object sender, RoutedEventArgs e)
        {
            BufferKeyboardApply();
        }

        private void Button_KeyboardRestore_Click(object sender, RoutedEventArgs e)
        {
            BufferKeyboardRestore();
        }

        private void Button_MouseApply_Click(object sender, RoutedEventArgs e)
        {
            BufferMouseApply();
        }

        private void Button_MouseRestore_Click(object sender, RoutedEventArgs e)
        {
            BufferMouseRestore();
        }

        private void Button_NetworkNagleApply_Click(object sender, RoutedEventArgs e)
        {
            NetworkNagleDisable();
        }

        private void Button_NetworkNagleRestore_Click(object sender, RoutedEventArgs e)
        {
            NetworkNagleRestore();
        }

        private void Button_GameDVRApply_Click(object sender, RoutedEventArgs e)
        {
            GameDVRApply();
        }
        private void Button_GameDVRRestore_Click(object sender, RoutedEventArgs e)
        {
            GameDVRRestore();
        }
        #endregion

    }
}
