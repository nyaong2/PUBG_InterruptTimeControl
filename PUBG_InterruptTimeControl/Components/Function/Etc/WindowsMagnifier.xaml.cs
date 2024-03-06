using PUBG_InterruptTimeControl.Components.Modal.Action;
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

namespace PUBG_InterruptTimeControl.Components.Function.Etc
{
    public partial class WindowsMagnifier : UserControl
    {
        private MsgService msgService;

        public WindowsMagnifier()
        {
            InitializeComponent();
            msgService = new MsgService();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetCurrentReg();
            this.TextBox_SpeechSpeed.Focus(); // 생성자는 비동기적으로 동작하므로 여러가지 과정이 끝나지 않았다면, Focus를 생성자에 넣을 시 제대로 작동을 안함.
        }

        #region Function
        private void GetCurrentReg()
        {
            var value = Util.Reg.Read(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "SpeechSpeed");
            Label_CurrentValue.Content = (String.IsNullOrEmpty(value)) ? "없음" : value;
        }
        #endregion

        #region Handler
        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBox_SpeechSpeed.Text))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "값이 비어있습니다.");
                return;
            }
            if (Util.Reg.Write(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "SpeechSpeed", TextBox_SpeechSpeed.Text, Util.Reg.RegValueKind.DWORD))
                Label_CurrentValue.Content = TextBox_SpeechSpeed.Text;
        }

        private void Buttn_Restore_Click(object sender, RoutedEventArgs e)
        {
            Util.Reg.DeleteSubKey(@"HKEY_CURRENT_USER\Software\Microsoft", "ScreenMagnifier");
            GetCurrentReg();
        }

        private void Button_DefaultRegAdd_Click(object sender, RoutedEventArgs e)
        {
            Util.Reg.Write(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "MagnifierUIWindowDeltaX", "150", Util.Reg.RegValueKind.DWORD);
            Util.Reg.Write(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "MagnifierUIWindowDeltaY", "150", Util.Reg.RegValueKind.DWORD);
            Util.Reg.Write(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "MagnifierUIWindowMinimized", "0", Util.Reg.RegValueKind.DWORD);
            Util.Reg.Write(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "RunningState", "0", Util.Reg.RegValueKind.DWORD);
            Util.Reg.Write(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "SpeechVoice", "Microsoft Heami - Korean (Korean)", Util.Reg.RegValueKind.SZ);
        }

        private void Button_DefaultRegDel_Click(object sender, RoutedEventArgs e)
        {
            Util.Reg.Delete(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "MagnifierUIWindowDeltaX");
            Util.Reg.Delete(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "MagnifierUIWindowDeltaY");
            Util.Reg.Delete(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "MagnifierUIWindowMinimized");
            Util.Reg.Delete(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "RunningState");
            Util.Reg.Delete(@"HKEY_CURRENT_USER\Software\Microsoft\ScreenMagnifier", "SpeechVoice");
        }
        #endregion
    }
}
