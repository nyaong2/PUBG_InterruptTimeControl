using System;
using System.CodeDom;
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

namespace PUBG_InterruptTimeControl.Components.Modal.Warning
{
    /// <summary>
    /// CustomModal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WarningModal : Window
    {
        public UserControl userControl;

        public WarningModal()
        {
            InitializeComponent();
        }
        private void Warning_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock_Content.Text = "해당 프로그램은 PUBG의 공식 프로그램이 아닙니다.\r\n배그 전용임을 나타내기 위해 프로그램의 이름만 PUBG임을 참고 바랍니다.\r\n\r\n" +
                "해당 프로그램에서 \"위험\"에 있는 기능들은\r\nPUBG 운영정책에 위반 될 수 있음을 알려드립니다.\r\n\r\n이 프로그램을 사용함으로서 발생하는 일은 개발자가 책임지지 않습니다.\r\n\r\n"+
                "이에 대해 인지하셨고 동의하시면 \"동의\" 버튼을 눌러주세요.";
        }


        #region Handler
        private void Button_Consent_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Denial_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

    }
}
