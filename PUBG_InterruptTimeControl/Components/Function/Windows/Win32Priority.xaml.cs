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

namespace PUBG_InterruptTimeControl.Components.Function.Windows
{
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win32Priority : UserControl
    {
        private readonly MsgService msgService;
        private const string regPath = @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\PriorityControl";
        private const string regName = "Win32PrioritySeparation";
        private const int defaultValue = 2;
        private const int programValue = 38;
        private const int backgroundServiceValue = 24;
        
        public Win32Priority()
        {
            InitializeComponent();
            msgService = new MsgService();
        }
        private void Win32Priority_Loaded(object sender, RoutedEventArgs e)
        {
            GetCurrentValue();
            TextBox_Input.Focus();
        }


        #region Function
        private void GetCurrentValue()
        {
            Label_CurrentValue.Content = Util.Reg.Read(regPath, regName);
        }

        private void Apply()
        {
            if (Util.Convert.ParseStringToInt(TextBox_Input.Text) == false)
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "정수를 입력해주세요.");
                return;
            }
            Util.Reg.Write(regPath, regName, TextBox_Input.Text, Util.Reg.RegValueKind.DWORD);
            Label_CurrentValue.Content = TextBox_Input.Text;

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }

        private void Restore()
        {
            Util.Reg.Write(regPath, regName, defaultValue.ToString(), Util.Reg.RegValueKind.DWORD);
            Label_CurrentValue.Content = defaultValue;

            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "원상복구 되었습니다.");

        }
        #endregion

        #region Handler
        private void Button_Program_Click(object sender, RoutedEventArgs e)
        {
            Util.Reg.Write(regPath, regName, programValue.ToString(), Util.Reg.RegValueKind.DWORD);
            Label_CurrentValue.Content = programValue;
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }
        private void Button_Background_Click(object sender, RoutedEventArgs e)
        {
            Util.Reg.Write(regPath, regName, backgroundServiceValue.ToString(), Util.Reg.RegValueKind.DWORD);
            Label_CurrentValue.Content = backgroundServiceValue;
            msgService.Show(MsgEnum.Category.Info, MsgEnum.CloseType.Close, "적용 되었습니다.");
        }
        private void Button_InputApply_Click(object sender, RoutedEventArgs e)
        {
            Apply();
        }
        private void Button_Restore_Click(object sender, RoutedEventArgs e)
        {
            Restore();
        }
        #endregion


    }
}
