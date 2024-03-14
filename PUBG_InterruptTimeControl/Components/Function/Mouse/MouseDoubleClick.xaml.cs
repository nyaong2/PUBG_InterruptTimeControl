using PUBG_InterruptTimeControl.Components.Modal.Action;
using PUBG_InterruptTimeControl.Service.Msg;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// <summary>
    /// MouseDoubleClick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MouseDoubleClick : UserControl
    {
        private readonly MsgService msgService;

        public MouseDoubleClick()
        {
            InitializeComponent();
            msgService = new MsgService();
            
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetCurrentReg();
            Slider_Gage.Value = GetGageValue(Label_CurrentValue.Content.ToString());
        }

        #region Function
        private void GetCurrentReg()
        {
            Label_CurrentValue.Content = Util.Reg.Read(@"HKEY_CURRENT_USER\Control Panel\Mouse", "DoubleClickSpeed");
        }

        private void Apply(int gageValue)
        {
            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Mouse", "DoubleClickSpeed",
                           GetEnumValue(gageValue), Util.Reg.RegValueKind.SZ);
        }

        private string GetEnumValue(int gageValue)
        {
            switch(gageValue)
            {
                case 0:
                    return "900";
                case 1:
                    return "830";
                case 2:
                    return "760";
                case 3:
                    return "690";
                case 4:
                    return "620";
                case 5:
                    return "550";
                case 6:
                    return "480";
                case 7:
                    return "410";
                case 8:
                    return "340";
                case 9:
                    return "270";
                case 10:
                    return "200";
                default:
                    return null;
            }
        }
        private int GetGageValue(string regValue)
        {
            switch (regValue)
            {
                case "900":
                    return 0;
                case "830":
                    return 1;
                case "760":
                    return 2;
                case "690":
                    return 3;
                case "620":
                    return 4;
                case "550":
                    return 5;
                case "480":
                    return 6;
                case "410":
                    return 7;
                case "340":
                    return 8;
                case "270":
                    return 9;
                case "200":
                    return 10;
                default:
                    return -1;
            }
        }
        #endregion

        #region Event Handler
        private void Slider_Gage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = int.Parse(e.NewValue.ToString());
            if (value != -1)
            {
                Apply(value);
                GetCurrentReg();
            }
        }

        private void Button_InputApply_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(TextBox_Input.Text))
            {
                msgService.Show(MsgEnum.Category.Error, MsgEnum.CloseType.Close, "값이 비어있습니다.");
                return;
            }

            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Mouse", "DoubleClickSpeed",
                           TextBox_Input.Text,
                           Util.Reg.RegValueKind.SZ);
            GetCurrentReg();
        }

        private void Button_Restore_Click(object sender, RoutedEventArgs e)
        {
            Util.Reg.Write(@"HKEY_CURRENT_USER\Control Panel\Mouse", "DoubleClickSpeed",
                           "550",
                           Util.Reg.RegValueKind.SZ);
            GetCurrentReg();
        }
        #endregion
    }
}
