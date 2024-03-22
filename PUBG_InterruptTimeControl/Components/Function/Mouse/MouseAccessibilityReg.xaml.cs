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

namespace PUBG_InterruptTimeControl.Components.Function.Mouse
{
    public partial class MouseAccessibilityReg : UserControl
    {
        private readonly MsgService msgService;
        public MouseAccessibilityReg()
        {
            InitializeComponent();
            msgService = new MsgService();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetCurrentReg();
            this.TextBox_Flags.Focus(); // 생성자는 비동기적으로 동작하므로 여러가지 과정이 끝나지 않았다면, Focus를 생성자에 넣을 시 제대로 작동을 안함.
        }

        #region Function
        private void GetCurrentReg()
        {
            Label_Flags.Content = Util.Reg.Read(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "Flags");
            Label_MaximumSpeed.Content = Util.Reg.Read(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "MaximumSpeed");
            Label_TimeToMaximumSpeed.Content = Util.Reg.Read(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "TimeToMaximumSpeed");
        }
        #endregion

        #region Handler
        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBox_Flags.Text) ||
                String.IsNullOrEmpty(TextBox_MaximumSpeed.Text) ||
                String.IsNullOrEmpty(TextBox_TimeToMaximumSpeed.Text))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "값이 비어있습니다.");
                return;
            }

            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "Flags",
                           TextBox_Flags.Text, Util.Reg.RegValueKind.SZ);
            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "MaximumSpeed",
                           TextBox_MaximumSpeed.Text, Util.Reg.RegValueKind.SZ);
            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "TimeToMaximumSpeed",
                           TextBox_TimeToMaximumSpeed.Text, Util.Reg.RegValueKind.SZ);

            TextBox_Flags.Text = TextBox_MaximumSpeed.Text = TextBox_TimeToMaximumSpeed.Text = "";

            GetCurrentReg();

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }

        private void Buttn_Restore_Click(object sender, RoutedEventArgs e)
        {
            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "Flags",
                           "62", Util.Reg.RegValueKind.SZ);
            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "MaximumSpeed",
                           "80", Util.Reg.RegValueKind.SZ);
            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Accessibility\MouseKeys", "TimeToMaximumSpeed",
                           "3000", Util.Reg.RegValueKind.SZ);
            GetCurrentReg();

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "원상복구 되었습니다.");
        }
        #endregion
    }
}
